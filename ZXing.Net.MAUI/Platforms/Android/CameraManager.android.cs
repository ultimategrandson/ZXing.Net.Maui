using Android.Graphics;
using AndroidX.Camera.Core;
using AndroidX.Camera.Lifecycle;
using AndroidX.Camera.View;
using AndroidX.Core.Content;
using Java.Util.Concurrent;
using System;

namespace ZXing.Net.Maui
{
    internal partial class CameraManager
    {
        private AndroidX.Camera.Core.Preview? cameraPreview;
        private ImageAnalysis? imageAnalyzer;
        private PreviewView? previewView;
        private IExecutorService? cameraExecutor;
        private CameraSelector? cameraSelector = null;
        private ProcessCameraProvider? cameraProvider;
        private ICamera? camera;

        public NativePlatformCameraPreviewView CreateNativeView()
        {
            previewView = new PreviewView(Context?.Context ?? throw new NullReferenceException("Context is required here."));
            cameraExecutor = Executors.NewSingleThreadExecutor();
            return previewView;
        }

        public void Connect()
        {
            var cameraProviderFuture = ProcessCameraProvider.GetInstance(Context?.Context ?? throw new NullReferenceException("Context is required here."));

            cameraProviderFuture.AddListener(new Java.Lang.Runnable(() =>
            {
                if (cameraExecutor == null || previewView == null)
                    throw new NullReferenceException("CreateNativeView first.");

                // Used to bind the lifecycle of cameras to the lifecycle owner
                cameraProvider = (ProcessCameraProvider?)cameraProviderFuture.Get();

                // Preview
                cameraPreview = new AndroidX.Camera.Core.Preview.Builder().Build();
                cameraPreview.SetSurfaceProvider(cameraExecutor, previewView.SurfaceProvider);

                // Frame by frame analyze
                imageAnalyzer = new ImageAnalysis.Builder()
                    .SetDefaultResolution(new Android.Util.Size(1024, 576))
                    .SetOutputImageRotationEnabled(true)
                    .SetBackpressureStrategy(ImageAnalysis.StrategyKeepOnlyLatest)
                    .Build();

                imageAnalyzer.SetAnalyzer(cameraExecutor, new FrameAnalyzer((buffer, size) => CameraFrameReceiver.OnReceiveFrame(new Readers.PixelBufferHolder { Data = buffer, Size = size })));

                UpdateCamera();

            }), ContextCompat.GetMainExecutor(Context.Context)); //GetMainExecutor: returns an Executor that runs on the main thread.
        }

        public void Disconnect() { }

        public void UpdateCamera()
        {
            if (cameraProvider == null || Context?.Context == null || cameraPreview == null || imageAnalyzer == null)
                return;

            // Unbind use cases before rebinding
            cameraProvider.UnbindAll();

            var cameraLocation = CameraLocation;

            // Select back camera as a default, or front camera otherwise
            if (cameraLocation == CameraLocation.Rear && cameraProvider.HasCamera(CameraSelector.DefaultBackCamera))
                cameraSelector = CameraSelector.DefaultBackCamera;
            else if (cameraLocation == CameraLocation.Front && cameraProvider.HasCamera(CameraSelector.DefaultFrontCamera))
                cameraSelector = CameraSelector.DefaultFrontCamera;
            else
                cameraSelector = CameraSelector.DefaultBackCamera;

            if (cameraSelector == null)
                throw new System.Exception("Camera not found");

            // The Context here SHOULD be something that's a lifecycle owner
            if (Context.Context is AndroidX.Lifecycle.ILifecycleOwner lifecycleOwner)
            {
                camera = cameraProvider.BindToLifecycle(lifecycleOwner, cameraSelector, cameraPreview, imageAnalyzer);
            }
            else if (Microsoft.Maui.ApplicationModel.Platform.CurrentActivity is AndroidX.Lifecycle.ILifecycleOwner maLifecycleOwner)
            {
                // if not, this should be sufficient as a fallback
                camera = cameraProvider.BindToLifecycle(maLifecycleOwner, cameraSelector, cameraPreview, imageAnalyzer);
            }
        }

        public void UpdateTorch(bool on)
        {
            camera?.CameraControl.EnableTorch(on);
        }

        public void Focus(Point point)
        {
            if (camera == null || previewView?.LayoutParameters == null)
                return;

            camera.CameraControl.CancelFocusAndMetering();

            var factory = new SurfaceOrientedMeteringPointFactory(previewView.LayoutParameters.Width, previewView.LayoutParameters.Height);
            var fpoint = factory.CreatePoint(point.X, point.Y);

            var action = new FocusMeteringAction.Builder(fpoint, FocusMeteringAction.FlagAf)
                .DisableAutoCancel()
                .Build();

            camera.CameraControl.StartFocusAndMetering(action);
        }

        public void AutoFocus()
        {
            if (camera == null || previewView?.LayoutParameters == null)
                return;

            camera.CameraControl.CancelFocusAndMetering();

            var factory = new SurfaceOrientedMeteringPointFactory(1f, 1f);
            var fpoint = factory.CreatePoint(0.5f, 0.5f);
            var action = new FocusMeteringAction.Builder(fpoint, FocusMeteringAction.FlagAf).Build();

            camera.CameraControl.StartFocusAndMetering(action);
        }

        public void Dispose()
        {
            if (cameraExecutor == null)
                return;

            cameraExecutor.Shutdown();
            cameraExecutor.Dispose();
        }
    }
}
