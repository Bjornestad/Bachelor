using Bachelor.Services;
using Avalonia.Media.Imaging;
using System;
using ReactiveUI;

namespace Bachelor.ViewModels
{
    public class BasicCameraViewModel : ViewModelBase
    {
        private readonly MediaPipeListener _mediaPipeListener;
        private Bitmap _cameraFrame;

        public BasicCameraViewModel(MediaPipeListener mediaPipeListener)
        {
            _mediaPipeListener = mediaPipeListener;
            // Constructor injection of MediaPipeListener service
        }

        public Bitmap CameraFrame
        {
            get => _cameraFrame;
            private set => this.RaiseAndSetIfChanged(ref _cameraFrame, value);
        }

        public MediaPipeListener FaceListener => _mediaPipeListener;
    }
}