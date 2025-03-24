using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Bachelor.Services;
using Microsoft.Extensions.DependencyInjection;
using Bachelor.ViewModels;

namespace Bachelor.Views
{

    public partial class MainWindowView : Window
    {
        public MainWindowView()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        protected override void OnClosing(WindowClosingEventArgs e)
        {
            var app = Application.Current as App;
            var pythonLauncher = app?.Services?.GetService<PythonLauncherService>();
            pythonLauncher?.StopPythonScript();
    
            base.OnClosing(e);
        }

    }
    
}
