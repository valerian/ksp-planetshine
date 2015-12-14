using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlanetShine
{
    public static class LayerMask
    {
        public const int Default = (1 << 0);
        public const int TransparentFX = (1 << 1);
        public const int IgnoreRaycast = (1 << 2);
        public const int Layer3 = (1 << 3);
        public const int Water = (1 << 4);
        public const int UI = (1 << 5);
        public const int Layer6 = (1 << 6);
        public const int Layer7 = (1 << 7);
        public const int PartsList_Icons = (1 << 8);
        public const int Atmosphere = (1 << 9);
        public const int ScaledScenery = (1 << 10);
        public const int UI_Culled = (1 << 11);
        public const int UI_Main = (1 << 12);
        public const int UI_Mask = (1 << 13);
        public const int Screens = (1 << 14);
        public const int LocalScenery = (1 << 15);
        public const int kerbals = (1 << 16);
        public const int Editor_UI = (1 << 17);
        public const int SkySphere = (1 << 18);
        public const int DisconnectedParts = (1 << 19);
        public const int InternalSpace = (1 << 20);
        public const int PartTriggers = (1 << 21);
        public const int KerbalInstructors = (1 << 22);
        public const int ScaledSpaceSun = (1 << 23);
        public const int MapFX = (1 << 24);
        public const int EzGUI_UI = (1 << 25);
        public const int WheelCollidersIgnore = (1 << 26);
        public const int WheelColliders = (1 << 27);
        public const int TerrainColliders = (1 << 28);
        public const int Layer29 = (1 << 29);
        public const int Layer30 = (1 << 30);
        public const int Vectors = (1 << 31);
    }

}
