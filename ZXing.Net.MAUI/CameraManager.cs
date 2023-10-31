using Microsoft.Maui;
using System;

namespace ZXing.Net.Maui
{
    internal partial class CameraManager : IDisposable
    {
        public CameraManager(IMauiContext? context, CameraLocation cameraLocation, ICameraFrameReceiver frameReceiver)
        {
            Context = context;
            CameraLocation = cameraLocation;
            CameraFrameReceiver = frameReceiver;
        }

        protected ICameraFrameReceiver CameraFrameReceiver { get; }

        protected IMauiContext? Context { get; }

        public CameraLocation CameraLocation { get; private set; }

        public void UpdateCameraLocation(CameraLocation cameraLocation)
        {
            CameraLocation = cameraLocation;
            UpdateCamera();
        }
    }
}
