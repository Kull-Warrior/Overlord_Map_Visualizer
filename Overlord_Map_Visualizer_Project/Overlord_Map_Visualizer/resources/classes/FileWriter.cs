using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace Overlord_Map_Visualizer
{
    class FileWriter
    {
        public FileWriter()
        {

        }

        public void WriteMapDataToFile(string filePath, byte[] data, int offset, int width, int height, int bytesPerPoint)
        {
            int totalNumberOfBytes = width * height * bytesPerPoint;

            using (BinaryWriter writer = new BinaryWriter(new FileStream(filePath, FileMode.OpenOrCreate)))
            {
                writer.BaseStream.Seek(offset, SeekOrigin.Begin);
                writer.Write(data, 0, totalNumberOfBytes);
            }
        }

        public void WriteTiffDataToFile(string filePath, byte[] data, int width, int height)
        {
            double dpi = 50;
            PixelFormat format = PixelFormats.Rgb48;
            int stride = ((width * format.BitsPerPixel) + 7) / 8;

            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                WriteableBitmap writableBitmap = new WriteableBitmap(width, height, dpi, dpi, format, null);
                writableBitmap.WritePixels(new Int32Rect(0, 0, width, height), data, stride, 0);

                // Encode as a TIFF
                TiffBitmapEncoder encoder = new TiffBitmapEncoder { Compression = TiffCompressOption.None };
                encoder.Frames.Add(BitmapFrame.Create(writableBitmap));

                encoder.Save(stream);
            }
        }
    }
}
