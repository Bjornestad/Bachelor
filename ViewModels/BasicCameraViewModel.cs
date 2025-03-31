using Bachelor.Services;
using Avalonia.Media.Imaging;
using System;
using ReactiveUI;

namespace Bachelor.ViewModels
{
    public class BasicCameraViewModel : ViewModelBase
    {
        private readonly OpenFaceListener _openFaceListener;
        private Bitmap _cameraFrame;

        public BasicCameraViewModel(OpenFaceListener openFaceListener)
        {
            _openFaceListener = openFaceListener;
            // Constructor injection of OpenFaceListener service
        }

        public Bitmap CameraFrame
        {
            get => _cameraFrame;
            private set => this.RaiseAndSetIfChanged(ref _cameraFrame, value);
        }

        public OpenFaceListener FaceListener => _openFaceListener;
    }
}