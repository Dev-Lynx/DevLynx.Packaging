using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
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

namespace DevLynx.Packaging.Visualizer.Views
{
    /// <summary>
    /// Interaction logic for SimView.xaml
    /// </summary>
    public partial class SimView : UserControl
    {
        public SimView(IRegionManager rgm)
        {
            InitializeComponent();

            rgm.RegisterViewWithRegion<Space>(AppBase.SPACE_REGION);
        }
    }
}
