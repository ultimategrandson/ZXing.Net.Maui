using System.Diagnostics;

namespace ZXing.Net.Maui
{
    internal partial class CameraManager
    {
        public NativePlatformCameraPreviewView CreateNativeView()
        {
            return new NativePlatformCameraPreviewView();
        }

        public void Connect()
            => LogUnsupported();

        public void Disconnect()
            => LogUnsupported();

        public void UpdateCamera()
            => LogUnsupported();

        public void UpdateTorch(bool on)
            => LogUnsupported();

        public void Focus(Microsoft.Maui.Graphics.Point point)
            => LogUnsupported();

        public void AutoFocus()
            => LogUnsupported();

        public void Dispose()
            => LogUnsupported();

        void LogUnsupported()
            => Debug.WriteLine("Camera preview is not supported on this platform.");
    }
}
