using DevLynx.Packaging.Visualizer.Extensions;
using DevLynx.Packaging.Visualizer.Models;
using DevLynx.Packaging.Visualizer.Models.Contexts;
using DevLynx.Packaging.Visualizer.Services;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Wpf.Ui.Controls;
using UIDialog = Wpf.Ui.Controls.Dialog;
using UISnackbar = Wpf.Ui.Controls.Snackbar;

namespace DevLynx.Packaging.Visualizer.ViewModels
{
    internal class ShellViewModel : BindableBase
    {
        public ICommand DialogLoadedCommand { get; }
        public ICommand SnackbarLoadedCommand { get; }

        public DialogContext Dialog => _appService.Context.Dialog;
        public PackageContext PContext => _packagingService.Context;

        private readonly IAppService _appService;
        private readonly IMessageService _messageService;
        private readonly IPackagingService _packagingService;

        public ShellViewModel(IAppService appService, IMessageService messageService, IPackagingService packagingService)
        {
            DialogLoadedCommand = new DelegateCommand<object>(HandleDialogLoaded);
            SnackbarLoadedCommand = new DelegateCommand<object>(HandleSnackbarLoaded);

            _appService = appService;
            _messageService = messageService;
            _packagingService = packagingService;
        }

        private void HandleDialogLoaded(object obj)
        {
            if (obj is not RoutedEventArgs e) return;
            if (e.Source is not UIDialog dialog) return;            

            if (_appService is AppService ap)
                ap.RegisterDialog(dialog);
        }

        private void HandleSnackbarLoaded(object obj)
        {
            if (obj is not RoutedEventArgs e) return;
            if (e.Source is not UISnackbar snackbar) return;

            if (_messageService is UIMessageService ums)
                ums.RegisterSnackbar(snackbar);
        }
    }
}
