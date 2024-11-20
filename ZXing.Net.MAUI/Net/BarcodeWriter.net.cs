using Microsoft.Maui.Graphics;

namespace ZXing.Net.Maui
{
    public class BarcodeWriter : BarcodeWriter<NativePlatformImage>
    {
        public Color ForegroundColor { get; set; }

        public Color BackgroundColor { get; set; }
    }
}
