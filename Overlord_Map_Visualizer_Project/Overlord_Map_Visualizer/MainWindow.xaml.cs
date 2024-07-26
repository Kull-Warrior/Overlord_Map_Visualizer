using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
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
using DrawingPoint = System.Drawing.Point;
using System.Windows.Media.Media3D;

namespace Overlord_Map_Visualizer
{
    public enum MapMode
    {
        HeightMap,
        MainTextureMap,
        WallTextureMap,
        FoliageMap,
        UnknownMap,
        Full,
        ThreeDimensional
    }
    public enum CursorMode
    {
        Select,
        Pipette,
        Square,
        Circle,
        Rotate
    }

    public enum CursorSubMode
    {
        Set,
        Add,
        Sub
    }

    public enum DrawingType
    {
        Map,
        SelectedColor
    }

    public partial class MainWindow : Window
    {
        private OverlordMap CurrentMap = new OverlordMap();

        private MapMode CurrentMapMode;

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
                        CurrentMap.FilePath = openFileDialog.FileName;
                        offset = CurrentMap.GetMapDataOffset();
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
                byte[] data = CurrentMap.ReadMapDataFromFile(offset, openFileDialog.FileName, bytesPerPoint);
                CurrentMap.SetMapData(data, bytesPerPoint, mapMode, isTiffImage);
                CurrentMap.WaterLevel = CurrentMap.GetMapWaterLevel();

                data = CurrentMap.CreateTiffData(CurrentMap.Width, CurrentMap.Height, mapMode);
                DrawTiffImage(CurrentMap.Width, CurrentMap.Height, DrawingType.Map, data);

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
                offset = CurrentMap.GetMapDataOffset();
                bytesPerPoint = 4;
                filePath = CurrentMap.FilePath;
                data = CurrentMap.GetMapData(bytesPerPoint, MapMode.Full);
                CurrentMap.WriteMapDataToFile(data, offset, filePath, bytesPerPoint);
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
                    data = CurrentMap.GetMapData(bytesPerPoint, CurrentMapMode);
                    CurrentMap.WriteMapDataToFile(data, offset, filePath, bytesPerPoint);
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
                        int stride = ((CurrentMap.Width * format.BitsPerPixel) + 7) / 8;
                        data = CurrentMap.CreateTiffData(CurrentMap.Width, CurrentMap.Height, CurrentMapMode);
                        WriteableBitmap writableBitmap = new WriteableBitmap(CurrentMap.Width, CurrentMap.Height, dpi, dpi, format, null);
                        writableBitmap.WritePixels(new Int32Rect(0, 0, CurrentMap.Width, CurrentMap.Height), data, stride, 0);

                        // Encode as a TIFF
                        TiffBitmapEncoder encoder = new TiffBitmapEncoder { Compression = TiffCompressOption.None };
                        encoder.Frames.Add(BitmapFrame.Create(writableBitmap));

