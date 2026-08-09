# Mini Wallet API

An ASP.NET Core 8 / EF Core / SQLite REST backend demonstrating an auditable, idempotent wallet ledger.

## Design

`User` owns exactly one `Wallet`. A `Wallet` owns many financial `WalletTransaction` records. Every client money request creates one `WalletOperation`; credit/debit produces one transaction and a transfer produces two. A failed insufficient-funds operation is persisted without a financial transaction.

- `Users.Email` and `Users.MobileNumber` are unique.
- `Wallets.UserId` is a unique foreign key, enforcing one user to one wallet.
- `Operations.ReferenceId` is globally unique. Retrying an identical request returns its persisted original result; a changed request with that reference returns `409 Conflict`.
- `Transactions.OperationId` is a required foreign key to `Operations.Id`.
- SQLite enforces `CHECK (Balance >= 0)`.
- Credit, debit, and transfer use serializable transactions. SQLite lock errors are retried with a cleared EF change tracker; operation reference uniqueness preserves idempotency during retry.

## Run

Install the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0), then run from this folder:

```powershell
dotnet restore MiniWallet.Api/MiniWallet.Api.csproj
dotnet build MiniWallet.Api/MiniWallet.Api.csproj --configuration Release
dotnet test MiniWallet.Api.Tests/MiniWallet.Api.Tests.csproj --configuration Release
dotnet run --project MiniWallet.Api/MiniWallet.Api.csproj
```

The application runs EF Core migrations at startup and creates `wallet.db`. Swagger is available at the `/swagger` URL printed by ASP.NET.

## API

| Method | Path |
|---|---|
| POST | `/api/wallets` |
| POST | `/api/wallets/{walletId}/credit` |
| POST | `/api/wallets/{walletId}/debit` |
| POST | `/api/wallets/transfer` |
| GET | `/api/wallets/{walletId}/balance` |
| GET | `/api/wallets/{walletId}/transactions?type=Credit&pageNumber=1&pageSize=20` |

Money endpoints return an `OperationResponse`, including the operation reference, status, and the original wallet result. Invalid inputs return `400`, missing valid resources return `404`, duplicate/mismatched references and insufficient-funds operations return `409`, and unexpected errors are logged and returned as generic `500` responses.

## Local performance verification

The balance query is read-only and uses no unnecessary tracking. Transaction history has an index on `(WalletId, CreatedAtUtc)`. Money operations use short transaction scopes. To record local timing, start the API in Release mode and use a repeatable tool such as `wrk`, `bombardier`, or PowerShell `Measure-Command` against representative credit/debit/transfer requests. The 150–200 ms target is a local-development target, not a production SLA.

## Known environment limitation

The source was statically reviewed in this workspace, but this machine has the .NET 8 runtime only—not the SDK—so restore, migration, compilation, API, benchmark, and automated-test execution could not be performed here.
