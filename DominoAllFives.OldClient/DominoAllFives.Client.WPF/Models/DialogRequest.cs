using System;

namespace DominoAllFives.Client.WPF.Models
{
    /// <summary>
    /// Encapsulates the configuration parameters required to display a custom dialog.
    /// </summary>
    public class DialogRequest
    {
        private string _titleKey = string.Empty;
        private string _messageKey = string.Empty;

        /// <summary>
        /// Gets or sets the functional category or visual intent of the dialog.
        /// </summary>
        public DialogType Type { get; set; }

        /// <summary>
        /// Gets or sets the localization resource key for the dialog title.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when value 
        /// is null or empty.
        /// </exception>
        public string TitleKey
        {
            get => _titleKey;
            set => _titleKey = ValidateNotNullOrEmpty(value, nameof(TitleKey));
        }

        /// <summary>
        /// Gets or sets the localization resource key for the main message.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when value 
        /// is null or empty.
        /// </exception>
        public string MessageKey
        {
            get => _messageKey;
            set => _messageKey = ValidateNotNullOrEmpty(value, nameof(MessageKey));
        }

        /// <summary>
        /// Gets or sets the callback action executed when the user 
        /// confirms or accepts the dialog.
        /// </summary>
        public Action OnAccept { get; set; }

        /// <summary>
        /// Gets or sets the optional callback action executed when the user 
        /// cancels or dismisses the dialog.
        /// </summary>
        public Action OnCancel { get; set; }

        private static string ValidateNotNullOrEmpty(string value, string paramName)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(paramName, 
                    $"{paramName} cannot be null or empty.");
            }

            return value;
        }
    }
}
