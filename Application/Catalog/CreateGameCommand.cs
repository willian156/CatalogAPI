using CatalogAPI.Contracts;
using CatalogAPI.Domain.Catalog;
using MassTransit;
using MediatR;

namespace CatalogAPI.Application.Catalog;

public class CreateGameCommand : IRequest<GameDto>
{
    public GameWriteDto Data { get; set; } = new();
}
