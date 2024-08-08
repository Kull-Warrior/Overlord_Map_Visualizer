namespace Overlord_Map_Visualizer
{
    class CursorManagement
    {
        public CursorMode Mode;
        public CursorSubMode SubMode;
        public int CursorDiameter = 51;

        public CursorManagement()
        {
            Mode = CursorMode.Select;
            SubMode = CursorSubMode.Set;
            CursorDiameter = 51;
        }
    }
}
