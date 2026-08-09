using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MiniWallet.Api.Contracts;
using MiniWallet.Api.Data;
using MiniWallet.Api.Domain;
using MiniWallet.Api.Services;
using Xunit;

namespace MiniWallet.Api.Tests;

public sealed class WalletServiceTests : IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private DbContextOptions<WalletDbContext> _options = null!;

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        _options = new DbContextOptionsBuilder<WalletDbContext>().UseSqlite(_connection).Options;
        await using var db = new WalletDbContext(_options);
        await db.Database.MigrateAsync();
    }
    public Task DisposeAsync() => _connection.DisposeAsync().AsTask();

    [Fact]
    public async Task Credit_is_idempotent_and_returns_original_result()
    {
        var wallet = await CreateWalletAsync(100m);
        var first = await Service().CreditAsync(new MoneyRequest { WalletId = wallet.WalletId, Amount = 25m, ReferenceId = "credit-1" }, default);
        var retry = await Service().CreditAsync(new MoneyRequest { WalletId = wallet.WalletId, Amount = 25m, ReferenceId = "credit-1" }, default);
        Assert.Equal(OperationStatus.Completed, first.Status);
        Assert.Equal(first, retry);
        Assert.Equal(125m, (await Service().GetAsync(wallet.WalletId, default)).Balance);
    }

    [Fact]
    public async Task Insufficient_debit_is_persisted_without_changing_balance()
    {
        var wallet = await CreateWalletAsync(100m);
        var operation = await Service().DebitAsync(new MoneyRequest { WalletId = wallet.WalletId, Amount = 101m, ReferenceId = "debit-fail-1" }, default);
        Assert.Equal(OperationStatus.Failed, operation.Status);
        Assert.Equal("INSUFFICIENT_FUNDS", operation.FailureCode);
        Assert.Equal(100m, (await Service().GetAsync(wallet.WalletId, default)).Balance);
        await using var db = new WalletDbContext(_options);
        Assert.Empty(await db.Transactions.Where(x => x.OperationId == operation.OperationId).ToListAsync());
    }

    [Fact]
    public async Task Transfer_creates_two_entries_and_is_atomic()
    {
        var source = await CreateWalletAsync(100m);
        var target = await CreateWalletAsync(0m);
        var operation = await Service().TransferAsync(new TransferRequest { FromWalletId = source.WalletId, ToWalletId = target.WalletId, Amount = 60m, ReferenceId = "transfer-1" }, default);
        Assert.Equal(OperationStatus.Completed, operation.Status);
        Assert.Equal(40m, (await Service().GetAsync(source.WalletId, default)).Balance);
        Assert.Equal(60m, (await Service().GetAsync(target.WalletId, default)).Balance);
        await using var db = new WalletDbContext(_options);
        Assert.Equal(2, await db.Transactions.CountAsync(x => x.OperationId == operation.OperationId));
    }

    private WalletService Service() => new(new WalletDbContext(_options), NullLogger<WalletService>.Instance);
    private Task<WalletResponse> CreateWalletAsync(decimal balance) => Service().CreateAsync(new CreateWalletRequest
    {
        Name = $"Test {Guid.NewGuid():N}", Email = $"{Guid.NewGuid():N}@example.com", MobileNumber = $"+919{Random.Shared.Next(100000000, 999999999)}", InitialBalance = balance
    }, default);
}
