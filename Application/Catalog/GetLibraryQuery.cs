using CatalogAPI.Contracts;
using CatalogAPI.Domain.Catalog;
using MassTransit;
using MediatR;

namespace CatalogAPI.Application.Catalog;

public class GetLibraryQuery : IRequest<IReadOnlyList<GameDto>>
{
    public Guid UserId { get; set; }
}
