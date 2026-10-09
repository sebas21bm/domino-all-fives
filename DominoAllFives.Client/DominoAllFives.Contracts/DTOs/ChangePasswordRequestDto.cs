
namespace DominoAllFives.Contracts.DTOs
{
    /// <summary>
    /// Represents a request to change a player account password.
    /// </summary>
    public class ChangePasswordRequestDto
    {
        public int PlayerId { get; set; }

        public string CurrentPassword { get; set; }

        public string NewPassword { get; set; }
    }
}
