using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace DominoAllFives.Client.WPF.Services
{
    interface IModalNavigator
    {
        void OpenModal(UserControl view);
        void CloseModal();
    }
}
