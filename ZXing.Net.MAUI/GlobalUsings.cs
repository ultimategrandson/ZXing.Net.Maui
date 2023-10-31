#if IOS || MACCATALYST
global using NativePlatformImage = global::UIKit.UIImage;
global using NativePlatformImageView = global::UIKit.UIImageView;
global using NativePlatformCameraPreviewView = global::UIKit.UIView;
global using NativePlatformView = global::UIKit.UIView;
#elif ANDROID
global using NativePlatformCameraPreviewView = global::AndroidX.Camera.View.PreviewView;
global using NativePlatformView = global::Android.Views.View;
global using NativePlatformImageView = global::Android.Widget.ImageView;
global using NativePlatformImage = global::Android.Graphics.Bitmap;
#elif WINDOWS
global using NativePlatformCameraPreviewView = global::Microsoft.UI.Xaml.FrameworkElement;
global using NativePlatformView = global::Microsoft.UI.Xaml.FrameworkElement;
global using NativePlatformImageView = global::Microsoft.UI.Xaml.Controls.Image;
global using NativePlatformImage = global::Microsoft.UI.Xaml.Media.Imaging.WriteableBitmap;
#else
global using NativePlatformCameraPreviewView = ZXing.Net.Maui.NativePlatformCameraPreviewView;
global using NativePlatformView = ZXing.Net.Maui.NativePlatformView;
global using NativePlatformImageView = ZXing.Net.Maui.NativePlatformImageView;
global using NativePlatformImage = ZXing.Net.Maui.NativePlatformImage;
#endif