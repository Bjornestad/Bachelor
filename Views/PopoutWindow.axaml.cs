using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

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
        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            this.Topmost = true;
        }

        protected override void OnGotFocus(GotFocusEventArgs e)
        {
            base.OnGotFocus(e);
            this.Topmost = true;
        }
    }
}