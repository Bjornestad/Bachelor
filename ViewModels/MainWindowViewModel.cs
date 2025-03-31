using ReactiveUI;
using System;
using System.Reactive;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Bachelor.Services;
using Bachelor.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Bachelor.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ViewModelBase _currentViewModel;
        private object _currentContent;

        public MainWindowViewModel()
        {
            Greeting = "Hello World!";

            NavigateToBasicCameraCommand = ReactiveCommand.Create(
                NavigateToBasicCamera,
                outputScheduler: RxApp.MainThreadScheduler);

            NavigateToHomeCommand = ReactiveCommand.Create(
                NavigateToHome,
                outputScheduler: RxApp.MainThreadScheduler);
        }

        public string Greeting { get; }

        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            private set => this.RaiseAndSetIfChanged(ref _currentViewModel, value);
        }

        public object CurrentContent
        {
            get => _currentContent;
            private set => this.RaiseAndSetIfChanged(ref _currentContent, value);
        }

        public ReactiveCommand<Unit, Unit> NavigateToBasicCameraCommand { get; }
        public ReactiveCommand<Unit, Unit> NavigateToHomeCommand { get; }

        private void NavigateToBasicCamera()
        {
            var app = (IClassicDesktopStyleApplicationLifetime)App.Current.ApplicationLifetime;
            var mainWindow = (MainWindowView)app.MainWindow;

            var mainContent = mainWindow.FindControl<ContentControl>("MainContent");
            if (mainContent == null)
            {
                Console.WriteLine("Warning: MainContent control not found in MainWindow");
                return;
            }

            // Get the OpenFaceListener service directly from App.Services
            var openFaceListener = ((App)App.Current).Services?.GetService<OpenFaceListener>();

            if (openFaceListener == null)
            {
                Console.WriteLine("Warning: OpenFaceListener service not found");
                return;
            }

            var basicCameraViewModel = new BasicCameraViewModel(openFaceListener);
            var basicCameraView = new BasicCameraView
            {
                DataContext = basicCameraViewModel
            };

            basicCameraView.ConnectToOpenFaceListener(openFaceListener);

            // Update the content area of MainWindow
            mainContent.Content = basicCameraView;

            // Register the camera view with the main window
            mainWindow.SetCameraView(basicCameraView, openFaceListener);
            CurrentViewModel = basicCameraViewModel;
            CurrentContent = basicCameraView;
        }

        private void NavigateToHome()
        {
            var app = (IClassicDesktopStyleApplicationLifetime)App.Current.ApplicationLifetime;
            var mainWindow = (MainWindowView)app.MainWindow;

            var contentControl = mainWindow.FindControl<ContentControl>("MainContent");
            if (contentControl == null)
            {
                Console.WriteLine("Warning: MainContent control not found in MainWindow");
                return;
            }

            // Clear the content to show the default "home" view
            contentControl.Content = null;

            // Tell main window there's no camera view active
            mainWindow.SetCameraView(null, null);
            CurrentViewModel = this; // Reset to MainWindowViewModel
            CurrentContent = null;
        }
    }
}
