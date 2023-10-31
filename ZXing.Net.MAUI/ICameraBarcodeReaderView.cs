namespace ZXing.Net.Maui
{
    public interface ICameraBarcodeReaderView : ICameraView
    {
        BarcodeReaderOptions Options { get; }

        void OnBarcodesDetected(BarcodeResult[] barcodes);

        bool IsDetecting { get; set; }
    }
}
