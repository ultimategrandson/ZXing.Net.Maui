using System;
using System.Threading.Tasks;

namespace ZXing.Net.Maui.Readers
{
    public class ZXingBarcodeReader : Readers.IBarcodeReader
    {
        private readonly BarcodeReaderGeneric zxingReader;
        private BarcodeReaderOptions? options;

        public ZXingBarcodeReader()
        {
            zxingReader = new BarcodeReaderGeneric();
        }

        public BarcodeReaderOptions Options
        {

            get => options ??= new BarcodeReaderOptions();
            set
            {
                options = value ?? new BarcodeReaderOptions();
                zxingReader.Options.PossibleFormats = options.Formats.ToZXingList();
                zxingReader.Options.TryHarder = options.TryHarder;
                zxingReader.AutoRotate = options.AutoRotate;
                zxingReader.Options.TryInverted = options.TryInverted;
            }
        }

        public Task<BarcodeResult[]> DecodeAsync(PixelBufferHolder image)
        {
            if (image.Data == null)
                return Task.FromResult(Array.Empty<BarcodeResult>());

            var w = (int)image.Size.Width;
            var h = (int)image.Size.Height;
            LuminanceSource ls;

#if ANDROID
            ls = new ByteBufferYUVLuminanceSource(image.Data, w, h, 0, 0, w, h);
#elif MACCATALYST || IOS
            ls = new CVPixelBufferBGRA32LuminanceSource(image.Data, w, h);
#elif WINDOWS
            ls = new SoftwareBitmapLuminanceSource(image.Data);
#else
            throw new NotSupportedException();
#endif

#pragma warning disable CS0162 // Unreachable code detected
            if (Options.Multiple)
                return Task.FromResult(zxingReader.DecodeMultiple(ls)?.ToBarcodeResults() ?? Array.Empty<BarcodeResult>());

            var result = zxingReader.Decode(ls);

            if (result != null)
                return Task.FromResult(new[] { result }.ToBarcodeResults());

            return Task.FromResult(Array.Empty<BarcodeResult>());
#pragma warning restore CS0162 // Unreachable code detected
        }
    }
}