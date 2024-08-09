namespace Overlord_Map_Visualizer
{
    enum OverlordObjectType
    {
        BrownMinionGate,
        RedMinionGate,
        GreenMinionGate,
        BlueMinionGate,
        TowerGate,
        TowerGateVariant,
        HealthPillar,
        ManaPillar,
        MinionPillar,
        SpellStone,
        SpellCatalyst,
        Smelter,
        Mould,
        ForgeStone,
        MinionHive,
        OtherObjects
    }
    class OverlordObject
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public OverlordObjectType Type { get; set; }

        public OverlordObject(int x, int y, int z, OverlordObjectType type)
        {
            X = x;
            Y = y;
            Z = z;
            Type = type;
        }
    }
}
