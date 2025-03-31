using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Bachelor.Services;

namespace Bachelor.Views
{
    public partial class BasicCameraView : UserControl
    {
        private Image _cameraImage;
        private Button _popButton;
        public event EventHandler<EventArgs> PopButtonClicked; 
        
        public BasicCameraView()
        {
            InitializeComponent();
            Loaded += (s, e) => {
                _cameraImage = this.FindControl<Image>("CameraImage");
                Console.WriteLine($"Camera image control found: {_cameraImage != null}");
            };
            _popButton = this.FindControl<Button>("PopButton");
            _popButton.Click += OnPopButtonClick;
        }
        private void OnPopButtonClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Console.WriteLine("Pop button clicked");
            PopButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        public void SetPopButtonText(String text)
        {
            if (_popButton != null)
            {
                _popButton.Content = text;
            }
        }
        
        public void ConnectToOpenFaceListener(MediaPipeListener listener)
        {
            if (listener == null)
            {
                Console.WriteLine("MediaPipeListener is null");
                return;
            }
            
            //listener.VideoFrameReceived -= OnVideoFrameReceived;
            Console.WriteLine("ConnectToOpenFaceListener called");
            listener.VideoFrameReceived += OnVideoFrameReceived;
            Console.WriteLine("Connected to MediaPipeListener");
        }

        private void OnVideoFrameReceived(object sender, Bitmap bitmap)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (_cameraImage != null)
                {
                    _cameraImage.Source = bitmap;
                }
                else
                {
                    Console.WriteLine("CameraImage control is null");
                }
            });
        }
    }
}