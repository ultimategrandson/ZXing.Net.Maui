using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Threading.Tasks;
using ZXing.Net.Maui.Readers;

namespace ZXing.Net.Maui.Controls
{
    public partial class CameraBarcodeReaderView : View, ICameraBarcodeReaderView
    {
        public event EventHandler<BarcodeDetectionEventArgs> BarcodesDetected;
        public event EventHandler<CameraFrameBufferEventArgs> FrameReady;

        void ICameraBarcodeReaderView.OnBarcodesDetected(BarcodeResult[] barcodes) => BarcodesDetected?.Invoke(this, new BarcodeDetectionEventArgs(barcodes));

        void ICameraFrameReceiver.OnReceiveFrame(PixelBufferHolder data) => FrameReady?.Invoke(this, new CameraFrameBufferEventArgs(data));

        public static readonly BindableProperty OptionsProperty =
            BindableProperty.Create(nameof(Options), typeof(BarcodeReaderOptions), typeof(CameraBarcodeReaderView), defaultValueCreator: bindableObj => new BarcodeReaderOptions());

        public BarcodeReaderOptions Options
        {
            get => (BarcodeReaderOptions)GetValue(OptionsProperty);
            set => SetValue(OptionsProperty, value);
        }

        public static readonly BindableProperty IsDetectingProperty =
            BindableProperty.Create(nameof(IsDetecting), typeof(bool), typeof(CameraBarcodeReaderView), defaultValue: true);

        public bool IsDetecting
        {
            get => (bool)GetValue(IsDetectingProperty);
            set => SetValue(IsDetectingProperty, value);
        }

        public static readonly BindableProperty IsTorchOnProperty =
            BindableProperty.Create(nameof(IsTorchOn), typeof(bool), typeof(CameraBarcodeReaderView), defaultValue: false);

        public bool IsTorchOn
        {
            get => (bool)GetValue(IsTorchOnProperty);
            set => SetValue(IsTorchOnProperty, value);
        }

        public static readonly BindableProperty CameraLocationProperty =
            BindableProperty.Create(nameof(CameraLocation), typeof(CameraLocation), typeof(CameraBarcodeReaderView), defaultValue: CameraLocation.Rear);

        public CameraLocation CameraLocation
        {
            get => (CameraLocation)GetValue(CameraLocationProperty);
            set => SetValue(CameraLocationProperty, value);
        }

        public void AutoFocus()
            => StrongHandler?.Invoke(nameof(AutoFocus), null);

        public void Focus(Point point)
            => StrongHandler?.Invoke(nameof(Focus), point);

        public Task StartAsync() => StrongHandler?.StartAsync();

        public void Stop() => StrongHandler?.Stop();

        CameraBarcodeReaderViewHandler StrongHandler
            => Handler as CameraBarcodeReaderViewHandler;
    }
}
