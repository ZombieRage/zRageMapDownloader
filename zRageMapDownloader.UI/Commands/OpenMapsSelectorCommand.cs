using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using zRageMapDownloader.ViewModels;

namespace zRageMapDownloader.Commands
{
    public class OpenMapsSelectorCommand : ICommand
    {
        DownloadMapsViewModel _vm { get; set; }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        public OpenMapsSelectorCommand(DownloadMapsViewModel vm)
        {
            _vm = vm;
        }

        public bool CanExecute(object parameter)
        {
            return !_vm.DownloadInProgress;
        }

        public void Execute(object parameter)
        {
            _vm.OpenMapsSelector();
        }
    }
}
