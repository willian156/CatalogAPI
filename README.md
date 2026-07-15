# CatalogAPI

Microsserviço responsável pelo CRUD de jogos e pelo fluxo de compra. Publica `OrderPlacedEvent`, consome `PaymentProcessedEvent` e adiciona o jogo à biblioteca do usuário quando o pagamento é aprovado.

O CRUD exige JWT com perfil `Admin`; a compra exige usuário autenticado.

## Executar

```bash
dotnet run --project CatalogAPI.sln
```

## Variáveis de ambiente

- `ASPNETCORE_URLS`
- `RabbitMq__Host`
- `RabbitMq__Username`
- `RabbitMq__Password`
- `RabbitMq__PaymentQueue`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__Key`
- `ConnectionStrings__Database` (PostgreSQL exclusivo do serviço)

## Docker

```bash
docker build -t fcg/catalog-api:latest .
```

## Kubernetes

```bash
kubectl apply -f k8s/
```
