﻿using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace Overlord_Map_Visualizer
{
    class OverlordMap
    {
        public string FilePath { get; set; }
        public int Width { get; set; }
        public int Depth { get; set; }

        public string Environment { get; set; }

        public BitmapImage[] TileMapImages { get; set; }

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
            Depth = 512;
            WaterLevel = 0;
            HeightMapDigitsOneAndTwo = new byte[Width, Depth];
            HeightMapDigitsThreeAndFour = new byte[Width, Depth];

            MainTextureMap = new byte[Width, Depth];
            FoliageMap = new byte[Width, Depth];
            WallTextureMap = new byte[Width, Depth];
            UnknownMap = new byte[Width, Depth];

            ObjectList = new List<OverlordObject>();
        }

        public OverlordMap(string filePath, int width, int height, double waterLevel, byte[,] heightMapDigitsOneAndTwo, byte[,] heightMapDigitsThreeAndFour, byte[,] mainTextureMap, byte[,] foliageMap, byte[,] wallTextureMap, byte[,] unknownMap, List<OverlordObject> objectList)
        {
            FilePath = filePath;
            Width = width;
            Depth = height;
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
            int totalNumberOfBytes = Width * Depth * bytesPerPoint;
            byte[] data = new byte[totalNumberOfBytes];
            int xOffset = bytesPerPoint;
            int yOffset = 0;
            int numberOfBytesInRow = Width * bytesPerPoint;
            int totalOffset;

            for (int y = 0; y < Depth; y++)
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

            for (int y = 0; y < Depth; y++)
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

        public void EditMapData(CursorManagement cursor, string colorCode, byte[,] data)
        {
            int yMin = 0;
            int xMin = 0;
            int xMax = Width - 1;
            int yMax = Depth - 1;
            int cursorRadius = (int)(cursor.SizeSlider.Value / 2);

            if ((cursor.X - cursorRadius) >= xMin)
            {
                xMin = cursor.X - cursorRadius;
            }
            if ((cursor.X + cursorRadius) <= xMax)
            {
                xMax = cursor.X + cursorRadius;
            }

            if ((cursor.Y - cursorRadius) >= yMin)
            {
                yMin = cursor.Y - cursorRadius;
            }
            if ((cursor.Y + cursorRadius) <= yMax)
            {
                yMax = cursor.Y + cursorRadius;
            }

            for (int y = yMin; y <= yMax; y++)
            {
                for (int x = xMin; x <= xMax; x++)
                {
                    if (cursor.Mode == CursorMode.Square || (((x - cursor.X) * (x - cursor.X)) + ((y - cursor.Y) * (y - cursor.Y)) < (int)Math.Ceiling((decimal)cursor.SizeSlider.Value / 2) * (int)Math.Ceiling((decimal)cursor.SizeSlider.Value / 2)))
                    {
                        byte selectedValue = Convert.ToByte(colorCode, 16);
                        int pixelValue = data[x, y];
                        switch (cursor.SubMode)
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

        public void EditMapData(CursorManagement cursor, string colorCode, byte[,] lowerByteData, byte[,] higherByteData)
        {
            int yMin = 0;
            int xMin = 0;
            int xMax = Width - 1;
            int yMax = Depth - 1;
            int cursorRadius = (int)(cursor.SizeSlider.Value / 2);

            if ((cursor.X - cursorRadius) >= xMin)
            {
                xMin = cursor.X - cursorRadius;
            }
            if ((cursor.X + cursorRadius) <= xMax)
            {
                xMax = cursor.X + cursorRadius;
            }

            if ((cursor.Y - cursorRadius) >= yMin)
            {
                yMin = cursor.Y - cursorRadius;
            }
            if ((cursor.Y + cursorRadius) <= yMax)
            {
                yMax = cursor.Y + cursorRadius;
            }

            for (int y = yMin; y <= yMax; y++)
            {
                for (int x = xMin; x <= xMax; x++)
                {
                    if (cursor.Mode == CursorMode.Square || (((x - cursor.X) * (x - cursor.X)) + ((y - cursor.Y) * (y - cursor.Y)) < (int)Math.Ceiling((decimal)cursor.SizeSlider.Value / 2) * (int)Math.Ceiling((decimal)cursor.SizeSlider.Value / 2)))
                    {
                        byte[] tempByteArray = new byte[2];

                        tempByteArray[0] = Convert.ToByte("" + colorCode[2] + colorCode[3], 16);
                        tempByteArray[1] = Convert.ToByte("" + colorCode[0] + colorCode[1], 16);

                        int selectedValue = (tempByteArray[1] << 8) + tempByteArray[0];
                        int pixelValue = (higherByteData[x, y] << 8) + lowerByteData[x, y];
                        switch (cursor.SubMode)
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
                for (int y = x; y < Depth - x - 1; y++)
                {
                    byte temp = byteData[x, y];

                    byteData[x, y] = byteData[Width - 1 - y, x];

                    byteData[Width - 1 - y, x] = byteData[Width - 1 - x, Depth - 1 - y];

                    byteData[Width - 1 - x, Depth - 1 - y] = byteData[y, Depth - 1 - x];

                    byteData[y, Depth - 1 - x] = temp;
                }
            }
        }

        public float[,] GetFloatMap()
        {
            float[,] floatMap = new float[Width, Depth];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Depth; y++)
                {
                    double highestDigit = Math.Pow(16, 1) * (HeightMapDigitsThreeAndFour[x, y] & 0x0F);
                    double middleDigit = Math.Pow(16, 0) * ((HeightMapDigitsOneAndTwo[x, y] & 0xF0) >> 4);
                    double smallestDigit = Math.Pow(16, -1) * (HeightMapDigitsOneAndTwo[x, y] & 0x0F);

                    floatMap[x, y] = (float)(highestDigit + middleDigit + smallestDigit) / 2;
                }
            }

            return floatMap;
        }

        public List<GeometryModel3D> GetTerrainGeometryModel(BitmapImage[] tilemap)
        {
            List<GeometryModel3D> terrainTileGroups = new List<GeometryModel3D>();
            List<MeshGeometry3D> meshes = new List<MeshGeometry3D>();
            List<DiffuseMaterial> materials = new List<DiffuseMaterial>();
            List<ImageBrush> brushes = new List<ImageBrush>();
            float[,] floatMap = GetFloatMap();

            for (int i = 0; i < 16; i++)
            {
                meshes.Add(new MeshGeometry3D());
                brushes.Add(new ImageBrush());

                brushes[i].ImageSource = tilemap[i];
                materials.Add(new DiffuseMaterial(brushes[i]));
            }

            for (int y = 0; y < Depth - 1; y++)
            {
                for (int x = 0; x < Width - 1; x++)
                {
                    MeshGeometry3D temp_mesh = GetTerrainTile(new Point3D(x, floatMap[x, y], y), new Point3D(x + 1, floatMap[x + 1, y], y), new Point3D(x, floatMap[x, y + 1], y + 1), new Point3D(x + 1, floatMap[x + 1, y + 1], y + 1), meshes[MainTextureMap[x, y]].TriangleIndices.Count);

                    for (int i = 0; i < 6; i++)
                    {
                        meshes[MainTextureMap[x, y]].Positions.Add(temp_mesh.Positions[i]);
                        meshes[MainTextureMap[x, y]].TextureCoordinates.Add(temp_mesh.TextureCoordinates[i]);
                        meshes[MainTextureMap[x, y]].TriangleIndices.Add(temp_mesh.TriangleIndices[i]);
                    }
                }
            }

            for (int i = 0; i < 16; i++)
            {
                //Make the mesh's model. 
                GeometryModel3D tileGroup = new GeometryModel3D(meshes[i], materials[i]);

                // Make the surface visible from both sides.
                tileGroup.BackMaterial = materials[i];

                terrainTileGroups.Add(tileGroup);

                // Make the surface visible from both sides.
                tileGroup.BackMaterial = materials[i];
            }

            return terrainTileGroups;
        }

        public MeshGeometry3D GetTerrainTile(Point3D lowerLeft, Point3D lowerRight, Point3D upperLeft, Point3D upperRight, int triangleIndicesOffset)
        {
            // Make a mesh to hold the surface.
            MeshGeometry3D mesh = new MeshGeometry3D();

            // Set the triangle's points.
            mesh.Positions.Add(lowerLeft);
            mesh.Positions.Add(lowerRight);
            mesh.Positions.Add(upperRight);
            mesh.Positions.Add(lowerLeft);
            mesh.Positions.Add(upperLeft);
            mesh.Positions.Add(upperRight);

            // Set the points' texture coordinates.
            mesh.TextureCoordinates.Add(new System.Windows.Point(0, 0));
            mesh.TextureCoordinates.Add(new System.Windows.Point(1, 0));
            mesh.TextureCoordinates.Add(new System.Windows.Point(1, 1));
            mesh.TextureCoordinates.Add(new System.Windows.Point(0, 0));
            mesh.TextureCoordinates.Add(new System.Windows.Point(0, 1));
            mesh.TextureCoordinates.Add(new System.Windows.Point(1, 1));

            // Create the triangle.
            mesh.TriangleIndices.Add(triangleIndicesOffset + 0);
            mesh.TriangleIndices.Add(triangleIndicesOffset + 1);
            mesh.TriangleIndices.Add(triangleIndicesOffset + 2);
            mesh.TriangleIndices.Add(triangleIndicesOffset + 3);
            mesh.TriangleIndices.Add(triangleIndicesOffset + 4);
            mesh.TriangleIndices.Add(triangleIndicesOffset + 5);

            return mesh;
        }
        public GeometryModel3D GetWaterGeometryModel()
        {
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

            for (int i = 0; i < WaterLevel; i++)
            {

                triangleCounter = waterPoint3DCollection.Count;

                waterPoint3DCollection.Add(new Point3D(-Width, WaterLevel, -Depth));
                waterPoint3DCollection.Add(new Point3D(+Width, WaterLevel, +Depth));
                waterPoint3DCollection.Add(new Point3D(-Width, WaterLevel, +Depth));
                waterPoint3DCollection.Add(new Point3D(+Width, WaterLevel, -Depth));

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
