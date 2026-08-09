namespace MiniWallet.Api.Domain;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class Wallet
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public Guid Id { get; set; } = Guid.NewGuid();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public Guid UserId { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public User User { get; set; } = null!;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public decimal Balance { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public List<WalletTransaction> Transactions { get; set; } = [];
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
