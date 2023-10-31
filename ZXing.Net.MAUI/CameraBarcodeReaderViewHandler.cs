using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using System;
using System.Threading.Tasks;
using ZXing.Net.Maui.Readers;

namespace ZXing.Net.Maui
{
    public partial class CameraBarcodeReaderViewHandler : ViewHandler<ICameraBarcodeReaderView, NativePlatformCameraPreviewView>, ICameraFrameReceiver
    {
        public readonly static PropertyMapper<ICameraBarcodeReaderView, CameraBarcodeReaderViewHandler> CameraBarcodeReaderViewMapper = new()
        {
            [nameof(ICameraBarcodeReaderView.Options)] = MapOptions,
            [nameof(ICameraBarcodeReaderView.IsDetecting)] = MapIsDetecting,
            [nameof(ICameraBarcodeReaderView.IsTorchOn)] = (handler, virtualView) => handler.cameraManager?.UpdateTorch(virtualView.IsTorchOn),
            [nameof(ICameraBarcodeReaderView.CameraLocation)] = (handler, virtualView) => handler.cameraManager?.UpdateCameraLocation(virtualView.CameraLocation)
        };

        public readonly static CommandMapper<ICameraBarcodeReaderView, CameraBarcodeReaderViewHandler> CameraBarcodeReaderCommandMapper = new()
        {
            [nameof(ICameraBarcodeReaderView.Focus)] = MapFocus,
            [nameof(ICameraBarcodeReaderView.AutoFocus)] = MapAutoFocus,
        };

        private CameraManager? cameraManager;
        private Readers.IBarcodeReader? barcodeReader;
        private bool stopping;
        private bool started;

        public CameraBarcodeReaderViewHandler() : base(CameraBarcodeReaderViewMapper, CameraBarcodeReaderCommandMapper)
        {
        }

        public CameraBarcodeReaderViewHandler(PropertyMapper propertyMapper = null, CommandMapper commandMapper = null)
            : base(propertyMapper ?? CameraBarcodeReaderViewMapper, commandMapper ?? CameraBarcodeReaderCommandMapper)
        {
        }

        protected Readers.IBarcodeReader BarcodeReader => barcodeReader ??= (Services?.GetService<Readers.IBarcodeReader>() ?? throw new Exception("Barcode reader service missing."));

        private Readers.IBarcodeReader GetBarcodeReader() => Services?.GetService<Readers.IBarcodeReader>() ?? throw new Exception("Barcode reader service missing.");

        protected override NativePlatformCameraPreviewView CreatePlatformView()
        {
            cameraManager ??= new(MauiContext ?? throw new Exception("Context required"), VirtualView?.CameraLocation ?? CameraLocation.Rear, this);
            return cameraManager.CreateNativeView();
        }

        public async Task StartAsync()
        {
            if (started || cameraManager == null)
                return;

            started = false;
            stopping = false;

            var result = await Permissions.RequestAsync<Permissions.Camera>();

            if (result == PermissionStatus.Granted)
            {
                cameraManager.Connect();
                started = true;
            }
            else
                throw new PermissionException("Camera permissions required.");
        }

        public void Stop()
        {
            started = false;
            stopping = true;
            cameraManager?.Disconnect();
        }

        protected override void DisconnectHandler(NativePlatformCameraPreviewView nativeView)
        {
            if (started)
                Stop();

            base.DisconnectHandler(nativeView);
        }

        public async void OnReceiveFrame(PixelBufferHolder data)
        {
            try
            {
                var vv = VirtualView;
                vv.OnReceiveFrame(data);

                if (vv.IsDetecting)
                {
                    var barcodes = await GetBarcodeReader().DecodeAsync(data);

                    if (barcodes.Length > 0)
                        vv.OnBarcodesDetected(barcodes);
                }
            }
            catch
            {
                if (!stopping) // If its stopping, just ignore any errors.
                    throw;
            }
        }

        public static void MapOptions(CameraBarcodeReaderViewHandler handler, ICameraBarcodeReaderView cameraBarcodeReaderView)
            => handler.BarcodeReader.Options = cameraBarcodeReaderView.Options;

        public static void MapIsDetecting(CameraBarcodeReaderViewHandler handler, ICameraBarcodeReaderView cameraBarcodeReaderView)
        { }

        public void Focus(Point point)
            => cameraManager?.Focus(point);

        public void AutoFocus()
            => cameraManager?.AutoFocus();

        public static void MapFocus(CameraBarcodeReaderViewHandler handler, ICameraBarcodeReaderView cameraBarcodeReaderView, object? parameter)
        {
            if (parameter is not Point point)
                throw new ArgumentException("Invalid parameter", nameof(parameter));

            handler.Focus(point);
        }

        public static void MapAutoFocus(CameraBarcodeReaderViewHandler handler, ICameraBarcodeReaderView cameraBarcodeReaderView, object? parameters)
            => handler.AutoFocus();
    }
}
