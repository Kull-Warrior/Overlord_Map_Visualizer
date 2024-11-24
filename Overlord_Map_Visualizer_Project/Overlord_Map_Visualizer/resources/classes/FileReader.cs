using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

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

        public int[] GetTileMap(OverlordMap map)
        {
            switch (map.FilePath)
            {
                case string a when a.Contains("Exp - HalflingMain"):
                    return new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
                case string a when a.Contains("Exp - Halfling Abyss"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("Exp - ElfMain"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("Exp - Elf Abyss"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("Exp - PaladinMain"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("Exp - Paladin Abyss"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("Exp - DwarfMain"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("Exp - Dwarf Abyss"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("Exp - WarriorMain"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("Exp - Warrior Abyss - 01"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("Exp - Warrior Abyss - 02"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("Exp - Tower_Dungeon"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("Exp - Tower_Spawnpit"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("Exp - Tower"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("HalflingMain"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("SlaveCamp"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("HalflingHomes1of2"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("HalflingHomes2of2"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("HellsKitchen"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("EntryCastleSpree"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("SpreeDungeon"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("ElfMain"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("GreenCave"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("SkullDen"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("TrollTemple"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("PaladinMain"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("BlueCave"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("Sewers1of2"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("Sewers2of2"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("Red Light Inn"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("Citadel"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("DwarfMain"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("GoldMine"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("Quarry"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("HomeyHalls1of2"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("HomeyHalls2of2"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("ArcaniumMine"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("RoyalHalls"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("WarriorMain"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("2P_Deathtrap"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("2P_Gates"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("2P_LastStand"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("2P_PartyCrashers"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("2P_Plunder"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("2P_TombRobber"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("Tower_Dungeon"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("Tower_Spawnpit"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("Tower"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("PlayerMap"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("2P_Arena2"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("2P_Bombs"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("2P_GrabTheMaidens"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("2P_KillTheHoard"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("2P_KingoftheHill"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("2P_March_Mellow_Maidens"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("2P_Misty"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("2P_RockyRace"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("LM0A Netherworld"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("LM0C Netherworld Burrows"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("LM0D Netherworld Foundations"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("Exp - LM0B Netherworld Arena"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("LM1A Hunting Grounds"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("LM1C Nordberg Town"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("LM1D Nordberg Fairyland"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("LM1E Nordhaven"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("LM1F Prelude"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("LM1G Nordberg Commune"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("LM2A Everlight Gates"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("LM2B Everlight Jungle A"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("LM2C Everlight Facility"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("LM2D Everlight Jungle B"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("LM2E Spider Boss"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("LM2F Everlight Town"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("LM3A Wasteland"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("LM3B Wasteland Sanctuary Depths"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("LM3C Wasteland Sanctuary Town"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("LM4A Empire Heartland Harbour"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("LM4B Empire Heartland City"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("LM4C Empire Heartland Assault"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("LM4D Empire City"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("LM4E Empire Sewers"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("LM4F Empire Arena"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("LM4G Empire Palace"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("LM4H Empire EndBoss"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("MPC1_Arena"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("MPC2_Invasion"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("MPV1_Dominate"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                case string a when a.Contains("MPV2_PiratePlunder"):
                    return new int[] { 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
                default:
                    return new int[] { 00, 01, 02, 03, 04, 05, 06, 07, 08, 09, 10, 11, 12, 13, 14, 15};
            }
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
                return environments[1];
            else
                if (environments.Count > 0)
                    return environments[0];
                else
                    return "default";
        }
    }
}
