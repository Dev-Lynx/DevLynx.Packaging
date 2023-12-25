using DevLynx.Packaging.Visualizer.Extensions;
using DevLynx.Packaging.Visualizer.Models.Contexts;
using DevLynx.Packaging.Visualizer.Services;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace DevLynx.Packaging.Visualizer.ViewModels
{
    internal class ItemDialogViewModel : BindableBase
    {
        public ICommand CreateCommand { get; }
        public ICommand KeyUpCommand { get; }

        public PackageContext Context => _packagingService.Context;

        private readonly IAppService _appService;
        private readonly IMessageService _messageService;
        private readonly IPackagingService _packagingService;

        public ItemDialogViewModel(IAppService appService, IPackagingService packagingService, IMessageService messageService)
        {
            _appService = appService;
            _messageService = messageService;
            _packagingService = packagingService;

            CreateCommand = new DelegateCommand(HandleCreate);
            KeyUpCommand = new DelegateCommand<object>(HandleKeyUp);

            _appService.DialogAccepted += HandleAccept;
        }

        private void HandleAccept(object sender, EventArgs e)
        {
            HandleCreate();
        }

        private void HandleKeyUp(object obj)
        {
            if (obj is not KeyEventArgs e) return;

            if (e.Key == Key.Return)
                HandleCreate();
        }

        private void HandleCreate()
        {
            Dim item = _packagingService.Context.NewItem;

            bool valid = true;

            if (!(valid &= item.Width > 0))
                _messageService.NotifyError("Please specify a valid width");
            else if (!(valid &= item.Height > 0))
                _messageService.NotifyError("Please specify a valid height");
            else if (!(valid &= item.Depth > 0))
                _messageService.NotifyError("Please specify a valid depth");

            if (!valid) return;

            _appService.HideDialog();

            var items = _packagingService.Context.Items;

            foreach (var itm in items)
            {
                if (itm.Width == item.Width && itm.Height == item.Height && itm.Depth == item.Depth)
                {
                    itm.Count++;
                    return;
                }
            }

            items.Add(new NDim(item));
        }
    }
}
