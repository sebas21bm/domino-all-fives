namespace DominoAllFives.Contracts.Enums
{
    /// <summary>
    /// Define the possible reason of a failed information recovery operation attempt.
    /// </summary>
    public enum RetrieveInformationFailureReason
    {
        None,
        ServiceUnavailable,
        NotFound
    }
}
