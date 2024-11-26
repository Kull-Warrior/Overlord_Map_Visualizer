using Pfim;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;

namespace Overlord_Map_Visualizer
{
    class FileReader
    {
        public FileReader()
        {

        }

        public byte[] ReadMapDataFromFile(string filePath, int offset, int width, int height, int bytesPerPoint)
        {
            int totalNumberOfBytes = width * height * bytesPerPoint;
            byte[] data = new byte[totalNumberOfBytes];

            using (BinaryReader reader = new BinaryReader(new FileStream(filePath, FileMode.Open)))
            {
                reader.BaseStream.Seek(offset, SeekOrigin.Begin);
                reader.Read(data, 0, totalNumberOfBytes);
            }

            return data;
        }

        public int GetMapDataOffset(string filePath, int width, int height)
        {
            int srcOffset = 200 + width * height * 4;
            int totalNumberOfBytes = 500;
            byte[] src = new byte[totalNumberOfBytes];

            using (BinaryReader reader = new BinaryReader(new FileStream(filePath, FileMode.Open)))
            {
                reader.BaseStream.Seek(srcOffset, SeekOrigin.Begin);
                reader.Read(src, 0, totalNumberOfBytes);
            }

            byte[] pattern = new byte[9] { (byte)'I', (byte)'n', (byte)'d', (byte)'o', (byte)'o', (byte)'r', (byte)'S', (byte)'e', (byte)'t' };//IndoorSet

            int maxFirstCharSlot = src.Length - pattern.Length + 1;
            for (int i = 0; i < maxFirstCharSlot; i++)
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
                    if (j == 1)
                    {
                        return i + 200 - 4;
                    }
                }

            }

