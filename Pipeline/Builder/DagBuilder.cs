using System;
using System.Collections.Generic;
using System.Linq;

public interface IDagBuilder {
    BuildContext BuildContext(TParameters parameters);
}

/// Le builder utilise un contexte de construction (ctx) comme espace de travail intermédiaire.
/// Les steps enrichissent ce contexte au fur et à mesure, puis "IBuildContextToVmConverter" le convertit en ViewModel final.
public sealed class DagBuilder<TParams,TVm>: IDagBuilder
{
    
    private readonly IReadOnlyList<IStep<TParams, TVm>> _steps;

    public DagBuilder(IEnumerable<IStep<TParams,TVm>> steps)
    {
        _steps = (steps ?? throw new ArgumentNullException(nameof(steps))).ToList();
        if (!_steps.Any()) throw new ArgumentException("Au moins un step est requis.", nameof(steps));
    }

    /// <summary>
    /// Gère les différentes étapes (validation, ordonnanement)
    /// On renvoie un ctx (merci Obvious ! :-)
    /// </summary>
    public BuildContext BuildContext<TParameters>(TParameters parameters) where TParameters : class
    {
        ValidateGraph();

        var batches = TopologicalSortBatches();
        var ctx = new BuildContext();

        if (parameters != null)
        {
            ctx.Set(parameters);
        }

        foreach (var batch in batches)
        {
            // Évite les effets de bord entre steps d’un même batch et permet parrallèlisation
            var producedList = new List<(IStep step, object produced)>();

            foreach (var step in batch)
            {
                var produced = step.Execute(ctx);
                producedList.Add((step, produced));
            }

            // Commit results into context
            foreach (var (step, produced) in producedList)
            {
                if (produced == null) continue;
                ctx.Set(step.ProducedType, produced);
            }
        }

        return ctx;
    }
    // public void ExecuteUntil<T>(BuildContext ctx)
    // {
    //     var requiredTypes = GetRequiredTypes(typeof(T));

    //     foreach (var batch in TopologicalSortBatches())
    //     {
    //         foreach (var step in batch)
    //         {
    //             if (requiredTypes.Contains(step.ProducedType))
    //                 step.Execute(ctx);
    //         }
    //     }
    // }

    #region Validation des dépendances
    // Validation / detection / topo sort 
    private void ValidateGraph()
    {
        // Chaque IStep déclare le type qu'elle produit (ProducedType),
        // qui correspond au type retourné par son exécution (ExecuteTyped()).
        // On regroupe ici toutes les étapes par type produit.
        var producersByType = _steps.GroupBy(s => s.ProducedType)
                                    .ToDictionary(g => g.Key, g => g.ToList());
        // Règle : un type ne peut être produit que par UNE seule étape.
        // Sinon, le graphe devient ambigu (on ne saurait pas quelle étape utiliser).
        var duplicates = producersByType.Where(kv => kv.Value.Count > 1).ToList();
        if (duplicates.Any())
        {
            var names = string.Join(", ", duplicates.Select(d => d.Key.Name));
            throw new InvalidOperationException($"Producteurs dupliqués pour les types: {names}");
        }

        // Ensemble des types réellement produits par le graphe.
        // Utilisé pour des recherches rapides (Contains).
        var producedTypes = producersByType.Keys.ToHashSet();
 
        // _steps contient toutes les étapes du graphe (injectées via la DI).
        // Chaque étape déclare les types dont elle dépend (DependencyTypes).
        // On vérifie ici que chaque dépendance correspond bien
        // à un type produit par une autre étape.
        foreach (var step in _steps)
        {
            foreach (var dep in step.DependencyTypes)
            {
                if (!producedTypes.Contains(dep))
                    throw new InvalidOperationException(
                        $"L'étape produisant {step.ProducedType.Name} dépend de {dep.Name}, mais aucune étape ne produit ce type."
                    );
            }
        }
        // Vérifie qu'il n'existe pas de dépendances circulaires entre les types
        // (ex: A dépend de B, B dépend de A).
        DetectCyclesOnTypes();
    }

