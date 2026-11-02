namespace DominoAllFives.Client.WPF.Models
{
    /// <summary>
    /// Enumeration for the different message types that can be displayed 
    /// with the CustomMessage control.
    /// </summary>
    public enum DialogType
    {
        /// <summary>
        /// Dialog type used to indicate the success of an operation
        /// </summary>
        Success,

        /// <summary>
        /// Dialog type used to indicate that an error ocured during an operation
        /// </summary>
        Error,

        /// <summary>
        /// Dialog type used to warn or notify certain information to the user during an operation
        /// </summary>
        Warning,

        /// <summary>
        /// Dialog type used to inform the user about general information
        /// </summary>
        /// 
        Information,

        /// <summary>
        /// Dialog type used to ask for confirmation to the used upon doing an operation
        /// </summary>
        Confirmation
    }
}
