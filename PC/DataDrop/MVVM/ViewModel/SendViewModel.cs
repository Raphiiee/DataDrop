using System.Windows.Input;
using DataDrop.BusinessLayer;
using DataDrop.Core;

namespace DataDrop.MVVM.ViewModel
{
    public class SendViewModel
    {
        
        private ICommand _toggleServerCommand;
        private ServerHandler ServerHandler = new();

        public ICommand ToggleServerCommand => _toggleServerCommand ??= new RelayCommand(ToggleServer);
        private void ToggleServer(object commandParameter)
        {
            if (ServerHandler.Status)
            {
                ServerHandler.Stop();
            }
            else
            {
                ServerHandler.Start("");
            }
        }
    }
}