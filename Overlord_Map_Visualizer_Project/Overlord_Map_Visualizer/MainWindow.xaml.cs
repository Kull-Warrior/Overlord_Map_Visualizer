using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;
using MediaBrushes = System.Windows.Media.Brushes;
using MediaColor = System.Windows.Media.Color;
using MediaPen = System.Windows.Media.Pen;
using Pen = System.Drawing.Pen;
using PixelFormat = System.Windows.Media.PixelFormat;
using DrawingPoint = System.Drawing.Point;

namespace Overlord_Map_Visualizer
{
    public partial class MainWindow : Window
    {
        private enum MapMode
        {
            HeightMap,
            MainTextureMap,
            WallTextureMap,
            FoliageMap,
            UnknownMap,
            Full
        }

        private enum CursorMode
        {
            Select,
            Pipette,
            Square,
            Circle,
            Rotate
        }

        private enum CursorSubMode
        {
            Set,
            Add,
            Sub
        }

        private enum DrawingType
        {
            Map,
            SelectedColor
        }

        private MapMode CurrentMapMode;
        private readonly int MapWidth = 512;
        private readonly int MapHeight = 512;
        private string OMPFilePathString;
        private double WaterLevel = 0;

        private byte[,] HeightMapDigitsOneAndTwo;
        private byte[,] HeightMapDigitsThreeAndFour;
        private byte[,] MainTextureMap;
        private byte[,] FoliageMap;
        private byte[,] WallTextureMap;
        private byte[,] UnknownMap;

        private List<OverlordObject> MapObjectList = new System.Collections.Generic.List<OverlordObject>();

        private bool IsAnyMapLoaded = false;

        private CursorMode CurrentCursorMode;
        private CursorSubMode CurrentCursorSubMode;
        private int CursorDiameter = 51;

        public MainWindow()
        {
            InitializeComponent();
            Initialise();
            DrawCoordinateSystem();
        }

        private void Initialise()
        {
            HeightMapDigitsOneAndTwo = new byte[MapWidth, MapHeight];
            HeightMapDigitsThreeAndFour = new byte[MapWidth, MapHeight];

            MainTextureMap = new byte[MapWidth, MapHeight];
            FoliageMap = new byte[MapWidth, MapHeight];
            WallTextureMap = new byte[MapWidth, MapHeight];
            UnknownMap = new byte[MapWidth, MapHeight];

            CurrentCursorMode = CursorMode.Select;
            CurrentCursorSubMode = CursorSubMode.Set;
            HighlightCurrentCursorMode();
            HighlightCurrentCursorSubMode();
            SelectedColorCode.Text = "0000";
            cursorDiameterLabel.Content = "Cursor Diameter: " + CursorSizeSlider.Value;
        }

        private void DrawCoordinateSystem()
        {
            using (Bitmap coordinateSystem = new Bitmap((int)CoordinateSystem.Width, (int)CoordinateSystem.Height))
            using (Graphics mapGraphics = Graphics.FromImage(coordinateSystem))
            using (Pen coordinatePen = new Pen(new SolidBrush(Color.Black)))
            {
                //Y Axis Arrow
                mapGraphics.DrawLine(coordinatePen, 1, 8, 9, 0);
                mapGraphics.DrawLine(coordinatePen, 9, 531, 9, 0);
                mapGraphics.DrawLine(coordinatePen, 17, 8, 9, 0);

                //X Axis Arrow
                mapGraphics.DrawLine(coordinatePen, 523, 514, 531, 522);
                mapGraphics.DrawLine(coordinatePen, 0, 522, 531, 522);
                mapGraphics.DrawLine(coordinatePen, 523, 530, 531, 522);

                //Y Axis Marker
                for (int y = 522; y >= 21; y -= 50)
                {
                    mapGraphics.DrawLine(coordinatePen, 0, y, 8, y);
                }

                //X Axis Marker
                for (int x = 9; x <= 509; x += 50)
                {
                    mapGraphics.DrawLine(coordinatePen, x, 522, x, 531);
                }

                CreateNewLabel("CoordMarkerOrigin", "  0", 257 + 560, 579);

                CreateNewLabel("CoordMarkerX", "X", 811 - 540, 562);

                for (int i = 10; i >= 1; i--)
                {
                    string content = "" + i * 50;
                    if (i == 1)
                    {
                        content = " " + content;
                    }
                    CreateNewLabel("CoordMarkerX" + i, content, 278 + 550 - (i * 50) - 37, 579);
                }

                CreateNewLabel("CoordMarkerY", "Y", 284 + 513, 35);

                for (int i = 1; i <= 10; i++)
                {
                    string content = "" + i * 50;
                    if (i == 1)
                    {
                        content = "  " + content;
                    }
                    CreateNewLabel("CoordMarkerY" + i, content, 257 + 555, 561 - (i * 50));
                }

                //Set Origin Bottom Right
                coordinateSystem.RotateFlip(RotateFlipType.RotateNoneFlipX);

                CoordinateSystem.Source = GetBmpImageFromBmp(coordinateSystem);
            }
        }

        private void CreateNewLabel(string labelName, string labelContent, int x, int y)
        {
            Label dynamicLabel = new Label
            {
                Name = labelName,
                Content = labelContent,
                Margin = new Thickness(x, y, 0, 0),
            };

            appGrid.Children.Insert(0, dynamicLabel);
        }

        private byte[] ReadDataFromFile(int offset, string filePath, int bytesPerPoint)
        {
            int totalNumberOfBytes = MapWidth * MapHeight * bytesPerPoint;
            byte[] data = new byte[totalNumberOfBytes];

            using (BinaryReader reader = new BinaryReader(new FileStream(filePath, FileMode.Open)))
            {
                reader.BaseStream.Seek(offset, SeekOrigin.Begin);
                reader.Read(data, 0, totalNumberOfBytes);
            }

            return data;
        }

        private void SetMapData(byte[] data, int bytesPerPoint, MapMode mapMode, bool isTiffImage)
        {
            int xOffset = bytesPerPoint;
            int yOffset = 0;
            int numberOfBytesInRow = MapWidth * bytesPerPoint;
            int totalOffset;

            for (int y = 0; y < MapHeight; y++)
            {
                if (y != 0)
                {
                    yOffset = y * numberOfBytesInRow;
                }
                for (int x = 0; x < MapWidth; x++)
                {
                    totalOffset = x * xOffset + yOffset;

                    switch (mapMode)
                    {
                        case MapMode.HeightMap:
                            if (isTiffImage)
                            {
                                int grayscale = (data[totalOffset + 1] << 8) + data[totalOffset];
                                grayscale /= 16;

                                HeightMapDigitsOneAndTwo[x, y] = (byte)(grayscale & 0x00FF);
                                HeightMapDigitsThreeAndFour[x, y] = (byte)((grayscale & 0x0F00) >> 8);
                            }
                            else
                            {
                                HeightMapDigitsOneAndTwo[x, y] = data[totalOffset];
                                HeightMapDigitsThreeAndFour[x, y] = data[totalOffset + 1];
                            }
                            break;
                        case MapMode.MainTextureMap:
                            if (isTiffImage)
                            {
                                int blue = (data[totalOffset + 1] << 8) + data[totalOffset];
                                int green = (data[totalOffset + 3] << 8) + data[totalOffset + 2];
                                int red = (data[totalOffset + 5] << 8) + data[totalOffset + 4];

                                MainTextureMap[x, y] = (byte)GetFourBitRgbPaletteIndexFromTiffRgb(blue, green, red);
                            }
                            else
                            {
                                MainTextureMap[x, y] = data[totalOffset];
                            }
                            break;
                        case MapMode.FoliageMap:
                            if (isTiffImage)
                            {
                                int blue = (data[totalOffset + 1] << 8) + data[totalOffset];
                                int green = (data[totalOffset + 3] << 8) + data[totalOffset + 2];
                                int red = (data[totalOffset + 5] << 8) + data[totalOffset + 4];

                                FoliageMap[x, y] = (byte)GetFourBitRgbPaletteIndexFromTiffRgb(blue, green, red);
                            }
                            else
                            {
                                FoliageMap[x, y] = data[totalOffset];
                            }
                            break;
                        case MapMode.WallTextureMap:
                            if (isTiffImage)
                            {
                                int blue = (data[totalOffset + 1] << 8) + data[totalOffset];
                                int green = (data[totalOffset + 3] << 8) + data[totalOffset + 2];
                                int red = (data[totalOffset + 5] << 8) + data[totalOffset + 4];

                                WallTextureMap[x, y] = (byte)GetFourBitRgbPaletteIndexFromTiffRgb(blue, green, red);
                            }
                            else
                            {
                                WallTextureMap[x, y] = (byte)(data[totalOffset] >> 4);
                            }
                            break;
                        case MapMode.UnknownMap:
                            if (isTiffImage)
                            {
                                int blue = (data[totalOffset + 1] << 8) + data[totalOffset];
                                int green = (data[totalOffset + 3] << 8) + data[totalOffset + 2];
                                int red = (data[totalOffset + 5] << 8) + data[totalOffset + 4];

                                UnknownMap[x, y] = (byte)GetFourBitRgbPaletteIndexFromTiffRgb(blue, green, red);
                            }
                            else
                            {
                                UnknownMap[x, y] = (byte)(data[totalOffset] >> 4);
                            }
                            break;
                        case MapMode.Full:
                            HeightMapDigitsOneAndTwo[x, y] = data[totalOffset];
                            HeightMapDigitsThreeAndFour[x, y] = data[totalOffset + 1];
                            MainTextureMap[x, y] = (byte)(data[totalOffset + 2] & 0x0F);
                            FoliageMap[x, y] = (byte)((data[totalOffset + 2] & 0xF0) >> 4);
                            WallTextureMap[x, y] = (byte)(data[totalOffset + 3] & 0x0F);
                            UnknownMap[x, y] = (byte)((data[totalOffset + 3] & 0xF0) >> 4);
                            break;
                    }
                }
            }
        }

