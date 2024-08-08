using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MediaBrushes = System.Windows.Media.Brushes;
using MediaColor = System.Windows.Media.Color;
using MediaPen = System.Windows.Media.Pen;

namespace Overlord_Map_Visualizer
{
    class CursorManagement
    {
        public CursorMode Mode;
        public CursorSubMode SubMode;
        public int CursorDiameter = 51;

        public CursorManagement()
        {
            Mode = CursorMode.Select;
            SubMode = CursorSubMode.Set;
            CursorDiameter = 51;
        }

        public void Update()
        {
            SolidColorBrush fillBrush;
            switch (Mode)
            {
                case CursorMode.Select:
                    Mouse.OverrideCursor = null;
                    break;
                case CursorMode.Pipette:
                    fillBrush = MediaBrushes.Black;
                    Mouse.OverrideCursor = CreateCursor(28, 28, fillBrush, null);
                    break;
                case CursorMode.Square:
                    fillBrush = new SolidColorBrush(MediaColor.FromArgb(127, 0xFF, 0xFF, 0xFF));
                    Mouse.OverrideCursor = CreateCursor(CursorDiameter, CursorDiameter, fillBrush, MediaBrushes.Black);
                    break;
                case CursorMode.Circle:
                    fillBrush = new SolidColorBrush(MediaColor.FromArgb(127, 0xFF, 0xFF, 0xFF));
                    Mouse.OverrideCursor = CreateCursor(CursorDiameter, CursorDiameter, fillBrush, MediaBrushes.Black);
                    break;
                case CursorMode.Rotate:
                    fillBrush = MediaBrushes.Black;
                    Mouse.OverrideCursor = CreateCursor(28, 32, fillBrush, null);
                    break;
            }
        }

        private Cursor CreateCursor(double cursorWidth, double cursorHeight, SolidColorBrush fillBrush, SolidColorBrush borderBrush)
        {
            System.Windows.Point centrePoint;
            int borderWidth;
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                switch (Mode)
                {
                    case CursorMode.Square:
                        borderWidth = 2;
                        centrePoint = new System.Windows.Point((cursorWidth + (borderWidth * 2)) / 2, (cursorHeight + (borderWidth * 2)) / 2);

                        //Draw Cursor Background Color
                        drawingContext.DrawRectangle(fillBrush, null, new Rect(2, 2, cursorWidth, cursorHeight));

                        //Draw Cursor Border
                        drawingContext.DrawLine(new MediaPen(borderBrush, 2.0), new System.Windows.Point(1, 0), new System.Windows.Point(1, cursorHeight + (borderWidth * 2)));
                        drawingContext.DrawLine(new MediaPen(borderBrush, 2.0), new System.Windows.Point(cursorWidth + (borderWidth * 2) - 1, 0), new System.Windows.Point(cursorWidth + (borderWidth * 2) - 1, cursorHeight + (borderWidth * 2)));
                        drawingContext.DrawLine(new MediaPen(borderBrush, 2.0), new System.Windows.Point(0, 1), new System.Windows.Point(cursorWidth + (borderWidth * 2), 1));
                        drawingContext.DrawLine(new MediaPen(borderBrush, 2.0), new System.Windows.Point(0, cursorHeight + (borderWidth * 2) - 1), new System.Windows.Point(cursorWidth + (borderWidth * 2), cursorHeight + (borderWidth * 2) - 1));

                        //Draw Cursor Cross
                        drawingContext.DrawLine(new MediaPen(borderBrush, 1.0), new System.Windows.Point(centrePoint.X - 10, centrePoint.Y), new System.Windows.Point(centrePoint.X + 10, centrePoint.Y));
                        drawingContext.DrawLine(new MediaPen(borderBrush, 1.0), new System.Windows.Point(centrePoint.X, centrePoint.Y - 10), new System.Windows.Point(centrePoint.X, centrePoint.Y + 10));
                        drawingContext.Close();
                        break;
                    case CursorMode.Pipette:
                        borderWidth = 0;
                        centrePoint = new System.Windows.Point(0, 0);
                        drawingContext.DrawImage(new BitmapImage(new Uri("pack://application:,,,/resources/cursor/Pipette_White_Border_Black.ico")), new Rect(0, 0, cursorWidth, cursorHeight));
                        drawingContext.Close();
                        break;
                    case CursorMode.Circle:
                        borderWidth = 2;
                        centrePoint = new System.Windows.Point((cursorWidth + (borderWidth * 2)) / 2, (cursorHeight + (borderWidth * 2)) / 2);

                        //Draw Cursor Border &  Background Color
                        drawingContext.DrawEllipse(fillBrush, new MediaPen(borderBrush, 1.0), centrePoint, (cursorWidth / 2) + 1, (cursorWidth / 2) + 1);

                        //Draw Cursor Cross
                        drawingContext.DrawLine(new MediaPen(borderBrush, 1.0), new System.Windows.Point(centrePoint.X - 10, centrePoint.Y), new System.Windows.Point(centrePoint.X + 10, centrePoint.Y));
                        drawingContext.DrawLine(new MediaPen(borderBrush, 1.0), new System.Windows.Point(centrePoint.X, centrePoint.Y - 10), new System.Windows.Point(centrePoint.X, centrePoint.Y + 10));
                        drawingContext.Close();
                        break;
                    case CursorMode.Rotate:
                        borderWidth = 0;
                        centrePoint = new System.Windows.Point((cursorWidth + borderWidth) / 2, (cursorHeight + borderWidth) / 2);
                        drawingContext.DrawImage(new BitmapImage(new Uri("pack://application:,,,/resources/cursor/Rotate_White_Border_Black.ico")), new Rect(0, 0, cursorWidth, cursorHeight));
                        drawingContext.Close();
                        break;
                    default:
                        centrePoint = new System.Windows.Point(0, 0);
                        borderWidth = 0;
                        break;
                }
            }
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)cursorWidth + (borderWidth * 2), (int)cursorHeight + (borderWidth * 2), 96, 96, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(drawingVisual);

