using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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

namespace Overlord_Map_Visualizer
{
    public partial class MainWindow : Window
    {
        private enum ColorFormat
        {
            RGB555,
            RGB555Flipped,
            RGB565,
            RGB565Flipped,
            BGR555,
            BGR555Flipped,
            BGR565,
            BGR565Flipped,
            RGBA4444,
            RGBA4444Flipped,
            ARGB4444,
            ARGB4444Flipped,
            Gray16,
            RGB888,
            RGB888Flipped,
            BGR888,
            BGR888Flipped,
            Gray12
        }

        private enum MapMode
        {
            HeightMap,
            TextureDistributionMap,
            Full
        }

        private enum CursorMode
        {
            Normal,
            Pipette,
            Square,
            Rotate
        }

        private enum DrawingType
        {
            Map,
            SelectedColor
        }

        private MapMode currentMapMode;
        private readonly int MapWidth = 512;
        private readonly int MapHeight = 512;
        private string OMPFilePathString;

        private byte[,] HeightMapDigitsOneAndTwo;
        private byte[,] HeightMapDigitsThreeAndFour;
        private byte[,] TextureDistributionDigitsOneAndTwo;
        private byte[,] TextureDistributionDigitsThreeAndFour;

        private bool IsAnyMapLoaded = false;

        private CursorMode currentCursorMode;
        private int cursorDiameter = 50;

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

            TextureDistributionDigitsOneAndTwo = new byte[MapWidth, MapHeight];
            TextureDistributionDigitsThreeAndFour = new byte[MapWidth, MapHeight];

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
                mapGraphics.DrawLine(coordinatePen, 13, 550, 18, 555);
                mapGraphics.DrawLine(coordinatePen, 18, 36, 18, 555);
                mapGraphics.DrawLine(coordinatePen, 23, 550, 18, 555);

                //X Axis Arrow
                mapGraphics.DrawLine(coordinatePen, 531, 41, 536, 36);
                mapGraphics.DrawLine(coordinatePen, 18, 36, 536, 36);
                mapGraphics.DrawLine(coordinatePen, 531, 31, 536, 36);

                //Y Axis Marker
                for (int y = 36; y < 568; y += 50)
                {
                    mapGraphics.DrawLine(coordinatePen, 9, y, 18, y);
                }

                //X Axis Marker
                for (int x = 18; x < 550; x += 50)
                {
                    mapGraphics.DrawLine(coordinatePen, x, 27, x, 36);
                }

                CreateNewLabel("CoordMarkerOrigin", "0", 274, 577);

                CreateNewLabel("CoordMarkerX", "X", 811, 562);

                for (int i = 1; i <= 10; i++)
                {
                    string content = "" + i * 50;
                    if (i == 1)
                    {
                        content = " " + content;
                    }
                    CreateNewLabel("CoordMarkerX" + i, content, 278 + (i * 50), 577);
                }

                CreateNewLabel("CoordMarkerY", "Y", 284, 35);

                for (int i = 1; i <= 10; i++)
                {
                    string content = "" + i * 50;
                    if (i == 1)
                    {
                        content = "  " + content;
                    }
                    CreateNewLabel("CoordMarkerY" + i, content, 257, 561 - (i * 50));
                }

                //Set Origin Bottom Left
                coordinateSystem.RotateFlip(RotateFlipType.RotateNoneFlipY);

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

        private void ImportfromFile(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            OpenFileDialog openFileDialog;
            string dialogTitle;
            string fileExtension;
            string filter;

            switch (button.Name)
            {
                case string a when a.Equals("ImportfromOMPFileButton"):
                    dialogTitle = "Browse OMP map files";
                    fileExtension = "omp";
                    filter = "omp files (*.omp)|*.omp";
                    break;
                case string a when a.Equals("ImportMapButton"):
                    dialogTitle = "Browse overlord map data";
                    fileExtension = "mapdata";
                    filter = "mapdata files (*.mapdata)|*.mapdata";
                    break;
                default:
                    dialogTitle = "";
                    fileExtension = "";
                    filter = "";
                    break;
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
                if(fileExtension == "omp")
                {
                    FilePath.Text = openFileDialog.FileName;
                    OMPFilePathString = openFileDialog.FileName;
                    GetMapData(GetMapDataOffset(), openFileDialog.FileName, 4, MapMode.Full);
                }
                else
                {
                    GetMapData(0, openFileDialog.FileName, 2, currentMapMode);
                }

                Render();

                if (!IsAnyMapLoaded)
                {
                    MapModeDropDown.SelectedIndex = 0;
                    MapModeDropDown.IsEnabled = true;
                    ImportMapButton.IsEnabled = true;
                    ExportMapButton.IsEnabled = true;
                    ExportToOMPFileButton.IsEnabled = true;
                    SelectedColorCode.IsEnabled = true;
                    SelectedColorCode.Visibility = Visibility.Visible;
                    SelectedColorImage.Visibility = Visibility.Visible;
                    SelectedColorBorder.Visibility = Visibility.Visible;
                    SelectedColorHeight.Visibility = Visibility.Visible;
                    cursorModeSelect.Visibility = Visibility.Visible;
                    cursorModePipette.Visibility = Visibility.Visible;
                    cursorModeSquare.Visibility = Visibility.Visible;
                    cursorModeRotate.Visibility = Visibility.Visible;
                    IsAnyMapLoaded = true;
                }
            }
        }

