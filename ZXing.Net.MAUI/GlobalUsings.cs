#if IOS || MACCATALYST
global using NativePlatformCameraPreviewView = global::UIKit.UIView;
global using NativePlatformImage = global::UIKit.UIImage;
global using NativePlatformImageView = global::UIKit.UIImageView;
global using NativePlatformView = global::UIKit.UIView;
#elif ANDROID
global using NativePlatformCameraPreviewView = global::AndroidX.Camera.View.PreviewView;
global using NativePlatformImage = global::Android.Graphics.Bitmap;
global using NativePlatformImageView = global::Android.Widget.ImageView;
global using NativePlatformView = global::Android.Views.View;
#elif WINDOWS
global using NativePlatformCameraPreviewView = global::Microsoft.UI.Xaml.FrameworkElement;
global using NativePlatformImage = global::Microsoft.UI.Xaml.Media.Imaging.WriteableBitmap;
global using NativePlatformImageView = global::Microsoft.UI.Xaml.Controls.Image;
global using NativePlatformView = global::Microsoft.UI.Xaml.FrameworkElement;
#else
global using NativePlatformCameraPreviewView = ZXing.Net.Maui.NativePlatformCameraPreviewView;
global using NativePlatformImage = ZXing.Net.Maui.NativePlatformImage;
global using NativePlatformImageView = ZXing.Net.Maui.NativePlatformImageView;
global using NativePlatformView = ZXing.Net.Maui.NativePlatformView;
#endif