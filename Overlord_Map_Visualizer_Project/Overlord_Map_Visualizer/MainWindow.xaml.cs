using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;
using Pen = System.Drawing.Pen;
using DrawingPoint = System.Drawing.Point;
using System.Windows.Media.Media3D;

namespace Overlord_Map_Visualizer
{
    public partial class MainWindow : Window
    {
        private OverlordMap CurrentMap = new OverlordMap();

        private CursorManagement CurrentCursor = new CursorManagement();

        private MapMode CurrentMapMode;

        private bool IsAnyMapLoaded = false;

        Trackball trackball = new Trackball();

        public MainWindow()
        {
            InitializeComponent();
            Initialise();
            DrawCoordinateSystem();
        }

        private void Initialise()
        {
            CurrentCursor.Select = cursorModeSelect;
            CurrentCursor.Pipette = cursorModePipette;
            CurrentCursor.Square = cursorModeSquare;
            CurrentCursor.Circle = cursorModeCircle;
            CurrentCursor.Rotate = cursorModeRotate;

            CurrentCursor.Set = cursorSubModeSet;
            CurrentCursor.Add = cursorSubModeAdd;
            CurrentCursor.Sub = cursorSubModeSub;

            CurrentCursor.SizeSlider = CursorSizeSlider;

            CurrentCursor.HighlightCurrentCursorMode();
            CurrentCursor.HighlightCurrentCursorSubMode();
            SelectedColorCode.Text = "0000";
            cursorDiameterLabel.Content = "Cursor Diameter: " + CursorSizeSlider.Value;

            trackball.Attach(this);
            trackball.Slaves.Add(mainViewport);
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
            FileReader reader = new FileReader();
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
                        offset = reader.GetMapDataOffset(CurrentMap.FilePath, CurrentMap.Width,CurrentMap.Height);
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
                byte[] data = reader.ReadMapDataFromFile(openFileDialog.FileName, offset, CurrentMap.Width, CurrentMap.Height, bytesPerPoint);
                CurrentMap.SetMapData(data, bytesPerPoint, mapMode, isTiffImage);
                CurrentMap.WaterLevel = reader.GetMapWaterLevel(CurrentMap);

                if(CurrentMapMode == MapMode.ThreeDimensional)
                {
                    Map3DTerrainAndWater.Children.Clear();

                    GeometryModel3D terrainGeometryModel = CurrentMap.GetTerrainGeometryModel();
                    Draw3DModel(terrainGeometryModel);
                    GeometryModel3D waterGeometryModel = CurrentMap.GetWaterGeometryModel();
                    Draw3DModel(waterGeometryModel);
                }
                else
                {
                    TiffImage image = new TiffImage(CurrentMap.Width, CurrentMap.Height, CurrentMap.CreateTiffData(CurrentMap.Width, CurrentMap.Height, CurrentMapMode));
                    DrawTiffImage(image.Encode(), DrawingType.Map);
                }

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
            FileWriter writer = new FileWriter();
            FileReader reader = new FileReader();
            byte[] data;
            int bytesPerPoint;
            string filePath;
            int offset;

            if (button.Name == "ExportToOMPFileButton")
            {
                offset = reader.GetMapDataOffset(CurrentMap.FilePath,CurrentMap.Width,CurrentMap.Height);
                bytesPerPoint = 4;
                filePath = CurrentMap.FilePath;
                data = CurrentMap.GetMapData(bytesPerPoint, MapMode.Full);
                writer.WriteMapDataToFile(filePath, data, offset, CurrentMap.Width, CurrentMap.Height, bytesPerPoint);
            }
            else
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
                if(button.Name == "ExportMapImage")
                {
                    dialogTitle = "Select Directory and file name for the overlord map image";
                    fileExtension = "tiff";
                    filter = "tiff image files (*.tiff)|*.tiff";
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
                    if(button.Name == "ExportMapImage")
                    {
                        data = CurrentMap.CreateTiffData(CurrentMap.Width, CurrentMap.Height, CurrentMapMode);
                        writer.WriteTiffDataToFile(saveFileDialog.FileName, data, CurrentMap.Width, CurrentMap.Height);
                    }
                    else
                    {
                        data = CurrentMap.GetMapData(bytesPerPoint, CurrentMapMode);
                        writer.WriteMapDataToFile(saveFileDialog.FileName, data, 0, CurrentMap.Width, CurrentMap.Height, bytesPerPoint);
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

        private void DrawTiffImage(TiffBitmapEncoder encoder, DrawingType type)
        {
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
                        allMapObjectLocationsBitmap = DrawMinionGate(allMapObjectLocationsBitmap, CurrentMap.ObjectList[i].X, CurrentMap.ObjectList[i].Y, solidBrush);
                        break;
                    case OverlordObjectType.RedMinionGate:
                        solidBrush = new SolidBrush(Color.FromArgb(255, 255, 000, 000));
                        allMapObjectLocationsBitmap = DrawMinionGate(allMapObjectLocationsBitmap, CurrentMap.ObjectList[i].X, CurrentMap.ObjectList[i].Y, solidBrush);
                        break;
                    case OverlordObjectType.GreenMinionGate:
                        solidBrush = new SolidBrush(Color.FromArgb(255, 000, 255, 000));
                        allMapObjectLocationsBitmap = DrawMinionGate(allMapObjectLocationsBitmap, CurrentMap.ObjectList[i].X, CurrentMap.ObjectList[i].Y, solidBrush);
                        break;
                    case OverlordObjectType.BlueMinionGate:
                        solidBrush = new SolidBrush(Color.FromArgb(255, 000, 000, 255));
                        allMapObjectLocationsBitmap = DrawMinionGate(allMapObjectLocationsBitmap, CurrentMap.ObjectList[i].X, CurrentMap.ObjectList[i].Y, solidBrush);
                        break;
                    case OverlordObjectType.TowerGate:
                        allMapObjectLocationsBitmap = DrawTowerGate(allMapObjectLocationsBitmap, CurrentMap.ObjectList[i].X, CurrentMap.ObjectList[i].Y);
                        break;
                    default:
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

        private void Draw3DModel(GeometryModel3D model3D)
        {
            Map3DTerrainAndWater.Children.Add(model3D);
        }

        private void ToolClick(object sender, MouseButtonEventArgs e)
        {
            CurrentCursor.X = 511 - (int)e.GetPosition(Map).X;
            CurrentCursor.Y = 511 - (int)e.GetPosition(Map).Y;

            if (CurrentMapMode != MapMode.Full && CurrentMapMode != MapMode.ThreeDimensional)
            {
                TiffImage image;

                if (CurrentCursor.Mode == CursorMode.Select)
                {
                    MessageBox.Show("Location : X:" + CurrentCursor.X + " | Y:" + CurrentCursor.Y);
                }
                else if (CurrentCursor.Mode == CursorMode.Pipette)
                {
                    switch (CurrentMapMode)
                    {
                        case MapMode.HeightMap:
                            SelectedColorCode.Text = CurrentMap.HeightMapDigitsThreeAndFour[CurrentCursor.X, CurrentCursor.Y].ToString("X2") + CurrentMap.HeightMapDigitsOneAndTwo[CurrentCursor.X, CurrentCursor.Y].ToString("X2");
                            break;
                        case MapMode.MainTextureMap:
                            SelectedColorCode.Text = CurrentMap.MainTextureMap[CurrentCursor.X, CurrentCursor.Y].ToString("X1");
                            break;
                        case MapMode.FoliageMap:
                            SelectedColorCode.Text = CurrentMap.FoliageMap[CurrentCursor.X, CurrentCursor.Y].ToString("X1");
                            break;
                        case MapMode.WallTextureMap:
                            SelectedColorCode.Text = CurrentMap.WallTextureMap[CurrentCursor.X, CurrentCursor.Y].ToString("X1");
                            break;
                        case MapMode.UnknownMap:
                            SelectedColorCode.Text = CurrentMap.UnknownMap[CurrentCursor.X, CurrentCursor.Y].ToString("X1");
                            break;
                        default:
                            SelectedColorCode.Text = "0000";
                            break;
                    }
                }
                else if (CurrentCursor.Mode == CursorMode.Rotate)
                {
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
                    image = new TiffImage(CurrentMap.Width, CurrentMap.Height, CurrentMap.CreateTiffData(CurrentMap.Width, CurrentMap.Height, CurrentMapMode));
                    DrawTiffImage(image.Encode(), DrawingType.Map);
                }
                else
                {
                    if (SelectedColorCode.Text.Length == GetNeededColorCodeLength())
                    {
                        switch (CurrentMapMode)
                        {
                            case MapMode.HeightMap:
                                CurrentMap.EditMapData(CurrentCursor, SelectedColorCode.Text, CurrentMap.HeightMapDigitsOneAndTwo, CurrentMap.HeightMapDigitsThreeAndFour);
                                break;
                            case MapMode.MainTextureMap:
                                CurrentMap.EditMapData(CurrentCursor, SelectedColorCode.Text, CurrentMap.MainTextureMap);
                                break;
                            case MapMode.FoliageMap:
                                CurrentMap.EditMapData(CurrentCursor, SelectedColorCode.Text, CurrentMap.FoliageMap);
                                break;
                            case MapMode.WallTextureMap:
                                CurrentMap.EditMapData(CurrentCursor, SelectedColorCode.Text, CurrentMap.WallTextureMap);
                                break;
                            case MapMode.UnknownMap:
                                CurrentMap.EditMapData(CurrentCursor, SelectedColorCode.Text, CurrentMap.UnknownMap);
                                break;
                        }
                        image = new TiffImage(CurrentMap.Width, CurrentMap.Height, CurrentMap.CreateTiffData(CurrentMap.Width, CurrentMap.Height, CurrentMapMode));
                        DrawTiffImage(image.Encode(), DrawingType.Map);
                    }
                    else
                    {
                        MessageBox.Show("Error\nThe selected color has to be " + GetNeededColorCodeLength() + " digits long.");
                    }
                }
            }
            else if (CurrentMapMode == MapMode.Full)
            {
                MessageBox.Show("Location : X:" + CurrentCursor.X + " | Y:" + CurrentCursor.Y);
            }
        }

        private void MapModeChanged(object sender, SelectionChangedEventArgs e)
        {
            TiffImage mapImage, selectedColorImage;
            Map3DTerrainAndWater.Children.Clear();
            trackball.Enabled = false;
            Reset.Visibility = Visibility.Hidden;

            switch (MapModeDropDown.SelectedIndex)
            {
                case 0:
                    CurrentMapMode = MapMode.HeightMap;
                    CurrentCursor.Update();
                    SelectedColorCode.Text = "0000";
                    mapImage = new TiffImage(CurrentMap.Width, CurrentMap.Height, CurrentMap.CreateTiffData(CurrentMap.Width, CurrentMap.Height, CurrentMapMode));
                    DrawTiffImage(mapImage.Encode(), DrawingType.Map);
                    selectedColorImage = new TiffImage((int)SelectedColorImage.Width, (int)SelectedColorImage.Height, CreateTiffDataForSelectedColor((int)SelectedColorImage.Width, (int)SelectedColorImage.Height));
                    DrawTiffImage(selectedColorImage.Encode(), DrawingType.SelectedColor);
                    ShowImportExportButtons();
                    UpdateToolBar();
                    break;
                case 1:
                    CurrentMapMode = MapMode.MainTextureMap;
                    CurrentCursor.Update();
                    SelectedColorCode.Text = "0";
                    mapImage = new TiffImage(CurrentMap.Width, CurrentMap.Height, CurrentMap.CreateTiffData(CurrentMap.Width, CurrentMap.Height, CurrentMapMode));
                    DrawTiffImage(mapImage.Encode(), DrawingType.Map);
                    selectedColorImage = new TiffImage((int)SelectedColorImage.Width, (int)SelectedColorImage.Height, CreateTiffDataForSelectedColor((int)SelectedColorImage.Width, (int)SelectedColorImage.Height));
                    DrawTiffImage(selectedColorImage.Encode(), DrawingType.SelectedColor);
                    ShowImportExportButtons();
                    UpdateToolBar();
                    break;
                case 2:
                    CurrentMapMode = MapMode.FoliageMap;
                    CurrentCursor.Update();
                    SelectedColorCode.Text = "0";
                    mapImage = new TiffImage(CurrentMap.Width, CurrentMap.Height, CurrentMap.CreateTiffData(CurrentMap.Width, CurrentMap.Height, CurrentMapMode));
                    DrawTiffImage(mapImage.Encode(), DrawingType.Map);
                    selectedColorImage = new TiffImage((int)SelectedColorImage.Width, (int)SelectedColorImage.Height, CreateTiffDataForSelectedColor((int)SelectedColorImage.Width, (int)SelectedColorImage.Height));
                    DrawTiffImage(selectedColorImage.Encode(), DrawingType.SelectedColor);
                    ShowImportExportButtons();
                    UpdateToolBar();
                    break;
                case 3:
                    CurrentMapMode = MapMode.WallTextureMap;
                    CurrentCursor.Update();
                    SelectedColorCode.Text = "0";
                    mapImage = new TiffImage(CurrentMap.Width, CurrentMap.Height, CurrentMap.CreateTiffData(CurrentMap.Width, CurrentMap.Height, CurrentMapMode));
                    DrawTiffImage(mapImage.Encode(), DrawingType.Map);
                    selectedColorImage = new TiffImage((int)SelectedColorImage.Width, (int)SelectedColorImage.Height, CreateTiffDataForSelectedColor((int)SelectedColorImage.Width, (int)SelectedColorImage.Height));
                    DrawTiffImage(selectedColorImage.Encode(), DrawingType.SelectedColor);
                    ShowImportExportButtons();
                    UpdateToolBar();
                    break;
                case 4:
                    CurrentMapMode = MapMode.UnknownMap;
                    CurrentCursor.Update();
                    SelectedColorCode.Text = "0";
                    mapImage = new TiffImage(CurrentMap.Width, CurrentMap.Height, CurrentMap.CreateTiffData(CurrentMap.Width, CurrentMap.Height, CurrentMapMode));
                    DrawTiffImage(mapImage.Encode(), DrawingType.Map);
                    selectedColorImage = new TiffImage((int)SelectedColorImage.Width, (int)SelectedColorImage.Height, CreateTiffDataForSelectedColor((int)SelectedColorImage.Width, (int)SelectedColorImage.Height));
                    DrawTiffImage(selectedColorImage.Encode(), DrawingType.SelectedColor);
                    ShowImportExportButtons();
                    UpdateToolBar();
                    break;
                case 5:
                    CurrentMapMode = MapMode.Full;
                    Mouse.OverrideCursor = null;
                    mapImage = new TiffImage(CurrentMap.Width, CurrentMap.Height, CurrentMap.CreateTiffData(CurrentMap.Width, CurrentMap.Height, CurrentMapMode));
                    DrawTiffImage(mapImage.Encode(), DrawingType.Map);
                    HideImportExportButtons();
                    HideSelectedColor();
                    CurrentCursor.HideCursorModes();
                    CurrentCursor.HideCursorSubModes();
                    CurrentCursor.HideCursorSlider();
                    break;
                case 6:
                    CurrentMapMode = MapMode.ThreeDimensional;
                    Mouse.OverrideCursor = null;
                    mapImage = new TiffImage(CurrentMap.Width, CurrentMap.Height, CurrentMap.CreateTiffData(CurrentMap.Width, CurrentMap.Height, CurrentMapMode));
                    DrawTiffImage(mapImage.Encode(), DrawingType.Map);
                    GeometryModel3D terrainGeometryModel = CurrentMap.GetTerrainGeometryModel();
                    Draw3DModel(terrainGeometryModel);
                    GeometryModel3D waterGeometryModel = CurrentMap.GetWaterGeometryModel();
                    Draw3DModel(waterGeometryModel);
                    trackball.Enabled = true;
                    HideImportExportButtons();
                    Reset.Visibility = Visibility.Visible;
                    HideSelectedColor();
                    CurrentCursor.HideCursorModes();
                    CurrentCursor.HideCursorSubModes();
                    CurrentCursor.HideCursorSlider();
                    break;
                default:
                    break;
            }

            if (CurrentCursor.Mode == CursorMode.Square)
            {
                CurrentCursor.Update();
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
                TiffImage selectedColorImage = new TiffImage((int)SelectedColorImage.Width, (int)SelectedColorImage.Height, CreateTiffDataForSelectedColor((int)SelectedColorImage.Width, (int)SelectedColorImage.Width));
                DrawTiffImage(selectedColorImage.Encode(), DrawingType.SelectedColor);

                if (CurrentMapMode == MapMode.HeightMap)
                {
                    int digitOne = Convert.ToInt32("" + SelectedColorCode.Text[3], 16);
                    int digitTwo = Convert.ToInt32("" + SelectedColorCode.Text[2], 16);
                    int digitThree = Convert.ToInt32("" + SelectedColorCode.Text[1], 16);
                    SelectedColorHeight.Content = "This color corresponds to a height of\r\n" + (((digitThree * Math.Pow(16, 1)) + (digitTwo * Math.Pow(16, 0)) + (digitOne * Math.Pow(16, -1))) / 2);
                }

                if (CurrentCursor.Mode == CursorMode.Square)
                {
                    CurrentCursor.Update();
                }
            }
        }

        private void CursorMode_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            CurrentCursor.Mode = (CursorMode)Enum.Parse(typeof(CursorMode), (string)button.Content);
            CurrentCursor.Update();
            UpdateToolBar();
        }

        private void CursorSubMode_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            CurrentCursor.SubMode = (CursorSubMode)Enum.Parse(typeof(CursorSubMode), (string)button.Content);
            UpdateToolBar();
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

        private void UpdateToolBar()
        {
            CurrentCursor.HighlightCurrentCursorMode();
            CurrentCursor.HighlightCurrentCursorSubMode();

            if (CurrentCursor.Mode == CursorMode.Select | CurrentCursor.Mode == CursorMode.Rotate)
            {
                CurrentCursor.ShowCursorModes();
                CurrentCursor.HideCursorSubModes();
                HideSelectedColor();
                CurrentCursor.HideCursorSlider();
            }
            else if (CurrentCursor.Mode == CursorMode.Square | CurrentCursor.Mode == CursorMode.Circle)
            {
                CurrentCursor.ShowCursorModes();
                CurrentCursor.ShowCursorSubModes();
                ShowSelectedColor();
                CurrentCursor.ShowCursorSlider();
            }
            else
            {
                CurrentCursor.ShowCursorModes();
                CurrentCursor.HideCursorSubModes();
                ShowSelectedColor();
                CurrentCursor.HideCursorSlider();
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsAnyMapLoaded)
            {
                cursorDiameterLabel.Content = "Cursor Diameter: " + CursorSizeSlider.Value;
                if (CurrentCursor.Mode == CursorMode.Square || CurrentCursor.Mode == CursorMode.Circle)
                {
                    CurrentCursor.Update();
                }
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            trackball.Reset();
        }
    }
}
