using Microsoft.EntityFrameworkCore;
using MiniWallet.Api.Domain;

namespace MiniWallet.Api.Data;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class WalletDbContext(DbContextOptions<WalletDbContext> options) : DbContext(options)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public DbSet<User> Users => Set<User>();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public DbSet<Wallet> Wallets => Set<Wallet>();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public DbSet<WalletTransaction> Transactions => Set<WalletTransaction>();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public DbSet<WalletOperation> Operations => Set<WalletOperation>();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    protected override void OnModelCreating(ModelBuilder modelBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(254).IsRequired();
            entity.Property(x => x.MobileNumber).HasMaxLength(16).IsRequired();
            entity.HasIndex(x => x.Email).IsUnique();
            entity.HasIndex(x => x.MobileNumber).IsUnique();
            entity.HasOne(x => x.Wallet).WithOne(x => x.User).HasForeignKey<Wallet>(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.Property(x => x.Balance).HasPrecision(18, 2);
            entity.HasIndex(x => x.UserId).IsUnique();
            entity.ToTable(x => x.HasCheckConstraint("CK_Wallet_Balance_NonNegative", "CAST(Balance AS REAL) >= 0"));
        });
        modelBuilder.Entity<WalletOperation>(entity =>
        {
            entity.Property(x => x.ReferenceId).HasMaxLength(100).IsRequired();
            entity.Property(x => x.RequestFingerprint).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.ResultBalance).HasPrecision(18, 2);
            entity.Property(x => x.ResultUserName).HasMaxLength(120);
            entity.Property(x => x.FailureCode).HasMaxLength(64);
            entity.HasIndex(x => x.ReferenceId).IsUnique();
            entity.ToTable(x => x.HasCheckConstraint("CK_Operation_Amount_Positive", "CAST(Amount AS REAL) > 0"));
        });
        modelBuilder.Entity<WalletTransaction>(entity =>
        {
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.BalanceBefore).HasPrecision(18, 2);
            entity.Property(x => x.BalanceAfter).HasPrecision(18, 2);
            entity.HasOne(x => x.Operation).WithMany(x => x.Transactions).HasForeignKey(x => x.OperationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.WalletId, x.CreatedAtUnixMilliseconds });
            entity.ToTable(x => x.HasCheckConstraint("CK_Transaction_Amount_Positive", "CAST(Amount AS REAL) > 0"));
        });
    }
}
