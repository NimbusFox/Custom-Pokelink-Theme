using Raylib_cs;

namespace NimbusFox.Pokelink.Theme;

internal static class Global {
    public static class Colors {
        public static class Types {
            public static readonly Color Bug = new Color(0xA8, 0xB8, 0x20, 0xFF);
            public static readonly Color Dark = new Color(0xC0, 0x20, 0x20, 0xFF);
            public static readonly Color Dragon = new Color(0x70, 0x38, 0xF8, 0xFF);
            public static readonly Color Electric = new Color(0xF8, 0xD0, 0x30, 0xFF);
            public static readonly Color Fairy = new Color(0xEE, 0x99, 0xAC, 0xFF);
            public static readonly Color Fighting = new Color(0xC0, 0x30, 0x28, 0xFF);
            public static readonly Color Fire = new Color(0xF0, 0x80, 0x30, 0xFF);
            public static readonly Color Flying = new Color(0xA8, 0x90, 0xF0, 0xFF);
            public static readonly Color Ghost = new Color(0x70, 0x58, 0x96, 0xFF);
            public static readonly Color Grass = new Color(0x78, 0xC8, 0x50, 0xFF);
            public static readonly Color Ground = new Color(0xE0, 0xC0, 0x68, 0xFF);
            public static readonly Color Ice = new Color(0x98, 0xD8, 0xD8, 0xFF);
            public static readonly Color Normal = new Color(0xA8, 0xA8, 0x78, 0xFF);
            public static readonly Color Poison = new Color(0xA0, 0x40, 0xA0, 0xFF);
            public static readonly Color Psychic = new Color(0xF8, 0x58, 0x88, 0xFF); 
            public static readonly Color Rock = new Color(0xB8, 0xA0, 0x38, 0xFF);
            public static readonly Color Steel = new Color(0xB8, 0xB8, 0xD0, 0xFF);
            public static readonly Color Water = new Color(0x68, 0x90, 0xF0, 0xFF);

            public static Color GetColor(string name) {
                return (Color)(typeof(Types).GetField(name)?.GetValue(null) ?? Color.Black);
            }
        }

        public static class StatusConditions {
            public static readonly Color Poisoned = new Color(0xC0, 0x60, 0xC0, 0xFF);
            public static readonly Color Paralyzed = new Color(0xB8, 0xB8, 0x18, 0xFF);
            public static readonly Color Asleep = new Color(0xA0, 0xA0, 0x88, 0xFF);
            public static readonly Color Frozen = new Color(0x88, 0xB0, 0xE0, 0xFF);
            public static readonly Color Burned = new Color(0xE0, 0x70, 0x50, 0XFF);
            public static readonly Color Fainted = new Color(0xE8, 0x50, 0x38, 0xFF);
        }
    }
}
