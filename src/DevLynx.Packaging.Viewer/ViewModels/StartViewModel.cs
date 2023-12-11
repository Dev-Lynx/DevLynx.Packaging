using DevLynx.Packaging.Visualizer.Extensions;
using DevLynx.Packaging.Visualizer.Models.Contexts;
using DevLynx.Packaging.Visualizer.Services;
using DevLynx.Packaging.Visualizer.Views;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Wpf.Ui.Controls.Interfaces;

namespace DevLynx.Packaging.Visualizer.ViewModels
{
    internal class StartViewModel : BindableBase
    {
        private readonly IPackagingService _packagingService;
        private readonly IAppService _appService;
        private readonly IRegionManager _regionManager;
        private readonly IMessageService _messageService;
        private readonly IContainerProvider _container;

        public PackageContext Context => _packagingService.Context;
        
        public ICommand AddCommand { get; }
        public ICommand RemoveCommand { get; }
        public ICommand VisualizeCommand { get; }
        

        public StartViewModel(IPackagingService packagingService, IAppService appService, IRegionManager regionManager, IMessageService messageService, IContainerProvider containerProvider)
        {
            _packagingService = packagingService;
            _regionManager = regionManager;
            _appService = appService;
            _messageService = messageService;
            _container = containerProvider;

            AddCommand = new DelegateCommand(() => _appService.ShowDialog(DialogKind.Item));
            RemoveCommand = new DelegateCommand<object>(HandleRemove);
            VisualizeCommand = new DelegateCommand(HandleVisualize);
        }

        private void HandleRemove(object obj)
        {
            if (obj is not NDim dim) return;

            _appService.Context.Dialog.Kind = DialogKind.None;
            IDialogControl dialog = _container.Resolve<IDialogControl>();

            Application.Current.Dispatcher.BeginInvoke(() =>
            {    
                dialog.ButtonLeftName = "Delete";
                dialog.ButtonRightName = "Cancel";
                dialog.DialogHeight = 200;
            });

            dialog.ShowAndWaitAsync("Delete?", "Are you sure you would like to delete this item? This action is irreversible.")
                .ContinueWith((res) =>
                {
                    if (!res.IsCompletedSuccessfully) return;

                    if (res.Result == IDialogControl.ButtonPressed.Left)
                    {
                        Application.Current.Dispatcher.BeginInvoke(() =>
                        {
                            Context.Items.Remove(dim);
                        });
                    }
                });
        }

        private void HandleVisualize()
        {
            var con = Context.Container;

            if (!Context.Items.Any())
                _messageService.NotifyError("Please add items to proceed.");
            else if (con.Width <= 0 || con.Height <= 0 || con.Depth <= 0)
                _messageService.NotifyError("Container dimensions are invalid, please input correct values and try again.");
            else
            {
                _regionManager.NavigateToView<Space>(AppBase.MAIN_REGION);
            }
        }
    }
}
