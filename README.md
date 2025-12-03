# PayFlow Solution

Solução composta por uma API principal em Minimal API e dois serviços Mock que simulam provedores de pagamento.

## Projetos
- `PayFlow.Api` (Minimal API)
- `FastPay.Mock` (Mock do provedor FastPay)
- `SecurePay.Mock` (Mock do provedor SecurePay)

## Requisitos
- .NET 10 SDK

## Endereços locais (launchSettings)
- `PayFlow.Api`: http://localhost:5175
- `FastPay.Mock`: http://localhost:5114
- `SecurePay.Mock`: http://localhost:5136

## Configuração
O projeto principal lê os endpoints dos provedores via configuração:
- `FASTPAY_URL`
- `SECUREPAY_URL`

Valores já definidos em `src/PayFlow.Api/appsettings.json` para execução local:
- `FASTPAY_URL`: `http://localhost:5114/fastpay/payments`
- `SECUREPAY_URL`: `http://localhost:5136/securepay/transactions`

Você pode sobrescrever por variável de ambiente conforme necessário.

## Executando os projetos (sem Docker)
- Terminal 1: `dotnet run --project src/FastPay.Mock`
- Terminal 2: `dotnet run --project src/SecurePay.Mock`
- Terminal 3: `dotnet run --project src/PayFlow.Api`

No Visual Studio, use Multiple Startup Projects e marque os três projetos.

## Swagger e documentação
Swagger habilitado em desenvolvimento:
- UI: `http://localhost:5175/swagger`

O endpoint principal está documentado com as regras de negócio diretamente no Swagger.

### Endpoint: POST `/payments`
Body:
```
{
  "amount": number,
  "currency": "string"
}
```

Regras de negócio (visíveis no Swagger):
- Se `amount < 100`, o provedor utilizado é `FastPay`; caso contrário, `SecurePay`.
- Taxas: 1.5% para valores abaixo de 100; 2.5% para valores a partir de 100.
- URLs dos provedores são lidas de `FASTPAY_URL` e `SECUREPAY_URL`.

Resposta (forma canônica):
```
{
  "externalId": "string",
  "provider": "FastPay|SecurePay",
  "status": "string",
  "grossAmount": number,
  "fee": number,
  "netAmount": number
}
```

Códigos de status:
- 200: Sucesso
- 400: Erro de validação (amount <= 0 ou currency em branco)
- 502: Erro ao chamar o provedor

## Serviços Mock

### `FastPay.Mock`
- Base URL: `http://localhost:5114`
- Endpoint: `POST /fastpay/payments`
- Retorno:
```
{
  "id": "FP-884512",
  "status": "approved",
  "status_detail": "Pagamento aprovado"
}
```

### `SecurePay.Mock`
- Base URL: `http://localhost:5136`
- Endpoint: `POST /securepay/transactions`
- Retorno:
```
{
  "transaction_id": "SP-19283",
  "result": "success"
}
```

## Testes rápidos
- Valor abaixo de 100 usa FastPay:
```
curl -X POST http://localhost:5175/payments \
  -H "Content-Type: application/json" \
  -d '{"amount": 50, "currency": "BRL"}'
```

- Valor a partir de 100 usa SecurePay:
```
curl -X POST http://localhost:5175/payments \
  -H "Content-Type: application/json" \
  -d '{"amount": 150, "currency": "BRL"}'