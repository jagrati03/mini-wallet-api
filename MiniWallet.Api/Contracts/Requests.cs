using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace MiniWallet.Api.Contracts;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class NotEmptyGuidAttribute : ValidationAttribute
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public override bool IsValid(object? value) => value is Guid id && id != Guid.Empty;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class E164PhoneAttribute : ValidationAttribute
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
    private static readonly Regex Pattern = new("^\\+[1-9]\\d{7,14}$", RegexOptions.Compiled);
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public override bool IsValid(object? value) => value is string number && Pattern.IsMatch(number);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class NotWhiteSpaceAttribute : ValidationAttribute
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public override bool IsValid(object? value) => value is string text && !string.IsNullOrWhiteSpace(text);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class CreateWalletRequest
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    [Required, NotWhiteSpace, StringLength(120, MinimumLength = 1)] public string Name { get; init; } = null!;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    [Required, EmailAddress, StringLength(254)] public string Email { get; init; } = null!;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    [Required, E164Phone] public string MobileNumber { get; init; } = null!;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    [Range(typeof(decimal), "0", "79228162514264337593543950335")] public decimal InitialBalance { get; init; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class MoneyRequest
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    [NotEmptyGuid] public Guid WalletId { get; init; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")] public decimal Amount { get; init; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    [Required, NotWhiteSpace, StringLength(100, MinimumLength = 1)] public string ReferenceId { get; init; } = null!;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class TransferRequest
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    [NotEmptyGuid] public Guid FromWalletId { get; init; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    [NotEmptyGuid] public Guid ToWalletId { get; init; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")] public decimal Amount { get; init; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    [Required, NotWhiteSpace, StringLength(100, MinimumLength = 1)] public string ReferenceId { get; init; } = null!;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
