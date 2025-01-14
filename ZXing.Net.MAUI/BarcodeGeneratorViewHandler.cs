using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using System;
using System.Linq;

namespace ZXing.Net.Maui
{
    public partial class BarcodeGeneratorViewHandler : ViewHandler<IBarcodeGeneratorView, NativePlatformImageView>
    {
        private Size desiredSize;
        private readonly BarcodeWriter writer;
        private NativePlatformImageView? imageView;

        public static PropertyMapper<IBarcodeGeneratorView, BarcodeGeneratorViewHandler> BarcodeGeneratorViewMapper = new()
        {
            [nameof(IBarcodeGeneratorView.Format)] = MapUpdateBarcode,
            [nameof(IBarcodeGeneratorView.Value)] = MapUpdateBarcode,
            [nameof(IBarcodeGeneratorView.ForegroundColor)] = MapUpdateBarcode,
            [nameof(IBarcodeGeneratorView.BackgroundColor)] = MapUpdateBarcode,
            [nameof(IBarcodeGeneratorView.Margin)] = MapUpdateBarcode,
        };

        public BarcodeGeneratorViewHandler() : base(BarcodeGeneratorViewMapper)
        {
            writer = new BarcodeWriter();
        }

        public BarcodeGeneratorViewHandler(PropertyMapper? mapper = null) : base(mapper ?? BarcodeGeneratorViewMapper)
        {
            writer = new BarcodeWriter();
        }

        public override void PlatformArrange(Rect rect)
        {
            base.PlatformArrange(rect);

            // Don't update if it's the same size, otherwise we could infinite loop
            if (desiredSize.Width == rect.Width && desiredSize.Height == rect.Height)
                return;

            desiredSize = rect.Size;

            UpdateBarcode();
        }

        protected override NativePlatformImageView CreatePlatformView()
        {
#if IOS || MACCATALYST
            imageView ??= new NativePlatformImageView { BackgroundColor = UIKit.UIColor.Clear };
#elif ANDROID
			imageView = new NativePlatformImageView(Context);
			imageView.SetBackgroundColor(Android.Graphics.Color.Transparent);
#elif WINDOWS
			imageView = new NativePlatformImageView();
#endif
            return imageView;
        }

        protected override void ConnectHandler(NativePlatformImageView nativeView)
        {
            base.ConnectHandler(nativeView);
            UpdateBarcode();
        }

        private void UpdateBarcode()
        {
            writer.Format = VirtualView.Format.ToZXingList().FirstOrDefault();
            writer.Options.Width = (int)desiredSize.Width;
            writer.Options.Height = (int)desiredSize.Height;
            writer.Options.Margin = VirtualView.BarcodeMargin;
            writer.ForegroundColor = VirtualView.ForegroundColor;
            writer.BackgroundColor = VirtualView.BackgroundColor;

            NativePlatformImage? image = null;
            if (!string.IsNullOrEmpty(VirtualView.Value))
                image = writer?.Write(VirtualView.Value);

            if (imageView != null)
#if IOS || MACCATALYST
                imageView.Image = image;
#elif ANDROID
			    imageView.SetImageBitmap(image);
#elif WINDOWS
                imageView.Source = image;
#else
                throw new NotSupportedException();
#endif
        }

        public static void MapUpdateBarcode(BarcodeGeneratorViewHandler handler, IBarcodeGeneratorView barcodeGeneratorView)
            => handler.UpdateBarcode();
    }
}
