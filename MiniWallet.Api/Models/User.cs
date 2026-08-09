namespace MiniWallet.Api.Domain;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class User
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public Guid Id { get; set; } = Guid.NewGuid();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string Name { get; set; } = null!;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string Email { get; set; } = null!;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public string MobileNumber { get; set; } = null!;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public Wallet Wallet { get; set; } = null!;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
