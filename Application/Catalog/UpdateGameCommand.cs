using CatalogAPI.Contracts;
using CatalogAPI.Domain.Catalog;
using MassTransit;
using MediatR;

namespace CatalogAPI.Application.Catalog;

public class UpdateGameCommand : IRequest<GameDto?>
{
    public Guid Id { get; set; }
    public GameWriteDto Data { get; set; } = new();
}
