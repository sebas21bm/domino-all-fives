namespace DominoAllFives.Contracts.DTOs
{
    /// <summary>
    /// Contains the information required to register a player account.
    /// </summary>
    public class RegisterAccountDto
    {
        public string Username { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string PreferredLanguageId { get; set; }
    }
}
