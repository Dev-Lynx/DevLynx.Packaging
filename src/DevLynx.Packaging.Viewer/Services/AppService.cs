using DevLynx.Packaging.Visualizer.Models.Contexts;
using DryIoc;
using Prism.DryIoc;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wpf.Ui.Controls;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;

namespace DevLynx.Packaging.Visualizer.Services
{
    public interface IAppService
    {
        RootContext Context { get; }

        void ShowDialog(DialogKind kind);
        void HideDialog();
    }

    internal class AppService : IAppService
    {
        public RootContext Context { get; } = new();
        
        private IDialogControl _dialog;
        private readonly IMessageService _messageService;
        private readonly IPackagingService _packagingService;
        private readonly IContainerProvider _containerProvider;

        public AppService(IMessageService msgSvc, IPackagingService pSvc, IContainerProvider ioc)
        {
            _messageService = msgSvc;
            _packagingService = pSvc;
            _containerProvider = ioc;
        }

        public void RegisterDialog(IDialogControl dialog)
        {
            if (_dialog != null) return;
            
            _dialog = dialog;
            _containerProvider.GetContainer().RegisterInstance(dialog);

            _dialog.Opened += (s, e) => Context.Dialog.IsActive = true;
            _dialog.Closed += (s, e) => Context.Dialog.IsActive = false;

            _dialog.ButtonRightClick += HandleDialogCancel;
            _dialog.ButtonLeftClick += HandleDialogAccept;
        }

        private void HandleDialogAccept(object sender, RoutedEventArgs e)
        {
            switch (Context.Dialog.Kind)
            {
                case DialogKind.Item:
                    Dim item = _packagingService.Context.NewItem;

                    bool valid = true;
                    
                    if (!(valid &= item.Width > 0))
                        _messageService.NotifyError("Please specify a valid width");
                    else if (!(valid &= item.Height > 0))
                        _messageService.NotifyError("Please specify a valid height");
                    else if (!(valid &= item.Depth > 0))
                        _messageService.NotifyError("Please specify a valid depth");

                    if (!valid) break;

                    App.Current.Dispatcher.BeginInvoke(() =>
                    {
                        _packagingService.Context.Items.Add(new NDim(item));
                    });
                    break;
            }

            _dialog.Hide();
        }

        private void HandleDialogCancel(object sender, RoutedEventArgs e)
        {
            _dialog.Hide();
        }

        public void ShowDialog(DialogKind kind)
        {
            DialogContext ctx = Context.Dialog;
            if (ctx.IsActive && ctx.Kind == kind) return;

            ctx.Kind = kind;

            if (_dialog == null) return;
            if (_dialog.IsShown) return;

            _dialog.Title = _dialog.Message = "";

            switch (kind)
            {
                case DialogKind.Item:
                    _dialog.ButtonLeftName = "Create";
                    _dialog.ButtonRightName = "Cancel";
                    _dialog.DialogHeight = 450;
                    _packagingService.Context.NewItem = new(1, 1, 1);
                    break;
            }

            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                _dialog.Show();
            });
        }

        public void HideDialog()
        {
            if (_dialog == null) return;
            if (!_dialog.IsShown) return;

            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                _dialog?.Hide();
            });
        }
    }
}
