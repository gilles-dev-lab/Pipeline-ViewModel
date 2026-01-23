using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

/// Classe de base pour ne pas dupliquer le code dans les step concrets (centralisation de la logique commune à tous les steps)
/// - automatise la déclaration des dépendances (ComputeDependencyTypes)
/// - ProducedType = typeof(TProduced)
/// - DependencyTypes are inferred from implemented IDependsOn<> marker interfaces.

public abstract class StepBase<TProduced> : IStep
{
    public Type ProducedType => typeof(TProduced);

    public virtual IReadOnlyCollection<Type> DependencyTypes => ComputeDependencyTypes(); 

    protected abstract TProduced? ExecuteTyped(BuildContext ctx);
    object? IStep.Execute(BuildContext ctx) => ExecuteTyped(ctx);

    // Renvoie les dépendances de IDependsOn<A, B...>
    private IReadOnlyCollection<Type> ComputeDependencyTypes()
    {
        var deps = new List<Type>();
        var interfaces = GetType().GetTypeInfo().ImplementedInterfaces;
        foreach (var iface in interfaces)
        {
            if (!iface.IsGenericType) continue;
            var def = iface.GetGenericTypeDefinition();
            if (def == typeof(IDependsOn<>) || def == typeof(IDependsOn<,>))
            {
                deps.AddRange(iface.GetGenericArguments());
            }
        }
        return deps;
    }
    protected T Require<T>(BuildContext ctx) where T : class
    {   
        if (!ctx.TryGet<T>(out var value))
            throw new InvalidOperationException(
                $"{GetType().Name} requires {typeof(T)}");

        return value;
    }
}