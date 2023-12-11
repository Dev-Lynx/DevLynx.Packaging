﻿using DevLynx.Packaging.Visualizer.ViewModels;
using DevLynx.Packaging.Visualizer.Views;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace DevLynx.Packaging.Visualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Shell : UiWindow
    {
        public Shell(IContainerProvider ioc, IRegionManager rgm)
        {
            DataContext = ioc.Resolve<ShellViewModel>();
            InitializeComponent();
            
            rgm.RegisterViewWithRegion<StartView>(AppBase.MAIN_REGION);
            rgm.RegisterViewWithRegion<Space>(AppBase.MAIN_REGION);
        }
    }
}