        private void ImportfromFile(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            OpenFileDialog openFileDialog;
            string dialogTitle;
            string fileExtension;
            string filter;

            if (button.Name == "ImportfromOMPFileButton")
            {
                dialogTitle = "Browse OMP map files";
                fileExtension = "omp";
                filter = "omp files (*.omp)|*.omp";
            }
            else if (button.Name == "ImportMapData")
            {
                if (CurrentMapMode == MapMode.HeightMap)
                {
                    dialogTitle = "Browse overlord height data";
                    fileExtension = "ohd";
                    filter = "ohd files (*.ohd)|*.ohd";
                }
                else
                {
                    dialogTitle = "Browse overlord general data";
                    fileExtension = "ogd";
                    filter = "ogd files (*.ogd)|*.ogd";
                }
            }
            else if (button.Name == "ImportMapImage")
            {
                dialogTitle = "Browse overlord map image";
                fileExtension = "tiff";
                filter = "mapdata files (*.tiff)|*.tiff";
            }
            else
            {
                dialogTitle = "";
                fileExtension = "";
                filter = "";
            }

            openFileDialog = new OpenFileDialog
            {
                InitialDirectory = @"C:\",
                RestoreDirectory = true,
                Title = dialogTitle,
                DefaultExt = fileExtension,
                Filter = filter,
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                int offset;
                int bytesPerPoint;
                MapMode mapMode;
                bool isTiffImage;
                switch (fileExtension)
                {
                    case "omp":
                        bytesPerPoint = 4;
                        mapMode = MapMode.Full;
                        isTiffImage = false;
                        FilePath.Text = openFileDialog.FileName;
                        OMPFilePathString = openFileDialog.FileName;
                        offset = GetMapDataOffset();
                        break;
                    case "tiff":
                        bytesPerPoint = 6;
                        mapMode = CurrentMapMode;
                        isTiffImage = true;
                        offset = 8;
                        break;
                    case "ohd":
                        bytesPerPoint = 2;
                        mapMode = CurrentMapMode;
                        isTiffImage = false;
                        offset = 0;
                        break;
                    case "ogd":
                        bytesPerPoint = 1;
                        mapMode = CurrentMapMode;
                        isTiffImage = false;
                        offset = 0;
                        break;
                    default:
                        bytesPerPoint = 1;
                        mapMode = CurrentMapMode;
                        isTiffImage = false;
                        offset = 0;
                        break;
                }
                byte[] data = ReadDataFromFile(offset, openFileDialog.FileName, bytesPerPoint);
                SetMapData(data, bytesPerPoint, mapMode, isTiffImage);
                WaterLevel = GetMapWaterLevel();

                DrawTiffImage(MapWidth, MapHeight, DrawingType.Map);

                if (!IsAnyMapLoaded)
                {
                    MapModeDropDown.SelectedIndex = 0;
                    MapModeDropDown.IsEnabled = true;
                    ImportMapData.IsEnabled = true;
                    ImportMapImage.IsEnabled = true;
                    ExportMapData.IsEnabled = true;
                    ExportMapImage.IsEnabled = true;
                    ExportToOMPFileButton.IsEnabled = true;
                    SelectedColorCode.IsEnabled = true;
                    IsAnyMapLoaded = true;
                }
            }
        }

        private byte[] GetMapData(int bytesPerPoint, MapMode mapMode)
        {
            int totalNumberOfBytes = MapWidth * MapHeight * bytesPerPoint;
            byte[] data = new byte[totalNumberOfBytes];
            int xOffset = bytesPerPoint;
            int yOffset = 0;
            int numberOfBytesInRow = MapWidth * bytesPerPoint;
            int totalOffset;

            for (int y = 0; y < MapHeight; y++)
            {
                if (y != 0)
                {
                    yOffset = y * numberOfBytesInRow;
                }
                for (int x = 0; x < MapWidth; x++)
                {
                    totalOffset = x * xOffset + yOffset;

                    switch (mapMode)
                    {
                        case MapMode.HeightMap:
                            data[totalOffset] = HeightMapDigitsOneAndTwo[x, y];
                            data[totalOffset + 1] = HeightMapDigitsThreeAndFour[x, y];
                            break;
                        case MapMode.MainTextureMap:
                            data[totalOffset] = MainTextureMap[x, y];
                            break;
                        case MapMode.FoliageMap:
                            data[totalOffset] = (byte)(FoliageMap[x, y] << 4);
                            break;
                        case MapMode.WallTextureMap:
                            data[totalOffset] = WallTextureMap[x, y];
                            break;
                        case MapMode.UnknownMap:
                            data[totalOffset] = (byte)(MainTextureMap[x, y] << 4);
                            break;
                        case MapMode.Full:
                            data[totalOffset] = HeightMapDigitsOneAndTwo[x, y];
                            data[totalOffset + 1] = HeightMapDigitsThreeAndFour[x, y];
                            data[totalOffset + 2] = MainTextureMap[x, y];
                            data[totalOffset + 2] += (byte)(FoliageMap[x, y] << 4);
                            data[totalOffset + 3] = WallTextureMap[x, y];
                            data[totalOffset + 3] += (byte)(UnknownMap[x, y] << 4);
                            break;
                    }
                }
            }
            return data;
        }

        private void WriteMapData(byte[] data, int offset, string filePath, int bytesPerPoint)
        {
            int totalNumberOfBytes = MapWidth * MapHeight * bytesPerPoint;

            using (BinaryWriter writer = new BinaryWriter(new FileStream(filePath, FileMode.OpenOrCreate)))
            {
                writer.BaseStream.Seek(offset, SeekOrigin.Begin);
                writer.Write(data, 0, totalNumberOfBytes);
            }
        }