    private void DetectCyclesOnTypes()
    {
        var nodes = _steps.Select(s => s.ProducedType).ToHashSet();
        var adj = nodes.ToDictionary(t => t, _ => new List<Type>());

        foreach (var step in _steps)
        {
            var produced = step.ProducedType;
            foreach (var dep in step.DependencyTypes)
            {
                if (!adj.ContainsKey(dep)) adj[dep] = new List<Type>();
                adj[dep].Add(produced);
            }
        }

        var visited = new HashSet<Type>();
        var stack = new HashSet<Type>();

        foreach (var node in adj.Keys)
        {
            if (!visited.Contains(node))
                Visit(node, visited, stack, adj);
        }
    }

    private void Visit(Type node, HashSet<Type> visited, HashSet<Type> stack, Dictionary<Type, List<Type>> adj)
    {
        if (stack.Contains(node))
            throw new InvalidOperationException($"Cycle détecté impliquant le type {node.Name}.");

        if (visited.Contains(node)) return;

        stack.Add(node);

        if (adj.TryGetValue(node, out var children))
        {
            foreach (var child in children) Visit(child, visited, stack, adj);
        }

        stack.Remove(node);
        visited.Add(node);
    }
    #endregion

    /// <summary>
    /// Construit un graphe de dépendances entre étapes
    /// Calcule le nombre de dépendances restantes pour chaque étape (indegree)
    /// Exécute un tri topologique
    /// Regroupe les étapes exécutables en parallèle dans des batches
    /// </summary>
    private List<List<IStep>> TopologicalSortBatches()
    {
        // indegree = nombre de dépendances NON encore satisfaites pour chaque étape.
        // Au départ, c'est simplement le nombre de types dont l'étape dépend.
        var indegree = _steps.ToDictionary(s => s, s => s.DependencyTypes.Count);
        // Pour chaque type produit, on conserve la liste des étapes
        // qui dépendent de ce type.
        // Cela permet, lorsqu'un type est produit, de "débloquer"
        // les étapes correspondantes.
        var dependents = new Dictionary<Type, List<IStep>>();
        foreach (var step in _steps)
        {
            foreach (var dep in step.DependencyTypes)
            {
                if (!dependents.TryGetValue(dep, out var list))
                {
                    list = new List<IStep>();
                    dependents[dep] = list;
                }
                list.Add(step);
            }
        }
        // Résultat : chaque sous-liste représente un "batch"
        // d'étapes pouvant être exécutées en parallèle.
        var result = new List<List<IStep>>();
        // Algorithme de tri topologique (variante de Kahn)
        while (indegree.Any())
        {
            // Étapes dont toutes les dépendances sont satisfaites
            // (aucun type requis restant).
            var ready = indegree.Where(kv => kv.Value == 0).Select(kv => kv.Key).ToList();
            // S'il n'y a aucune étape prête alors qu'il en reste,
            // le graphe contient un cycle ou une dépendance impossible.
            if (!ready.Any())
                throw new InvalidOperationException("Impossible de résoudre l'ordre des étapes (cycle ou dépendance non satisfaite).");
            // Ce batch peut être exécuté immédiatement (en parallèle)
            result.Add(ready);

            // On retire les étapes exécutées du graphe
            // et on met à jour les dépendances restantes
            foreach (var step in ready)
            {
                indegree.Remove(step);
                var producedType = step.ProducedType;
                // Toutes les étapes qui dépendaient de ce type
                // ont maintenant une dépendance en moins.
                if (dependents.TryGetValue(producedType, out var deps))
                {
                    foreach (var depStep in deps)
                    {
                        if (indegree.ContainsKey(depStep))
                            indegree[depStep]--;
                    }
                }
            }
        }

        return result;
    }
}