using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf.Ui.Controls.Interfaces;

namespace DevLynx.Packaging.Visualizer.Services
{
    public interface IMessageService
    {
        void Notify(string message);
        void NotifyError(string message);
    }

    internal class UIMessageService : IMessageService
    {
        private ISnackbarControl _snackbar;

        public void RegisterSnackbar(ISnackbarControl snackbar)
        {
            _snackbar = snackbar;
        }

        public void Notify(string message)
        {
            _snackbar?.Show("Notification", message);
        }

        public void NotifyError(string message)
        {
            _snackbar?.Show("An error occured...", message);
        }
    }
}
