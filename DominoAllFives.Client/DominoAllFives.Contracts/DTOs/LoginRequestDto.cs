namespace DominoAllFives.Contracts.DTOs
{
    /// <summary>
    /// Represents the data of a loging request
    /// </summary>
    public class LoginRequestDto
    {
        public string EmailOrUsername { get; set; }
        public string Password { get; set; }
    }
}
