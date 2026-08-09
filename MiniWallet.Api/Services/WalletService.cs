using System.Data;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MiniWallet.Api.Contracts;
using MiniWallet.Api.Data;
using MiniWallet.Api.Domain;

namespace MiniWallet.Api.Services;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class WalletService(WalletDbContext db, ILogger<WalletService> logger) : IWalletService
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<WalletResponse> CreateAsync(CreateWalletRequest request, CancellationToken ct) => await WriteWithRetryAsync(async () =>
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var mobile = request.MobileNumber.Trim();
        if (await db.Users.AnyAsync(u => u.Email == email || u.MobileNumber == mobile, ct))
            throw new ConflictException("Email or mobile number is already registered.");
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var now = DateTimeOffset.UtcNow;
        var user = new User { Name = request.Name.Trim(), Email = email, MobileNumber = mobile, CreatedAtUtc = now, UpdatedAtUtc = now };
        var wallet = new Wallet { User = user, Balance = request.InitialBalance, CreatedAtUtc = now, UpdatedAtUtc = now };
        db.Wallets.Add(wallet);
        if (request.InitialBalance > 0)
        {
            var operation = NewOperation($"INITIAL-{wallet.Id:N}", OperationType.InitialCredit, wallet.Id, null, request.InitialBalance, now);
            Complete(operation, wallet, now);
            db.Operations.Add(operation);
            db.Transactions.Add(NewTransaction(operation, wallet, TransactionType.Credit, request.InitialBalance, 0, request.InitialBalance, now));
        }
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException) { throw new ConflictException("Email or mobile number is already registered."); }
        await tx.CommitAsync(ct);
        logger.LogInformation("Created wallet {WalletId} for user {UserId}", wallet.Id, user.Id);
        return ToWalletResponse(wallet);
    }, ct);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public Task<OperationResponse> CreditAsync(MoneyRequest request, CancellationToken ct) =>
        ChangeBalanceAsync(request, OperationType.Credit, TransactionType.Credit, ct);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public Task<OperationResponse> DebitAsync(MoneyRequest request, CancellationToken ct) =>
        ChangeBalanceAsync(request, OperationType.Debit, TransactionType.Debit, ct);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    private async Task<OperationResponse> ChangeBalanceAsync(MoneyRequest request, OperationType operationType, TransactionType transactionType, CancellationToken ct) => await WriteWithRetryAsync(async () =>
    {
        var referenceId = request.ReferenceId.Trim();
        var fingerprint = Fingerprint(operationType, request.WalletId, null, request.Amount);
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var previous = await db.Operations.AsNoTracking().SingleOrDefaultAsync(o => o.ReferenceId == referenceId, ct);
        if (previous is not null) return Existing(previous, referenceId, fingerprint);

        var wallet = await db.Wallets.Include(w => w.User).SingleOrDefaultAsync(w => w.Id == request.WalletId, ct) ?? throw new WalletNotFoundException(request.WalletId);
        var now = DateTimeOffset.UtcNow;
        var operation = NewOperation(referenceId, operationType, wallet.Id, null, request.Amount, now, fingerprint);
        if (transactionType == TransactionType.Debit && wallet.Balance < request.Amount)
        {
            operation.Status = OperationStatus.Failed;
            operation.FailureCode = "INSUFFICIENT_FUNDS";
            operation.CompletedAtUtc = now;
            db.Operations.Add(operation);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            logger.LogInformation("Rejected debit on wallet {WalletId}; ref {ReferenceId}", wallet.Id, referenceId);
            return ToOperationResponse(operation);
        }
        var before = wallet.Balance;
        wallet.Balance = transactionType == TransactionType.Credit ? before + request.Amount : before - request.Amount;
        wallet.UpdatedAtUtc = now;
        Complete(operation, wallet, now);
        db.Operations.Add(operation);
        db.Transactions.Add(NewTransaction(operation, wallet, transactionType, request.Amount, before, wallet.Balance, now));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        logger.LogInformation("{OperationType} {Amount} on wallet {WalletId}; ref {ReferenceId}", operationType, request.Amount, wallet.Id, referenceId);
        return ToOperationResponse(operation);
    }, ct);

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<OperationResponse> TransferAsync(TransferRequest request, CancellationToken ct) => await WriteWithRetryAsync(async () =>
    {
        if (request.FromWalletId == request.ToWalletId) throw new ValidationException("Source and destination wallets must differ.");
        var referenceId = request.ReferenceId.Trim();
        var fingerprint = Fingerprint(OperationType.Transfer, request.FromWalletId, request.ToWalletId, request.Amount);
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var previous = await db.Operations.AsNoTracking().SingleOrDefaultAsync(o => o.ReferenceId == referenceId, ct);
        if (previous is not null) return Existing(previous, referenceId, fingerprint);

        var ids = new[] { request.FromWalletId, request.ToWalletId }.OrderBy(x => x).ToArray();
        var wallets = await db.Wallets.Include(w => w.User).Where(w => ids.Contains(w.Id)).ToDictionaryAsync(w => w.Id, ct);
        if (!wallets.TryGetValue(request.FromWalletId, out var source)) throw new WalletNotFoundException(request.FromWalletId);
        if (!wallets.TryGetValue(request.ToWalletId, out var target)) throw new WalletNotFoundException(request.ToWalletId);
        var now = DateTimeOffset.UtcNow;
        var operation = NewOperation(referenceId, OperationType.Transfer, source.Id, target.Id, request.Amount, now, fingerprint);
        if (source.Balance < request.Amount)
        {
            operation.Status = OperationStatus.Failed;
            operation.FailureCode = "INSUFFICIENT_FUNDS";
            operation.CompletedAtUtc = now;
            db.Operations.Add(operation);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            logger.LogInformation("Rejected transfer from {WalletId}; ref {ReferenceId}", source.Id, referenceId);
            return ToOperationResponse(operation);
        }
        var sourceBefore = source.Balance;
        var targetBefore = target.Balance;
        source.Balance -= request.Amount;
        target.Balance += request.Amount;
        source.UpdatedAtUtc = target.UpdatedAtUtc = now;
        Complete(operation, source, now);
        db.Operations.Add(operation);
        db.Transactions.AddRange(NewTransaction(operation, source, TransactionType.Debit, request.Amount, sourceBefore, source.Balance, now),
            NewTransaction(operation, target, TransactionType.Credit, request.Amount, targetBefore, target.Balance, now));
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        logger.LogInformation("Transferred {Amount} from {FromWalletId} to {ToWalletId}; ref {ReferenceId}", request.Amount, source.Id, target.Id, referenceId);
        return ToOperationResponse(operation);
    }, ct);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<WalletResponse> GetAsync(Guid walletId, CancellationToken ct) =>
        ToWalletResponse(await db.Wallets.AsNoTracking().Include(w => w.User).SingleOrDefaultAsync(w => w.Id == walletId, ct) ?? throw new WalletNotFoundException(walletId));
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<PagedResponse<TransactionResponse>> GetTransactionsAsync(Guid walletId, TransactionType? type, DateTimeOffset? from, DateTimeOffset? to, int pageNumber, int pageSize, CancellationToken ct)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        if (from.HasValue && to.HasValue && from > to) throw new ValidationException("From date must be earlier than or equal to to date.");
        if (!await db.Wallets.AnyAsync(w => w.Id == walletId, ct)) throw new WalletNotFoundException(walletId);
        var query = db.Transactions.AsNoTracking().Include(t => t.Operation).Where(t => t.WalletId == walletId);
        if (type.HasValue) query = query.Where(t => t.Type == type.Value);
        if (from.HasValue) query = query.Where(t => t.CreatedAtUnixMilliseconds >= from.Value.ToUnixTimeMilliseconds());
        if (to.HasValue) query = query.Where(t => t.CreatedAtUnixMilliseconds <= to.Value.ToUnixTimeMilliseconds());
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(t => t.CreatedAtUnixMilliseconds).ThenByDescending(t => t.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .Select(t => new TransactionResponse(t.Id, t.WalletId, t.Type, t.Amount, t.BalanceBefore, t.BalanceAfter, t.Operation.ReferenceId, t.Status, t.CreatedAtUtc)).ToListAsync(ct);
        return new PagedResponse<TransactionResponse>(items, pageNumber, pageSize, total, (int)Math.Ceiling(total / (double)pageSize));
    }

    private static WalletOperation NewOperation(string referenceId, OperationType type, Guid from, Guid? to, decimal amount, DateTimeOffset now, string? fingerprint = null) =>
        new() { ReferenceId = referenceId, Type = type, FromWalletId = from, ToWalletId = to, Amount = amount, RequestFingerprint = fingerprint ?? Fingerprint(type, from, to, amount), Status = OperationStatus.Completed, CreatedAtUtc = now };
    private static WalletTransaction NewTransaction(WalletOperation operation, Wallet wallet, TransactionType type, decimal amount, decimal before, decimal after, DateTimeOffset now) =>
        new() { Operation = operation, WalletId = wallet.Id, Type = type, Amount = amount, BalanceBefore = before, BalanceAfter = after, CreatedAtUtc = now, CreatedAtUnixMilliseconds = now.ToUnixTimeMilliseconds() };
    private static void Complete(WalletOperation operation, Wallet wallet, DateTimeOffset now) { operation.Status = OperationStatus.Completed; operation.ResultWalletId = wallet.Id; operation.ResultUserName = wallet.User.Name; operation.ResultBalance = wallet.Balance; operation.ResultUpdatedAtUtc = wallet.UpdatedAtUtc; operation.CompletedAtUtc = now; }
    private static OperationResponse Existing(WalletOperation operation, string reference, string fingerprint)
    {
        if (operation.RequestFingerprint != fingerprint) throw new ConflictException("Reference ID was already used for a different operation.");
        return ToOperationResponse(operation);
    }
    private static OperationResponse ToOperationResponse(WalletOperation o) => new(o.Id, o.ReferenceId, o.Type, o.Status,
        o.ResultWalletId.HasValue && o.ResultUserName is not null && o.ResultBalance.HasValue && o.ResultUpdatedAtUtc.HasValue ? new WalletResponse(o.ResultWalletId.Value, o.ResultUserName, o.ResultBalance.Value, o.ResultUpdatedAtUtc.Value) : null,
        o.FailureCode, o.CreatedAtUtc, o.CompletedAtUtc);
    private static WalletResponse ToWalletResponse(Wallet w) => new(w.Id, w.User.Name, w.Balance, w.UpdatedAtUtc);
    private static string Fingerprint(OperationType type, Guid from, Guid? to, decimal amount)
    {
        var target = to?.ToString("N") ?? string.Empty;
        var value = $"{type}|{from:N}|{target}|{amount.ToString("F2", CultureInfo.InvariantCulture)}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }
    private async Task<T> WriteWithRetryAsync<T>(Func<Task<T>> work, CancellationToken ct)
    {
        for (var attempt = 0; ; attempt++)
        {
            try { return await work(); }
            catch (Exception ex) when (IsRetryableSqliteLock(ex) && attempt < 3)
            {
                logger.LogWarning("SQLite lock while processing wallet operation; retry {Attempt}", attempt + 1);
                db.ChangeTracker.Clear();
                await Task.Delay(TimeSpan.FromMilliseconds(25 * (attempt + 1)), ct);
            }
        }
    }
    private static bool IsRetryableSqliteLock(Exception exception)
    {
        var sqlite = exception as SqliteException ?? exception.InnerException as SqliteException;
        return sqlite is not null && (sqlite.SqliteErrorCode == 5 || sqlite.SqliteErrorCode == 6);
    }
}
