using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MiniWallet.Api.Data;

#nullable disable

namespace MiniWallet.Api.Migrations;

[DbContext(typeof(WalletDbContext))]
[Migration("202608090002_AddTransactionUnixTimestamp")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public partial class AddTransactionUnixTimestamp : Migration
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    protected override void Up(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        migrationBuilder.AddColumn<long>(name: "CreatedAtUnixMilliseconds", table: "Transactions", type: "INTEGER", nullable: false, defaultValue: 0L);
        // Backfill any transactions created before this migration. New rows set the value in the application.
        migrationBuilder.Sql("UPDATE Transactions SET CreatedAtUnixMilliseconds = CAST(strftime('%s', CreatedAtUtc) AS INTEGER) * 1000 WHERE CreatedAtUnixMilliseconds = 0;");
        migrationBuilder.DropIndex(name: "IX_Transactions_WalletId_CreatedAtUtc", table: "Transactions");
        migrationBuilder.CreateIndex(name: "IX_Transactions_WalletId_CreatedAtUnixMilliseconds", table: "Transactions", columns: new[] { "WalletId", "CreatedAtUnixMilliseconds" });
    }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    protected override void Down(MigrationBuilder migrationBuilder)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        migrationBuilder.DropIndex(name: "IX_Transactions_WalletId_CreatedAtUnixMilliseconds", table: "Transactions");
        migrationBuilder.DropColumn(name: "CreatedAtUnixMilliseconds", table: "Transactions");
        migrationBuilder.CreateIndex(name: "IX_Transactions_WalletId_CreatedAtUtc", table: "Transactions", columns: new[] { "WalletId", "CreatedAtUtc" });
    }
}
