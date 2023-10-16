using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

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

        private MapMode Mode;
        private readonly int MapWidth = 512;
        private readonly int MapHeight = 512;
        private string OMPFilePathString;

        private byte[,] HeightMapDigitsOneAndTwo;
        private byte[,] HeightMapDigitsThreeAndFour;
        private byte[,] TextureDistributionDigitsOneAndTwo;
        private byte[,] TextureDistributionDigitsThreeAndFour;

        private Bitmap MapBitmap;

        private bool IsAnyMapLoaded = false;

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
                    GetMapData(0, openFileDialog.FileName, Mode);
                }

                Render();

                if(IsAnyMapLoaded == false)
                {
                    MapModeDropDown.SelectedIndex = 0;
                    MapModeDropDown.IsEnabled = true;
                    ImportMapButton.IsEnabled = true;
                    ExportMapButton.IsEnabled = true;
                    ExportToOMPFileButton.IsEnabled = true;
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
                    
                    HeightMapDigitsOneAndTwo[y, x] = allMapBytes[totalOffset];

                    int tempValue = allMapBytes[totalOffset + 1];
                    //tempValue &= ~(1 << 7);
                    //tempValue &= ~(1 << 6);
                    //tempValue &= ~(1 << 5);
                    //tempValue &= ~(1 << 4);
                    //tempValue |= 1 << 5;

                    HeightMapDigitsThreeAndFour[y, x] = (byte) tempValue;
                    TextureDistributionDigitsOneAndTwo[y, x] = allMapBytes[totalOffset + 2];
                    TextureDistributionDigitsThreeAndFour[y, x] = allMapBytes[totalOffset + 3];
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

                    if (mapMode == MapMode.HeightMap)
                    {
                        HeightMapDigitsOneAndTwo[y, x] = allMapBytes[totalOffset];

                        int tempValue = allMapBytes[totalOffset + 1];
                        //tempValue &= ~(1 << 7);
                        //tempValue &= ~(1 << 6);
                        //tempValue &= ~(1 << 5);
                        //tempValue &= ~(1 << 4);
                        //tempValue |= 1 << 5;

                        HeightMapDigitsThreeAndFour[y, x] = (byte)tempValue;
                    }
                    else if (mapMode == MapMode.TextureDistributionMap)
                    {
                        TextureDistributionDigitsOneAndTwo[y, x] = allMapBytes[totalOffset];
                        TextureDistributionDigitsThreeAndFour[y, x] = allMapBytes[totalOffset + 1];
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
                    WriteMapData(0, saveFileDialog.FileName, Mode);
                }
            }
        }

        private void WriteMapData(int OffsetMap, string filePath)
        {
            int totalNumberOfBytes = MapWidth * MapHeight * 4;
            byte[] allMapBytes = new byte[totalNumberOfBytes];
            int xOffset = 4;
            int yOffset = 0;

            for (int y = 0; y < MapHeight; y++)
            {
                if (y != 0)
                {
                    yOffset = y * 2048;
                }
                for (int x = 0; x < MapWidth; x++)
                {
                    int v = x * xOffset;
                    allMapBytes[v + yOffset] = HeightMapDigitsOneAndTwo[y, x];
                    allMapBytes[v + 1 + yOffset] = HeightMapDigitsThreeAndFour[y, x];
                    allMapBytes[v + 2 + yOffset] = TextureDistributionDigitsOneAndTwo[y, x];
                    allMapBytes[v + 3 + yOffset] = TextureDistributionDigitsThreeAndFour[y, x];
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

            for (int y = 0; y < MapHeight; y++)
            {
                if (y != 0)
                {
                    yOffset = y * 1024;
                }
                for (int x = 0; x < MapWidth; x++)
                {
                    int v = x * xOffset;

                    if (mapMode == MapMode.HeightMap)
                    {
                        allMapBytes[v + yOffset] = HeightMapDigitsOneAndTwo[y, x];
                        allMapBytes[v + 1 + yOffset] = HeightMapDigitsThreeAndFour[y, x];
                    }
                    else if (mapMode == MapMode.TextureDistributionMap)
                    {
                        allMapBytes[v + yOffset] = TextureDistributionDigitsOneAndTwo[y, x];
                        allMapBytes[v + 1 + yOffset] = TextureDistributionDigitsThreeAndFour[y, x];
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
            int blue, green, red, alpha;
            Color color;

            //RGB 555
            if (format == ColorFormat.RGB555)
            {
                blue = byte0 & 0x1F;
                green = ((byte1 << 2) & 0x18) | ((byte0 >> 5) & 0x07);
                red = (byte1 >> 2) & 0x1F;

                blue = blue * 255 / 31;
                green = green * 255 / 31;
                red = red * 255 / 31;

                color = Color.FromArgb(red, green, blue);

                return color;
            }
            //RGB 555 Bytes Exchanged
            else if (format == ColorFormat.RGB555Flipped)
            {
                blue = byte1 & 0x1F;
                green = ((byte0 << 2) & 0x18) | ((byte1 >> 5) & 0x07);
                red = (byte0 >> 2) & 0x1F;

                blue = blue * 255 / 31;
                green = green * 255 / 31;
                red = red * 255 / 31;

                color = Color.FromArgb(red, green, blue);

                return color;
            }
            //RGB 565
            else if (format == ColorFormat.RGB565)
            {
                blue = byte0 & 0x1F;
                green = ((byte1 << 3) & 0x18) | ((byte0 >> 5) & 0x07);
                red = (byte1 >> 3) & 0x1F;

                blue = blue * 255 / 31;
                green = green * 255 / 63;
                red = red * 255 / 31;

                color = Color.FromArgb(red, green, blue);

                return color;
            }
            //RGB 565 Bytes Exchanged
            else if (format == ColorFormat.RGB565Flipped)
            {
                blue = byte1 & 0x1F;
                green = ((byte0 << 3) & 0x18) | ((byte1 >> 5) & 0x07);
                red = (byte0 >> 3) & 0x1F;

                blue = blue * 255 / 31;
                green = green * 255 / 63;
                red = red * 255 / 31;

                color = Color.FromArgb(red, green, blue);

                return color;
            }
            //BGR 555
            else if (format == ColorFormat.BGR555)
            {
                red = byte0 & 0x1F;
                green = ((byte1 << 2) & 0x18) | ((byte0 >> 5) & 0x07);
                blue = (byte1 >> 2) & 0x1F;

                blue = blue * 255 / 31;
                green = green * 255 / 31;
                red = red * 255 / 31;

                color = Color.FromArgb(red, green, blue);

                return color;
            }
            //BGR 555 Bytes Exchanged
            else if (format == ColorFormat.BGR555Flipped)
            {
                red = byte1 & 0x1F;
                green = ((byte0 << 2) & 0x18) | ((byte1 >> 5) & 0x07);
                blue = (byte0 >> 2) & 0x1F;

                blue = blue * 255 / 31;
                green = green * 255 / 31;
                red = red * 255 / 31;

                color = Color.FromArgb(red, green, blue);

                return color;
            }
            //BGR 565
            else if (format == ColorFormat.BGR565)
            {
                red = byte0 & 0x1F;
                green = ((byte1 << 3) & 0x38) | ((byte0 >> 5) & 0x07);
                blue = (byte1 >> 3) & 0x1F;

                blue = blue * 255 / 31;
                green = green * 255 / 63;
                red = red * 255 / 31;

                color = Color.FromArgb(red, green, blue);

                return color;
            }
            //BGR 565 Bytes Exchanged
            else if (format == ColorFormat.BGR565Flipped)
            {
                red = byte1 & 0x1F;
                green = ((byte0 << 3) & 0x38) | ((byte1 >> 5) & 0x07);
                blue = (byte0 >> 3) & 0x1F;

                blue = blue * 255 / 31;
                green = green * 255 / 63;
                red = red * 255 / 31;

                color = Color.FromArgb(red, green, blue);

                return color;
            }
            //RGBA 4444
            else if (format == ColorFormat.RGBA4444)
            {
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
            }
            //RGBA 4444 Bytes Exchanged
            else if (format == ColorFormat.RGBA4444Flipped)
            {
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
            }
            //ARGB 4444
            else if (format == ColorFormat.ARGB4444)
            {
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
            }
            //ARGB 4444 Bytes Exchanged
            else if (format == ColorFormat.ARGB4444Flipped)
            {
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
            }
            else if (format == ColorFormat.Gray16)
            {
                int grayscale = (int)Math.Ceiling(0.2989 * (byte0 + byte1) + 0.5870 * (byte0 + byte1) + 0.1140 * (byte0 + byte1));
                grayscale /= 255;

                color = Color.FromArgb(grayscale, grayscale, grayscale);

                return color;
            }
            else if (format == ColorFormat.Gray12)
            {
                int grayscale = (int)Math.Ceiling(0.2989 * (byte1 & 0x0F) + 0.5870 * ((byte0 >> 4) & 0x0F) + 0.1140 * (byte0 & 0x0F));
                grayscale = grayscale * 255 / 15;

                color = Color.FromArgb(grayscale, grayscale, grayscale);

                return color;
            }
            else
            {
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
                        switch (Mode)
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
                MapBitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
                Map.Source = GetBmpImageFromBmp(MapBitmap);
            }
        }

        private void GetCoordinates(object sender, MouseButtonEventArgs e)
        {
            int x = (int)e.GetPosition(Map).X;
            int y = 512 - (int)e.GetPosition(Map).Y;

            MessageBox.Show("Location : X:" + x + " | Y:" + y);
        }

        private void MapModeChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (MapModeDropDown.SelectedIndex)
            {
                case 0:
                    Mode = MapMode.HeightMap;
                    ImportMapButton.Content = "Import Heightmap";
                    ExportMapButton.Content = "Export Heightmap";
                    Render();
                    break;
                case 1:
                    Mode = MapMode.TextureDistributionMap;
                    ImportMapButton.Content = "Import Texture Distribution";
                    ExportMapButton.Content = "Export Texture Distribution";
                    Render();
                    break;
                default:
                    break;
            }
        }
    }
}
