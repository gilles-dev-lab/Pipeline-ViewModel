public abstract class PipelineException : Exception
{
    protected PipelineException(string message) : base(message) { }
}

// Errors 
public sealed class MissingDependencyException : PipelineException
{
    public Type Step { get; }
    public Type MissingType { get; }

    public MissingDependencyException(Type step, Type missingType)
        : base($"L'étape {step.Name} dépend de {missingType.Name}, mais aucune étape ne le produit.")
    {
        Step = step;
        MissingType = missingType;
    }
}
// Ex. implémentation : throw new MissingDependencyException(GetType(), typeof(T)); 

public sealed class DuplicateProducerException : PipelineException
{
    public IReadOnlyList<Type> Duplicates { get; }

    public DuplicateProducerException(IEnumerable<Type> duplicates)
        : base($"Producteurs dupliqués pour les types: {string.Join(", ", duplicates.Select(t => t.Name))}")
    {
        Duplicates = duplicates.ToList();
    }
}

public sealed class CycleDetectedException : PipelineException
{
    public Type Node { get; }

    public CycleDetectedException(Type node)
        : base($"Cycle détecté impliquant le type {node.Name}.")
    {
        Node = node;
    }
}

public sealed class StepExecutionException : PipelineException
{
    public Type Step { get; }

    public StepExecutionException(Type step, Exception inner)
        : base($"Erreur lors de l'exécution du step {step.Name}.", inner)
    {
        Step = step;
    }
}


// Pipeline Errors
public sealed class PipelineErrors
{
    private readonly List<PipelineError> _errors = new();

    public IReadOnlyList<PipelineError> Errors => _errors;

    public void Add(Exception ex, Type? step = null)
    {
        _errors.Add(new PipelineError(ex.Message, ex, step));
    }

    public bool HasErrors => _errors.Count > 0;
}
public sealed class PipelineError
{
    public string Message { get; }
    public Exception Exception { get; }
    public Type? Step { get; }

    public PipelineError(string message, Exception exception, Type? step = null)
    {
        Message = message;
        Exception = exception;
        Step = step;
    }
}

// Dans build Context :
public class BuildContext
{
    private readonly Dictionary<Type, object> _values = new();

    public PipelineErrors Errors { get; } = new();

    // ... Set / TryGet etc.
}

// Dans stepbase.cs
object? IStep.Execute(BuildContext ctx) { try { return ExecuteTyped(ctx); } catch (Exception ex) { throw new StepExecutionException(GetType(), ex); } }

// Dans DagBuilder
if (!_steps.Any()) throw new PipelineException("Aucun step n'a été fourni au DagBuilder.");