                        encoder.Save(stream);
                    }
                }
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

        private byte[] CreateTiffDataForSelectedColor(int width, int height)
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
                    if(CurrentMapMode == MapMode.HeightMap)
                    {
                        byte[] singleColorDataTwoByte = new byte[2];

                        singleColorDataTwoByte[0] = Convert.ToByte("" + SelectedColorCode.Text[2] + SelectedColorCode.Text[3], 16);
                        singleColorDataTwoByte[1] = Convert.ToByte("" + SelectedColorCode.Text[0] + SelectedColorCode.Text[1], 16);

                        grayScale = ((singleColorDataTwoByte[1] << 8) & 0x0FFF) + singleColorDataTwoByte[0];

                        grayScale = grayScale * 65535 / 4095;

                        data[totalOffset] = (byte)(grayScale & 0x00FF);
                        data[totalOffset + 1] = (byte)((grayScale & 0xFF00) >> 8);
                        data[totalOffset + 2] = (byte)(grayScale & 0x00FF);
                        data[totalOffset + 3] = (byte)((grayScale & 0xFF00) >> 8);
                        data[totalOffset + 4] = (byte)(grayScale & 0x00FF);
                        data[totalOffset + 5] = (byte)((grayScale & 0xFF00) >> 8);
                    }
                    else
                    {
                        byte singleColorData = Convert.ToByte(SelectedColorCode.Text, 16);
                        rgb = CurrentMap.GetTiffRgbFromFourBitRgbPalette(singleColorData);
                        red = rgb[0];
                        green = rgb[1];
                        blue = rgb[2];

                        data[totalOffset] = (byte)(blue & 0x00FF);
                        data[totalOffset + 1] = (byte)((blue & 0xFF00) >> 8);
                        data[totalOffset + 2] = (byte)(green & 0x00FF);
                        data[totalOffset + 3] = (byte)((green & 0xFF00) >> 8);
                        data[totalOffset + 4] = (byte)(red & 0x00FF);
                        data[totalOffset + 5] = (byte)((red & 0xFF00) >> 8);
                    }
                }
            }
            return data;
        }

        private void DrawTiffImage(int width, int height, DrawingType type, byte[] data)
        {
            double dpi = 50;
            PixelFormat format = PixelFormats.Rgb48;
            int stride = ((width * format.BitsPerPixel) + 7) / 8;

            WriteableBitmap writableBitmap = new WriteableBitmap(width, height, dpi, dpi, format, null);
            writableBitmap.WritePixels(new Int32Rect(0, 0, width, height), data, stride, 0);

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
            Bitmap allMapObjectLocationsBitmap = new Bitmap(CurrentMap.Width, CurrentMap.Height);

            for(int i = 0; i < CurrentMap.ObjectList.Count; i++)
            {
                switch (CurrentMap.ObjectList[i].Type)
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

        private void Draw3DTerrain(OverlordMap map)
        {
            float halfSize = map.Width/ 2;
            float halfheight = map.Height / 2;

            float[,] floatMap = map.GetFloatMap();

            //creation of the terrain
            GeometryModel3D terrainGeometryModel = new GeometryModel3D(new MeshGeometry3D(), new DiffuseMaterial(MediaBrushes.Gray));
            Point3DCollection point3DCollection = new Point3DCollection();
            Int32Collection triangleIndices = new Int32Collection();

            //adding point
            for (var y = 0; y < 512; y++)
            {
                for (var x = 0; x < 512; x++)
                {
                    point3DCollection.Add(new Point3D(x - halfSize, y - halfSize, floatMap[x, y] - halfheight));;
                }
            }
            ((MeshGeometry3D)terrainGeometryModel.Geometry).Positions = point3DCollection;

            //defining triangles
            int ind1, ind2;
            int xLenght = 512;

            for (var y = 0; y < 512 - 1; y++)
            {
                for (var x = 0; x < 512 - 1; x++)
                {
                    ind1 = x + y * xLenght;
                    ind2 = ind1 + xLenght;

                    //first triangle
                    triangleIndices.Add(ind1);
                    triangleIndices.Add(ind2 + 1);
                    triangleIndices.Add(ind2);

                    //second triangle
                    triangleIndices.Add(ind1);
                    triangleIndices.Add(ind1 + 1);
                    triangleIndices.Add(ind2 + 1);
                }
            }
            ((MeshGeometry3D)terrainGeometryModel.Geometry).TriangleIndices = triangleIndices;

            Map3D.Children.Add(terrainGeometryModel);
        }

        private void DrawWater(OverlordMap map)
        {
            float halfSize = map.Width / 2;
            float halfheight = map.Height / 2;

            // creation of the water layers
            // I'm going to use a series of emissive layer for water
            SolidColorBrush waterSolidColorBrush = new SolidColorBrush(Colors.Blue);
            waterSolidColorBrush.Opacity = 0.2;
            GeometryModel3D myWaterGeometryModel =
            new GeometryModel3D(new MeshGeometry3D(), new EmissiveMaterial(waterSolidColorBrush));
            Point3DCollection waterPoint3DCollection = new Point3DCollection();
            Int32Collection triangleIndices = new Int32Collection();

            int triangleCounter;
            float dfMul = 5;

            for (int i = 0; i < 10; i++)
            {

                triangleCounter = waterPoint3DCollection.Count;

                waterPoint3DCollection.Add(new Point3D(-halfSize, -halfSize, map.WaterLevel - i * dfMul - halfheight));
                waterPoint3DCollection.Add(new Point3D(+halfSize, +halfSize, map.WaterLevel - i * dfMul - halfheight));
                waterPoint3DCollection.Add(new Point3D(-halfSize, +halfSize, map.WaterLevel - i * dfMul - halfheight));
                waterPoint3DCollection.Add(new Point3D(+halfSize, -halfSize, map.WaterLevel - i * dfMul - halfheight));

                triangleIndices.Add(triangleCounter);
                triangleIndices.Add(triangleCounter + 1);
                triangleIndices.Add(triangleCounter + 2);
                triangleIndices.Add(triangleCounter);
                triangleIndices.Add(triangleCounter + 3);
                triangleIndices.Add(triangleCounter + 1);
            }
            ((MeshGeometry3D)myWaterGeometryModel.Geometry).Positions = waterPoint3DCollection;
            ((MeshGeometry3D)myWaterGeometryModel.Geometry).TriangleIndices = triangleIndices;

            Map3D.Children.Add(myWaterGeometryModel);
        }

        private void ToolClick(object sender, MouseButtonEventArgs e)
        {
            int xMouseCoordinate = 511 - (int)e.GetPosition(Map).X;
            int yMouseCoordinate = 511 - (int)e.GetPosition(Map).Y;

            if (CurrentMapMode != MapMode.Full && CurrentMapMode != MapMode.ThreeDimensional)
            {
                byte[] data;
                switch (CurrentCursorMode)
                {
                    case CursorMode.Select:
                        MessageBox.Show("Location : X:" + xMouseCoordinate + " | Y:" + yMouseCoordinate);
                        break;
                    case CursorMode.Pipette:
                        switch (CurrentMapMode)
                        {
                            case MapMode.HeightMap:
                                SelectedColorCode.Text = CurrentMap.HeightMapDigitsThreeAndFour[xMouseCoordinate, yMouseCoordinate].ToString("X2") + CurrentMap.HeightMapDigitsOneAndTwo[xMouseCoordinate, yMouseCoordinate].ToString("X2");
                                break;
                            case MapMode.MainTextureMap:
                                SelectedColorCode.Text = CurrentMap.MainTextureMap[xMouseCoordinate, yMouseCoordinate].ToString("X1");
                                break;
                            case MapMode.FoliageMap:
                                SelectedColorCode.Text = CurrentMap.FoliageMap[xMouseCoordinate, yMouseCoordinate].ToString("X1");
                                break;
                            case MapMode.WallTextureMap:
                                SelectedColorCode.Text = CurrentMap.WallTextureMap[xMouseCoordinate, yMouseCoordinate].ToString("X1");
                                break;
                            case MapMode.UnknownMap:
                                SelectedColorCode.Text = CurrentMap.UnknownMap[xMouseCoordinate, yMouseCoordinate].ToString("X1");
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
                                    CurrentMap.EditMapData(CurrentCursorMode, CurrentCursorSubMode, CursorDiameter, xMouseCoordinate, yMouseCoordinate, SelectedColorCode.Text, CurrentMap.HeightMapDigitsOneAndTwo, CurrentMap.HeightMapDigitsThreeAndFour);
                                    break;
                                case MapMode.MainTextureMap:
                                    CurrentMap.EditMapData(CurrentCursorMode, CurrentCursorSubMode, CursorDiameter, xMouseCoordinate, yMouseCoordinate, SelectedColorCode.Text, CurrentMap.MainTextureMap);
                                    break;
                                case MapMode.FoliageMap:
                                    CurrentMap.EditMapData(CurrentCursorMode, CurrentCursorSubMode, CursorDiameter, xMouseCoordinate, yMouseCoordinate, SelectedColorCode.Text, CurrentMap.FoliageMap);
                                    break;
                                case MapMode.WallTextureMap:
                                    CurrentMap.EditMapData(CurrentCursorMode, CurrentCursorSubMode, CursorDiameter, xMouseCoordinate, yMouseCoordinate, SelectedColorCode.Text, CurrentMap.WallTextureMap);
                                    break;
                                case MapMode.UnknownMap:
                                    CurrentMap.EditMapData(CurrentCursorMode, CurrentCursorSubMode, CursorDiameter, xMouseCoordinate, yMouseCoordinate, SelectedColorCode.Text, CurrentMap.UnknownMap);
                                    break;
                            }
                            data = CurrentMap.CreateTiffData(CurrentMap.Width, CurrentMap.Height, CurrentMapMode);
                            DrawTiffImage(CurrentMap.Width, CurrentMap.Height, DrawingType.Map, data);
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
                                    CurrentMap.EditMapData(CurrentCursorMode, CurrentCursorSubMode, CursorDiameter, xMouseCoordinate, yMouseCoordinate, SelectedColorCode.Text, CurrentMap.HeightMapDigitsOneAndTwo, CurrentMap.HeightMapDigitsThreeAndFour);
                                    break;
                                case MapMode.MainTextureMap:
                                    CurrentMap.EditMapData(CurrentCursorMode, CurrentCursorSubMode, CursorDiameter, xMouseCoordinate, yMouseCoordinate, SelectedColorCode.Text, CurrentMap.MainTextureMap);
                                    break;
                                case MapMode.FoliageMap:
                                    CurrentMap.EditMapData(CurrentCursorMode, CurrentCursorSubMode, CursorDiameter, xMouseCoordinate, yMouseCoordinate, SelectedColorCode.Text, CurrentMap.FoliageMap);
                                    break;
                                case MapMode.WallTextureMap:
                                    CurrentMap.EditMapData(CurrentCursorMode, CurrentCursorSubMode, CursorDiameter, xMouseCoordinate, yMouseCoordinate, SelectedColorCode.Text, CurrentMap.WallTextureMap);
                                    break;
                                case MapMode.UnknownMap:
                                    CurrentMap.EditMapData(CurrentCursorMode, CurrentCursorSubMode, CursorDiameter, xMouseCoordinate, yMouseCoordinate, SelectedColorCode.Text, CurrentMap.UnknownMap);
                                    break;
                            }
                            data = CurrentMap.CreateTiffData(CurrentMap.Width, CurrentMap.Height, CurrentMapMode);
                            DrawTiffImage(CurrentMap.Width, CurrentMap.Height, DrawingType.Map, data);
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
                                CurrentMap.RotateMapData(CurrentMap.HeightMapDigitsOneAndTwo);
                                CurrentMap.RotateMapData(CurrentMap.HeightMapDigitsThreeAndFour);
                                break;
                            case MapMode.MainTextureMap:
                                CurrentMap.RotateMapData(CurrentMap.MainTextureMap);
                                break;
                            case MapMode.FoliageMap:
                                CurrentMap.RotateMapData(CurrentMap.FoliageMap);
                                break;
                            case MapMode.WallTextureMap:
                                CurrentMap.RotateMapData(CurrentMap.WallTextureMap);
                                break;
                            case MapMode.UnknownMap:
                                CurrentMap.RotateMapData(CurrentMap.UnknownMap);
                                break;
                        }
                        data = CurrentMap.CreateTiffData(CurrentMap.Width, CurrentMap.Height, CurrentMapMode);
                        DrawTiffImage(CurrentMap.Width, CurrentMap.Height, DrawingType.Map, data);
                        break;
                    default:
                        break;
                }
            }
            else if (CurrentMapMode == MapMode.Full)
            {
                MessageBox.Show("Location : X:" + xMouseCoordinate + " | Y:" + yMouseCoordinate);
            }
        }

        private void MapModeChanged(object sender, SelectionChangedEventArgs e)
        {
            byte[] mapData, selectedData;
            Map3D.Children.Clear();
            switch (MapModeDropDown.SelectedIndex)
            {
                case 0:
                    CurrentMapMode = MapMode.HeightMap;
                    UpdateCursor();
                    SelectedColorCode.Text = "0000";
                    mapData = CurrentMap.CreateTiffData(CurrentMap.Width, CurrentMap.Height, CurrentMapMode);
                    DrawTiffImage(CurrentMap.Width, CurrentMap.Height, DrawingType.Map, mapData);
                    selectedData = CreateTiffDataForSelectedColor(CurrentMap.Width, CurrentMap.Height);
                    DrawTiffImage((int)SelectedColorImage.Width, (int)SelectedColorImage.Height, DrawingType.SelectedColor, selectedData);
                    ShowImportExportButtons();
                    UpdateToolBar();
                    break;
                case 1:
                    CurrentMapMode = MapMode.MainTextureMap;
                    UpdateCursor();
                    SelectedColorCode.Text = "0";
                    mapData = CurrentMap.CreateTiffData(CurrentMap.Width, CurrentMap.Height, CurrentMapMode);
                    DrawTiffImage(CurrentMap.Width, CurrentMap.Height, DrawingType.Map, mapData);
                    selectedData = CreateTiffDataForSelectedColor(CurrentMap.Width, CurrentMap.Height);
                    DrawTiffImage((int)SelectedColorImage.Width, (int)SelectedColorImage.Height, DrawingType.SelectedColor, selectedData);
                    ShowImportExportButtons();
                    UpdateToolBar();
                    break;
                case 2:
                    CurrentMapMode = MapMode.FoliageMap;
                    UpdateCursor();
                    SelectedColorCode.Text = "0";
                    mapData = CurrentMap.CreateTiffData(CurrentMap.Width, CurrentMap.Height, CurrentMapMode);
                    DrawTiffImage(CurrentMap.Width, CurrentMap.Height, DrawingType.Map, mapData);
                    selectedData = CreateTiffDataForSelectedColor(CurrentMap.Width, CurrentMap.Height);
                    DrawTiffImage((int)SelectedColorImage.Width, (int)SelectedColorImage.Height, DrawingType.SelectedColor, selectedData);
                    ShowImportExportButtons();
                    UpdateToolBar();
                    break;
                case 3:
                    CurrentMapMode = MapMode.WallTextureMap;
                    UpdateCursor();
                    SelectedColorCode.Text = "0";
                    mapData = CurrentMap.CreateTiffData(CurrentMap.Width, CurrentMap.Height, CurrentMapMode);
                    DrawTiffImage(CurrentMap.Width, CurrentMap.Height, DrawingType.Map, mapData);
                    selectedData = CreateTiffDataForSelectedColor(CurrentMap.Width, CurrentMap.Height);
                    DrawTiffImage((int)SelectedColorImage.Width, (int)SelectedColorImage.Height, DrawingType.SelectedColor, selectedData);
                    ShowImportExportButtons();
                    UpdateToolBar();
                    break;
                case 4:
                    CurrentMapMode = MapMode.UnknownMap;
                    UpdateCursor();
                    SelectedColorCode.Text = "0";
                    mapData = CurrentMap.CreateTiffData(CurrentMap.Width, CurrentMap.Height, CurrentMapMode);
                    DrawTiffImage(CurrentMap.Width, CurrentMap.Height, DrawingType.Map, mapData);
                    selectedData = CreateTiffDataForSelectedColor(CurrentMap.Width, CurrentMap.Height);
                    DrawTiffImage((int)SelectedColorImage.Width, (int)SelectedColorImage.Height, DrawingType.SelectedColor, selectedData);
                    ShowImportExportButtons();
                    UpdateToolBar();
                    break;
                case 5:
                    CurrentMapMode = MapMode.Full;
                    Mouse.OverrideCursor = null;
                    mapData = CurrentMap.CreateTiffData(CurrentMap.Width, CurrentMap.Height, CurrentMapMode);
                    DrawTiffImage(CurrentMap.Width, CurrentMap.Height, DrawingType.Map, mapData);
                    HideImportExportButtons();
                    HideSelectedColor();
                    HideCursorModes();
                    HideCursorSubModes();
                    HideCursorSlider();
                    break;
                case 6:
                    CurrentMapMode = MapMode.ThreeDimensional;
                    Mouse.OverrideCursor = null;
                    mapData = CurrentMap.CreateTiffData(CurrentMap.Width, CurrentMap.Height, MapMode.ThreeDimensional);
                    DrawTiffImage(CurrentMap.Width, CurrentMap.Height, DrawingType.Map, mapData);
                    Draw3DTerrain(CurrentMap);
                    DrawWater(CurrentMap);
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
                byte[] data = CreateTiffDataForSelectedColor(CurrentMap.Width, CurrentMap.Height);
                DrawTiffImage((int)SelectedColorImage.Width, (int)SelectedColorImage.Height, DrawingType.SelectedColor, data);

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
