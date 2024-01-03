using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevLynx.Packaging.Visualizer.ViewModels
{
    internal class AboutViewModel : BindableBase
    {
        internal class Linkable
        {
            public string Text { get; set; }
            public string Link { get; set; }
            public string Description { get; set; }

            public Linkable(string text, string link, string description)
            {
                Text = text;
                Link = link;
                Description = description;
            }
        }

        public List<Linkable> NotableLibraries { get; } = new List<Linkable>
        {
            new Linkable("WPF-UI", "https://wpfui.lepo.co", "Modern WPF UI Library."),
            new Linkable("Material Design In XAML", "http://materialdesigninxaml.net/", "Google's Material Design in XAML & WPF, for C# & VB.Net."),
            new Linkable("Prism Library", "https://prismlibrary.com/docs/", "Framework for building loosely coupled, maintainable, and testable XAML applications in WPF, Xamarin Forms, and Uno / Win UI Applications."),
            new Linkable("PropertyChanged.Fody", "https://github.com/Fody/PropertyChanged", "Extensible tool for weaving INotifyPropertyChanged into .NET assemblies."),
            new Linkable("NLog", "https://nlog-project.org", "Advanced and Structured Logging for Various .NET Platforms."),
        };
    }
}
