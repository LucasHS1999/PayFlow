# PayFlow Solution

O seguinte repositório contém uma solução .NET com uma API minimalista para processamento de pagamentos, integrando-se a dois provedores simulados: FastPay e SecurePay. Visando atender as regras de negócio especificadas. Como detalharei a baixo foi incluido swagger para documentação e facilitar os testes

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

## Executando os projetos (sem Docker)
- Terminal 1: `dotnet run --project src/FastPay.Mock`
- Terminal 2: `dotnet run --project src/SecurePay.Mock`
- Terminal 3: `dotnet run --project src/PayFlow.Api`

## Swagger e documentação
Swagger habilitado em desenvolvimento:
- UI: `http://localhost:5175/swagger`

### Endpoint: POST `/payments`
Body:
```
{
  "amount": number,
  "currency": "string"
}
```

Regras de negócio:
- Se `amount < 100`, o provedor utilizado o `FastPay`; caso contrário, `SecurePay`.
- Caso o provedor principal utilizado na regra de valor não responda, a API deve tentar o outro provedor.
- Taxas: 3.49% para valores abaixo de 100; 2.99% + R$ 0.40 para valores a partir de 100.
- URLs dos provedores são lidas de `FASTPAY_URL` e `SECUREPAY_URL`.

Resposta (forma canonica):
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

Cdigos de status:
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
```
curl -X POST http://localhost:5175/payments \
  -H "Content-Type: application/json" \
  -d '{"amount": 50, "currency": "BRL"}'
```

## Testes rápido de saúde da API
```
curl -X GET http://localhost:5175/ 
```
- Retorno esperado:
```
{"status":"ok","service":"PayFlow.Api"}
``` 

## Para realização dos testes com docker-compose
```
docker-compose up --build
```

- A porta utilizada se torna a porta 5000

```
curl -X POST http://localhost:5000/payments \
  -H "Content-Type: application/json" \
  -d '{"amount": 50, "currency": "BRL"}'
```

