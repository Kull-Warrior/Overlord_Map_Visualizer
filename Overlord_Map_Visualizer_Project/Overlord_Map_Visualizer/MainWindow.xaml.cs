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
using Brush = System.Drawing.Brush;
using Color = System.Drawing.Color;
using MediaBrushes = System.Windows.Media.Brushes;
using MediaColor = System.Windows.Media.Color;
using MediaPen = System.Windows.Media.Pen;
using Pen = System.Drawing.Pen;

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
            TextureDistributionMap
        }

        private enum CursorMode
        {
            Normal,
            Pipette,
            Square
        }

        private MapMode currentMapMode;
        private readonly int MapWidth = 512;
        private readonly int MapHeight = 512;
        private string OMPFilePathString;

        private byte[,] HeightMapDigitsOneAndTwo;
        private byte[,] HeightMapDigitsThreeAndFour;
        private byte[,] TextureDistributionDigitsOneAndTwo;
        private byte[,] TextureDistributionDigitsThreeAndFour;

        private Bitmap MapBitmap;

        private bool IsAnyMapLoaded = false;

        private CursorMode currentCursorMode;
        private Color SelectedColor;
        private int cursorDiameter = 50;

        public MainWindow()
        {
            InitializeComponent();
            Initialise();
            DrawCoordinateSystem();
            UpdateSelectedColor(Color.Black);
        }

        private void Initialise()
        {
            HeightMapDigitsOneAndTwo = new byte[MapWidth, MapHeight];
            HeightMapDigitsThreeAndFour = new byte[MapWidth, MapHeight];

            TextureDistributionDigitsOneAndTwo = new byte[MapWidth, MapHeight];
            TextureDistributionDigitsThreeAndFour = new byte[MapWidth, MapHeight];

            SelectedColorCode.Text = "0000";
        }

        private void DrawCoordinateSystem()
        {
            Pen coordinatePen = new Pen(new SolidBrush(Color.Black));
            MapBitmap = new Bitmap((int)CoordinateSystem.Width, (int)CoordinateSystem.Height);
            using (Graphics MapGraphics = Graphics.FromImage(MapBitmap))
            {
                //Y Axis Arrow
                MapGraphics.DrawLine(coordinatePen, 13, 550, 18, 555);
                MapGraphics.DrawLine(coordinatePen, 18, 36, 18, 555);
                MapGraphics.DrawLine(coordinatePen, 23, 550, 18, 555);

                //X Axis Arrow
                MapGraphics.DrawLine(coordinatePen, 531, 41, 536, 36);
                MapGraphics.DrawLine(coordinatePen, 18, 36, 536, 36);
                MapGraphics.DrawLine(coordinatePen, 531, 31, 536, 36);

                //Y Axis Marker
                for (int y = 36; y < 568; y += 50)
                {
                    MapGraphics.DrawLine(coordinatePen, 9, y, 18, y);
                }

                //X Axis Marker
                for (int x = 18; x < 550; x += 50)
                {
                    MapGraphics.DrawLine(coordinatePen, x, 27, x, 36);
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

                for (int i = 1; i <= 10; i++)
                {
                    string content = "" + i * 50;
                    if (i == 1)
                    {
                        content = "  " + content;
                    }
                    CreateNewLabel("CoordMarkerX" + i, content, 257, 561 - (i * 50));
                }

                CreateNewLabel("CoordMarkerY", "Y", 284, 35);

                //Set Origin Bottom Left
                MapBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

                CoordinateSystem.Source = GetBmpImageFromBmp(MapBitmap);
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
                    GetMapData(GetMapDataOffset(), openFileDialog.FileName);
                }
                else
                {
                    GetMapData(0, openFileDialog.FileName, currentMapMode);
                }

                Render();

                if(IsAnyMapLoaded == false)
                {
                    MapModeDropDown.SelectedIndex = 0;
                    MapModeDropDown.IsEnabled = true;
                    ImportMapButton.IsEnabled = true;
                    ExportMapButton.IsEnabled = true;
                    ExportToOMPFileButton.IsEnabled = true;
                    SelectedColorCode.IsEnabled = true;
                    SelectedColorCode.Visibility = Visibility.Visible;
                    SelectedColorImage.Visibility = Visibility.Visible;
                    SelectedColorHeight.Visibility = Visibility.Visible;
                    cursorModeSelect.Visibility = Visibility.Visible;
                    cursorModeSquare.Visibility = Visibility.Visible;
                    IsAnyMapLoaded = true;
                }
            }
        }

        private void GetMapData(int OffsetMap, string filePath)
        {
            int totalNumberOfBytes = MapWidth * MapHeight * 4;
            byte[] allMapBytes = new byte[totalNumberOfBytes];
            int xOffset = 4;
            int yOffset = 0;
            int numberOfBytesInRow = MapWidth * 4; //One point is described by four bytes
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

                    HeightMapDigitsOneAndTwo[x, y] = allMapBytes[totalOffset];
                    HeightMapDigitsThreeAndFour[x, y] = allMapBytes[totalOffset + 1];
                    TextureDistributionDigitsOneAndTwo[x, y] = allMapBytes[totalOffset + 2];
                    TextureDistributionDigitsThreeAndFour[x, y] = allMapBytes[totalOffset + 3];
                }
            }
        }

        private void GetMapData(int OffsetMap, string filePath, MapMode mapMode)
        {
            int totalNumberOfBytes = MapWidth * MapHeight * 2;
            byte[] allMapBytes = new byte[totalNumberOfBytes];
            int xOffset = 2;
            int yOffset = 0;
            int numberOfBytesInRow = MapWidth * 2; //One point is described by two bytes
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
                    }
                }
            }
        }

        private void ExportToFile(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            if(button.Name == "ExportToOMPFileButton")
            {
                WriteMapData(GetMapDataOffset(), OMPFilePathString);
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
                    WriteMapData(0, saveFileDialog.FileName, currentMapMode);
                }
            }
        }

        private void WriteMapData(int OffsetMap, string filePath)
        {
            int totalNumberOfBytes = MapWidth * MapHeight * 4;
            byte[] allMapBytes = new byte[totalNumberOfBytes];
            int xOffset = 4;
            int yOffset = 0;
            int numberOfBytesInRow = MapWidth * 4; //One point is described by two bytes
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
                    allMapBytes[totalOffset] = HeightMapDigitsOneAndTwo[x, y];
                    allMapBytes[totalOffset + 1] = HeightMapDigitsThreeAndFour[x, y];
                    allMapBytes[totalOffset + 2] = TextureDistributionDigitsOneAndTwo[x, y];
                    allMapBytes[totalOffset + 3] = TextureDistributionDigitsThreeAndFour[x, y];
                }
            }

            using (BinaryWriter writer = new BinaryWriter(new FileStream(filePath, FileMode.OpenOrCreate)))
            {
                writer.BaseStream.Seek(OffsetMap, SeekOrigin.Begin);
                writer.Write(allMapBytes, 0, totalNumberOfBytes);
            }
        }

        private void WriteMapData(int OffsetMap, string filePath, MapMode mapMode)
        {
            int totalNumberOfBytes = MapWidth * MapHeight * 2;
            byte[] allMapBytes = new byte[totalNumberOfBytes];
            int xOffset = 2;
            int yOffset = 0;
            int numberOfBytesInRow = MapWidth * 2; //One point is described by two bytes
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

        private Color GetColor(byte byte0, byte byte1, ColorFormat format)
        {
            int blue, green, red, alpha, grayscale;
            Color color;

            switch (format)
            {
                //RGB 555
                case ColorFormat.RGB555:
                    blue = byte0 & 0x1F;
                    green = ((byte1 << 2) & 0x18) | ((byte0 >> 5) & 0x07);
                    red = (byte1 >> 2) & 0x1F;

                    blue = blue * 255 / 31;
                    green = green * 255 / 31;
                    red = red * 255 / 31;

                    color = Color.FromArgb(red, green, blue);

                    return color;
                //RGB 555 Bytes Exchanged
                case ColorFormat.RGB555Flipped:
                    blue = byte1 & 0x1F;
                    green = ((byte0 << 2) & 0x18) | ((byte1 >> 5) & 0x07);
                    red = (byte0 >> 2) & 0x1F;

                    blue = blue * 255 / 31;
                    green = green * 255 / 31;
                    red = red * 255 / 31;

                    color = Color.FromArgb(red, green, blue);

                    return color;
                //RGB 565
                case ColorFormat.RGB565:
                    blue = byte0 & 0x1F;
                    green = ((byte1 << 3) & 0x18) | ((byte0 >> 5) & 0x07);
                    red = (byte1 >> 3) & 0x1F;

                    blue = blue * 255 / 31;
                    green = green * 255 / 63;
                    red = red * 255 / 31;

                    color = Color.FromArgb(red, green, blue);

                    return color;
                //RGB 565 Bytes Exchanged
                case ColorFormat.RGB565Flipped:
                    blue = byte1 & 0x1F;
                    green = ((byte0 << 3) & 0x18) | ((byte1 >> 5) & 0x07);
                    red = (byte0 >> 3) & 0x1F;

                    blue = blue * 255 / 31;
                    green = green * 255 / 63;
                    red = red * 255 / 31;

                    color = Color.FromArgb(red, green, blue);

                    return color;
                //BGR 555
                case ColorFormat.BGR555:
                    red = byte0 & 0x1F;
                    green = ((byte1 << 2) & 0x18) | ((byte0 >> 5) & 0x07);
                    blue = (byte1 >> 2) & 0x1F;

                    blue = blue * 255 / 31;
                    green = green * 255 / 31;
                    red = red * 255 / 31;

                    color = Color.FromArgb(red, green, blue);

                    return color;
                //BGR 555 Bytes Exchanged
                case ColorFormat.BGR555Flipped:
                    red = byte1 & 0x1F;
                    green = ((byte0 << 2) & 0x18) | ((byte1 >> 5) & 0x07);
                    blue = (byte0 >> 2) & 0x1F;

                    blue = blue * 255 / 31;
                    green = green * 255 / 31;
                    red = red * 255 / 31;

                    color = Color.FromArgb(red, green, blue);

                    return color;
                //BGR 565
                case ColorFormat.BGR565:
                    red = byte0 & 0x1F;
                    green = ((byte1 << 3) & 0x38) | ((byte0 >> 5) & 0x07);
                    blue = (byte1 >> 3) & 0x1F;

                    blue = blue * 255 / 31;
                    green = green * 255 / 63;
                    red = red * 255 / 31;

                    color = Color.FromArgb(red, green, blue);

                    return color;
                //BGR 565 Bytes Exchanged
                case ColorFormat.BGR565Flipped:
                    red = byte1 & 0x1F;
                    green = ((byte0 << 3) & 0x38) | ((byte1 >> 5) & 0x07);
                    blue = (byte0 >> 3) & 0x1F;

                    blue = blue * 255 / 31;
                    green = green * 255 / 63;
                    red = red * 255 / 31;

                    color = Color.FromArgb(red, green, blue);

                    return color;
                //RGBA 4444
                case ColorFormat.RGBA4444:
                    blue = byte0 & 0x0F;
                    green = (byte0 >> 4) & 0x0F;
                    red = byte1 & 0x0F;
                    alpha = (byte1 >> 4) & 0x0F;

                    blue = blue * 255 / 15;
                    green = green * 255 / 15;
                    red = red * 255 / 15;
                    alpha = alpha * 255 / 15;

                    color = Color.FromArgb(alpha, red, green, blue);

                    return color;
                //RGBA 4444 Bytes Exchanged
                case ColorFormat.RGBA4444Flipped:
                    blue = byte1 & 0x0F;
                    green = (byte1 >> 4) & 0x0F;
                    red = byte0 & 0x0F;
                    alpha = (byte0 >> 4) & 0x0F;

                    blue = blue * 255 / 15;
                    green = green * 255 / 15;
                    red = red * 255 / 15;
                    alpha = alpha * 255 / 15;

                    color = Color.FromArgb(alpha, red, green, blue);

                    return color;
                //ARGB 4444
                case ColorFormat.ARGB4444:
                    alpha = byte0 & 0x0F;
                    blue = (byte0 >> 4) & 0x0F;
                    green = byte1 & 0x0F;
                    red = (byte1 >> 4) & 0x0F;

                    blue = blue * 255 / 15;
                    green = green * 255 / 15;
                    red = red * 255 / 15;
                    alpha = alpha * 255 / 15;

                    color = Color.FromArgb(alpha, red, green, blue);

                    return color;
                //ARGB 4444 Bytes Exchanged
                case ColorFormat.ARGB4444Flipped:
                    alpha = byte1 & 0x0F;
                    blue = (byte1 >> 4) & 0x0F;
                    green = byte0 & 0x0F;
                    red = (byte0 >> 4) & 0x0F;

                    blue = blue * 255 / 15;
                    green = green * 255 / 15;
                    red = red * 255 / 15;
                    alpha = alpha * 255 / 15;

                    //alpha = 255;

                    color = Color.FromArgb(alpha, red, green, blue);

                    return color;
                //Gray 16
                case ColorFormat.Gray16:
                    grayscale = (int)Math.Ceiling(0.2989 * (byte0 + byte1) + 0.5870 * (byte0 + byte1) + 0.1140 * (byte0 + byte1));
                    grayscale /= 255;

                    color = Color.FromArgb(grayscale, grayscale, grayscale);

                    return color;
                //Gray 12 (4 highest bits are ignored)
                case ColorFormat.Gray12:
                    grayscale = (int)Math.Ceiling(0.2989 * (byte1 & 0x0F) + 0.5870 * ((byte0 >> 4) & 0x0F) + 0.1140 * (byte0 & 0x0F));
                    grayscale = grayscale * 255 / 15;

                    color = Color.FromArgb(grayscale, grayscale, grayscale);

                    return color;
                default:
                    return Color.Black;
            }
        }

        private BitmapImage GetBmpImageFromBmp(Bitmap bitMap)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                bitMap.Save(memoryStream, ImageFormat.Png);
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
            MapBitmap = new Bitmap(MapWidth, MapHeight);
            using (Graphics MapGraphics = Graphics.FromImage(MapBitmap))
            {
                for (int x = 0; x < MapWidth; x++)
                {
                    for (int y = 0; y < MapHeight; y++)
                    {
                        Brush MapBrush;
                        switch (currentMapMode)
                        {
                            case MapMode.HeightMap:
                                MapBrush = new SolidBrush(GetColor(HeightMapDigitsOneAndTwo[x, y], HeightMapDigitsThreeAndFour[x, y], ColorFormat.Gray12));
                                break;
                            case MapMode.TextureDistributionMap:
                                MapBrush = new SolidBrush(GetColor(TextureDistributionDigitsOneAndTwo[x, y], TextureDistributionDigitsThreeAndFour[x, y], ColorFormat.BGR565));
                                break;
                            default:
                                MapBrush = new SolidBrush(Color.Black);
                                break;
                        }

                        MapGraphics.FillRectangle(MapBrush, x, y, 1, 1);
                    }
                }

                //Set Origin Bottom Left
                MapBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

                Map.Source = GetBmpImageFromBmp(MapBitmap);
            }
        }

        private void GetCoordinates(object sender, MouseButtonEventArgs e)
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
                    break;
                default:
                    break;
            }
        }

        private byte[] GetByteArrayFromHexString(string fullHexStumber)
        {
            string digitOneAndTwoString = "" + fullHexStumber[2] + fullHexStumber[3];
            string digitThreeAndFourString = "" + fullHexStumber[0] + fullHexStumber[1];

            byte digitOneAndTwoNumber = Convert.ToByte(digitOneAndTwoString, 16);
            byte digitThreeAndFourNumber = Convert.ToByte(digitThreeAndFourString, 16);

            byte[] tempByteArray = new byte[2];
            tempByteArray[0] = digitOneAndTwoNumber;
            tempByteArray[1] = digitThreeAndFourNumber;

            return tempByteArray;
        }

        private void MapModeChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (MapModeDropDown.SelectedIndex)
            {
                case 0:
                    currentMapMode = MapMode.HeightMap;
                    ImportMapButton.Content = "Import Heightmap";
                    ExportMapButton.Content = "Export Heightmap";
                    Render();
                    UpdateSelectedColor(GetColorFromHexString(SelectedColorCode.Text));
                    break;
                case 1:
                    currentMapMode = MapMode.TextureDistributionMap;
                    ImportMapButton.Content = "Import Texture Distribution";
                    ExportMapButton.Content = "Export Texture Distribution";
                    Render();
                    UpdateSelectedColor(GetColorFromHexString(SelectedColorCode.Text));
                    break;
                default:
                    break;
            }

            if (currentCursorMode == CursorMode.Square)
            {
                ShowSquareCursor();
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
                UpdateSelectedColor(GetColorFromHexString(SelectedColorCode.Text));
            }

            if(currentCursorMode == CursorMode.Square)
            {
                ShowSquareCursor();
            }
        }

        private void UpdateSelectedColor(Color color)
        {
            SelectedColor = color;
            SolidBrush selectedColorBrush = new SolidBrush(color);
            Bitmap selectedColorBitmap = new Bitmap((int)SelectedColorImage.Width, (int)SelectedColorImage.Height);
            using (Graphics MapGraphics = Graphics.FromImage(selectedColorBitmap))
            {
                MapGraphics.FillRectangle(selectedColorBrush, 0, 0, (int)SelectedColorImage.Width, (int)SelectedColorImage.Height);
                SelectedColorImage.Source = GetBmpImageFromBmp(selectedColorBitmap);
            }
        }

        private Color GetColorFromHexString(string fullHexStumber)
        {
            string digitOneAndTwoString = "" + fullHexStumber[2] + fullHexStumber[3];
            string digitThreeAndFourString = "" + fullHexStumber[0] + fullHexStumber[1];

            int digitOne = Convert.ToInt32("" + fullHexStumber[3], 16);
            int digitTwo = Convert.ToInt32("" + fullHexStumber[2], 16);
            int digitThree = Convert.ToInt32("" + fullHexStumber[1], 16);
            int digitFour = Convert.ToInt32("" + fullHexStumber[0], 16);

            byte digitOneAndTwoNumber = Convert.ToByte(digitOneAndTwoString, 16);
            byte digitThreeAndFourNumber = Convert.ToByte(digitThreeAndFourString, 16);

            SelectedColorHeight.Content = "This color corresponds to a height of\r\n" + (((digitThree * Math.Pow(16, 1)) + (digitTwo * Math.Pow(16, 0)) + (digitOne * Math.Pow(16, -1))) / 2);

            switch (currentMapMode)
            {
                case MapMode.HeightMap:
                    int grayscale = (int)Math.Ceiling(0.2989 * (digitThreeAndFourNumber & 0x0F) + 0.5870 * ((digitOneAndTwoNumber >> 4) & 0x0F) + 0.1140 * (digitOneAndTwoNumber & 0x0F));
                    grayscale = grayscale * 255 / 15;

                    return Color.FromArgb(grayscale, grayscale, grayscale);
                case MapMode.TextureDistributionMap:
                    int blue, green, red;

                    red = digitOneAndTwoNumber & 0x1F;
                    green = ((digitThreeAndFourNumber << 3) & 0x38) | ((digitOneAndTwoNumber >> 5) & 0x07);
                    blue = (digitThreeAndFourNumber >> 3) & 0x1F;

                    blue = blue * 255 / 31;
                    green = green * 255 / 63;
                    red = red * 255 / 31;

                    return Color.FromArgb(red, green, blue);
                default:
                    return Color.Black;
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
                        drawingContext.DrawImage(new BitmapImage(new Uri("pack://application:,,,/Pipette.ico")), new Rect(0, 0, cursorWidth, cursorHeight));
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
            Mouse.OverrideCursor = null;
        }

        private void CursorModePipette_Click(object sender, RoutedEventArgs e)
        {
            currentCursorMode = CursorMode.Pipette;
            ShowPipetteCursor();
        }

        private void CursorModeSquare_Click(object sender, RoutedEventArgs e)
        {
            currentCursorMode = CursorMode.Square;
            ShowSquareCursor();
        }

        private void ShowSquareCursor()
        {
            SolidColorBrush fillBrush = new SolidColorBrush(MediaColor.FromArgb(127, SelectedColor.R, SelectedColor.G, SelectedColor.B));

            Mouse.OverrideCursor = CreateCursor(cursorDiameter, cursorDiameter, fillBrush, MediaBrushes.Black, null);
        }

        private void ShowPipetteCursor()
        {
            SolidColorBrush fillBrush = MediaBrushes.Black;

            Mouse.OverrideCursor = CreateCursor(26, 26, fillBrush, null, null);
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsAnyMapLoaded)
            {
                CursorRadiosLabel.Content = "Cursor Radius: " + slider.Value;
                cursorDiameter = (int) slider.Value;
                if (currentCursorMode == CursorMode.Square)
                {
                    ShowSquareCursor();
                }
            }
        }
    }
}
