using DataDrop.Core;

namespace DataDrop.MVVM.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        private object _currentView;
        public OptionsViewModel OptionsVM { get; set; } 
        public ReceiveViewModel ReceiveVM { get; set; } 
        public SendViewModel SendVM { get; set; } 
        public RelayCommand OptionsViewCommand {get; set;}
        public RelayCommand ReceiveViewCommand {get; set;}
        public RelayCommand SendViewCommand {get; set;}

        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            OptionsVM = new OptionsViewModel();
            ReceiveVM = new ReceiveViewModel();
            SendVM = new SendViewModel();
            CurrentView = SendVM;

            OptionsViewCommand = new RelayCommand(o =>
            {
                CurrentView = OptionsVM;
            });

            ReceiveViewCommand = new RelayCommand(o =>
            {
                CurrentView = ReceiveVM;
            });

            SendViewCommand = new RelayCommand(o =>
            {
                CurrentView = SendVM;
            });
        }
    }
}