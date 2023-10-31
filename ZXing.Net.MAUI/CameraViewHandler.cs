using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using System;
using ZXing.Net.Maui.Readers;

namespace ZXing.Net.Maui
{

    public partial class CameraViewHandler : ViewHandler<ICameraView, NativePlatformCameraPreviewView>, ICameraFrameReceiver
    {
        public readonly static PropertyMapper<ICameraView, CameraViewHandler> CameraViewMapper = new()
        {
            [nameof(ICameraView.IsTorchOn)] = (handler, virtualView) => handler.cameraManager?.UpdateTorch(virtualView.IsTorchOn),
            [nameof(ICameraView.CameraLocation)] = (handler, virtualView) => handler.cameraManager?.UpdateCameraLocation(virtualView.CameraLocation)
        };

        public readonly static CommandMapper<ICameraView, CameraViewHandler> CameraCommandMapper = new()
        {
            [nameof(ICameraView.Focus)] = MapFocus,
            [nameof(ICameraView.AutoFocus)] = MapAutoFocus,
        };

        private CameraManager? cameraManager;
        public event EventHandler<CameraFrameBufferEventArgs>? FrameReady;

        public CameraViewHandler() : base(CameraViewMapper)
        {
        }

        public CameraViewHandler(PropertyMapper? mapper = null) : base(mapper ?? CameraViewMapper)
        {
        }

        protected override NativePlatformCameraPreviewView CreatePlatformView()
        {
            cameraManager ??= new(MauiContext, VirtualView?.CameraLocation ?? CameraLocation.Rear, this);
            var v = cameraManager.CreateNativeView();
            return v;
        }

        void ICameraFrameReceiver.OnReceiveFrame(PixelBufferHolder data)
        {
            FrameReady?.Invoke(this, new CameraFrameBufferEventArgs(data));
        }

        protected override void DisconnectHandler(NativePlatformCameraPreviewView nativeView)
        {
            cameraManager?.Disconnect();
            base.DisconnectHandler(nativeView);
        }

        public void Dispose()
            => cameraManager?.Dispose();

        public void Focus(Point point)
            => cameraManager?.Focus(point);

        public void AutoFocus()
            => cameraManager?.AutoFocus();

        public static void MapFocus(CameraViewHandler handler, ICameraView cameraBarcodeReaderView, object? parameter)
        {
            if (parameter is not Point point)
                throw new ArgumentException("Invalid parameter", "point");

            handler.Focus(point);
        }

        public static void MapAutoFocus(CameraViewHandler handler, ICameraView cameraBarcodeReaderView, object? parameters)
            => handler.AutoFocus();
    }
}
