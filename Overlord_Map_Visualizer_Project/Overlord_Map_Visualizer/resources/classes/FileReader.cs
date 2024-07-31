using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

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
                default:
                    return 0;
            }
        }
    }
}
