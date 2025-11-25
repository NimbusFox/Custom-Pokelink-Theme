using System.Numerics;
using Raylib_cs;

namespace NimbusFox.Pokelink.Theme;

internal static class Math {
    internal static float GetPercentage(float value, float max) {
        return value / max * 100;
    }
    
    internal struct Vector2I(int x = 0, int y = 0) {
        public int X = x;
        public int Y = y;

        public Vector2 ToVector2() {
            return new Vector2(X, Y);
        }
    }

    extension(Texture2D texture) {
        internal Vector2 GetRatioSizeW(int targetWidth) {
            if (texture.Width < targetWidth) {
                return new Vector2(texture.Width, texture.Height);
            }
            
            var targetHeight = (int)System.Math.Round(texture.Height * (targetWidth / (double)texture.Width));

            return new Vector2(targetWidth, targetHeight);
        }

        internal Vector2 GetRatioSizeH(int targetHeight) {
            if (texture.Height < targetHeight) {
                return new Vector2(texture.Width, texture.Height);
            }
            
            var targetWidth = (int)System.Math.Round(texture.Width * (targetHeight / (double)texture.Height));

            return new Vector2(targetWidth, targetHeight);
        }
    }
}
