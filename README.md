# CatalogAPI

Microsserviço responsável pelo CRUD de jogos e pelo fluxo de compra. Publica `OrderPlacedEvent`, consome `PaymentProcessedEvent` e adiciona o jogo à biblioteca do usuário quando o pagamento é aprovado.

O CRUD exige JWT com perfil `Admin`; a compra exige usuário autenticado.

## Executar

```bash
dotnet run --project CatalogAPI.csproj
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

Os manifests ativos ficam no repositório de orquestração, em `k8s/` e `deploy/azure/`. Os arquivos locais de `k8s/` deste repositório são históricos da Fase 3; não os aplique junto da configuração nova. Consulte `docs/DEPLOY_AZURE_DEVOPS.md` na orquestração.

## Fase 4: testes e Azure DevOps

`dotnet test tests/CatalogAPI.Tests/CatalogAPI.Tests.csproj` executa os testes automatizados. `azure-pipelines.yml` valida PRs e publica a branch main no ACR/AKS com rolling update. Configure o variable group `fcg-production` conforme o tutorial central. Credenciais são obrigatórias via configuração externa; os defaults de senha/chave foram removidos.

`GET /search?q=portl` consulta Elasticsearch com fuzzy search e relevância. Configure `Search__Endpoint`, `Search__ApiKey` (somente nuvem/HTTPS), `ConnectionStrings__Redis` e `ConnectionStrings__MongoDb`. Alterações são indexadas por worker com outbox persistente; `POST /search/reindex` exige Admin e enfileira jogos antigos. Avaliações permanecem no MongoDB e o cache tem fallback para PostgreSQL em falhas de Redis.