        private void ExportToFile(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            SaveFileDialog saveFileDialog;
            byte[] data;
            int bytesPerPoint;
            string filePath;
            int offset;

            if (button.Name == "ExportToOMPFileButton")
            {
                offset = GetMapDataOffset();
                bytesPerPoint = 4;
                filePath = OMPFilePathString;
                data = GetMapData(bytesPerPoint, MapMode.Full);
                WriteMapData(data, offset, filePath, bytesPerPoint);
            }
            else if (button.Name == "ExportMapData")
            {
                string dialogTitle;
                string fileExtension;
                string filter;

                if (CurrentMapMode == MapMode.HeightMap)
                {
                    dialogTitle = "Browse overlord height data";
                    fileExtension = "ohd";
                    filter = "ohd files (*.ohd)|*.ohd";
                    bytesPerPoint = 2;
                }
                else
                {
                    dialogTitle = "Browse overlord general data";
                    fileExtension = "ogd";
                    filter = "ogd files (*.ogd)|*.ogd";
                    bytesPerPoint = 1;
                }

                saveFileDialog = new SaveFileDialog
                {
                    InitialDirectory = @"C:\",
                    RestoreDirectory = true,
                    Title = dialogTitle,
                    DefaultExt = fileExtension,
                    Filter = filter
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    offset = 0;
                    filePath = saveFileDialog.FileName;
                    data = GetMapData(bytesPerPoint, CurrentMapMode);
                    WriteMapData(data, offset, filePath, bytesPerPoint);
                }
            }
            else if (button.Name == "ExportMapImage")
            {
                saveFileDialog = new SaveFileDialog
                {
                    InitialDirectory = @"C:\",
                    RestoreDirectory = true,
                    Title = "Select Directory and file name for the overlord map image",
                    DefaultExt = "tiff",
                    Filter = "tiff image files (*.tiff)|*.tiff"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (FileStream stream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                    {
                        double dpi = 50;
                        PixelFormat format = PixelFormats.Rgb48;
                        int stride = ((MapWidth * format.BitsPerPixel) + 7) / 8;
                        data = CreateTiffData(MapWidth, MapHeight, DrawingType.Map);
                        WriteableBitmap writableBitmap = new WriteableBitmap(MapWidth, MapHeight, dpi, dpi, format, null);
                        writableBitmap.WritePixels(new Int32Rect(0, 0, MapWidth, MapHeight), data, stride, 0);

                        // Encode as a TIFF
                        TiffBitmapEncoder encoder = new TiffBitmapEncoder { Compression = TiffCompressOption.None };
                        encoder.Frames.Add(BitmapFrame.Create(writableBitmap));

                        encoder.Save(stream);
                    }
                }
            }
        }

        private void EditMapData(int xMouseCoordinate, int yMouseCoordinate, byte[,] data)
        {
            int yMin = 0;
            int xMin = 0;
            int xMax = 511;
            int yMax = 511;
            int cursorRadius = CursorDiameter / 2;

            if ((xMouseCoordinate - cursorRadius) >= xMin)
            {
                xMin = xMouseCoordinate - cursorRadius;
            }
            if ((xMouseCoordinate + cursorRadius) <= xMax)
            {
                xMax = xMouseCoordinate + cursorRadius;
            }

            if ((yMouseCoordinate - cursorRadius) >= yMin)
            {
                yMin = yMouseCoordinate - cursorRadius;
            }
            if ((yMouseCoordinate + cursorRadius) <= yMax)
            {
                yMax = yMouseCoordinate + cursorRadius;
            }

            for (int y = yMin; y <= yMax; y++)
            {
                for (int x = xMin; x <= xMax; x++)
                {
                    if (CurrentCursorMode == CursorMode.Square || (((x - xMouseCoordinate) * (x - xMouseCoordinate)) + ((y - yMouseCoordinate) * (y - yMouseCoordinate)) < (int)Math.Ceiling((decimal)CursorDiameter / 2) * (int)Math.Ceiling((decimal)CursorDiameter / 2)))
                    {
                        byte selectedValue = Convert.ToByte(SelectedColorCode.Text, 16);
                        int pixelValue = data[x, y];
                        switch (CurrentCursorSubMode)
                        {
                            case CursorSubMode.Set:
                                data[x, y] = selectedValue;
                                break;
                            case CursorSubMode.Add:
                                if (pixelValue + selectedValue <= 15)
                                {
                                    data[x, y] = (byte)(pixelValue + selectedValue);
                                }
                                else
                                {
                                    data[x, y] = 15;
                                }
                                break;
                            case CursorSubMode.Sub:
                                if (pixelValue - selectedValue >= 0)
                                {
                                    data[x, y] = (byte)(pixelValue - selectedValue);
                                }
                                else
                                {
                                    data[x, y] = 0;
                                }
                                break;
                        }
                    }
                }
            }
        }

        private void EditMapData(int xMouseCoordinate, int yMouseCoordinate, byte[,] lowerByteData, byte[,] higherByteData)
        {
            int yMin = 0;
            int xMin = 0;
            int xMax = 511;
            int yMax = 511;
            int cursorRadius = CursorDiameter / 2;

            if ((xMouseCoordinate - cursorRadius) >= xMin)
            {
                xMin = xMouseCoordinate - cursorRadius;
            }
            if ((xMouseCoordinate + cursorRadius) <= xMax)
            {
                xMax = xMouseCoordinate + cursorRadius;
            }

            if ((yMouseCoordinate - cursorRadius) >= yMin)
            {
                yMin = yMouseCoordinate - cursorRadius;
            }
            if ((yMouseCoordinate + cursorRadius) <= yMax)
            {
                yMax = yMouseCoordinate + cursorRadius;
            }

            for (int y = yMin; y <= yMax; y++)
            {
                for (int x = xMin; x <= xMax; x++)
                {
                    if (CurrentCursorMode == CursorMode.Square || (((x - xMouseCoordinate) * (x - xMouseCoordinate)) + ((y - yMouseCoordinate) * (y - yMouseCoordinate)) < (int)Math.Ceiling((decimal)CursorDiameter / 2) * (int)Math.Ceiling((decimal)CursorDiameter / 2)))
                    {
                        byte[] tempByteArray = GetByteArrayFromHexString(SelectedColorCode.Text);
                        int selectedValue = (tempByteArray[1] << 8) + tempByteArray[0];
                        int pixelValue = (higherByteData[x, y] << 8) + lowerByteData[x, y];
                        switch (CurrentCursorSubMode)
                        {
                            case CursorSubMode.Set:
                                lowerByteData[x, y] = tempByteArray[0];
                                higherByteData[x, y] = tempByteArray[1];
                                break;
                            case CursorSubMode.Add:
                                if (pixelValue + selectedValue <= 65535)
                                {
                                    lowerByteData[x, y] = (byte)((pixelValue + selectedValue) & 0x00FF);
                                    higherByteData[x, y] = (byte)((pixelValue + selectedValue) >> 8);
                                }
                                else
                                {
                                    lowerByteData[x, y] = 255;
                                    higherByteData[x, y] = 255;
                                }
                                break;
                            case CursorSubMode.Sub:
                                if (pixelValue - selectedValue >= 0)
                                {
                                    lowerByteData[x, y] = (byte)((pixelValue - selectedValue) & 0x00FF);
                                    higherByteData[x, y] = (byte)((pixelValue - selectedValue) >> 8);
                                }
                                else
                                {
                                    lowerByteData[x, y] = 0;
                                    higherByteData[x, y] = 0;
                                }
                                break;
                        }
                    }
                }
            }
        }

        private void RotateMapData(byte[,] byteData)
        {
            for (int x = 0; x < MapWidth / 2; x++)
            {
                for (int y = x; y < MapHeight - x - 1; y++)
                {
                    byte temp = byteData[x, y];

                    byteData[x, y] = byteData[MapWidth - 1 - y, x];

                    byteData[MapWidth - 1 - y, x] = byteData[MapWidth - 1 - x, MapHeight - 1 - y];

                    byteData[MapWidth - 1 - x, MapHeight - 1 - y] = byteData[y, MapHeight - 1 - x];

                    byteData[y, MapHeight - 1 - x] = temp;
                }
            }
        }

        private int GetMapDataOffset()
        {
            int srcOffset = 200 + MapWidth * MapHeight * 4;
            int totalNumberOfBytes = 500;
            byte[] src = new byte[totalNumberOfBytes];

            using (BinaryReader reader = new BinaryReader(new FileStream(OMPFilePathString, FileMode.Open)))
            {
                reader.BaseStream.Seek(srcOffset, SeekOrigin.Begin);
                reader.Read(src, 0, totalNumberOfBytes);
            }

            byte[] pattern = new byte[9] { (byte)'I', (byte)'n', (byte)'d', (byte)'o', (byte)'o', (byte)'r', (byte)'S', (byte)'e', (byte)'t' };//IndoorSet

            int maxFirstCharSlot = src.Length - pattern.Length + 1;
            for (int i = 0; i < maxFirstCharSlot;i++)
            {
                if (src[i] != pattern[0])
                {
                    continue;
                }

                for (int j = pattern.Length - 1; j >= 1; j--)
                {
                    if (src[i + j] != pattern[j])
                    {
                        break;
                    }
                    if (j== 1)
                    {
                        return i + 200 - 4;
                    }
                }

            }

            return 0;
        }

        private double GetMapWaterLevel()
        {
            switch (OMPFilePathString)
            {
                case string a when a.Contains("Exp - HalflingMain"):
                    return 15;
                case string a when a.Contains("Exp - Halfling Abyss"):
                    return 50;
                case string a when a.Contains("Exp - ElfMain"):
                    return 15.3125;
                case string a when a.Contains("Exp - Elf Abyss"):
                    return 15;
                case string a when a.Contains("Exp - PaladinMain"):
                    return 15;
                case string a when a.Contains("Exp - Paladin Abyss"):
                    return 15;
                case string a when a.Contains("Exp - DwarfMain"):
                    return 50;
                case string a when a.Contains("Exp - Dwarf Abyss"):
                    return 0;
                case string a when a.Contains("Exp - WarriorMain"):
                    return 15;
                case string a when a.Contains("Exp - Warrior Abyss - 01"):
                    return 0;
                case string a when a.Contains("Exp - Warrior Abyss - 02"):
                    return 15.03125;
                case string a when a.Contains("Exp - Tower_Dungeon"):
                    return 15;
                case string a when a.Contains("Exp - Tower_Spawnpit"):
                    return 36.03125;
                case string a when a.Contains("Exp - Tower"):
                    return 0;
                case string a when a.Contains("HalflingMain"):
                    return 15;
                case string a when a.Contains("SlaveCamp"):
                    return 16.03125;
                case string a when a.Contains("HalflingHomes1of2"):
                    return 15;
                case string a when a.Contains("HalflingHomes2of2"):
                    return 15;
                case string a when a.Contains("HellsKitchen"):
                    return 20;
                case string a when a.Contains("EntryCastleSpree"):
                    return 15;
                case string a when a.Contains("SpreeDungeon"):
                    return 15;
                case string a when a.Contains("ElfMain"):
                    return 15;
                case string a when a.Contains("GreenCave"):
                    return 43;
                case string a when a.Contains("SkullDen"):
                    return 15.03125;
                case string a when a.Contains("TrollTemple"):
                    return 0;
                case string a when a.Contains("PaladinMain"):
                    return 15;
                case string a when a.Contains("BlueCave"):
                    return 25.03125;
                case string a when a.Contains("Sewers1of2"):
                    return 27.84375;
                case string a when a.Contains("Sewers2of2"):
                    return 18.03125;
                case string a when a.Contains("Red Light Inn"):
                    return 15;
                case string a when a.Contains("Citadel"):
                    return 15;
                case string a when a.Contains("DwarfMain"):
                    return 50;
                case string a when a.Contains("GoldMine"):
                    return 15;
                case string a when a.Contains("Quarry"):
                    return 40.71875;
                case string a when a.Contains("HomeyHalls1of2"):
                    return 0;
                case string a when a.Contains("HomeyHalls2of2"):
                    return 57.03125;
                case string a when a.Contains("ArcaniumMine"):
                    return 0;
                case string a when a.Contains("RoyalHalls"):
                    return 15;
                case string a when a.Contains("WarriorMain"):
                    return 15;
                case string a when a.Contains("2P_Deathtrap"):
                    return 0;
                case string a when a.Contains("2P_Gates"):
                    return 21;
                case string a when a.Contains("2P_LastStand"):
                    return 15.03125;
                case string a when a.Contains("2P_PartyCrashers"):
                    return 15.03125;
                case string a when a.Contains("2P_Plunder"):
                    return 15;
                case string a when a.Contains("2P_TombRobber"):
                    return 15;
                case string a when a.Contains("Tower_Dungeon"):
                    return 15;
                case string a when a.Contains("Tower_Spawnpit"):
                    return 36.03125;
                case string a when a.Contains("Tower"):
                    return 0;
                case string a when a.Contains("PlayerMap"):
                    return 15;
                case string a when a.Contains("2P_Arena2"):
                    return 0;
                case string a when a.Contains("2P_Bombs"):
                    return 15;
                case string a when a.Contains("2P_GrabTheMaidens"):
                    return 0;
                case string a when a.Contains("2P_KillTheHoard"):
                    return 0;
                case string a when a.Contains("2P_KingoftheHill"):
                    return 0;
                case string a when a.Contains("2P_March_Mellow_Maidens"):
                    return 0;
                case string a when a.Contains("2P_Misty"):
                    return 0;
                case string a when a.Contains("2P_RockyRace"):
                    return 0;
                default:
                    return 0;
            }
        }

        private BitmapImage GetBmpImageFromBmp(Bitmap bitMap)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                bitMap.Save(memoryStream, ImageFormat.Tiff);

                memoryStream.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        private int[] GetTiffRgbFromFourBitRgbPalette(byte value)
        {
            int[] rgb = new int[3];
            switch (value)
            {
                case 0:
                    rgb[0] = 0x00;
                    rgb[1] = 0x00;
                    rgb[2] = 0x00;
                    break;
                case 1:
                    rgb[0] = 0x6B;
                    rgb[1] = 0xC2;
                    rgb[2] = 0xF5;
                    break;
                case 2:
                    rgb[0] = 0xA5;
                    rgb[1] = 0xBD;
                    rgb[2] = 0x53;
                    break;
                case 3:
                    rgb[0] = 0xBE;
                    rgb[1] = 0xA5;
                    rgb[2] = 0x43;
                    break;
                case 4:
                    rgb[0] = 0x57;
                    rgb[1] = 0x78;
                    rgb[2] = 0xF0;
                    break;
                case 5:
                    rgb[0] = 0xD6;
                    rgb[1] = 0x62;
                    rgb[2] = 0x5C;
                    break;
                case 6:
                    rgb[0] = 0xDD;
                    rgb[1] = 0xB8;
                    rgb[2] = 0xEB;
                    break;
                case 7:
                    rgb[0] = 0xFF;
                    rgb[1] = 0x78;
                    rgb[2] = 0xF3;
                    break;
                case 8:
                    rgb[0] = 0x6D;
                    rgb[1] = 0xB0;
                    rgb[2] = 0x4F;
                    break;
                case 9:
                    rgb[0] = 0xE2;
                    rgb[1] = 0xD6;
                    rgb[2] = 0xCB;
                    break;
                case 10:
                    rgb[0] = 0x34;
                    rgb[1] = 0x2C;
                    rgb[2] = 0xBF;
                    break;
                case 11:
                    rgb[0] = 0xD3;
                    rgb[1] = 0xE7;
                    rgb[2] = 0xCA;
                    break;
                case 12:
                    rgb[0] = 0x37;
                    rgb[1] = 0x91;
                    rgb[2] = 0xD4;
                    break;
                case 13:
                    rgb[0] = 0x8D;
                    rgb[1] = 0x39;
                    rgb[2] = 0xBE;
                    break;
                case 14:
                    rgb[0] = 0xFF;
                    rgb[1] = 0xFF;
                    rgb[2] = 0x00;
                    break;
                case 15:
                    rgb[0] = 0xFF;
                    rgb[1] = 0xFF;
                    rgb[2] = 0xFF;
                    break;
                default:
                    rgb[0] = 0x00;
                    rgb[1] = 0x00;
                    rgb[2] = 0x00;
                    break;
            }

            rgb[0] = rgb[0] * 65535 / 255;
            rgb[1] = rgb[1] * 65535 / 255;
            rgb[2] = rgb[2] * 65535 / 255;

            return rgb;
        }

        private int GetFourBitRgbPaletteIndexFromTiffRgb(int red, int green, int blue)
        {
            red = red * 255 / 65535;
            green = green * 255 / 65535;
            blue = blue * 255 / 65535;

            if (red == 0 && green == 0 && blue == 0)
            {
                return 0;
            }
            else if (red == 0xF5 && green == 0xC2 && blue == 0x6B)
            {
                return 1;
            }
            else if (red == 0x53 && green == 0xBD && blue == 0xA5)
            {
                return 2;
            }
            else if (red == 0x43 && green == 0xA5 && blue == 0xBE)
            {
                return 3;
            }
            else if (red == 0xF0 && green == 0x78 && blue == 0x57)
            {
                return 4;
            }
            else if (red == 0x5C && green == 0x62 && blue == 0xD6)
            {
                return 5;
            }
            else if (red == 0xEB && green == 0xB8 && blue == 0xDD)
            {
                return 6;
            }
            else if (red == 0xF3 && green == 0x78 && blue == 0xFF)
            {
                return 7;
            }
            else if (red == 0x4F && green == 0xB0 && blue == 0x6D)
            {
                return 8;
            }
            else if (red == 0xCB && green == 0xD6 && blue == 0xE2)
            {
                return 9;
            }
            else if (red == 0xBF && green == 0x2C && blue == 0x34)
            {
                return 10;
            }
            else if (red == 0xCA && green == 0xE7 && blue == 0xD3)
            {
                return 11;
            }
            else if (red == 0xD4 && green == 0x91 && blue == 0x37)
            {
                return 12;
            }
            else if (red == 0xBE && green == 0x39 && blue == 0x8D)
            {
                return 13;
            }
            else if (red == 0x00 && green == 0xFF && blue == 0xFF)
            {
                return 14;
            }
            else if (red == 0xFF && green == 0xFF && blue == 0xFF)
            {
                return 15;
            }
            else
            {
                return 0;
            }
        }

        private byte[] CreateTiffData(int width, int height, DrawingType type)
        {
            int[] rgb;
            int red;
            int blue;
            int green;
            int grayScale;
            int xOffset = 6;
            int yOffset = 0;
            int numberOfBytesInRow = width * 6; //One point is described by six bytes
            int totalOffset;
            byte[] data = new byte[width * height * 6];

            for (int y = 0; y < height; y++)
            {
                if (y != 0)
                {
                    yOffset = y * numberOfBytesInRow;
                }
                for (int x = 0; x < width; x++)
                {
                    totalOffset = x * xOffset + yOffset;
                    switch (CurrentMapMode)
                    {
                        case MapMode.HeightMap:
                            switch (type)
                            {
                                case DrawingType.Map:
                                    grayScale = ((HeightMapDigitsThreeAndFour[x, y] << 8) & 0x0FFF) + HeightMapDigitsOneAndTwo[x, y];
                                    break;
                                case DrawingType.SelectedColor:
                                    byte[] singleColorData = GetByteArrayFromHexString(SelectedColorCode.Text);
                                    grayScale = ((singleColorData[1] << 8) & 0x0FFF) + singleColorData[0];
                                    break;
                                default:
                                    grayScale = 0;
                                    break;
                            }

                            grayScale = grayScale * 65535 / 4095;

                            data[totalOffset] = (byte)(grayScale & 0x00FF);
                            data[totalOffset + 1] = (byte)((grayScale & 0xFF00) >> 8);
                            data[totalOffset + 2] = (byte)(grayScale & 0x00FF);
                            data[totalOffset + 3] = (byte)((grayScale & 0xFF00) >> 8);
                            data[totalOffset + 4] = (byte)(grayScale & 0x00FF);
                            data[totalOffset + 5] = (byte)((grayScale & 0xFF00) >> 8);
                            break;
                        case MapMode.MainTextureMap:
                            switch (type)
                            {
                                case DrawingType.Map:
                                    rgb = GetTiffRgbFromFourBitRgbPalette(MainTextureMap[x, y]);
                                    red = rgb[0];
                                    green = rgb[1];
                                    blue = rgb[2];
                                    break;
                                case DrawingType.SelectedColor:
                                    byte singleColorData = Convert.ToByte(SelectedColorCode.Text, 16);
                                    rgb = GetTiffRgbFromFourBitRgbPalette(singleColorData);
                                    red = rgb[0];
                                    green = rgb[1];
                                    blue = rgb[2];
                                    break;
                                default:
                                    red = 0;
                                    green = 0;
                                    blue = 0;
                                    break;
                            }

                            data[totalOffset] = (byte)(blue & 0x00FF);
                            data[totalOffset + 1] = (byte)((blue & 0xFF00) >> 8);
                            data[totalOffset + 2] = (byte)(green & 0x00FF);
                            data[totalOffset + 3] = (byte)((green & 0xFF00) >> 8);
                            data[totalOffset + 4] = (byte)(red & 0x00FF);
                            data[totalOffset + 5] = (byte)((red & 0xFF00) >> 8);
                            break;
                        case MapMode.FoliageMap:
                            switch (type)
                            {
                                case DrawingType.Map:
                                    rgb = GetTiffRgbFromFourBitRgbPalette(FoliageMap[x, y]);
                                    red = rgb[0];
                                    green = rgb[1];
                                    blue = rgb[2];
                                    break;
                                case DrawingType.SelectedColor:
                                    byte singleColorData = Convert.ToByte(SelectedColorCode.Text, 16);
                                    rgb = GetTiffRgbFromFourBitRgbPalette(singleColorData);
                                    red = rgb[0];
                                    green = rgb[1];
                                    blue = rgb[2];
                                    break;
                                default:
                                    red = 0;
                                    green = 0;
                                    blue = 0;
                                    break;
                            }

                            data[totalOffset] = (byte)(blue & 0x00FF);
                            data[totalOffset + 1] = (byte)((blue & 0xFF00) >> 8);
                            data[totalOffset + 2] = (byte)(green & 0x00FF);
                            data[totalOffset + 3] = (byte)((green & 0xFF00) >> 8);
                            data[totalOffset + 4] = (byte)(red & 0x00FF);
                            data[totalOffset + 5] = (byte)((red & 0xFF00) >> 8);
                            break;
                        case MapMode.WallTextureMap:
                            switch (type)
                            {
                                case DrawingType.Map:
                                    rgb = GetTiffRgbFromFourBitRgbPalette(WallTextureMap[x, y]);
                                    red = rgb[0];
                                    green = rgb[1];
                                    blue = rgb[2];
                                    break;
                                case DrawingType.SelectedColor:
                                    byte singleColorData = Convert.ToByte(SelectedColorCode.Text, 16);
                                    rgb = GetTiffRgbFromFourBitRgbPalette(singleColorData);
                                    red = rgb[0];
                                    green = rgb[1];
                                    blue = rgb[2];
                                    break;
                                default:
                                    red = 0;
                                    green = 0;
                                    blue = 0;
                                    break;
                            }

                            data[totalOffset] = (byte)(blue & 0x00FF);
                            data[totalOffset + 1] = (byte)((blue & 0xFF00) >> 8);
                            data[totalOffset + 2] = (byte)(green & 0x00FF);
                            data[totalOffset + 3] = (byte)((green & 0xFF00) >> 8);
                            data[totalOffset + 4] = (byte)(red & 0x00FF);
                            data[totalOffset + 5] = (byte)((red & 0xFF00) >> 8);
                            break;
                        case MapMode.UnknownMap:
                            switch (type)
                            {
                                case DrawingType.Map:
                                    rgb = GetTiffRgbFromFourBitRgbPalette(UnknownMap[x, y]);
                                    red = rgb[0];
                                    green = rgb[1];
                                    blue = rgb[2];
                                    break;
                                case DrawingType.SelectedColor:
                                    byte singleColorData = Convert.ToByte(SelectedColorCode.Text, 16);
                                    rgb = GetTiffRgbFromFourBitRgbPalette(singleColorData);
                                    red = rgb[0];
                                    green = rgb[1];
                                    blue = rgb[2];
                                    break;
                                default:
                                    red = 0;
                                    green = 0;
                                    blue = 0;
                                    break;
                            }

                            data[totalOffset] = (byte)(blue & 0x00FF);
                            data[totalOffset + 1] = (byte)((blue & 0xFF00) >> 8);
                            data[totalOffset + 2] = (byte)(green & 0x00FF);
                            data[totalOffset + 3] = (byte)((green & 0xFF00) >> 8);
                            data[totalOffset + 4] = (byte)(red & 0x00FF);
                            data[totalOffset + 5] = (byte)((red & 0xFF00) >> 8);
                            break;
                        case MapMode.Full:
                            if (type != DrawingType.SelectedColor)
                            {
                                double highestDigit = Math.Pow(16, 1) * (HeightMapDigitsThreeAndFour[x, y] & 0x0F);
                                double middleDigit = Math.Pow(16, 0) * ((HeightMapDigitsOneAndTwo[x, y] & 0xF0) >> 4);
                                double smallestDigit = Math.Pow(16, -1) * (HeightMapDigitsOneAndTwo[x, y] & 0x0F);
                                double heightValue = (highestDigit + middleDigit + smallestDigit) / 2;

                                if (heightValue > WaterLevel)
                                {
                                    blue = 0xBA;
                                    green = 0xA9;
                                    red = 0x7C;
                                }
                                else
                                {
                                    red = 0x38;
                                    green = 0x6C;
                                    blue = 0x78;
                                }
                                blue = blue * 65535 / 255;
                                green = green * 65535 / 255;
                                red = red * 65535 / 255;

                                data[totalOffset] = (byte)(blue & 0x00FF);
                                data[totalOffset + 1] = (byte)((blue & 0xFF00) >> 8);
                                data[totalOffset + 2] = (byte)(green & 0x00FF);
                                data[totalOffset + 3] = (byte)((green & 0xFF00) >> 8);
                                data[totalOffset + 4] = (byte)(red & 0x00FF);
                                data[totalOffset + 5] = (byte)((red & 0xFF00) >> 8);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            return data;
        }

        private void DrawTiffImage(int width, int height, DrawingType type)
        {
            double dpi = 50;
            PixelFormat format = PixelFormats.Rgb48;
            int stride = ((width * format.BitsPerPixel) + 7) / 8;

            WriteableBitmap writableBitmap = new WriteableBitmap(width, height, dpi, dpi, format, null);
            writableBitmap.WritePixels(new Int32Rect(0, 0, width, height), CreateTiffData(width, height, type), stride, 0);

            // Encode as a TIFF
            TiffBitmapEncoder encoder = new TiffBitmapEncoder { Compression = TiffCompressOption.None };
            encoder.Frames.Add(BitmapFrame.Create(writableBitmap));

            // Convert to a bitmap
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);

                using (Bitmap map = new Bitmap(ms))
                {
                    //Set Origin Bottom Right
                    map.RotateFlip(RotateFlipType.RotateNoneFlipY);
                    map.RotateFlip(RotateFlipType.RotateNoneFlipX);

                    switch (type)
                    {
                        case DrawingType.Map:
                            Map.Source = GetBmpImageFromBmp(map);
                            break;
                        case DrawingType.SelectedColor:
                            SelectedColorImage.Source = GetBmpImageFromBmp(map);
                            break;
                    }
                }
            }
        }

        private void DrawAllMapObjects()
        {
            SolidBrush solidBrush;
            Bitmap allMapObjectLocationsBitmap = new Bitmap(MapHeight, MapWidth);

            for(int i = 0; i < MapObjectList.Count; i++)
            {
                switch (MapObjectList[i].Type)
                {
                    case OverlordObjectType.BrownMinionGate:
                        solidBrush = new SolidBrush(Color.FromArgb(255, 215, 183, 020));
                        //allMapObjectLocationsBitmap = DrawMinionGate(allMapObjectLocationsBitmap, MapObjectList[i].X, MapObjectList[i].Y, solidBrush);
                        break;
                    case OverlordObjectType.RedMinionGate:
                        solidBrush = new SolidBrush(Color.FromArgb(255, 255, 000, 000));
                        //allMapObjectLocationsBitmap = DrawMinionGate(allMapObjectLocationsBitmap, MapObjectList[i].X, MapObjectList[i].Y, solidBrush);
                        break;
                    case OverlordObjectType.GreenMinionGate:
                        solidBrush = new SolidBrush(Color.FromArgb(255, 000, 255, 000));
                        //allMapObjectLocationsBitmap = DrawMinionGate(allMapObjectLocationsBitmap, MapObjectList[i].X, MapObjectList[i].Y, solidBrush);
                        break;
                    case OverlordObjectType.BlueMinionGate:
                        solidBrush = new SolidBrush(Color.FromArgb(255, 000, 000, 255));
                        //allMapObjectLocationsBitmap = DrawMinionGate(allMapObjectLocationsBitmap, MapObjectList[i].X, MapObjectList[i].Y, solidBrush);
                        break;
                    case OverlordObjectType.TowerGate:
                        solidBrush = new SolidBrush(Color.FromArgb(255, 000, 000, 000));
                        //allMapObjectLocationsBitmap = DrawTowerGate(allMapObjectLocationsBitmap, MapObjectList[i].X, MapObjectList[i].Y, solidBrush);
                        break;
                    default:
                        solidBrush = new SolidBrush(Color.FromArgb(255, 000, 000, 000));
                        break;
                }
            }

            allMapObjectLocationsBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            allMapObjectLocationsBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);

            LocationMarkers.Source = GetBmpImageFromBmp(allMapObjectLocationsBitmap);
        }

        private Bitmap DrawMinionGate(Bitmap entireLocationBitmap, int x, int y, SolidBrush objectSolidColor)
        {
            int diameter = 7;

            using (Graphics locationGraphics = Graphics.FromImage(entireLocationBitmap))
            using (Pen objectPen = new Pen(objectSolidColor))
            {
                locationGraphics.DrawEllipse(objectPen, x - (diameter / 2), y - (diameter / 2), diameter, diameter);
                locationGraphics.FillEllipse(objectSolidColor, x - (diameter / 2), y - (diameter / 2), diameter, diameter);

                return entireLocationBitmap;
            }
        }

        private Bitmap DrawTowerGate(Bitmap entireLocationBitmap, int x, int y)
        {
            using (Graphics locationGraphics = Graphics.FromImage(entireLocationBitmap))
            using (Pen borderPen = new Pen(new SolidBrush(Color.FromArgb(255, 255, 255, 255))))
            using (Pen objectPen = new Pen(new SolidBrush(Color.FromArgb(255, 000, 000, 000))))
            {
				locationGraphics.DrawLine(objectPen, 0, 0, 0, 0);
                GraphicsPath path = new GraphicsPath();
                DrawingPoint point01 = new DrawingPoint(x - 14, y - 09);
                DrawingPoint point02 = new DrawingPoint(x - 13, y - 09);
                DrawingPoint point03 = new DrawingPoint(x - 13, y - 11);
                DrawingPoint point04 = new DrawingPoint(x - 12, y - 11);
                DrawingPoint point05 = new DrawingPoint(x - 12, y - 12);
                DrawingPoint point06 = new DrawingPoint(x - 10, y - 12);
                DrawingPoint point07 = new DrawingPoint(x - 10, y - 10);
                DrawingPoint point08 = new DrawingPoint(x - 09, y - 10);
                DrawingPoint point09 = new DrawingPoint(x - 09, y - 08);
                DrawingPoint point10 = new DrawingPoint(x - 08, y - 08);

                locationGraphics.DrawPath(objectPen, path);

                //locationGraphics.DrawRectangle(objectPen, new Rectangle(x, y, 28, 24));
                return entireLocationBitmap;
            }
        }

        private Bitmap DrawTowerGateVariant(Bitmap entireLocationBitmap, int x, int y)
        {
            using (Graphics locationGraphics = Graphics.FromImage(entireLocationBitmap))
            using (Pen borderPen = new Pen(new SolidBrush(Color.FromArgb(255, 255, 255, 255))))
            using (Pen objectPen = new Pen(new SolidBrush(Color.FromArgb(255, 000, 000, 000))))
            {
                return entireLocationBitmap;
            }
        }

        private void ToolClick(object sender, MouseButtonEventArgs e)
        {
            int xMouseCoordinate = 511 - (int)e.GetPosition(Map).X;
            int yMouseCoordinate = 511 - (int)e.GetPosition(Map).Y;

            if (CurrentMapMode != MapMode.Full)
            {
                switch (CurrentCursorMode)
                {
                    case CursorMode.Select:
                        MessageBox.Show("Location : X:" + xMouseCoordinate + " | Y:" + yMouseCoordinate);
                        break;
                    case CursorMode.Pipette:
                        switch (CurrentMapMode)
                        {
                            case MapMode.HeightMap:
                                SelectedColorCode.Text = HeightMapDigitsThreeAndFour[xMouseCoordinate, yMouseCoordinate].ToString("X2") + HeightMapDigitsOneAndTwo[xMouseCoordinate, yMouseCoordinate].ToString("X2");
                                break;
                            case MapMode.MainTextureMap:
                                SelectedColorCode.Text = MainTextureMap[xMouseCoordinate, yMouseCoordinate].ToString("X1");
                                break;
                            case MapMode.FoliageMap:
                                SelectedColorCode.Text = FoliageMap[xMouseCoordinate, yMouseCoordinate].ToString("X1");
                                break;
                            case MapMode.WallTextureMap:
                                SelectedColorCode.Text = WallTextureMap[xMouseCoordinate, yMouseCoordinate].ToString("X1");
                                break;
                            case MapMode.UnknownMap:
                                SelectedColorCode.Text = UnknownMap[xMouseCoordinate, yMouseCoordinate].ToString("X1");
                                break;
                            default:
                                SelectedColorCode.Text = "0000";
                                break;
                        }
                        break;
                    case CursorMode.Square:
                        if (SelectedColorCode.Text.Length == GetNeededColorCodeLength())
                        {
                            switch (CurrentMapMode)
                            {
                                case MapMode.HeightMap:
                                    EditMapData(xMouseCoordinate, yMouseCoordinate, HeightMapDigitsOneAndTwo, HeightMapDigitsThreeAndFour);
                                    break;
                                case MapMode.MainTextureMap:
                                    EditMapData(xMouseCoordinate, yMouseCoordinate, MainTextureMap);
                                    break;
                                case MapMode.FoliageMap:
                                    EditMapData(xMouseCoordinate, yMouseCoordinate, FoliageMap);
                                    break;
                                case MapMode.WallTextureMap:
                                    EditMapData(xMouseCoordinate, yMouseCoordinate, WallTextureMap);
                                    break;
                                case MapMode.UnknownMap:
                                    EditMapData(xMouseCoordinate, yMouseCoordinate, UnknownMap);
                                    break;
                            }
                            DrawTiffImage(MapWidth, MapHeight, DrawingType.Map);
                        }
                        else
                        {
                            MessageBox.Show("Error\nThe selected color has to be " + GetNeededColorCodeLength() + " digits long.");
                        }
                        break;
                    case CursorMode.Circle:
                        if (SelectedColorCode.Text.Length == GetNeededColorCodeLength())
                        {
                            switch (CurrentMapMode)
                            {
                                case MapMode.HeightMap:
                                    EditMapData(xMouseCoordinate, yMouseCoordinate, HeightMapDigitsOneAndTwo, HeightMapDigitsThreeAndFour);
                                    break;
                                case MapMode.MainTextureMap:
                                    EditMapData(xMouseCoordinate, yMouseCoordinate, MainTextureMap);
                                    break;
                                case MapMode.FoliageMap:
                                    EditMapData(xMouseCoordinate, yMouseCoordinate, FoliageMap);
                                    break;
                                case MapMode.WallTextureMap:
                                    EditMapData(xMouseCoordinate, yMouseCoordinate, WallTextureMap);
                                    break;
                                case MapMode.UnknownMap:
                                    EditMapData(xMouseCoordinate, yMouseCoordinate, UnknownMap);
                                    break;
                            }
                            DrawTiffImage(MapWidth, MapHeight, DrawingType.Map);
                        }
                        else
                        {
                            MessageBox.Show("Error\nThe selected color has to be " + GetNeededColorCodeLength() + " digits long.");
                        }
                        break;
                    case CursorMode.Rotate:
                        switch (CurrentMapMode)
                        {
                            case MapMode.HeightMap:
                                RotateMapData(HeightMapDigitsOneAndTwo);
                                RotateMapData(HeightMapDigitsThreeAndFour);
                                break;
                            case MapMode.MainTextureMap:
                                RotateMapData(MainTextureMap);
                                break;
                            case MapMode.FoliageMap:
                                RotateMapData(FoliageMap);
                                break;
                            case MapMode.WallTextureMap:
                                RotateMapData(WallTextureMap);
                                break;
                            case MapMode.UnknownMap:
                                RotateMapData(UnknownMap);
                                break;
                        }
                        DrawTiffImage(MapWidth, MapHeight, DrawingType.Map);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                MessageBox.Show("Location : X:" + xMouseCoordinate + " | Y:" + yMouseCoordinate);
            }
        }

        private byte[] GetByteArrayFromHexString(string fullHexString)
        {
            byte[] tempByteArray = new byte[2];

            tempByteArray[0] = Convert.ToByte("" + fullHexString[2] + fullHexString[3], 16);
            tempByteArray[1] = Convert.ToByte("" + fullHexString[0] + fullHexString[1], 16);

            return tempByteArray;
        }

        private void MapModeChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (MapModeDropDown.SelectedIndex)
            {
                case 0:
                    CurrentMapMode = MapMode.HeightMap;
                    UpdateCursor();
                    SelectedColorCode.Text = "0000";
                    DrawTiffImage(MapWidth, MapHeight, DrawingType.Map);
                    DrawTiffImage((int)SelectedColorImage.Width, (int)SelectedColorImage.Height, DrawingType.SelectedColor);
                    ShowImportExportButtons();
                    UpdateToolBar();
                    break;
                case 1:
                    CurrentMapMode = MapMode.MainTextureMap;
                    UpdateCursor();
                    SelectedColorCode.Text = "0";
                    DrawTiffImage(MapWidth, MapHeight, DrawingType.Map);
                    DrawTiffImage((int)SelectedColorImage.Width, (int)SelectedColorImage.Height, DrawingType.SelectedColor);
                    ShowImportExportButtons();
                    UpdateToolBar();
                    break;
                case 2:
                    CurrentMapMode = MapMode.FoliageMap;
                    UpdateCursor();
                    SelectedColorCode.Text = "0";
                    DrawTiffImage(MapWidth, MapHeight, DrawingType.Map);
                    DrawTiffImage((int)SelectedColorImage.Width, (int)SelectedColorImage.Height, DrawingType.SelectedColor);
                    ShowImportExportButtons();
                    UpdateToolBar();
                    break;
                case 3:
                    CurrentMapMode = MapMode.WallTextureMap;
                    UpdateCursor();
                    SelectedColorCode.Text = "0";
                    DrawTiffImage(MapWidth, MapHeight, DrawingType.Map);
                    DrawTiffImage((int)SelectedColorImage.Width, (int)SelectedColorImage.Height, DrawingType.SelectedColor);
                    ShowImportExportButtons();
                    UpdateToolBar();
                    break;
                case 4:
                    CurrentMapMode = MapMode.UnknownMap;
                    UpdateCursor();
                    SelectedColorCode.Text = "0";
                    DrawTiffImage(MapWidth, MapHeight, DrawingType.Map);
                    DrawTiffImage((int)SelectedColorImage.Width, (int)SelectedColorImage.Height, DrawingType.SelectedColor);
                    ShowImportExportButtons();
                    UpdateToolBar();
                    break;
                case 5:
                    CurrentMapMode = MapMode.Full;
                    Mouse.OverrideCursor = null;
                    DrawTiffImage(MapWidth, MapHeight, DrawingType.Map);
                    HideImportExportButtons();
                    HideSelectedColor();
                    HideCursorModes();
                    HideCursorSubModes();
                    HideCursorSlider();

                    break;
                default:
                    break;
            }

            if (CurrentCursorMode == CursorMode.Square)
            {
                UpdateCursor();
            }
        }

        private string LeaveOnlyHexNumbers(string inString)
        {
            foreach (char character in inString.ToCharArray())
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(character.ToString(), "^[0-9a-fA-F]*$"))
                {
                    inString = inString.Replace(character.ToString(), "");
                }
            }

            return inString;
        }

