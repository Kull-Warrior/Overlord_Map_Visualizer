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
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public OverlordObjectType Type { get; set; }

        public OverlordObject(float x, float y, float z, OverlordObjectType type)
        {
            X = x;
            Y = y;
            Z = z;
            Type = type;
        }
    }
}
