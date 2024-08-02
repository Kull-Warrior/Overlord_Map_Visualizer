using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using MediaBrushes = System.Windows.Media.Brushes;

namespace Overlord_Map_Visualizer
{
    class OverlordMap
    {
        public string FilePath { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public double WaterLevel { get; set; }

        public byte[,] HeightMapDigitsOneAndTwo { get; set; }
        public byte[,] HeightMapDigitsThreeAndFour { get; set; }
        public byte[,] MainTextureMap { get; set; }
        public byte[,] FoliageMap { get; set; }
        public byte[,] WallTextureMap { get; set; }
        public byte[,] UnknownMap { get; set; }

        public List<OverlordObject> ObjectList { get; set; }

        public OverlordMap()
        {
            FilePath = "";
            Width = 512;
            Height = 512;
            WaterLevel = 0;
            HeightMapDigitsOneAndTwo = new byte[Width, Height];
            HeightMapDigitsThreeAndFour = new byte[Width, Height];

            MainTextureMap = new byte[Width, Height];
            FoliageMap = new byte[Width, Height];
            WallTextureMap = new byte[Width, Height];
            UnknownMap = new byte[Width, Height];

            ObjectList = new List<OverlordObject>();
        }

        public OverlordMap(string filePath, int width, int height, double waterLevel, byte[,] heightMapDigitsOneAndTwo, byte[,] heightMapDigitsThreeAndFour, byte[,] mainTextureMap, byte[,] foliageMap, byte[,] wallTextureMap, byte[,] unknownMap, List<OverlordObject> objectList)
        {
            FilePath = filePath;
            Width = width;
            Height = height;
            WaterLevel = waterLevel;
            HeightMapDigitsOneAndTwo = heightMapDigitsOneAndTwo;
            HeightMapDigitsThreeAndFour = heightMapDigitsThreeAndFour;
            MainTextureMap = mainTextureMap;
            FoliageMap = foliageMap;
            WallTextureMap = wallTextureMap;
            UnknownMap = unknownMap;
            ObjectList = objectList;
        }

        public int[] GetTiffRgbFromFourBitRgbPalette(byte value)
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

        public int GetFourBitRgbPaletteIndexFromTiffRgb(int red, int green, int blue)
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

        public byte[] GetMapData(int bytesPerPoint, MapMode mapMode)
        {
            int totalNumberOfBytes = Width * Height * bytesPerPoint;
            byte[] data = new byte[totalNumberOfBytes];
            int xOffset = bytesPerPoint;
            int yOffset = 0;
            int numberOfBytesInRow = Width * bytesPerPoint;
            int totalOffset;

            for (int y = 0; y < Height; y++)
            {
                if (y != 0)
                {
                    yOffset = y * numberOfBytesInRow;
                }
                for (int x = 0; x < Width; x++)
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
                            data[totalOffset] = FoliageMap[x, y];
                            break;
                        case MapMode.WallTextureMap:
                            data[totalOffset] = WallTextureMap[x, y];
                            break;
                        case MapMode.UnknownMap:
                            data[totalOffset] = UnknownMap[x, y];
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

        public void SetMapData(byte[] data, int bytesPerPoint, MapMode mapMode, bool isTiffImage)
        {
            int xOffset = bytesPerPoint;
            int yOffset = 0;
            int numberOfBytesInRow = Width * bytesPerPoint;
            int totalOffset;

            for (int y = 0; y < Height; y++)
            {
                if (y != 0)
                {
                    yOffset = y * numberOfBytesInRow;
                }
                for (int x = 0; x < Width; x++)
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
                                WallTextureMap[x, y] = data[totalOffset];
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
                                UnknownMap[x, y] = data[totalOffset];
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

        public byte[] CreateTiffData(int width, int height, MapMode mapMode)
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
                    switch (mapMode)
                    {
                        case MapMode.HeightMap:
                            grayScale = ((HeightMapDigitsThreeAndFour[x, y] << 8) & 0x0FFF) + HeightMapDigitsOneAndTwo[x, y];
                            grayScale = grayScale * 65535 / 4095;

                            data[totalOffset] = (byte)(grayScale & 0x00FF);
                            data[totalOffset + 1] = (byte)((grayScale & 0xFF00) >> 8);
                            data[totalOffset + 2] = (byte)(grayScale & 0x00FF);
                            data[totalOffset + 3] = (byte)((grayScale & 0xFF00) >> 8);
                            data[totalOffset + 4] = (byte)(grayScale & 0x00FF);
                            data[totalOffset + 5] = (byte)((grayScale & 0xFF00) >> 8);
                            break;
                        case MapMode.MainTextureMap:
                            rgb = GetTiffRgbFromFourBitRgbPalette(MainTextureMap[x, y]);
                            red = rgb[0];
                            green = rgb[1];
                            blue = rgb[2];

                            data[totalOffset] = (byte)(blue & 0x00FF);
                            data[totalOffset + 1] = (byte)((blue & 0xFF00) >> 8);
                            data[totalOffset + 2] = (byte)(green & 0x00FF);
                            data[totalOffset + 3] = (byte)((green & 0xFF00) >> 8);
                            data[totalOffset + 4] = (byte)(red & 0x00FF);
                            data[totalOffset + 5] = (byte)((red & 0xFF00) >> 8);
                            break;
                        case MapMode.FoliageMap:
                            rgb = GetTiffRgbFromFourBitRgbPalette(FoliageMap[x, y]);
                            red = rgb[0];
                            green = rgb[1];
                            blue = rgb[2];

                            data[totalOffset] = (byte)(blue & 0x00FF);
                            data[totalOffset + 1] = (byte)((blue & 0xFF00) >> 8);
                            data[totalOffset + 2] = (byte)(green & 0x00FF);
                            data[totalOffset + 3] = (byte)((green & 0xFF00) >> 8);
                            data[totalOffset + 4] = (byte)(red & 0x00FF);
                            data[totalOffset + 5] = (byte)((red & 0xFF00) >> 8);
                            break;
                        case MapMode.WallTextureMap:
                            rgb = GetTiffRgbFromFourBitRgbPalette(WallTextureMap[x, y]);
                            red = rgb[0];
                            green = rgb[1];
                            blue = rgb[2];

                            data[totalOffset] = (byte)(blue & 0x00FF);
                            data[totalOffset + 1] = (byte)((blue & 0xFF00) >> 8);
                            data[totalOffset + 2] = (byte)(green & 0x00FF);
                            data[totalOffset + 3] = (byte)((green & 0xFF00) >> 8);
                            data[totalOffset + 4] = (byte)(red & 0x00FF);
                            data[totalOffset + 5] = (byte)((red & 0xFF00) >> 8);
                            break;
                        case MapMode.UnknownMap:
                            rgb = GetTiffRgbFromFourBitRgbPalette(UnknownMap[x, y]);
                            red = rgb[0];
                            green = rgb[1];
                            blue = rgb[2];

                            data[totalOffset] = (byte)(blue & 0x00FF);
                            data[totalOffset + 1] = (byte)((blue & 0xFF00) >> 8);
                            data[totalOffset + 2] = (byte)(green & 0x00FF);
                            data[totalOffset + 3] = (byte)((green & 0xFF00) >> 8);
                            data[totalOffset + 4] = (byte)(red & 0x00FF);
                            data[totalOffset + 5] = (byte)((red & 0xFF00) >> 8);
                            break;
                        case MapMode.Full:
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
                            break;
                        case MapMode.ThreeDimensional:
                            data[totalOffset] = 0xFF;
                            data[totalOffset + 1] = 0xFF;
                            data[totalOffset + 2] = 0xFF;
                            data[totalOffset + 3] = 0xFF;
                            data[totalOffset + 4] = 0xFF;
                            data[totalOffset + 5] = 0xFF;
                            break;
                        default:
                            data[totalOffset] = 0xFF;
                            data[totalOffset + 1] = 0xFF;
                            data[totalOffset + 2] = 0xFF;
                            data[totalOffset + 3] = 0xFF;
                            data[totalOffset + 4] = 0xFF;
                            data[totalOffset + 5] = 0xFF;
                            break;
                    }
                }
            }
            return data;
        }

        public void EditMapData(CursorMode CurrentCursorMode, CursorSubMode CurrentCursorSubMode, int cursorDiameter, int xMouseCoordinate, int yMouseCoordinate, string colorCode, byte[,] data)
        {
            int yMin = 0;
            int xMin = 0;
            int xMax = 511;
            int yMax = 511;
            int cursorRadius = cursorDiameter / 2;

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
                    if (CurrentCursorMode == CursorMode.Square || (((x - xMouseCoordinate) * (x - xMouseCoordinate)) + ((y - yMouseCoordinate) * (y - yMouseCoordinate)) < (int)Math.Ceiling((decimal)cursorDiameter / 2) * (int)Math.Ceiling((decimal)cursorDiameter / 2)))
                    {
                        byte selectedValue = Convert.ToByte(colorCode, 16);
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

        public void EditMapData(CursorMode CurrentCursorMode, CursorSubMode CurrentCursorSubMode, int cursorDiameter, int xMouseCoordinate, int yMouseCoordinate, string colorCode, byte[,] lowerByteData, byte[,] higherByteData)
        {
            int yMin = 0;
            int xMin = 0;
            int xMax = 511;
            int yMax = 511;
            int cursorRadius = cursorDiameter / 2;

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
                    if (CurrentCursorMode == CursorMode.Square || (((x - xMouseCoordinate) * (x - xMouseCoordinate)) + ((y - yMouseCoordinate) * (y - yMouseCoordinate)) < (int)Math.Ceiling((decimal)cursorDiameter / 2) * (int)Math.Ceiling((decimal)cursorDiameter / 2)))
                    {
                        byte[] tempByteArray = new byte[2];

                        tempByteArray[0] = Convert.ToByte("" + colorCode[2] + colorCode[3], 16);
                        tempByteArray[1] = Convert.ToByte("" + colorCode[0] + colorCode[1], 16);

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

        public void RotateMapData(byte[,] byteData)
        {
            for (int x = 0; x < Width / 2; x++)
            {
                for (int y = x; y < Height - x - 1; y++)
                {
                    byte temp = byteData[x, y];

                    byteData[x, y] = byteData[Width - 1 - y, x];

                    byteData[Width - 1 - y, x] = byteData[Width - 1 - x, Height - 1 - y];

                    byteData[Width - 1 - x, Height - 1 - y] = byteData[y, Height - 1 - x];

                    byteData[y, Height - 1 - x] = temp;
                }
            }
        }

        public float[,] GetFloatMap()
        {
            float[,] floatMap = new float[Width, Height];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    double highestDigit = Math.Pow(16, 1) * (HeightMapDigitsThreeAndFour[x, y] & 0x0F);
                    double middleDigit = Math.Pow(16, 0) * ((HeightMapDigitsOneAndTwo[x, y] & 0xF0) >> 4);
                    double smallestDigit = Math.Pow(16, -1) * (HeightMapDigitsOneAndTwo[x, y] & 0x0F);
                    
                    floatMap[x, y] = (float)(highestDigit + middleDigit + smallestDigit) / 2;
                }
            }

            return floatMap;
        }

        public GeometryModel3D GetTerrainGeometryModel()
        {
            float halfSize = Width / 2;
            float halfheight = Height / 2;

            float[,] floatMap = GetFloatMap();

            //creation of the terrain
            GeometryModel3D terrainGeometryModel = new GeometryModel3D(new MeshGeometry3D(), new DiffuseMaterial(MediaBrushes.Gray));
            terrainGeometryModel.BackMaterial = terrainGeometryModel.Material;
            Point3DCollection point3DCollection = new Point3DCollection();
            Int32Collection triangleIndices = new Int32Collection();

            //adding point
            for (var y = 0; y < 512; y++)
            {
                for (var x = 0; x < 512; x++)
                {
                    point3DCollection.Add(new Point3D(x - halfSize, floatMap[x, y] - halfheight, y - halfSize)); ;
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

            Transform3DGroup myTransformGroup = new Transform3DGroup();

            // Create a transform to scale the size.
            ScaleTransform3D myScaleTransform = new ScaleTransform3D();

            // Create a transform to rotate the button
            RotateTransform3D myRotateTransform = new RotateTransform3D();

            //Create a transform to move from one position to other
            TranslateTransform3D myTranslateTransform = new TranslateTransform3D();

            myTransformGroup.Children.Add(myScaleTransform);
            myTransformGroup.Children.Add(myRotateTransform);
            myTransformGroup.Children.Add(myTranslateTransform);

            terrainGeometryModel.Transform = myTransformGroup;

            return terrainGeometryModel;
        }

        public GeometryModel3D GetWaterGeometryModel()
        {
            float halfSize = Width / 2;
            float halfheight = Height / 2;

            // creation of the water layers
            // I'm going to use a series of emissive layer for water
            SolidColorBrush waterSolidColorBrush = new SolidColorBrush(Colors.Blue);
            waterSolidColorBrush.Opacity = 0.2;
            GeometryModel3D myWaterGeometryModel =
            new GeometryModel3D(new MeshGeometry3D(), new EmissiveMaterial(waterSolidColorBrush));
            Point3DCollection waterPoint3DCollection = new Point3DCollection();
            Int32Collection triangleIndices = new Int32Collection();

            myWaterGeometryModel.BackMaterial = myWaterGeometryModel.Material;

            int triangleCounter;
            float dfMul = 5;

            for (int i = 0; i < 10; i++)
            {

                triangleCounter = waterPoint3DCollection.Count;

                waterPoint3DCollection.Add(new Point3D(-halfSize, WaterLevel - i * dfMul - halfheight, - halfSize));
                waterPoint3DCollection.Add(new Point3D(+halfSize, WaterLevel - i * dfMul - halfheight, +halfSize));
                waterPoint3DCollection.Add(new Point3D(-halfSize, WaterLevel - i * dfMul - halfheight, + halfSize));
                waterPoint3DCollection.Add(new Point3D(+halfSize, WaterLevel - i * dfMul - halfheight, - halfSize));

                triangleIndices.Add(triangleCounter);
                triangleIndices.Add(triangleCounter + 1);
                triangleIndices.Add(triangleCounter + 2);
                triangleIndices.Add(triangleCounter);
                triangleIndices.Add(triangleCounter + 3);
                triangleIndices.Add(triangleCounter + 1);
            }
            ((MeshGeometry3D)myWaterGeometryModel.Geometry).Positions = waterPoint3DCollection;
            ((MeshGeometry3D)myWaterGeometryModel.Geometry).TriangleIndices = triangleIndices;

            Transform3DGroup myTransformGroup = new Transform3DGroup();

            // Create a transform to scale the size.
            ScaleTransform3D myScaleTransform = new ScaleTransform3D();

            // Create a transform to rotate the button
            RotateTransform3D myRotateTransform = new RotateTransform3D();

            //Create a transform to move from one position to other
            TranslateTransform3D myTranslateTransform = new TranslateTransform3D();

            myTransformGroup.Children.Add(myScaleTransform);
            myTransformGroup.Children.Add(myRotateTransform);
            myTransformGroup.Children.Add(myTranslateTransform);

            myWaterGeometryModel.Transform = myTransformGroup;

            return myWaterGeometryModel;
        }
    }
}
