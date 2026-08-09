using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MiniWallet.Api.Contracts;
using MiniWallet.Api.Domain;
using MiniWallet.Api.Services;

namespace MiniWallet.Api.Controllers;

[ApiController]
[Route("api/wallets")]
[Produces("application/json")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class WalletsController(IWalletService wallets) : ControllerBase
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
    [HttpPost]
    [ProducesResponseType(typeof(WalletResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<ActionResult<WalletResponse>> Create(CreateWalletRequest request, CancellationToken ct)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        try { var result = await wallets.CreateAsync(request, ct); return CreatedAtAction(nameof(GetBalance), new { walletId = result.WalletId }, result); }
        catch (ConflictException ex) { return ConflictProblem(ex.Message); }
    }

    [HttpPost("{walletId:guid}/credit")]
    [ProducesResponseType(typeof(OperationResponse), StatusCodes.Status200OK)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<ActionResult<OperationResponse>> Credit(Guid walletId, MoneyRequest request, CancellationToken ct)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        if (walletId == Guid.Empty || request.WalletId != walletId) return BadRequestProblem("Wallet ID must be present and match the route.");
        return await ExecuteOperation(() => wallets.CreditAsync(request, ct));
    }

    [HttpPost("{walletId:guid}/debit")]
    [ProducesResponseType(typeof(OperationResponse), StatusCodes.Status200OK)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<ActionResult<OperationResponse>> Debit(Guid walletId, MoneyRequest request, CancellationToken ct)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        if (walletId == Guid.Empty || request.WalletId != walletId) return BadRequestProblem("Wallet ID must be present and match the route.");
        return await ExecuteOperation(() => wallets.DebitAsync(request, ct));
    }

    [HttpPost("transfer")]
    [ProducesResponseType(typeof(OperationResponse), StatusCodes.Status200OK)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<ActionResult<OperationResponse>> Transfer(TransferRequest request, CancellationToken ct) =>
        await ExecuteOperation(() => wallets.TransferAsync(request, ct));
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    [HttpGet("{walletId:guid}/balance")]
    [ProducesResponseType(typeof(WalletResponse), StatusCodes.Status200OK)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<ActionResult<WalletResponse>> GetBalance(Guid walletId, CancellationToken ct)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        if (walletId == Guid.Empty) return BadRequestProblem("Wallet ID must not be empty.");
        try { return Ok(await wallets.GetAsync(walletId, ct)); }
        catch (WalletNotFoundException ex) { return NotFoundProblem(ex.Message); }
    }

    [HttpGet("{walletId:guid}/transactions")]
    [ProducesResponseType(typeof(PagedResponse<TransactionResponse>), StatusCodes.Status200OK)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public async Task<ActionResult<PagedResponse<TransactionResponse>>> GetTransactions(Guid walletId, [FromQuery, EnumDataType(typeof(TransactionType))] TransactionType? type,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        [FromQuery] DateTimeOffset? fromDate, [FromQuery] DateTimeOffset? toDate, [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20, CancellationToken ct = default)
    {
        if (walletId == Guid.Empty) return BadRequestProblem("Wallet ID must not be empty.");
        try { return Ok(await wallets.GetTransactionsAsync(walletId, type, fromDate, toDate, pageNumber, pageSize, ct)); }
        catch (WalletNotFoundException ex) { return NotFoundProblem(ex.Message); }
        catch (ValidationException ex) { return BadRequestProblem(ex.Message); }
    }

    private async Task<ActionResult<OperationResponse>> ExecuteOperation(Func<Task<OperationResponse>> operation)
    {
        try
        {
            var result = await operation();
            return result.Status == OperationStatus.Failed
                ? Problem(statusCode: StatusCodes.Status409Conflict, title: "Operation failed", detail: result.FailureCode)
                : Ok(result);
        }
        catch (WalletNotFoundException ex) { return NotFoundProblem(ex.Message); }
        catch (ValidationException ex) { return BadRequestProblem(ex.Message); }
        catch (ConflictException ex) { return ConflictProblem(ex.Message); }
    }
    private ObjectResult BadRequestProblem(string detail) => Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid request", detail: detail);
    private ObjectResult NotFoundProblem(string detail) => Problem(statusCode: StatusCodes.Status404NotFound, title: "Resource not found", detail: detail);
    private ObjectResult ConflictProblem(string detail) => Problem(statusCode: StatusCodes.Status409Conflict, title: "Conflict", detail: detail);
}
