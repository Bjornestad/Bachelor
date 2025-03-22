using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
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

    }
}
