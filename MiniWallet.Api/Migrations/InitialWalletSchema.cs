using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MiniWallet.Api.Data;

#nullable disable

namespace MiniWallet.Api.Migrations;

[DbContext(typeof(WalletDbContext))]
[Migration("202608090001_InitialWalletSchema")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public partial class InitialWalletSchema : Migration
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    protected override void Up(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                Email = table.Column<string>(type: "TEXT", maxLength: 254, nullable: false),
                MobileNumber = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
            }, constraints: table => table.PrimaryKey("PK_Users", x => x.Id));
        migrationBuilder.CreateTable(
            name: "Wallets",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                Balance = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_Wallets", x => x.Id);
                table.CheckConstraint("CK_Wallet_Balance_NonNegative", "CAST(Balance AS REAL) >= 0");
                table.ForeignKey("FK_Wallets_Users_UserId", x => x.UserId, "Users", "Id", onDelete: ReferentialAction.Restrict);
            });
        migrationBuilder.CreateTable(
            name: "Operations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false), ReferenceId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                Type = table.Column<int>(type: "INTEGER", nullable: false), Status = table.Column<int>(type: "INTEGER", nullable: false),
                FromWalletId = table.Column<Guid>(type: "TEXT", nullable: true), ToWalletId = table.Column<Guid>(type: "TEXT", nullable: true),
                Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false), RequestFingerprint = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                FailureCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true), ResultWalletId = table.Column<Guid>(type: "TEXT", nullable: true),
                ResultUserName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true), ResultBalance = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                ResultUpdatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true), CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false), CompletedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
            }, constraints: table => { table.PrimaryKey("PK_Operations", x => x.Id); table.CheckConstraint("CK_Operation_Amount_Positive", "CAST(Amount AS REAL) > 0"); });
        migrationBuilder.CreateTable(
            name: "Transactions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false), OperationId = table.Column<Guid>(type: "TEXT", nullable: false),
                WalletId = table.Column<Guid>(type: "TEXT", nullable: false), Type = table.Column<int>(type: "INTEGER", nullable: false),
                Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false), BalanceBefore = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                BalanceAfter = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false), Status = table.Column<int>(type: "INTEGER", nullable: false), CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_Transactions", x => x.Id);
                table.CheckConstraint("CK_Transaction_Amount_Positive", "CAST(Amount AS REAL) > 0");
                table.ForeignKey("FK_Transactions_Operations_OperationId", x => x.OperationId, "Operations", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_Transactions_Wallets_WalletId", x => x.WalletId, "Wallets", "Id", onDelete: ReferentialAction.Cascade);
            });
        migrationBuilder.CreateIndex(name: "IX_Users_Email", table: "Users", column: "Email", unique: true);
        migrationBuilder.CreateIndex(name: "IX_Users_MobileNumber", table: "Users", column: "MobileNumber", unique: true);
        migrationBuilder.CreateIndex(name: "IX_Wallets_UserId", table: "Wallets", column: "UserId", unique: true);
        migrationBuilder.CreateIndex(name: "IX_Operations_ReferenceId", table: "Operations", column: "ReferenceId", unique: true);
        migrationBuilder.CreateIndex(name: "IX_Transactions_OperationId", table: "Transactions", column: "OperationId");
        migrationBuilder.CreateIndex(name: "IX_Transactions_WalletId_CreatedAtUtc", table: "Transactions", columns: new[] { "WalletId", "CreatedAtUtc" });
    }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    protected override void Down(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        migrationBuilder.DropTable(name: "Transactions"); migrationBuilder.DropTable(name: "Operations"); migrationBuilder.DropTable(name: "Wallets"); migrationBuilder.DropTable(name: "Users");
    }
}