        private int GetNeededColorCodeLength()
        {
            switch (CurrentMapMode)
            {
                case MapMode.HeightMap:
                    return 4;
                case MapMode.MainTextureMap:
                    return 1;
                case MapMode.FoliageMap:
                    return 1;
                case MapMode.WallTextureMap:
                    return 1;
                case MapMode.UnknownMap:
                    return 1;
                default:
                    return 1;
            }
        }

        private void SelectedColorCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            SelectedColorCode.Text = LeaveOnlyHexNumbers(SelectedColorCode.Text);

            if (SelectedColorCode.Text.Length == GetNeededColorCodeLength())
            {
                DrawTiffImage((int)SelectedColorImage.Width, (int)SelectedColorImage.Height, DrawingType.SelectedColor);

                if (CurrentMapMode == MapMode.HeightMap)
                {
                    int digitOne = Convert.ToInt32("" + SelectedColorCode.Text[3], 16);
                    int digitTwo = Convert.ToInt32("" + SelectedColorCode.Text[2], 16);
                    int digitThree = Convert.ToInt32("" + SelectedColorCode.Text[1], 16);
                    SelectedColorHeight.Content = "This color corresponds to a height of\r\n" + (((digitThree * Math.Pow(16, 1)) + (digitTwo * Math.Pow(16, 0)) + (digitOne * Math.Pow(16, -1))) / 2);
                }

                if (CurrentCursorMode == CursorMode.Square)
                {
                    UpdateCursor();
                }
            }
        }

