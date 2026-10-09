
namespace DominoAllFives.Contracts.Enums
{
    /// <summary>
    /// Defines the possible reasons for a failed password change attempt.
    /// </summary>
    public enum ChangePasswordFailureReason
    {
        None,
        AccountNotFound,
        IncorrectCurrentPassword,
        InvalidNewPassword,
        SameAsCurrentPassword,
        ServiceUnavailable
    }
}
