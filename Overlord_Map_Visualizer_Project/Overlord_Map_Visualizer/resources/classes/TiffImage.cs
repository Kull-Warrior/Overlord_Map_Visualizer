using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Overlord_Map_Visualizer
{
    class TiffImage
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public byte[] Data { get; set; }

        public TiffImage (int width, int height,byte[] data)
        {
            Width = width;
            Height = height;
            Data = data;
        }

        public TiffBitmapEncoder Encode()
        {
            double dpi = 50;
            PixelFormat format = PixelFormats.Rgb48;
            int stride = ((Width * format.BitsPerPixel) + 7) / 8;

            WriteableBitmap writableBitmap = new WriteableBitmap(Width, Height, dpi, dpi, format, null);
            writableBitmap.WritePixels(new Int32Rect(0, 0, Width, Height), Data, stride, 0);

            // Encode as a TIFF
            TiffBitmapEncoder encoder = new TiffBitmapEncoder { Compression = TiffCompressOption.None };
            encoder.Frames.Add(BitmapFrame.Create(writableBitmap));

            return encoder;
        }
    }
}
