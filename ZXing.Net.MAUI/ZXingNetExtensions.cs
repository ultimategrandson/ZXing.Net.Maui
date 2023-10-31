using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZXing.Net.Maui
{
    public static class ZXingNetExtensions
    {
        public static BarcodeResult[] ToBarcodeResults(this ZXing.Result[]? results)
        {
            if (results == null)
                return Array.Empty<BarcodeResult>();

            var list = new List<BarcodeResult>();

            foreach (var result in results)
            {
                if (result == null)
                    continue;

                IReadOnlyDictionary<MetadataType, object?> metadata;
                PointF[] points = Array.Empty<PointF>();

                if (result.ResultMetadata != null)
                    metadata = new Dictionary<MetadataType, object?>(result.ResultMetadata.Select(md => new KeyValuePair<MetadataType, object?>((MetadataType)md.Key, md.Value)));
                else
                    metadata = new Dictionary<MetadataType, object?>();

                if (result.ResultPoints != null)
                    points = result.ResultPoints.Where(p => p is not null).Select(p => new PointF(p.X, p.Y)).ToArray();

                list.Add(new BarcodeResult()
                {
                    Raw = result.RawBytes,
                    Value = result.Text,
                    Format = (BarcodeFormat)(int)result.BarcodeFormat,
                    Metadata = metadata,
                    PointsOfInterest = points
                });
            }

            return list.ToArray();
        }
    }
}