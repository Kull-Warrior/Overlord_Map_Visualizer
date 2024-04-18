using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Overlord_Map_Visualizer
{
    enum OverlordObjectType
    {
        BrownMinionGate,
        RedMinionGate,
        GreenMinionGate,
        BlueMinionGate,
        TowerGate
    }
    class OverlordObject
    {
        private int X { get; set; }
        private int Y { get; set; }
        private int Z { get; set; }
        private OverlordObjectType Type { get; set; }

        public OverlordObject(int x, int y, int z, OverlordObjectType type)
        {
            X = x;
            Y = y;
            Z = z;
            Type = type;
        }
    }
}
