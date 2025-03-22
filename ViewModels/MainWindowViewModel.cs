using ReactiveUI;
using System;
using System.Reactive;
using Avalonia.Controls.ApplicationLifetimes;
using Bachelor.Views;

namespace Bachelor.ViewModels {

    public partial class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            Greeting = "Hello World!";
            // Specify the scheduler for the command
            NavigateToBasicCameraCommand = ReactiveCommand.Create(
                NavigateToBasicCamera, 
                outputScheduler: RxApp.MainThreadScheduler);
        }
    
        public string Greeting { get; }
        public ReactiveCommand<Unit, Unit> NavigateToBasicCameraCommand { get; }

        private void NavigateToBasicCamera()
        {
            var app = (IClassicDesktopStyleApplicationLifetime)App.Current.ApplicationLifetime;
            var mainWindow = (MainWindowView)app.MainWindow;
            mainWindow.Content = new BasicCameraView
            {
                DataContext = new BasicCameraViewModel()
            };
        }
    }
}