using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using Bachelor.ViewModels;
using Bachelor.Views;
using ReactiveUI;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using Bachelor.Services;
using System;

namespace Bachelor;

public partial class App : Application
{
    public IServiceProvider? Services { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Configure ReactiveUI
        RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;

        // Set up dependency injection
        var services = new ServiceCollection();
        
        // Register services
        services.AddSingleton<MovementManagerService>();
        services.AddSingleton<OpenFaceListener>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<PythonLauncherService>();

        
        // Build service provider
        Services = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Get MainWindowViewModel from DI container
            var viewModel = Services.GetRequiredService<MainWindowViewModel>();
            
            desktop.MainWindow = new MainWindowView
            {
                DataContext = viewModel,
            };
            
            // Start the OpenFace listener
            var listener = Services.GetRequiredService<OpenFaceListener>();
            listener.Start();
            
            var pythonLauncher = Services.GetRequiredService<PythonLauncherService>();
            pythonLauncher.StartPythonScript();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}