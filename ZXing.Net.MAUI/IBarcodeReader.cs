using System.Threading.Tasks;

namespace ZXing.Net.Maui.Readers
{
    public interface IBarcodeReader
    {
        BarcodeReaderOptions Options { get; set; }

        Task<BarcodeResult[]> DecodeAsync(PixelBufferHolder image);
    }
}
