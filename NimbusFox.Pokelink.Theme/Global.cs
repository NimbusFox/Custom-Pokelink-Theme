using Raylib_cs;

namespace NimbusFox.Pokelink.Theme;

/// <summary>
/// Contains global constants and static resources used throughout the theme.
/// </summary>
internal static class Global {
    /// <summary>
    /// Provides color definitions for various game elements.
    /// </summary>
    public static class Colors {
        /// <summary>
        /// Defines standard colors associated with Pokemon elemental types.
        /// </summary>
        public static class Types {
            /// <summary>Color for the Bug type.</summary>
            public static readonly Color Bug = new(0xA8, 0xB8, 0x20, 0xFF);
            /// <summary>Color for the Dark type.</summary>
            public static readonly Color Dark = new(0xC0, 0x20, 0x20, 0xFF);
            /// <summary>Color for the Dragon type.</summary>
            public static readonly Color Dragon = new(0x70, 0x38, 0xF8, 0xFF);
            /// <summary>Color for the Electric type.</summary>
            public static readonly Color Electric = new(0xF8, 0xD0, 0x30, 0xFF);
            /// <summary>Color for the Fairy type.</summary>
            public static readonly Color Fairy = new(0xEE, 0x99, 0xAC, 0xFF);
            /// <summary>Color for the Fighting type.</summary>
            public static readonly Color Fighting = new(0xC0, 0x30, 0x28, 0xFF);
            /// <summary>Color for the Fire type.</summary>
            public static readonly Color Fire = new(0xF0, 0x80, 0x30, 0xFF);
            /// <summary>Color for the Flying type.</summary>
            public static readonly Color Flying = new(0xA8, 0x90, 0xF0, 0xFF);
            /// <summary>Color for the Ghost type.</summary>
            public static readonly Color Ghost = new(0x70, 0x58, 0x96, 0xFF);
            /// <summary>Color for the Grass type.</summary>
            public static readonly Color Grass = new(0x78, 0xC8, 0x50, 0xFF);
            /// <summary>Color for the Ground type.</summary>
            public static readonly Color Ground = new(0xE0, 0xC0, 0x68, 0xFF);
            /// <summary>Color for the Ice type.</summary>
            public static readonly Color Ice = new(0x98, 0xD8, 0xD8, 0xFF);
            /// <summary>Color for the Normal type.</summary>
            public static readonly Color Normal = new(0xA8, 0xA8, 0x78, 0xFF);
            /// <summary>Color for the Poison type.</summary>
            public static readonly Color Poison = new(0xA0, 0x40, 0xA0, 0xFF);
            /// <summary>Color for the Psychic type.</summary>
            public static readonly Color Psychic = new(0xF8, 0x58, 0x88, 0xFF);
            /// <summary>Color for the Rock type.</summary>
            public static readonly Color Rock = new(0xB8, 0xA0, 0x38, 0xFF);
            /// <summary>Color for the Steel type.</summary>
            public static readonly Color Steel = new(0xB8, 0xB8, 0xD0, 0xFF);
            /// <summary>Color for the Water type.</summary>
            public static readonly Color Water = new(0x68, 0x90, 0xF0, 0xFF);

            /// <summary>
            /// Retrieves the color associated with a specific type name using reflection.
            /// </summary>
            /// <param name="name">The case-sensitive name of the Pokemon type (e.g., "Fire", "Water").</param>
            /// <returns>The corresponding Color, or Black if the type name is not found.</returns>
            public static Color GetColor(string name) {
                return (Color)(typeof(Types).GetField(name)?.GetValue(null) ?? Color.Black);
            }
        }

        /// <summary>
        /// Contains predefined <see cref="Color"/> instances for Pokémon status conditions.
        /// These colors are used to represent the visual state of a Pokémon in the UI.
        /// </summary>
        public static class StatusConditions {
            /// <summary>Color for the Poisoned status.</summary>
            public static readonly Color Poisoned = new(0xC0, 0x60, 0xC0, 0xFF);
            /// <summary>Color for the Paralyzed status.</summary>
            public static readonly Color Paralyzed = new(0xB8, 0xB8, 0x18, 0xFF);
            /// <summary>Color for the Asleep status.</summary>
            public static readonly Color Asleep = new(0xA0, 0xA0, 0x88, 0xFF);
            /// <summary>Color for the Frozen status.</summary>
            public static readonly Color Frozen = new(0x88, 0xB0, 0xE0, 0xFF);
            /// <summary>Color for the Burned status.</summary>
            public static readonly Color Burned = new(0xE0, 0x70, 0x50, 0xFF);
            /// <summary>Color for the Fainted status.</summary>
            public static readonly Color Fainted = new(0xE8, 0x50, 0x38, 0xFF);
        }
    }
}
