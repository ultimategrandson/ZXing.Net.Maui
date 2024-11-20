using AVFoundation;
using CoreFoundation;
using CoreVideo;
using Foundation;
using System;
using System.Linq;
using UIKit;
using MSize = Microsoft.Maui.Graphics.Size;

namespace ZXing.Net.Maui
{
    internal partial class CameraManager
    {
        private AVCaptureSession? captureSession;
        private AVCaptureDevice? captureDevice;
        private AVCaptureInput? captureInput = null;
        private PreviewView? view;
        private AVCaptureVideoDataOutput? videoDataOutput;
        private AVCaptureVideoPreviewLayer? videoPreviewLayer;
        private CaptureDelegate? captureDelegate;
        private DispatchQueue? dispatchQueue;
        private readonly object _configLock = new();

        public NativePlatformCameraPreviewView CreateNativeView()
        {
            captureSession = new AVCaptureSession
            {
                SessionPreset = AVCaptureSession.Preset640x480,
            };

            videoPreviewLayer = new(captureSession)
            {
                VideoGravity = AVLayerVideoGravity.ResizeAspectFill,
            };

            view = new PreviewView(this, videoPreviewLayer);
            return view;
        }

        public void Connect()
        {
            if (captureSession == null)
                return;

            UpdateCamera();

            if (videoDataOutput == null)
            {
                videoDataOutput = new AVCaptureVideoDataOutput();

                var videoSettings = NSDictionary.FromObjectAndKey(
                    new NSNumber((int)CVPixelFormatType.CV32BGRA),
                    CVPixelBuffer.PixelFormatTypeKey);

                videoDataOutput.WeakVideoSettings = videoSettings;

                captureDelegate ??= new CaptureDelegate
                {
                    SampleProcessor = cvPixelBuffer =>
                        CameraFrameReceiver.OnReceiveFrame(new Readers.PixelBufferHolder
                        {
                            Data = cvPixelBuffer,
                            Size = new MSize(cvPixelBuffer.Width, cvPixelBuffer.Height)
                        })
                };

                dispatchQueue ??= new DispatchQueue("CameraBufferQueue");

                videoDataOutput.AlwaysDiscardsLateVideoFrames = true;
                videoDataOutput.SetSampleBufferDelegate(captureDelegate, dispatchQueue);
            }

            captureSession.AddOutput(videoDataOutput);
        }

        public void UpdateCamera()
        {
            if (captureSession == null)
                return;

            lock (_configLock)
            {
                if (captureSession.Running)
                    captureSession.StopRunning();

                // Cleanup old input
                if (captureInput != null && captureSession.Inputs.Length > 0 && captureSession.Inputs.Contains(captureInput))
                {
                    captureSession.RemoveInput(captureInput);
                    captureInput.Dispose();
                    captureInput = null;
                }

                // Cleanup old device
                if (captureDevice != null)
                {
                    captureDevice.Dispose();
                    captureDevice = null;
                }

                var cameraDiscovery = AVCaptureDeviceDiscoverySession.Create(new AVCaptureDeviceType[] { AVCaptureDeviceType.BuiltInDualCamera }, AVMediaTypes.Video, CameraLocation switch
                {
                    CameraLocation.Front => AVCaptureDevicePosition.Front,
                    CameraLocation.Rear => AVCaptureDevicePosition.Back,
                    _ => AVCaptureDevicePosition.Back
                });

                captureDevice = cameraDiscovery.Devices.FirstOrDefault() ?? AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Video) ?? throw new Exception("No device");
                captureInput = new AVCaptureDeviceInput(captureDevice, out var err);

                captureSession.AddInput(captureInput);
                captureSession.StartRunning();
            }
        }

        public void Disconnect()
        {
            if (captureSession == null || videoDataOutput == null)
                return;

            lock (_configLock)
            {
                if (captureSession.Running)
                    captureSession.StopRunning();

                captureSession.RemoveOutput(videoDataOutput);

                // Cleanup old input
                if (captureInput != null && captureSession.Inputs.Length > 0 && captureSession.Inputs.Contains(captureInput))
                {
                    captureSession.RemoveInput(captureInput);
                    captureInput.Dispose();
                    captureInput = null;
                }

                // Cleanup old device
                if (captureDevice != null)
                {
                    captureDevice.Dispose();
                    captureDevice = null;
                }
            }
        }

