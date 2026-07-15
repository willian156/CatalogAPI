using CatalogAPI.Contracts;
using CatalogAPI.Domain.Catalog;
using MassTransit;
using MediatR;

namespace CatalogAPI.Application.Catalog;

public class GetGameQuery : IRequest<GameDto?>
{
    public Guid Id { get; set; }
}
