using MiniWallet.Api.Contracts;
using MiniWallet.Api.Domain;

namespace MiniWallet.Api.Services;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public interface IWalletService
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    Task<WalletResponse> CreateAsync(CreateWalletRequest request, CancellationToken cancellationToken);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    Task<OperationResponse> CreditAsync(MoneyRequest request, CancellationToken cancellationToken);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    Task<OperationResponse> DebitAsync(MoneyRequest request, CancellationToken cancellationToken);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    Task<OperationResponse> TransferAsync(TransferRequest request, CancellationToken cancellationToken);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    Task<WalletResponse> GetAsync(Guid walletId, CancellationToken cancellationToken);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    Task<PagedResponse<TransactionResponse>> GetTransactionsAsync(Guid walletId, TransactionType? type, DateTimeOffset? from,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        DateTimeOffset? to, int pageNumber, int pageSize, CancellationToken cancellationToken);
}