        private void SelectedColorCode_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.Back && SelectedColorCode.SelectionStart > 0)
            //{
            //    string insertText = "0";
            //    int selectionIndex = SelectedColorCode.SelectionStart;
            //    SelectedColorCode.Text = SelectedColorCode.Text.Insert(selectionIndex, insertText);
            //    SelectedColorCode.SelectionStart = selectionIndex; // restore cursor position
            //}
            //if (e.Key == Key.Delete && SelectedColorCode.SelectionStart < SelectedColorCode.MaxLength)
            //{
            //    string insertText = "0";
            //    int selectionIndex = SelectedColorCode.SelectionStart;
            //    SelectedColorCode.Text = SelectedColorCode.Text.Insert(selectionIndex, insertText);
            //    SelectedColorCode.SelectionStart = selectionIndex; // restore cursor position
            //}
        }

        private void CursorMode_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            CurrentCursorMode = (CursorMode)Enum.Parse(typeof(CursorMode), (string)button.Content);
            UpdateCursor();
            UpdateToolBar();
        }

        private void CursorSubMode_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            CurrentCursorSubMode = (CursorSubMode)Enum.Parse(typeof(CursorSubMode), (string)button.Content);
            UpdateToolBar();
        }

        private void UpdateCursor()
        {
            SolidColorBrush fillBrush;
            switch (CurrentCursorMode)
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
                switch (CurrentCursorMode)
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

        private void HideImportExportButtons()
        {
            ImportMapData.Visibility = Visibility.Hidden;
            ImportMapImage.Visibility = Visibility.Hidden;
            ExportMapData.Visibility = Visibility.Hidden;
            ExportMapImage.Visibility = Visibility.Hidden;
        }

        private void ShowImportExportButtons()
        {
            ImportMapData.Visibility = Visibility.Visible;
            ImportMapImage.Visibility = Visibility.Visible;
            ExportMapData.Visibility = Visibility.Visible;
            ExportMapImage.Visibility = Visibility.Visible;
        }

        private void HideCursorModes()
        {
            cursorModeSelect.Visibility = Visibility.Hidden;
            cursorModePipette.Visibility = Visibility.Hidden;
            cursorModeSquare.Visibility = Visibility.Hidden;
            cursorModeCircle.Visibility = Visibility.Hidden;
            cursorModeRotate.Visibility = Visibility.Hidden;
        }

        private void ShowCursorModes()
        {
            cursorModeSelect.Visibility = Visibility.Visible;
            cursorModePipette.Visibility = Visibility.Visible;
            cursorModeSquare.Visibility = Visibility.Visible;
            cursorModeCircle.Visibility = Visibility.Visible;
            cursorModeRotate.Visibility = Visibility.Visible;
        }

        private void HideCursorSubModes()
        {
            cursorSubModeSet.Visibility = Visibility.Hidden;
            cursorSubModeAdd.Visibility = Visibility.Hidden;
            cursorSubModeSub.Visibility = Visibility.Hidden;
        }

        private void ShowCursorSubModes()
        {
            cursorSubModeSet.Visibility = Visibility.Visible;
            cursorSubModeAdd.Visibility = Visibility.Visible;
            cursorSubModeSub.Visibility = Visibility.Visible;
        }

        private void HideSelectedColor()
        {
            SelectedColorCode.Visibility = Visibility.Hidden;
            SelectedColorImage.Visibility = Visibility.Hidden;
            SelectedColorBorder.Visibility = Visibility.Hidden;
            SelectedColorHeight.Visibility = Visibility.Hidden;
        }

        private void ShowSelectedColor()
        {
            SelectedColorCode.Visibility = Visibility.Visible;
            SelectedColorImage.Visibility = Visibility.Visible;
            SelectedColorBorder.Visibility = Visibility.Visible;
            SelectedColorHeight.Visibility = CurrentMapMode != MapMode.HeightMap ? Visibility.Hidden : Visibility.Visible;
        }

        private void HideCursorSlider()
        {
            CursorSizeSlider.Visibility = Visibility.Hidden;
            cursorDiameterLabel.Visibility = Visibility.Hidden;
        }

        private void ShowCursorSlider()
        {
            CursorSizeSlider.Visibility = Visibility.Visible;
            cursorDiameterLabel.Visibility = Visibility.Visible;
        }

        private void HighlightCurrentCursorMode()
        {
            switch (CurrentCursorMode)
            {
                case CursorMode.Select:
                    cursorModeSelect.Background = MediaBrushes.SkyBlue;
                    cursorModePipette.Background = new SolidColorBrush(MediaColor.FromRgb(0xDD, 0xDD, 0xDD));
                    cursorModeSquare.Background = new SolidColorBrush(MediaColor.FromRgb(0xDD, 0xDD, 0xDD));
                    cursorModeCircle.Background = new SolidColorBrush(MediaColor.FromRgb(0xDD, 0xDD, 0xDD));
                    cursorModeRotate.Background = new SolidColorBrush(MediaColor.FromRgb(0xDD, 0xDD, 0xDD));
                    break;
                case CursorMode.Pipette:
                    cursorModeSelect.Background = new SolidColorBrush(MediaColor.FromRgb(0xDD, 0xDD, 0xDD));
                    cursorModePipette.Background = MediaBrushes.SkyBlue;
                    cursorModeSquare.Background = new SolidColorBrush(MediaColor.FromRgb(0xDD, 0xDD, 0xDD));
                    cursorModeCircle.Background = new SolidColorBrush(MediaColor.FromRgb(0xDD, 0xDD, 0xDD));
                    cursorModeRotate.Background = new SolidColorBrush(MediaColor.FromRgb(0xDD, 0xDD, 0xDD));
                    break;
                case CursorMode.Square:
                    cursorModeSelect.Background = new SolidColorBrush(MediaColor.FromRgb(0xDD, 0xDD, 0xDD));
                    cursorModePipette.Background = new SolidColorBrush(MediaColor.FromRgb(0xDD, 0xDD, 0xDD));
                    cursorModeSquare.Background = MediaBrushes.SkyBlue;
                    cursorModeCircle.Background = new SolidColorBrush(MediaColor.FromRgb(0xDD, 0xDD, 0xDD));
                    cursorModeRotate.Background = new SolidColorBrush(MediaColor.FromRgb(0xDD, 0xDD, 0xDD));
                    break;
                case CursorMode.Circle:
                    cursorModeSelect.Background = new SolidColorBrush(MediaColor.FromRgb(0xDD, 0xDD, 0xDD));
                    cursorModePipette.Background = new SolidColorBrush(MediaColor.FromRgb(0xDD, 0xDD, 0xDD));
                    cursorModeSquare.Background = new SolidColorBrush(MediaColor.FromRgb(0xDD, 0xDD, 0xDD));
                    cursorModeCircle.Background = MediaBrushes.SkyBlue;
                    cursorModeRotate.Background = new SolidColorBrush(MediaColor.FromRgb(0xDD, 0xDD, 0xDD));
                    break;
                case CursorMode.Rotate:
                    cursorModeSelect.Background = new SolidColorBrush(MediaColor.FromRgb(0xDD, 0xDD, 0xDD));
                    cursorModePipette.Background = new SolidColorBrush(MediaColor.FromRgb(0xDD, 0xDD, 0xDD));
                    cursorModeSquare.Background = new SolidColorBrush(MediaColor.FromRgb(0xDD, 0xDD, 0xDD));
                    cursorModeCircle.Background = new SolidColorBrush(MediaColor.FromRgb(0xDD, 0xDD, 0xDD));
                    cursorModeRotate.Background = MediaBrushes.SkyBlue;
                    break;
            }
        }

        private void HighlightCurrentCursorSubMode()
        {
            switch (CurrentCursorSubMode)
            {
                case CursorSubMode.Set:
                    cursorSubModeSet.Background = MediaBrushes.SkyBlue;
                    cursorSubModeAdd.Background = new SolidColorBrush(MediaColor.FromRgb(0xDD, 0xDD, 0xDD));
                    cursorSubModeSub.Background = new SolidColorBrush(MediaColor.FromRgb(0xDD, 0xDD, 0xDD));
                    break;
                case CursorSubMode.Add:
                    cursorSubModeSet.Background = new SolidColorBrush(MediaColor.FromRgb(0xDD, 0xDD, 0xDD));
                    cursorSubModeAdd.Background = MediaBrushes.SkyBlue;
                    cursorSubModeSub.Background = new SolidColorBrush(MediaColor.FromRgb(0xDD, 0xDD, 0xDD));
                    break;
                case CursorSubMode.Sub:
                    cursorSubModeSet.Background = new SolidColorBrush(MediaColor.FromRgb(0xDD, 0xDD, 0xDD));
                    cursorSubModeAdd.Background = new SolidColorBrush(MediaColor.FromRgb(0xDD, 0xDD, 0xDD));
                    cursorSubModeSub.Background = MediaBrushes.SkyBlue;
                    break;
            }
        }

        private void UpdateToolBar()
        {
            HighlightCurrentCursorMode();
            HighlightCurrentCursorSubMode();
            switch (CurrentCursorMode)
            {
                case CursorMode.Select:
                    ShowCursorModes();
                    HideCursorSubModes();
                    HideSelectedColor();
                    HideCursorSlider();
                    break;
                case CursorMode.Pipette:
                    ShowCursorModes();
                    HideCursorSubModes();
                    ShowSelectedColor();
                    HideCursorSlider();
                    break;
                case CursorMode.Square:
                    ShowCursorModes();
                    ShowCursorSubModes();
                    ShowSelectedColor();
                    ShowCursorSlider();
                    break;
                case CursorMode.Circle:
                    ShowCursorModes();
                    ShowCursorSubModes();
                    ShowSelectedColor();
                    ShowCursorSlider();
                    break;
                case CursorMode.Rotate:
                    ShowCursorModes();
                    HideCursorSubModes();
                    HideSelectedColor();
                    HideCursorSlider();
                    break;
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsAnyMapLoaded)
            {
                cursorDiameterLabel.Content = "Cursor Diameter: " + CursorSizeSlider.Value;
                CursorDiameter = (int) CursorSizeSlider.Value;
                if (CurrentCursorMode == CursorMode.Square || CurrentCursorMode == CursorMode.Circle)
                {
                    UpdateCursor();
                }
            }
        }
    }
}