            using (MemoryStream memoryStreamOne = new MemoryStream())
            {
                PngBitmapEncoder pngBitmapEncoder = new PngBitmapEncoder();
                pngBitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
                pngBitmapEncoder.Save(memoryStreamOne);

                byte[] pngBytes = memoryStreamOne.ToArray();
                int size = pngBytes.GetLength(0);

                //.cur format spec http://en.wikipedia.org/wiki/ICO_(file_format)
                using (MemoryStream memoryStreamTwo = new MemoryStream())
                {
                    {//ICONDIR Structure
                        memoryStreamTwo.Write(BitConverter.GetBytes((short)0), 0, 2);//Reserved. Must always be 0. 
                        memoryStreamTwo.Write(BitConverter.GetBytes((short)2), 0, 2);//Specifies image type: 1 for icon (.ICO) image, 2 for cursor (.CUR) image. Other values are invalid. 
                        memoryStreamTwo.Write(BitConverter.GetBytes((short)1), 0, 2);//Specifies number of images in the file. 2 Bytes
                    }

                    {//ICONDIRENTRY structure
                        memoryStreamTwo.WriteByte((byte)(cursorWidth + borderWidth)); //Specifies image width in pixels. Can be any number between 0 and 255. Value 0 means image width is 256 pixels.
                        memoryStreamTwo.WriteByte((byte)(cursorHeight + borderWidth)); //Specifies image height in pixels. Can be any number between 0 and 255. Value 0 means image height is 256 pixels.

                        memoryStreamTwo.WriteByte(0); //Specifies number of colors in the color palette. Should be 0 if the image does not use a color palette. 
                        memoryStreamTwo.WriteByte(0); //Reserved. Should be 0. 

                        memoryStreamTwo.Write(BitConverter.GetBytes((short)centrePoint.X), 0, 2);//2 bytes. In CUR format: Specifies the horizontal coordinates of the hotspot in number of pixels from the left.
                        memoryStreamTwo.Write(BitConverter.GetBytes((short)centrePoint.Y), 0, 2);//2 bytes. In CUR format: Specifies the vertical coordinates of the hotspot in number of pixels from the top.

                        memoryStreamTwo.Write(BitConverter.GetBytes(size), 0, 4);//Specifies the size of the image's data in bytes 
                        memoryStreamTwo.Write(BitConverter.GetBytes(22), 0, 4);//Specifies the offset of BMP or PNG data from the beginning of the ICO/CUR file
                    }

                    memoryStreamTwo.Write(pngBytes, 0, size);//write the png data.
                    memoryStreamTwo.Seek(0, SeekOrigin.Begin);
                    return new Cursor(memoryStreamTwo);
                }
            }
        }
    }
}
