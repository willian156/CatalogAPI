namespace CatalogAPI.Application.Abstractions;

public interface ICommand<T> { }

public interface IQuery<T> { }

public interface ICommandHandler<in C, T>
    where C : ICommand<T>
{
    Task<T> Handle(C command, CancellationToken ct);
}

public interface IQueryHandler<in Q, T>
    where Q : IQuery<T>
{
    Task<T> Handle(Q query, CancellationToken ct);
}
