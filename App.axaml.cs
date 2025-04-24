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
        services.AddSingleton<MediaPipeListener>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<PythonLauncherService>();
        services.AddSingleton<OutputViewModel>();
        services.AddSingleton<InputService>();
        services.AddSingleton<SettingsManager>();
        services.AddSingleton<Models.SettingsModel>();
        services.AddSingleton<KeybindViewModel>();

        
        // Build service provider
        Services = services.BuildServiceProvider();

        
        var settingsModel = Services.GetRequiredService<Models.SettingsModel>();
        var movementManagerService = Services.GetRequiredService<MovementManagerService>();
        settingsModel.PropertyChanged += (sender, args) => {
            if (args.PropertyName == nameof(Models.SettingsModel.Settings))
            {
                movementManagerService.RefreshSettings();
            }
        };
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Get MainWindowViewModel from DI container
            var viewModel = Services.GetRequiredService<MainWindowViewModel>();
            
            desktop.MainWindow = new MainWindowView
            {
                DataContext = viewModel,
            };
            
            // Start the OpenFace listener
            var listener = Services.GetRequiredService<MediaPipeListener>();
            listener.Start();
            
            // Check that this initialization is happening
            
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