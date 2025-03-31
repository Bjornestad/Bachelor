using Avalonia;
using Avalonia.Controls;

namespace Bachelor.Views
{
    public partial class PopoutWindow : Window
    {
        public PopoutWindow()
        {
            Title = "Camera View";
            Width = 640;
            Height = 480;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
#if DEBUG
            this.AttachDevTools();
#endif
        }
    }
}