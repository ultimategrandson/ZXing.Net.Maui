using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using ZXing.Net.Maui.Readers;

namespace ZXing.Net.Maui
{
    public interface ICameraFrameReceiver
    {
        void OnReceiveFrame(PixelBufferHolder data);
    }

    public interface ICameraView : IView, ICameraFrameReceiver
    {
        CameraLocation CameraLocation { get; set; }

        void AutoFocus();

        void Focus(Point point);

        bool IsTorchOn { get; set; }
    }
}
