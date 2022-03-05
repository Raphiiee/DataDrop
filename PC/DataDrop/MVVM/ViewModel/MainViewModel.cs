using System.Windows.Input;
using DataDrop.BusinessLayer;
using DataDrop.Core;

namespace DataDrop.MVVM.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        private BaseViewModel _currentView;
        private string _filePath;
        public OptionsViewModel OptionsVM { get; set; } 
        public ReceiveViewModel ReceiveVM { get; set; } 
        public SendViewModel SendVM { get; set; } 
        public RelayCommand OptionsViewCommand {get; set;}
        public RelayCommand ReceiveViewCommand {get; set;}
        public RelayCommand SendViewCommand {get; set;}
        public BaseViewModel CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                RaisePropertyChangedEvent();
            }
        }
        public string FilePath 
        {
            get => _filePath;
            set
            {
                if (_filePath != value)
                {
                    _filePath = value;
                    RaisePropertyChangedEvent(nameof(FilePath));
                }
            }
        }

        public ServerHandler ServerHandler { get; set; }

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