        public void ResetTorch()
        {
            try
            {
                if (captureDevice != null && captureDevice.HasTorch && captureDevice.TorchAvailable && captureDevice.TorchActive)
                    CaptureDevicePerformWithLockedConfiguration(() => captureDevice.TorchMode = AVCaptureTorchMode.On);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void UpdateTorch(bool on)
        {
            if (captureDevice != null && captureDevice.HasTorch && captureDevice.TorchAvailable)
            {
                var isOn = captureDevice.TorchActive;

                try
                {
                    if (on != isOn)
                    {
                        CaptureDevicePerformWithLockedConfiguration(() =>
                            captureDevice.TorchMode = on ? AVCaptureTorchMode.On : AVCaptureTorchMode.Off);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        public void Focus(Microsoft.Maui.Graphics.Point point)
        {
            if (captureDevice == null)
                return;

            var focusMode = AVCaptureFocusMode.AutoFocus;
            if (captureDevice.IsFocusModeSupported(AVCaptureFocusMode.ContinuousAutoFocus))
                focusMode = AVCaptureFocusMode.ContinuousAutoFocus;

            //See if it supports focusing on a point
            if (captureDevice.FocusPointOfInterestSupported && !captureDevice.AdjustingFocus)
            {
                CaptureDevicePerformWithLockedConfiguration(() =>
                {
                    //Focus at the point touched
                    captureDevice.FocusPointOfInterest = point;
                    captureDevice.FocusMode = focusMode;
                });
            }
        }

        private void CaptureDevicePerformWithLockedConfiguration(Action handler)
        {
            if (captureDevice == null)
                return;

            lock (_configLock)
                if (captureDevice.LockForConfiguration(out var err))
                {
                    try
                    {
                        handler();
                    }
                    finally
                    {
                        captureDevice.UnlockForConfiguration();
                    }
                }
        }

        public void AutoFocus()
        {
            if (captureDevice == null)
                return;

            var focusMode = AVCaptureFocusMode.AutoFocus;

            if (captureDevice.IsFocusModeSupported(AVCaptureFocusMode.ContinuousAutoFocus))
                focusMode = AVCaptureFocusMode.ContinuousAutoFocus;

            CaptureDevicePerformWithLockedConfiguration(() =>
            {
                if (captureDevice.FocusPointOfInterestSupported)
                    captureDevice.FocusPointOfInterest = CoreGraphics.CGPoint.Empty;

                captureDevice.FocusMode = focusMode;
            });
        }

        public void Dispose()
        {
        }
    }

    class PreviewView : UIView
    {
        public PreviewView(CameraManager cameraManager, AVCaptureVideoPreviewLayer layer) : base()
        {
            _cameraManager = cameraManager;
            PreviewLayer = layer;

            PreviewLayer.Frame = Layer.Bounds;
            Layer.AddSublayer(PreviewLayer);
        }

        private readonly CameraManager _cameraManager;
        public readonly AVCaptureVideoPreviewLayer PreviewLayer;

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            var avSession = PreviewLayer.Session;

            if (avSession == null)
                return;

            foreach (var avConnection in avSession.Connections)
                avConnection.VideoOrientation = Window.WindowScene?.InterfaceOrientation switch
                {
                    UIInterfaceOrientation.Portrait => AVCaptureVideoOrientation.Portrait,
                    UIInterfaceOrientation.PortraitUpsideDown => AVCaptureVideoOrientation.PortraitUpsideDown,
                    UIInterfaceOrientation.LandscapeRight => AVCaptureVideoOrientation.LandscapeRight,
                    UIInterfaceOrientation.LandscapeLeft => AVCaptureVideoOrientation.LandscapeLeft,
                    _ => AVCaptureVideoOrientation.Portrait
                };

            _cameraManager.ResetTorch();
            PreviewLayer.Frame = Layer.Bounds;
        }
    }
}