            return 0;
        }

        public double GetMapWaterLevel(OverlordMap map)
        {
            switch (map.FilePath)
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
                case string a when a.Contains("LM0A Netherworld"):
                    return 0;
                case string a when a.Contains("LM0C Netherworld Burrows"):
                    return 0;
                case string a when a.Contains("LM0D Netherworld Foundations"):
                    return 0;
                case string a when a.Contains("Exp - LM0B Netherworld Arena"):
                    return 0;
                case string a when a.Contains("LM1A Hunting Grounds"):
                    return 15;
                case string a when a.Contains("LM1C Nordberg Town"):
                    return 22;
                case string a when a.Contains("LM1D Nordberg Fairyland"):
                    return 18;
                case string a when a.Contains("LM1E Nordhaven"):
                    return 22;
                case string a when a.Contains("LM1F Prelude"):
                    return 22;
                case string a when a.Contains("LM1G Nordberg Commune"):
                    return 30;
                case string a when a.Contains("LM2A Everlight Gates"):
                    return 25.25;
                case string a when a.Contains("LM2B Everlight Jungle A"):
                    return 74;
                case string a when a.Contains("LM2C Everlight Facility"):
                    return 71;
                case string a when a.Contains("LM2D Everlight Jungle B"):
                    return 70;
                case string a when a.Contains("LM2E Spider Boss"):
                    return 48.0625;
                case string a when a.Contains("LM2F Everlight Town"):
                    return 26.5;
                case string a when a.Contains("LM3A Wasteland"):
                    return 0;
                case string a when a.Contains("LM3B Wasteland Sanctuary Depths"):
                    return 16;
                case string a when a.Contains("LM3C Wasteland Sanctuary Town"):
                    return 0;
                case string a when a.Contains("LM4A Empire Heartland Harbour"):
                    return 20;
                case string a when a.Contains("LM4B Empire Heartland City"):
                    return 36.5;
                case string a when a.Contains("LM4C Empire Heartland Assault"):
                    return 15;
                case string a when a.Contains("LM4D Empire City"):
                    return 15;
                case string a when a.Contains("LM4E Empire Sewers"):
                    return 41;
                case string a when a.Contains("LM4F Empire Arena"):
                    return 40;
                case string a when a.Contains("LM4G Empire Palace"):
                    return 16;
                case string a when a.Contains("LM4H Empire EndBoss"):
                    return 15;
                case string a when a.Contains("MPC1_Arena"):
                    return 53;
                case string a when a.Contains("MPC2_Invasion"):
                    return 16;
                case string a when a.Contains("MPV1_Dominate"):
                    return 15;
                case string a when a.Contains("MPV2_PiratePlunder"):
                    return 25.3125;
                default:
                    return 0;
            }
        }

        public List<OverlordObject> GetMapObjects(OverlordMap map)
        {
            List<OverlordObject> objects = new List<OverlordObject>();

            switch (map.FilePath)
            {
                case string a when a.Contains("Exp - HalflingMain"):
                    break;
                case string a when a.Contains("Exp - Halfling Abyss"):
                    break;
                case string a when a.Contains("Exp - ElfMain"):
                    break;
                case string a when a.Contains("Exp - Elf Abyss"):
                    break;
                case string a when a.Contains("Exp - PaladinMain"):
                    break;
                case string a when a.Contains("Exp - Paladin Abyss"):
                    break;
                case string a when a.Contains("Exp - DwarfMain"):
                    break;
                case string a when a.Contains("Exp - Dwarf Abyss"):
                    break;
                case string a when a.Contains("Exp - WarriorMain"):
                    break;
                case string a when a.Contains("Exp - Warrior Abyss - 01"):
                    break;
                case string a when a.Contains("Exp - Warrior Abyss - 02"):
                    break;
                case string a when a.Contains("Exp - Tower_Dungeon"):
                    break;
                case string a when a.Contains("Exp - Tower_Spawnpit"):
                    break;
                case string a when a.Contains("Exp - Tower"):
                    break;
                case string a when a.Contains("HalflingMain"):
                    break;
                case string a when a.Contains("SlaveCamp"):
                    break;
                case string a when a.Contains("HalflingHomes1of2"):
                    break;
                case string a when a.Contains("HalflingHomes2of2"):
                    break;
                case string a when a.Contains("HellsKitchen"):
                    break;
                case string a when a.Contains("EntryCastleSpree"):
                    break;
                case string a when a.Contains("SpreeDungeon"):
                    break;
                case string a when a.Contains("ElfMain"):
                    break;
                case string a when a.Contains("GreenCave"):
                    break;
                case string a when a.Contains("SkullDen"):
                    break;
                case string a when a.Contains("TrollTemple"):
                    break;
                case string a when a.Contains("PaladinMain"):
                    break;
                case string a when a.Contains("BlueCave"):
                    break;
                case string a when a.Contains("Sewers1of2"):
                    break;
                case string a when a.Contains("Sewers2of2"):
                    break;
                case string a when a.Contains("Red Light Inn"):
                    break;
                case string a when a.Contains("Citadel"):
                    break;
                case string a when a.Contains("DwarfMain"):
                    break;
                case string a when a.Contains("GoldMine"):
                    break;
                case string a when a.Contains("Quarry"):
                    break;
                case string a when a.Contains("HomeyHalls1of2"):
                    break;
                case string a when a.Contains("HomeyHalls2of2"):
                    break;
                case string a when a.Contains("ArcaniumMine"):
                    break;
                case string a when a.Contains("RoyalHalls"):
                    break;
                case string a when a.Contains("WarriorMain"):
                    break;
                case string a when a.Contains("2P_Deathtrap"):
                    break;
                case string a when a.Contains("2P_Gates"):
                    break;
                case string a when a.Contains("2P_LastStand"):
                    break;
                case string a when a.Contains("2P_PartyCrashers"):
                    break;
                case string a when a.Contains("2P_Plunder"):
                    break;
                case string a when a.Contains("2P_TombRobber"):
                    break;
                case string a when a.Contains("Tower_Dungeon"):
                    break;
                case string a when a.Contains("Tower_Spawnpit"):
                    break;
                case string a when a.Contains("Tower"):
                    break;
                case string a when a.Contains("PlayerMap"):
                    break;
                case string a when a.Contains("2P_Arena2"):
                    break;
                case string a when a.Contains("2P_Bombs"):
                    break;
                case string a when a.Contains("2P_GrabTheMaidens"):
                    break;
                case string a when a.Contains("2P_KillTheHoard"):
                    break;
                case string a when a.Contains("2P_KingoftheHill"):
                    break;
                case string a when a.Contains("2P_March_Mellow_Maidens"):
                    break;
                case string a when a.Contains("2P_Misty"):
                    break;
                case string a when a.Contains("2P_RockyRace"):
                    break;
                case string a when a.Contains("LM0A Netherworld"):
                    break;
                case string a when a.Contains("LM0C Netherworld Burrows"):
                    break;
                case string a when a.Contains("LM0D Netherworld Foundations"):
                    break;
                case string a when a.Contains("Exp - LM0B Netherworld Arena"):
                    break;
                case string a when a.Contains("LM1A Hunting Grounds"):
                    break;
                case string a when a.Contains("LM1C Nordberg Town"):
                    break;
                case string a when a.Contains("LM1D Nordberg Fairyland"):
                    break;
                case string a when a.Contains("LM1E Nordhaven"):
                    break;
                case string a when a.Contains("LM1F Prelude"):
                    break;
                case string a when a.Contains("LM1G Nordberg Commune"):
                    break;
                case string a when a.Contains("LM2A Everlight Gates"):
                    break;
                case string a when a.Contains("LM2B Everlight Jungle A"):
                    break;
                case string a when a.Contains("LM2C Everlight Facility"):
                    break;
                case string a when a.Contains("LM2D Everlight Jungle B"):
                    break;
                case string a when a.Contains("LM2E Spider Boss"):
                    break;
                case string a when a.Contains("LM2F Everlight Town"):
                    break;
                case string a when a.Contains("LM3A Wasteland"):
                    break;
                case string a when a.Contains("LM3B Wasteland Sanctuary Depths"):
                    break;
                case string a when a.Contains("LM3C Wasteland Sanctuary Town"):
                    break;
                case string a when a.Contains("LM4A Empire Heartland Harbour"):
                    break;
                case string a when a.Contains("LM4B Empire Heartland City"):
                    break;
                case string a when a.Contains("LM4C Empire Heartland Assault"):
                    break;
                case string a when a.Contains("LM4D Empire City"):
                    break;
                case string a when a.Contains("LM4E Empire Sewers"):
                    break;
                case string a when a.Contains("LM4F Empire Arena"):
                    break;
                case string a when a.Contains("LM4G Empire Palace"):
                    break;
                case string a when a.Contains("LM4H Empire EndBoss"):
                    break;
                case string a when a.Contains("MPC1_Arena"):
                    break;
                case string a when a.Contains("MPC2_Invasion"):
                    break;
                case string a when a.Contains("MPV1_Dominate"):
                    break;
                case string a when a.Contains("MPV2_PiratePlunder"):
                    break;
                default:
                    break;
            }

            return objects;
        }

        public BitmapImage[] GetTileMapImages(OverlordMap map)
        {
            BitmapImage[] images = new BitmapImage[16];
            string gameDirectory = "";

            if (map.FilePath.Contains("Overlord II"))
            {
                gameDirectory = map.FilePath.Remove(map.FilePath.IndexOf("Overlord II") + 11);
            }
            else if (map.FilePath.Contains("Overlord"))
            {
                gameDirectory = map.FilePath.Remove(map.FilePath.IndexOf("Overlord") + 8);
            }

            if (Directory.Exists(@gameDirectory + "/Resources") | Directory.Exists(@gameDirectory + "/Expansion"))
            {
                BitmapImage fullTilemap = new BitmapImage();
                string environmentPath = "";
                var extensions = new List<string> { ".prp" };
                string[] files = Directory.GetFiles(gameDirectory, "*.*", SearchOption.AllDirectories).Where(f => extensions.IndexOf(Path.GetExtension(f)) >= 0).ToArray();

                for (int i = 0; i < files.Length; i++)
                {
                    if (files[i].Contains(map.Environment))
                    {
                        environmentPath = files[i];
                    }
                }

                images = GetTilemapFromRpkFile(environmentPath);
            }
            else//Use default texture
            {
                for (int i = 0; i < images.Length; i++)
                {
                    images[i] = new BitmapImage(new Uri("pack://application:,,,/resources/tile_palette/" + i + ".png"));
                }
            }

            return images;
        }

        private List<int[]> GetItem(List<int[]> list, int ID)
        {
            List<int[]> listA = new List<int[]>();

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i][0] == ID)
                {
                    listA.Add(list[i]);
                }
            }

            return listA;
        }

        private List<int[]> GetList(int listType, BinaryReader reader)
        {
            List<int[]> list = new List<int[]>();
            int countSmall, countBig;

            if (listType >= 128)
            {
                countSmall = listType - 128;
                countBig = reader.ReadInt32();

                for (int i = 0; i < countSmall; i++)
                {
                    int[] ints = new int[2];
                    ints[0] = reader.ReadByte();
                    ints[1] = reader.ReadByte();
                    list.Add(ints);
                }
                for (int i = 0; i < countBig; i++)
                {
                    int[] ints = new int[2];
                    ints[0] = reader.ReadInt32();
                    ints[1] = reader.ReadInt32();
                    list.Add(ints);
                }
            }
            else
            {
                countSmall = listType;
                for (int i = 0; i < countSmall; i++)
                {
                    int[] ints = new int[2];
                    ints[0] = reader.ReadByte();
                    ints[1] = reader.ReadByte();
                    list.Add(ints);
                }
            }
            int position = (int)reader.BaseStream.Position;
            List<int[]> listA = new List<int[]>();

            for (int i = 0; i < list.Count; i++)
            {
                int[] item = new int[2];
                item[0] = list[i][0];
                item[1] = position + list[i][1];
                listA.Add(item);
            }

            return listA;
        }

        private byte[] GetDDSHeader(int width, int height, string format)
        {
            Byte[] header = new Byte[128];

            Byte[] fileIdentifier = new Byte[4] { 0x44, 0x44, 0x53, 0x20 };
            Byte[] headerSize = new Byte[4] { 0x7C, 0x00, 0x00, 0x00 };
            Byte[] flags = new Byte[4] { 0x07, 0x10, 0x02, 0x00 };
            Byte[] imageHeight = BitConverter.GetBytes(height);
            Byte[] imageWidth = BitConverter.GetBytes(width);
            Byte[] pitchOrLinearSize = new Byte[4] { 0x00, 0x00, 0x00, 0x00 };
            Byte[] depth = new Byte[4] { 0x00, 0x00, 0x00, 0x00 };
            Byte[] mipMapCount = new Byte[4] { 0x01, 0x00, 0x00, 0x00 };
            Byte[] reserved = new Byte[44] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            Byte[] ddpiPixelFormatHeaderSize = new Byte[4] { 0x20, 0x00, 0x00, 0x00 };
            Byte[] ddpiPixelFormatFlags = new Byte[4] { 0x20, 0x00, 0x00, 0x00 };
            Byte[] ddpiPixelFourCC;
            if (format == "DXT3")
                ddpiPixelFourCC = new Byte[4] { 0x44, 0x58, 0x54, 0x33 };
            else
                ddpiPixelFourCC = new Byte[4] { 0x44, 0x58, 0x54, 0x35 };
            Byte[] ddpiPixelRGBBitCount = new Byte[4] { 0x00, 0x00, 0x00, 0x00 };
            Byte[] ddpiPixelRBitMask = new Byte[4] { 0x00, 0x00, 0x00, 0x00 };
            Byte[] ddpiPixelGBitMask = new Byte[4] { 0x00, 0x00, 0x00, 0x00 };
            Byte[] ddpiPixelBBitMask = new Byte[4] { 0x00, 0x00, 0x00, 0x00 };
            Byte[] ddpiPixelRGBAlphaBitMask = new Byte[4] { 0x00, 0x00, 0x00, 0x00 };
            Byte[] Caps = new Byte[4] { 0x08, 0x10, 0x40, 0x00 };
            Byte[] Caps2 = new Byte[4] { 0x00, 0x00, 0x00, 0x00 };
            Byte[] Caps3 = new Byte[4] { 0x00, 0x00, 0x00, 0x00 };
            Byte[] Caps4 = new Byte[4] { 0x00, 0x00, 0x00, 0x00 };
            Byte[] reserved2 = new Byte[4] { 0x00, 0x00, 0x00, 0x00 };

            Buffer.BlockCopy(fileIdentifier, 0, header, 0, 4);
            Buffer.BlockCopy(headerSize, 0, header, 4, 4);
            Buffer.BlockCopy(flags, 0, header, 8, 4);
            Buffer.BlockCopy(imageHeight, 0, header, 12, 4);
            Buffer.BlockCopy(imageWidth, 0, header, 16, 4);
            Buffer.BlockCopy(pitchOrLinearSize, 0, header, 20, 4);
            Buffer.BlockCopy(depth, 0, header, 24, 4);
            Buffer.BlockCopy(mipMapCount, 0, header, 28, 4);
            Buffer.BlockCopy(reserved, 0, header, 32, 4);
            Buffer.BlockCopy(ddpiPixelFormatHeaderSize, 0, header, 76, 4);
            Buffer.BlockCopy(ddpiPixelFormatFlags, 0, header, 80, 4);
            Buffer.BlockCopy(ddpiPixelFourCC, 0, header, 84, 4);
            Buffer.BlockCopy(ddpiPixelRGBBitCount, 0, header, 88, 4);
            Buffer.BlockCopy(ddpiPixelRBitMask, 0, header, 92, 4);
            Buffer.BlockCopy(ddpiPixelGBitMask, 0, header, 96, 4);
            Buffer.BlockCopy(ddpiPixelBBitMask, 0, header, 100, 4);
            Buffer.BlockCopy(ddpiPixelRGBAlphaBitMask, 0, header, 104, 4);
            Buffer.BlockCopy(Caps, 0, header, 108, 4);
            Buffer.BlockCopy(Caps2, 0, header, 112, 4);
            Buffer.BlockCopy(Caps3, 0, header, 116, 4);
            Buffer.BlockCopy(Caps4, 0, header, 120, 4);
            Buffer.BlockCopy(reserved2, 0, header, 124, 4);

            return header;
        }

        void RemoveImageTransparancy(Bitmap src)
        {
            Bitmap target = new Bitmap(src.Size.Width, src.Size.Height);
            Graphics g = Graphics.FromImage(target);
            g.DrawRectangle(new Pen(new SolidBrush(Color.White)), 0, 0, target.Width, target.Height);
            g.DrawImage(src, 0, 0);
            target.Save("Your target path");
        }

        private BitmapImage[] GetTilemapFromRpkFile(string filePath)
        {
            BitmapImage[] tilemap = new BitmapImage[16];

            byte[] ddsData = new byte[128 + 4194304];

            using (BinaryReader reader = new BinaryReader(new FileStream(filePath, FileMode.Open)))
            {
                reader.BaseStream.Seek(16, SeekOrigin.Begin);
                string title = Encoding.ASCII.GetString(reader.ReadBytes(160), 0, 160);

                int type = reader.ReadBytes(1)[0];
                List<int[]> list0 = GetList(type, reader);
                List<int[]> list26 = GetItem(list0, 26);

                for (int i0 = 0; i0 < list26.Count; i0++)
                {
                    reader.BaseStream.Seek(list26[i0][1], SeekOrigin.Begin);
                    reader.ReadBytes(3);
                    int type1 = reader.ReadBytes(1)[0];
                    List<int[]> list1 = GetList(type1, reader);

                    for (int i1 = 0; i1 < list1.Count; i1++)
                    {
                        reader.BaseStream.Seek(list1[i1][1], SeekOrigin.Begin);
                        int[] flag = new int[4];
                        flag[0] = reader.ReadByte();
                        flag[1] = reader.ReadByte();
                        flag[2] = reader.ReadByte();
                        flag[3] = reader.ReadByte();

                        if (flag.SequenceEqual(new int[] { 061, 000, 065, 000 }) | flag.SequenceEqual(new int[] { 153, 000, 065, 000 }) | flag.SequenceEqual(new int[] { 152, 000, 065, 000 }))
                        {
                            string textureChunk = "";
                            string name = "";
                            int type2 = reader.ReadBytes(1)[0];
                            List<int[]> list2 = GetList(type2, reader);
                            for (int i2 = 0; i2 < list2.Count; i2++)
                            {
                                reader.BaseStream.Seek(list2[i2][1], SeekOrigin.Begin);

                                if (list2[i2][0] == 20)
                                {
                                    textureChunk = Encoding.ASCII.GetString(reader.ReadBytes(32), 0, 32);
                                }

                                if (list2[i2][0] == 21)
                                {
                                    name = Encoding.ASCII.GetString(reader.ReadBytes(32), 0, 32);
                                }

                                if (list2[i2][0] == 1)
                                {
                                    int type3 = reader.ReadBytes(1)[0];
                                    List<int[]> list3 = GetList(type3, reader);
                                    for (int i3 = 0; i3 < list3.Count; i3++)
                                    {
                                        reader.BaseStream.Seek(list3[i3][1], SeekOrigin.Begin);

                                        if (list3[i3][0] == 20)
                                        {
                                            reader.ReadBytes(3);
                                            int type4 = reader.ReadBytes(1)[0];
                                            List<int[]> list4 = GetList(type4, reader);

                                            for (int i4 = 0; i4 < list4.Count; i4++)
                                            {
                                                reader.BaseStream.Seek(list4[i4][1], SeekOrigin.Begin);
                                                int[] flag1 = new int[4];
                                                flag1[0] = reader.ReadByte();
                                                flag1[1] = reader.ReadByte();
                                                flag1[2] = reader.ReadByte();
                                                flag1[3] = reader.ReadByte();

                                                if (flag1.SequenceEqual(new int[] { 036, 000, 065, 000 }))
                                                {
                                                    int width = 0;
                                                    int height = 0;
                                                    string format = "";
                                                    int offset = 0;
                                                    int blockSize = 0;

                                                    int type5 = reader.ReadBytes(1)[0];
                                                    List<int[]> list5 = GetList(type5, reader);
                                                    for (int i5 = 0; i5 < list5.Count; i5++)
                                                    {
                                                        reader.BaseStream.Seek(list5[i5][1], SeekOrigin.Begin);

                                                        if (list5[i5][0] == 20)
                                                        {
                                                            width = reader.ReadInt32();
                                                        }
                                                        if (list5[i5][0] == 21)
                                                        {
                                                            height = reader.ReadInt32();
                                                        }
                                                        if (list5[i5][0] == 23)
                                                        {
                                                            int formatNumber = reader.ReadInt32();

                                                            if (formatNumber == 7)
                                                            {
                                                                format = "DXT1";
                                                            }
                                                            if (formatNumber == 11)
                                                            {
                                                                format = "DXT5";
                                                            }
                                                            if (formatNumber == 9)
                                                            {
                                                                format = "DXT3";
                                                            }
                                                            if (formatNumber == 5)
                                                            {
                                                                format = "tga32";
                                                            }
                                                            if (formatNumber == 3)
                                                            {
                                                                format = "tga24";
                                                            }
                                                        }
                                                        if (list5[i5][0] == 22)
                                                        {
                                                            offset = (int)reader.BaseStream.Position;
                                                        }
                                                    }

                                                    reader.BaseStream.Seek(offset, SeekOrigin.Begin);

                                                    if (!name.Contains("NM") && format.Contains("DXT") && width == 2048 && height == 2048)
                                                    {
                                                        int mipmapCount = (int)(Math.Floor(Math.Log(Math.Max(width, height), 2)) + 1);

                                                        if (format == "DXT1")
                                                        {
                                                            blockSize = 8;
                                                        }
                                                        else
                                                        {
                                                            blockSize = 16;
                                                        }

                                                        int tempWidth = width;
                                                        int tempHeight = height;

                                                        //Number of mipmaps is ignored here but could be used to read the entire dds file
                                                        for (int i = 0; i < 1; i++)
                                                        {
                                                            int blocksWidth = (int)Math.Ceiling((double)tempWidth / (double)4);
                                                            int blocksHeight = (int)Math.Ceiling((double)tempHeight / (double)4);
                                                            int size = blocksWidth * blocksHeight * blockSize;

                                                            byte[] header = GetDDSHeader(width, height, format);
                                                            byte[] data = reader.ReadBytes(size);

                                                            Buffer.BlockCopy(header, 0, ddsData, 0, 128);
                                                            Buffer.BlockCopy(data, 0, ddsData, 128, size);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            using (Stream ddsDataStream = new MemoryStream(ddsData))
            {
                using (IImage image = Pfimage.FromStream(ddsDataStream))
                {
                    System.Drawing.Imaging.PixelFormat format;

                    // Convert from Pfim's backend agnostic image format into GDI+'s image format
                    switch (image.Format)
                    {
                        case ImageFormat.Rgba32:
                            format = System.Drawing.Imaging.PixelFormat.Format32bppRgb;
                            break;
                        default:
                            // see the sample for more details
                            throw new NotImplementedException();
                    }

                    IntPtr data = Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0);

                    Bitmap bitmap = new Bitmap(image.Width, image.Height, image.Stride, format, data);
                    BitmapImage tilemapImage;

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);

                        tilemapImage = new BitmapImage();
                        tilemapImage.BeginInit();
                        tilemapImage.StreamSource = memoryStream;
                        tilemapImage.CacheOption = BitmapCacheOption.OnLoad;
                        tilemapImage.EndInit();
                        tilemapImage.Freeze();
                    }

                    for (int y = 0; y < 4; y++)
                    {
                        for (int x = 0; x < 4; x++)
                        {
                            using (MemoryStream memoryStream = new MemoryStream())
                            {
                                int index = (4 * y) + x;

                                Bitmap tile = bitmap.Clone(new Rectangle(512 * x, 512 * y, 512, 512), System.Drawing.Imaging.PixelFormat.Format32bppRgb);

                                tile.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);

                                memoryStream.Position = 0;

                                tilemap[index] = new BitmapImage();
                                tilemap[index].BeginInit();
                                tilemap[index].StreamSource = memoryStream;
                                tilemap[index].CacheOption = BitmapCacheOption.OnLoad;
                                tilemap[index].EndInit();
                                tilemap[index].Freeze();
                            }
                        }
                    }
                }
            }

            return tilemap;
        }

        public string GetEnvironment(string filePath)
        {
            List<string> environments = new List<string>();
            List<int> rpkFileOffsets = new List<int>();

            int offset = 0;
            int blockSize = 512;

            using (BinaryReader reader = new BinaryReader(new FileStream(filePath, FileMode.Open)))
            {
                //Finding all rpk file offsets
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    byte[] block = new byte[blockSize];
                    if (offset + blockSize > reader.BaseStream.Length)
                    {
                        blockSize = (int)(reader.BaseStream.Length - reader.BaseStream.Position);
                    }
                    reader.BaseStream.Seek(offset, SeekOrigin.Begin);
                    block = reader.ReadBytes(blockSize);

                    string converted = Encoding.ASCII.GetString(block, 0, block.Length);

                    foreach (Match match in Regex.Matches(converted, "rpk"))
                    {
                        rpkFileOffsets.Add(offset + match.Index);
                    }
                    offset += blockSize;
                }

                //Reading rpk name and filtering by environments
                for (int i = 0; i < rpkFileOffsets.Count; i++)
                {
                    byte[] block = new byte[28];
                    reader.BaseStream.Seek(rpkFileOffsets[i] - 29, SeekOrigin.Begin);
                    block = reader.ReadBytes(28);

                    string converted = Encoding.ASCII.GetString(block, 0, block.Length);

                    if (converted.Contains("Exp - Env "))
                    {
                        environments.Add(converted.Substring(converted.IndexOf("Exp - Env")));
                    }
                    else if (converted.Contains("Env "))
                    {
                        MatchCollection matches = Regex.Matches(converted, "Env ");

                        List<string> imgCodes = matches.Cast<Match>().Select(x => x.Groups["content"].Value).ToList();
                        if (matches.Count > 1)
                        {
                            environments.Add(converted.Substring(matches[matches.Count - 1].Index));
                        }
                        else
                        {
                            environments.Add(converted.Substring(converted.IndexOf("Env ")));
                        }
                    }
                    else if (converted.Contains("Environment "))
                    {
                        environments.Add(converted.Substring(converted.IndexOf("Environment ")));
                    }
                }
            }

            //Removing all Environments not containing a tile map
            environments.Remove("Env Tower - Main");
            environments.Remove("Env Spawning Pits");
            environments.Remove("Env Spawning Pits");
            environments.Remove("Exp - MP Env Rocky Race");
            environments.Remove("Exp - Env HellSet");
            environments.Remove("Env Multiplayer 1");
            environments.Remove("Exp - MP Env Halls");

            if (filePath.Contains("Exp - Warrior Abyss - 01"))
            {
                return environments[1];
            }
            else
            {
                if (environments.Count > 0)
                {
                    return environments[0];
                }
                else
                {
                    return "default";
                }
            }
        }
    }
}