        private void GetMapData(int OffsetMap, string filePath, int bytesPerPoint, MapMode mapMode)
        {
            int totalNumberOfBytes = MapWidth * MapHeight * bytesPerPoint;
            byte[] allMapBytes = new byte[totalNumberOfBytes];
            int xOffset = bytesPerPoint;
            int yOffset = 0;
            int numberOfBytesInRow = MapWidth * bytesPerPoint;
            int totalOffset;

            using (BinaryReader reader = new BinaryReader(new FileStream(filePath, FileMode.Open)))
            {
                reader.BaseStream.Seek(OffsetMap, SeekOrigin.Begin);
                reader.Read(allMapBytes, 0, totalNumberOfBytes);
            }

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
                            HeightMapDigitsOneAndTwo[x, y] = allMapBytes[totalOffset];
                            HeightMapDigitsThreeAndFour[x, y] = allMapBytes[totalOffset + 1];
                            break;
                        case MapMode.TextureDistributionMap:
                            TextureDistributionDigitsOneAndTwo[x, y] = allMapBytes[totalOffset];
                            TextureDistributionDigitsThreeAndFour[x, y] = allMapBytes[totalOffset + 1];
                            break;
                        case MapMode.Full:
                            HeightMapDigitsOneAndTwo[x, y] = allMapBytes[totalOffset];
                            HeightMapDigitsThreeAndFour[x, y] = allMapBytes[totalOffset + 1];
                            TextureDistributionDigitsOneAndTwo[x, y] = allMapBytes[totalOffset + 2];
                            TextureDistributionDigitsThreeAndFour[x, y] = allMapBytes[totalOffset + 3];
                            break;
                    }
                }
            }
        }

        private void ExportToFile(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            if(button.Name == "ExportToOMPFileButton")
            {
                WriteMapData(GetMapDataOffset(), OMPFilePathString, 4, MapMode.Full);
            }
            else if (button.Name == "ExportMapButton")
            {
                SaveFileDialog saveFileDialog;

                saveFileDialog = new SaveFileDialog
                {
                    InitialDirectory = @"C:\",
                    RestoreDirectory = true,
                    Title = "Select Directory and file name for the overlord map data",
                    DefaultExt = "mapdata",
                    Filter = "mapdata files (*.mapdata)|*.mapdata"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    WriteMapData(0, saveFileDialog.FileName, 2, currentMapMode);
                }
            }
        }

        private void WriteMapData(int OffsetMap, string filePath, int bytesPerPoint, MapMode mapMode)
        {
            int totalNumberOfBytes = MapWidth * MapHeight * bytesPerPoint;
            byte[] allMapBytes = new byte[totalNumberOfBytes];
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
                            allMapBytes[totalOffset] = HeightMapDigitsOneAndTwo[x, y];
                            allMapBytes[totalOffset + 1] = HeightMapDigitsThreeAndFour[x, y];
                            break;
                        case MapMode.TextureDistributionMap:
                            allMapBytes[totalOffset] = TextureDistributionDigitsOneAndTwo[x, y];
                            allMapBytes[totalOffset + 1] = TextureDistributionDigitsThreeAndFour[x, y];
                            break;
                        case MapMode.Full:
                            allMapBytes[totalOffset] = HeightMapDigitsOneAndTwo[x, y];
                            allMapBytes[totalOffset + 1] = HeightMapDigitsThreeAndFour[x, y];
                            allMapBytes[totalOffset + 2] = TextureDistributionDigitsOneAndTwo[x, y];
                            allMapBytes[totalOffset + 3] = TextureDistributionDigitsThreeAndFour[x, y];
                            break;
                    }
                }
            }

            using (BinaryWriter writer = new BinaryWriter(new FileStream(filePath, FileMode.OpenOrCreate)))
            {
                writer.BaseStream.Seek(OffsetMap, SeekOrigin.Begin);
                writer.Write(allMapBytes, 0, totalNumberOfBytes);
            }
        }

        private int GetMapDataOffset()
        {
            switch (OMPFilePathString)
            {
                case string a when a.Contains("Exp - HalflingMain"):
                    return 408;
                case string a when a.Contains("Exp - Halfling Abyss"):
                    return 394;
                case string a when a.Contains("Exp - ElfMain"):
                    return 416;
                case string a when a.Contains("Exp - Elf Abyss"):
                    return 378;
                case string a when a.Contains("Exp - PaladinMain"):
                    return 405;
                case string a when a.Contains("Exp - Paladin Abyss"):
                    return 405;
                case string a when a.Contains("Exp - DwarfMain"):
                    return 386;
                case string a when a.Contains("Exp - Dwarf Abyss"):
                    return 403;
                case string a when a.Contains("Exp - WarriorMain"):
                    return 378;
                case string a when a.Contains("Exp - Warrior Abyss - 01"):
                    return 416;
                case string a when a.Contains("Exp - Warrior Abyss - 02"):
                    return 400;
                case string a when a.Contains("Exp - Tower"):
                    return 409;
                case string a when a.Contains("Exp - Tower_Dungeon"):
                    return 403;
                case string a when a.Contains("Exp - Tower_Spawnpit"):
                    return 424;
                case string a when a.Contains("HalflingMain"):
                    return 400;
                case string a when a.Contains("SlaveCamp"):
                    return 378;
                case string a when a.Contains("HalflingHomes1of2"):
                    return 378;
                case string a when a.Contains("HalflingHomes2of2"):
                    return 378;
                case string a when a.Contains("HellsKitchen"):
                    return 378;
                case string a when a.Contains("EntryCastleSpree"):
                    return 402;
                case string a when a.Contains("SpreeDungeon"):
                    return 386;
                case string a when a.Contains("ElfMain"):
                    return 408;
                case string a when a.Contains("GreenCave"):
                    return 394;
                case string a when a.Contains("SkullDen"):
                    return 390;
                case string a when a.Contains("TrollTemple"):
                    return 386;
                case string a when a.Contains("PaladinMain"):
                    return 397;
                case string a when a.Contains("BlueCave"):
                    return 409;
                case string a when a.Contains("Sewers1of2"):
                    return 410;
                case string a when a.Contains("Sewers2of2"):
                    return 408;
                case string a when a.Contains("Red Light Inn"):
                    return 403;
                case string a when a.Contains("Citadel"):
                    return 407;
                case string a when a.Contains("DwarfMain"):
                    return 378;
                case string a when a.Contains("GoldMine"):
                    return 378;
                case string a when a.Contains("Quarry"):
                    return 378;
                case string a when a.Contains("HomeyHalls1of2"):
                    return 370;
                case string a when a.Contains("HomeyHalls2of2"):
                    return 370;
                case string a when a.Contains("ArcaniumMine"):
                    return 370;
                case string a when a.Contains("RoyalHalls"):
                    return 370;
                case string a when a.Contains("WarriorMain"):
                    return 370;
                case string a when a.Contains("2P_Deathtrap"):
                    return 501;
                case string a when a.Contains("2P_Gates"):
                    return 488;
                case string a when a.Contains("2P_LastStand"):
                    return 464;
                case string a when a.Contains("2P_PartyCrashers"):
                    return 469;
                case string a when a.Contains("2P_Plunder"):
                    return 505;
                case string a when a.Contains("2P_TombRobber"):
                    return 505;
                case string a when a.Contains("Tower"):
                    return 401;
                case string a when a.Contains("Tower_Dungeon"):
                    return 395;
                case string a when a.Contains("Tower_Spawnpit"):
                    return 424;
                case string a when a.Contains("PlayerMap"):
                    return 250;
                case string a when a.Contains("2P_Arena2"):
                    return 492;
                case string a when a.Contains("2P_Bombs"):
                    return 523;
                case string a when a.Contains("2P_GrabTheMaidens"):
                    return 496;
                case string a when a.Contains("2P_KillTheHoard"):
                    return 500;
                case string a when a.Contains("2P_KingoftheHill"):
                    return 479;
                case string a when a.Contains("2P_March_Mellow_Maidens"):
                    return 514;
                case string a when a.Contains("2P_Misty"):
                    return 481;
                case string a when a.Contains("2P_RockyRace"):
                    return 458;
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

        private void Render()
        {
            switch (currentMapMode)
            {
                case MapMode.HeightMap:
                    DrawTiffImage(MapWidth,MapHeight,CreateTiffData(MapWidth,MapHeight, HeightMapDigitsOneAndTwo, HeightMapDigitsThreeAndFour), DrawingType.Map);
                    break;
                case MapMode.TextureDistributionMap:
                    DrawTiffImage(MapWidth, MapHeight, CreateTiffData(MapWidth, MapHeight, TextureDistributionDigitsOneAndTwo, TextureDistributionDigitsThreeAndFour), DrawingType.Map);
                    break;
                case MapMode.Full:
                    DrawTiffImage(MapWidth, MapHeight, CreateTiffData(MapWidth, MapHeight), DrawingType.Map);
                    break;
                default:
                    break;
            }

        }

        private byte[] CreateTiffData(int width, int height)
        {
            int red;
            int blue;
            int green;
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
                    double highestDigit = Math.Pow(16, 1) * (HeightMapDigitsThreeAndFour[x, y] & 0x0F);
                    double middleDigit = Math.Pow(16, 0) * ((HeightMapDigitsOneAndTwo[x, y] & 0xF0) >> 4);
                    double smallestDigit = Math.Pow(16, -1) * (HeightMapDigitsOneAndTwo[x, y] & 0x0F);
                    double heightValue = (highestDigit + middleDigit + smallestDigit) / 2;

                    if (heightValue >= 15.625)
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
            }
            return data;
        }

        private byte[] CreateTiffData(int width, int height, byte[,] lowerByteData, byte[,] higherByteData)
        {
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
                    switch (currentMapMode)
                    {
                        case MapMode.HeightMap:
                            int grayScale = ((higherByteData[x, y] << 8) & 0x0FFF) + lowerByteData[x, y];
                            grayScale = grayScale * 65535 / 4095;

                            data[totalOffset] = (byte)(grayScale & 0x00FF);
                            data[totalOffset + 1] = (byte)((grayScale & 0xFF00) >> 8);
                            data[totalOffset + 2] = (byte)(grayScale & 0x00FF);
                            data[totalOffset + 3] = (byte)((grayScale & 0xFF00) >> 8);
                            data[totalOffset + 4] = (byte)(grayScale & 0x00FF);
                            data[totalOffset + 5] = (byte)((grayScale & 0xFF00) >> 8);
                            break;
                        case MapMode.TextureDistributionMap:
                            int blue = lowerByteData[x,y] & 0x1F;
                            int green = ((higherByteData[x, y] << 3) & 0x38) | ((lowerByteData[x, y] >> 5) & 0x07);
                            int red = (higherByteData[x, y] >> 3) & 0x1F;

                            blue = blue * 65535 / 31;
                            green = green * 65535 / 63;
                            red = red * 65535 / 31;

                            data[totalOffset] = (byte)(blue & 0x00FF);
                            data[totalOffset + 1] = (byte)((blue & 0xFF00) >> 8);
                            data[totalOffset + 2] = (byte)(green & 0x00FF);
                            data[totalOffset + 3] = (byte)((green & 0xFF00) >> 8);
                            data[totalOffset + 4] = (byte)(red & 0x00FF);
                            data[totalOffset + 5] = (byte)((red & 0xFF00) >> 8);
                            break;
                        default:
                            break;
                    }
                }
            }
            return data;
        }

        private byte[] CreateTiffData(int width, int height, byte[] singleColorData)
        {
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
                    switch (currentMapMode)
                    {
                        case MapMode.HeightMap:
                            int grayScale = ((singleColorData[1] << 8) & 0x0FFF) + singleColorData[0];
                            grayScale = grayScale * 65535 / 4095;

                            data[totalOffset] = (byte)(grayScale & 0x00FF);
                            data[totalOffset + 1] = (byte)((grayScale & 0xFF00) >> 8);
                            data[totalOffset + 2] = (byte)(grayScale & 0x00FF);
                            data[totalOffset + 3] = (byte)((grayScale & 0xFF00) >> 8);
                            data[totalOffset + 4] = (byte)(grayScale & 0x00FF);
                            data[totalOffset + 5] = (byte)((grayScale & 0xFF00) >> 8);
                            break;
                        case MapMode.TextureDistributionMap:
                            int blue = singleColorData[0] & 0x1F;
                            int green = ((singleColorData[1] << 3) & 0x38) | ((singleColorData[0] >> 5) & 0x07);
                            int red = (singleColorData[1] >> 3) & 0x1F;

                            blue = blue * 65535 / 31;
                            green = green * 65535 / 63;
                            red = red * 65535 / 31;

                            data[totalOffset] = (byte)(blue & 0x00FF);
                            data[totalOffset + 1] = (byte)((blue & 0xFF00) >> 8);
                            data[totalOffset + 2] = (byte)(green & 0x00FF);
                            data[totalOffset + 3] = (byte)((green & 0xFF00) >> 8);
                            data[totalOffset + 4] = (byte)(red & 0x00FF);
                            data[totalOffset + 5] = (byte)((red & 0xFF00) >> 8);
                            break;
                        default:
                            break;
                    }

                }
            }
            return data;
        }

        private void DrawTiffImage(int width, int height, byte[] data, DrawingType type)
        {
            double dpi = 50;
            PixelFormat format = PixelFormats.Rgb48;
            int stride = ((width * format.BitsPerPixel) + 7) / 8;

            WriteableBitmap wb = new WriteableBitmap(width, height, dpi, dpi, format, null);
            wb.WritePixels(new Int32Rect(0, 0, width, height), data, stride, 0);

            // Encode as a TIFF
            TiffBitmapEncoder enc = new TiffBitmapEncoder { Compression = TiffCompressOption.None };
            enc.Frames.Add(BitmapFrame.Create(wb));

            // Convert to a bitmap
            using (MemoryStream ms = new MemoryStream())
            {
                enc.Save(ms);

                using (Bitmap map = new Bitmap(ms))
                {
                    //Set Origin Bottom Left
                    map.RotateFlip(RotateFlipType.RotateNoneFlipY);

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

        private void ToolClick(object sender, MouseButtonEventArgs e)
        {
            int xCoordinate = (int)e.GetPosition(Map).X;
            int yCoordinate = 512 - (int)e.GetPosition(Map).Y;

            switch (currentCursorMode)
            {
                case CursorMode.Normal:
                    MessageBox.Show("Location : X:" + xCoordinate + " | Y:" + yCoordinate);
                    break;
                case CursorMode.Pipette:
                    switch (currentMapMode)
                    {
                        case MapMode.HeightMap:
                            SelectedColorCode.Text = HeightMapDigitsThreeAndFour[xCoordinate, yCoordinate].ToString("X2") + HeightMapDigitsOneAndTwo[xCoordinate, yCoordinate].ToString("X2");
                            break;
                        case MapMode.TextureDistributionMap:
                            SelectedColorCode.Text = TextureDistributionDigitsThreeAndFour[xCoordinate, yCoordinate].ToString("X2") + TextureDistributionDigitsOneAndTwo[xCoordinate, yCoordinate].ToString("X2");
                            break;
                        default:
                            SelectedColorCode.Text = "0000";
                            break;
                    }
                    break;
                case CursorMode.Square:
                    if (SelectedColorCode.Text.Length == 4)
                    {
                        int yMin = 0;
                        int xMin = 0;
                        int xMax = 512;
                        int yMax = 512;

                        if ((xCoordinate - (cursorDiameter / 2)) >= xMin)
                        {
                            xMin = xCoordinate - (cursorDiameter / 2);
                        }
                        if ((xCoordinate + (cursorDiameter / 2)) <= xMax)
                        {
                            xMax = xCoordinate + (cursorDiameter / 2);
                        }

                        if ((yCoordinate - (cursorDiameter / 2)) >= yMin)
                        {
                            yMin = yCoordinate - (cursorDiameter / 2);
                        }
                        if ((yCoordinate + (cursorDiameter / 2)) <= yMax)
                        {
                            yMax = yCoordinate + (cursorDiameter / 2);
                        }

                        for (int y = yMin; y < yMax; y++)
                        {
                            for (int x = xMin; x < xMax; x++)
                            {
                                byte[] tempByteArray = GetByteArrayFromHexString(SelectedColorCode.Text);
                                switch (currentMapMode)
                                {
                                    case MapMode.HeightMap:
                                        HeightMapDigitsOneAndTwo[x, y] = tempByteArray[0];
                                        HeightMapDigitsThreeAndFour[x, y] = tempByteArray[1];
                                        break;
                                    case MapMode.TextureDistributionMap:
                                        TextureDistributionDigitsOneAndTwo[x, y] = tempByteArray[0];
                                        TextureDistributionDigitsThreeAndFour[x, y] = tempByteArray[1];
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                        Render();
                    }
                    else
                    {
                        MessageBox.Show("Error\nThe selected color has to be 4 digits long.");
                    }
                    break;
                case CursorMode.Rotate:
                    byte[,] rotatedHeightMapDigitsOneAndTwo = new byte[MapWidth, MapHeight];
                    byte[,] rotatedHeightMapDigitsThreeAndFour = new byte[MapWidth, MapHeight];
                    byte[,] rotatedTextureDistributionDigitsOneAndTwo = new byte[MapWidth, MapHeight];
                    byte[,] rotatedTextureDistributionDigitsThreeAndFour = new byte[MapWidth, MapHeight];

                    int i = 0;
                    for (int x = 0; x < MapHeight; x++)
                    {
                        for (int y = 512 - 1; y >= 0; y--)
                        {
                            switch (currentMapMode)
                            {
                                case MapMode.HeightMap:
                                    rotatedHeightMapDigitsOneAndTwo[x, i] = HeightMapDigitsOneAndTwo[y, x];
                                    rotatedHeightMapDigitsThreeAndFour[x, i] = HeightMapDigitsThreeAndFour[y, x];
                                    break;
                                case MapMode.TextureDistributionMap:
                                    rotatedTextureDistributionDigitsOneAndTwo[x, i] = TextureDistributionDigitsOneAndTwo[y, x];
                                    rotatedTextureDistributionDigitsThreeAndFour[x, i] = TextureDistributionDigitsThreeAndFour[y, x];
                                    break;
                                default:
                                    break;
                            }
                            i++;
                        }
                        i = 0;
                    }

                    switch (currentMapMode)
                    {
                        case MapMode.HeightMap:
                            HeightMapDigitsOneAndTwo = rotatedHeightMapDigitsOneAndTwo;
                            HeightMapDigitsThreeAndFour = rotatedHeightMapDigitsThreeAndFour;
                            break;
                        case MapMode.TextureDistributionMap:
                            TextureDistributionDigitsOneAndTwo = rotatedTextureDistributionDigitsOneAndTwo;
                            TextureDistributionDigitsThreeAndFour = rotatedTextureDistributionDigitsThreeAndFour;
                            break;
                    }
                    Render();
                    break;
                default:
                    break;
            }
        }

        private byte[] GetByteArrayFromHexString(string fullHexStumber)
        {
            byte[] tempByteArray = new byte[2];

            string digitOneAndTwoString = "" + fullHexStumber[2] + fullHexStumber[3];
            string digitThreeAndFourString = "" + fullHexStumber[0] + fullHexStumber[1];

            tempByteArray[0] = Convert.ToByte(digitOneAndTwoString, 16);
            tempByteArray[1] = Convert.ToByte(digitThreeAndFourString, 16);

            return tempByteArray;
        }

        private void MapModeChanged(object sender, SelectionChangedEventArgs e)
        {
            byte[] singleColorData;
            switch (MapModeDropDown.SelectedIndex)
            {
                case 0:
                    currentMapMode = MapMode.HeightMap;
                    ImportMapButton.Content = "Import Heightmap";
                    ExportMapButton.Content = "Export Heightmap";
                    Render();
                    singleColorData = GetByteArrayFromHexString(SelectedColorCode.Text);
                    DrawTiffImage((int)SelectedColorImage.Width, (int)SelectedColorImage.Height, CreateTiffData((int)SelectedColorImage.Width, (int)SelectedColorImage.Height, singleColorData), DrawingType.SelectedColor);
                    
                    ImportMapButton.Visibility = Visibility.Visible;
                    ExportMapButton.Visibility = Visibility.Visible;
                    SelectedColorCode.Visibility = Visibility.Visible;
                    SelectedColorImage.Visibility = Visibility.Visible;
                    SelectedColorBorder.Visibility = Visibility.Visible;
                    SelectedColorHeight.Visibility = Visibility.Visible;
                    cursorModeSelect.Visibility = Visibility.Visible;
                    cursorModePipette.Visibility = Visibility.Visible;
                    CursorSizeSlider.Visibility = Visibility.Visible;
                    cursorDiameterLabel.Visibility = Visibility.Visible;
                    cursorModeSquare.Visibility = Visibility.Visible;
                    cursorModeRotate.Visibility = Visibility.Visible;
                    break;
                case 1:
                    currentMapMode = MapMode.TextureDistributionMap;
                    ImportMapButton.Content = "Import Texture Distribution";
                    ExportMapButton.Content = "Export Texture Distribution";
                    Render();
                    singleColorData = GetByteArrayFromHexString(SelectedColorCode.Text);
                    DrawTiffImage((int)SelectedColorImage.Width, (int)SelectedColorImage.Height, CreateTiffData((int)SelectedColorImage.Width, (int)SelectedColorImage.Height, singleColorData), DrawingType.SelectedColor);
                    ImportMapButton.Visibility = Visibility.Visible;
                    ExportMapButton.Visibility = Visibility.Visible;
                    SelectedColorCode.Visibility = Visibility.Visible;
                    SelectedColorImage.Visibility = Visibility.Visible;
                    SelectedColorBorder.Visibility = Visibility.Visible;
                    SelectedColorHeight.Visibility = Visibility.Hidden;
                    cursorModeSelect.Visibility = Visibility.Visible;
                    cursorModePipette.Visibility = Visibility.Visible;
                    CursorSizeSlider.Visibility = Visibility.Visible;
                    cursorDiameterLabel.Visibility = Visibility.Visible;
                    cursorModeSquare.Visibility = Visibility.Visible;
                    cursorModeRotate.Visibility = Visibility.Visible;
                    break;
                case 2:
                    currentMapMode = MapMode.Full;
                    Render();
                    ImportMapButton.Visibility = Visibility.Hidden;
                    ExportMapButton.Visibility = Visibility.Hidden;
                    SelectedColorCode.Visibility = Visibility.Hidden;
                    SelectedColorImage.Visibility = Visibility.Hidden;
                    SelectedColorBorder.Visibility = Visibility.Hidden;
                    SelectedColorHeight.Visibility = Visibility.Hidden;
                    cursorModeSelect.Visibility = Visibility.Hidden;
                    cursorModePipette.Visibility = Visibility.Hidden;
                    CursorSizeSlider.Visibility = Visibility.Hidden;
                    cursorDiameterLabel.Visibility = Visibility.Hidden;
                    cursorModeSquare.Visibility = Visibility.Hidden;
                    cursorModeRotate.Visibility = Visibility.Hidden;
                    
                    break;
                default:
                    break;
            }

            if (currentCursorMode == CursorMode.Square)
            {
                UpdateCursor();
            }
        }

        private string LeaveOnlyHexNumbers(string inString)
        {
            foreach (char c in inString.ToCharArray())
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(c.ToString(), "^[0-9a-fA-F]*$"))
                {
                    inString = inString.Replace(c.ToString(), "");
                }
            }

            return inString;
        }

        private void SelectedColorCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            SelectedColorCode.Text = LeaveOnlyHexNumbers(SelectedColorCode.Text);

            if (SelectedColorCode.Text.Length == 4)
            {
                byte[] singleColorData = GetByteArrayFromHexString(SelectedColorCode.Text);
                DrawTiffImage((int)SelectedColorImage.Width, (int)SelectedColorImage.Height, CreateTiffData((int)SelectedColorImage.Width, (int)SelectedColorImage.Height, singleColorData), DrawingType.SelectedColor);

                int digitOne = Convert.ToInt32("" + SelectedColorCode.Text[3], 16);
                int digitTwo = Convert.ToInt32("" + SelectedColorCode.Text[2], 16);
                int digitThree = Convert.ToInt32("" + SelectedColorCode.Text[1], 16);
                SelectedColorHeight.Content = "This color corresponds to a height of\r\n" + (((digitThree * Math.Pow(16, 1)) + (digitTwo * Math.Pow(16, 0)) + (digitOne * Math.Pow(16, -1))) / 2);
                if (currentCursorMode == CursorMode.Square)
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

        private Cursor CreateCursor(double cursorWidth, double cursorHeight, SolidColorBrush fillBrush, SolidColorBrush borderBrush, MediaPen pen)
        {
            System.Windows.Point centrePoint;
            int borderWidth;
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                switch (currentCursorMode)
                {
                    case CursorMode.Square:
                        centrePoint = new System.Windows.Point(cursorWidth / 2, cursorHeight / 2);
                        borderWidth = 2;
                        drawingContext.DrawRectangle(fillBrush, new MediaPen(borderBrush, 1.0), new Rect(1, 1, cursorWidth, cursorHeight));
                        drawingContext.DrawLine(new MediaPen(borderBrush, 1.0), new System.Windows.Point(centrePoint.X - 10, centrePoint.Y), new System.Windows.Point(centrePoint.X + 10, centrePoint.Y));
                        drawingContext.DrawLine(new MediaPen(borderBrush, 1.0), new System.Windows.Point(centrePoint.X, centrePoint.Y - 10), new System.Windows.Point(centrePoint.X, centrePoint.Y + 10));
                        drawingContext.Close();
                        break;
                    case CursorMode.Pipette:
                        centrePoint = new System.Windows.Point(0, 0);
                        borderWidth = 0;
                        drawingContext.DrawImage(new BitmapImage(new Uri("pack://application:,,,/resources/cursor/Pipette_White_Border_Black.ico")), new Rect(0, 0, cursorWidth, cursorHeight));
                        drawingContext.Close();
                        break;
                    case CursorMode.Rotate:
                        centrePoint = new System.Windows.Point(cursorWidth / 2, cursorHeight / 2);
                        borderWidth = 0;
                        drawingContext.DrawImage(new BitmapImage(new Uri("pack://application:,,,/resources/cursor/Rotate_White_Border_Black.ico")), new Rect(0, 0, cursorWidth, cursorHeight));
                        drawingContext.Close();
                        break;
                    default:
                        centrePoint = new System.Windows.Point(0, 0);
                        borderWidth = 0;
                        break;
                }
            }
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int) cursorWidth + borderWidth, (int) cursorHeight + borderWidth, 96, 96, PixelFormats.Pbgra32);
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
                        memoryStreamTwo.WriteByte((byte) cursorWidth); //Specifies image width in pixels. Can be any number between 0 and 255. Value 0 means image width is 256 pixels.
                        memoryStreamTwo.WriteByte((byte) cursorHeight); //Specifies image height in pixels. Can be any number between 0 and 255. Value 0 means image height is 256 pixels.

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

        private void CursorModeSelect_Click(object sender, RoutedEventArgs e)
        {
            currentCursorMode = CursorMode.Normal;
            UpdateCursor();
            CursorSizeSlider.Visibility = Visibility.Hidden;
            cursorDiameterLabel.Visibility = Visibility.Hidden;
        }

        private void CursorModePipette_Click(object sender, RoutedEventArgs e)
        {
            currentCursorMode = CursorMode.Pipette;
            UpdateCursor();
            CursorSizeSlider.Visibility = Visibility.Hidden;
            cursorDiameterLabel.Visibility = Visibility.Hidden;
        }

        private void CursorModeSquare_Click(object sender, RoutedEventArgs e)
        {
            currentCursorMode = CursorMode.Square;
            UpdateCursor();
            CursorSizeSlider.Visibility = Visibility.Visible;
            cursorDiameterLabel.Visibility = Visibility.Visible;
        }

        private void CursorModeRotate_Click(object sender, RoutedEventArgs e)
        {
            currentCursorMode = CursorMode.Rotate;
            UpdateCursor();
            CursorSizeSlider.Visibility = Visibility.Hidden;
            cursorDiameterLabel.Visibility = Visibility.Hidden;
        }

        private void UpdateCursor()
        {
            SolidColorBrush fillBrush;
            switch (currentCursorMode)
            {
                case CursorMode.Normal:
                    Mouse.OverrideCursor = null;
                    break;
                case CursorMode.Pipette:
                    fillBrush = MediaBrushes.Black;
                    Mouse.OverrideCursor = CreateCursor(28, 28, fillBrush, null, null);
                    break;
                case CursorMode.Square:
                    fillBrush = new SolidColorBrush(MediaColor.FromArgb(127, 0xFF, 0xFF, 0xFF));
                    Mouse.OverrideCursor = CreateCursor(cursorDiameter, cursorDiameter, fillBrush, MediaBrushes.Black, null);
                    break;
                case CursorMode.Rotate:
                    fillBrush = MediaBrushes.Black;
                    Mouse.OverrideCursor = CreateCursor(28, 32, fillBrush, null, null);
                    break;
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsAnyMapLoaded)
            {
                cursorDiameterLabel.Content = "Cursor Diameter: " + CursorSizeSlider.Value;
                cursorDiameter = (int) CursorSizeSlider.Value;
                if (currentCursorMode == CursorMode.Square)
                {
                    UpdateCursor();
                }
            }
        }
    }
}
