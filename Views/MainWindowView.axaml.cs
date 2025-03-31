using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Bachelor.Services;
using Bachelor.ViewModels;

namespace Bachelor.Views
{
    public partial class MainWindowView : Window
    {
        private Button _popOutButton;
        private ContentControl _mainContent;
        private BasicCameraViewModel _cameraViewModel; // Store the view model instead
        private PopoutWindow _popoutWindow;
        private OpenFaceListener _openFaceListener; // Store for reuse

        public MainWindowView()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();

            _popOutButton = this.FindControl<Button>("PopOutButton");
            _mainContent = this.FindControl<ContentControl>("MainContent");

            _popOutButton.Click += OnPopOutButtonClick;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnPopOutButtonClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_popoutWindow == null && _cameraViewModel != null)
            {
                // Pop out - clear main content first
                _mainContent.Content = null;

                // Create a new view for the popup
                var popupCameraView = new BasicCameraView
                {
                    DataContext = _cameraViewModel
                };
                popupCameraView.ConnectToOpenFaceListener(_openFaceListener);

                _popoutWindow = new PopoutWindow();
                _popoutWindow.Content = popupCameraView;
                _popoutWindow.Closed += (s, e) => {
                    _popoutWindow = null;
                    
                    // Create a new view for the main window
                    var mainCameraView = new BasicCameraView
                    {
                        DataContext = _cameraViewModel
                    };
                    mainCameraView.ConnectToOpenFaceListener(_openFaceListener);
                    _mainContent.Content = mainCameraView;
                    
                    _popOutButton.Content = "Pop Out";
                };

                _popoutWindow.Show();
                _popOutButton.Content = "Pop In";
            }
            else if (_popoutWindow != null)
            {
                _popoutWindow.Close();
            }
        }

        public void SetCameraView(BasicCameraView cameraView, OpenFaceListener listener)
        {
            _cameraViewModel = cameraView?.DataContext as BasicCameraViewModel;
            _openFaceListener = listener;
            _popOutButton.IsVisible = (_cameraViewModel != null);
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            var app = Application.Current as App;
            if (app?.Services != null)
            {
                var pythonLauncher = app.Services.GetService(typeof(PythonLauncherService)) as PythonLauncherService;
                pythonLauncher?.StopPythonScript(); // Assuming the correct method name is Stop
            }

            _popoutWindow?.Close();
            base.OnClosing(e);
        }
    }
}