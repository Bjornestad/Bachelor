using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Bachelor.Services;
using Bachelor.ViewModels;

namespace Bachelor.Views
{
    public partial class MainWindowView : Window
    {
        private ContentControl _cameraContainer;
        private BasicCameraViewModel _cameraViewModel;
        private PopoutWindow _popoutWindow;
        private MediaPipeListener _mediaPipeListener;
        private KeybindViewModel _keybindViewModel;
        private ContentControl _keybindContainer;
        private ContentControl _outputContainer;
        private OutputViewModel _outputViewModel;



        public MainWindowView()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();

            _cameraContainer = this.FindControl<ContentControl>("CameraContainer");
            _keybindContainer = this.FindControl<ContentControl>("KeybindContainer");
            _outputContainer = this.FindControl<ContentControl>("OutputContainer");
            this.Loaded += MainWindowView_Loaded;
        }

        private void MainWindowView_Loaded(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            InitializeCamera();
            InitializeKeybindView();
            InitializeOutputView();
        }

        private void InitializeCamera()
        {
            // Get the listener from services
            var app = Application.Current as App;
            _mediaPipeListener = app?.Services?.GetService(typeof(MediaPipeListener)) as MediaPipeListener;

            if (_mediaPipeListener != null)
            {
                _cameraViewModel = new BasicCameraViewModel(_mediaPipeListener);

                var mainCameraView = new BasicCameraView
                {
                    DataContext = _cameraViewModel
                };

                mainCameraView.ConnectToOpenFaceListener(_mediaPipeListener);
                mainCameraView.PopButtonClicked += OnPopButtonClicked;
                mainCameraView.SetPopButtonText("Pop Out");
                _cameraContainer.Content = mainCameraView;
            }
        }
        
        private void InitializeKeybindView()
        {
            Console.WriteLine($"KeybindContainer found: {_keybindContainer != null}");
            var app = Application.Current as App;
            _keybindViewModel = app?.Services?.GetService(typeof(KeybindViewModel)) as KeybindViewModel;
    
            if (_keybindViewModel != null)
            {
                var keybindView = new KeybindView
                {
                    DataContext = _keybindViewModel
                };
                _keybindContainer.Content = keybindView;
            }
        }
        
        private void InitializeOutputView()
        {
            Console.WriteLine($"OutputContainer found: {_outputContainer != null}");
            var app = Application.Current as App;
            _outputViewModel = app?.Services?.GetService(typeof(OutputViewModel)) as OutputViewModel;

            if (_outputViewModel != null)
            {
                var outputView = new OutputView
                {
                    DataContext = _outputViewModel
                };

                _outputContainer.Content = outputView;
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnPopButtonClicked(object sender, EventArgs e)
        {
            if (_popoutWindow == null && _cameraViewModel != null)
            {
                // Pop out logic
                _cameraContainer.Content = null;

                var popupCameraView = new BasicCameraView
                {
                    DataContext = _cameraViewModel
                };
                popupCameraView.ConnectToOpenFaceListener(_mediaPipeListener);
                popupCameraView.SetPopButtonText("Pop In");
                popupCameraView.PopButtonClicked += OnPopButtonClicked;

                _popoutWindow = new PopoutWindow();
                _popoutWindow.Content = popupCameraView;
                _popoutWindow.Closed += (s, args) => {
                    _popoutWindow = null;

                    var mainCameraView = new BasicCameraView
                    {
                        DataContext = _cameraViewModel
                    };
                    mainCameraView.ConnectToOpenFaceListener(_mediaPipeListener);
                    mainCameraView.PopButtonClicked += OnPopButtonClicked;
                    mainCameraView.SetPopButtonText("Pop Out");
                    _cameraContainer.Content = mainCameraView;
                };

                _popoutWindow.Show();
            }
            else if (_popoutWindow != null)
            {
                _popoutWindow.Close();
            }
        }

        public void SetCameraView(BasicCameraView cameraView, MediaPipeListener listener)
        {
            _cameraViewModel = cameraView?.DataContext as BasicCameraViewModel;
            _mediaPipeListener = listener;
        }
        
        
        protected override void OnClosing(WindowClosingEventArgs e)
        {
            
            var app = Application.Current as App;
            if (app?.Services != null)
            {
                var pythonLauncher = app.Services.GetService(typeof(PythonLauncherService)) as PythonLauncherService;
                pythonLauncher?.StopPythonScript();
                
            }

            _popoutWindow?.Close();
            base.OnClosing(e);
        }
    }
}