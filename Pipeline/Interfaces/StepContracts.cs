using System;
using System.Collections.Generic;

public interface IDependsOn<T1> { }
public interface IDependsOn<T1, T2> { }

public interface IStep
{
    Type ProducedType { get; }
    IReadOnlyCollection<Type> DependencyTypes { get; }
    object? Execute(BuildContext ctx);
}