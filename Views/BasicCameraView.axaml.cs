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
        
        public BasicCameraView()
        {
            InitializeComponent();
            Loaded += (s, e) => {
                _cameraImage = this.FindControl<Image>("CameraImage");
                Console.WriteLine($"Camera image control found: {_cameraImage != null}");
            };
        }
        
        public void ConnectToOpenFaceListener(OpenFaceListener listener)
        {
            if (listener == null)
            {
                Console.WriteLine("OpenFaceListener is null");
                return;
            }
            
            //listener.VideoFrameReceived -= OnVideoFrameReceived;
            Console.WriteLine("ConnectToOpenFaceListener called");
            listener.VideoFrameReceived += OnVideoFrameReceived;
            Console.WriteLine("Connected to OpenFaceListener");
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