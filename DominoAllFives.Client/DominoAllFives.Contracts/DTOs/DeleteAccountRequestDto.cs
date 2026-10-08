namespace DominoAllFives.Contracts.DTOs
{
    /// <summary>
    /// Represents a request to delete a player account.
    /// </summary>
    public class DeleteAccountRequestDto
    {
        public int PlayerId { get; set; }

        public string Password { get; set; }
    }
}