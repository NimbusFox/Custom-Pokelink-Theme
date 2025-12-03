using System.Numerics;
using Raylib_cs;

namespace NimbusFox.Pokelink.Theme;

/// <summary>
/// Provides helper methods for mathematical calculations and 2D vector operations
/// useful for rendering and layout.
/// </summary>
internal static class Math {
    /// <summary>
    /// Calculates the percentage that a current value represents of a maximum value.
    /// </summary>
    /// <param name="value">The current value.</param>
    /// <param name="max">The maximum possible value.</param>
    /// <returns>The calculated percentage (0-100 scale).</returns>
    internal static float GetPercentage(float value, float max) {
        return value / max * 100;
    }

    /// <summary>
    /// Calculates the numeric value of a specific percentage of a base number.
    /// </summary>
    /// <param name="value">The base number.</param>
    /// <param name="percent">The percentage to calculate (0-100 scale).</param>
    /// <returns>The resulting value.</returns>
    internal static float GetPercentageOf(float value, float percent) {
        return value * percent / 100;
    }
    
    /// <summary>
    /// Represents a 2D vector using integer coordinates.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    internal struct Vector2I(int x = 0, int y = 0) {
        /// <summary>The X component of the vector.</summary>
        public int X = x;
        /// <summary>The Y component of the vector.</summary>
        public int Y = y;

        /// <summary>
        /// Converts this integer vector to a <see cref="Vector2"/> (float vector) used by Raylib.
        /// </summary>
        /// <returns>A <see cref="Vector2"/> instance with the same coordinates.</returns>
        public Vector2 ToVector2() {
            return new Vector2(X, Y);
        }
    }

    /// <summary>
    /// Extension methods for <see cref="Texture2D"/> to compute scaled sizes while preserving aspect ratio.
    /// </summary>
    extension(Texture2D texture) {
        /// <summary>
        /// Calculates the dimensions of the texture scaled to fit a target width while maintaining the aspect ratio.
        /// If the texture is smaller than the target width, the original dimensions are returned.
        /// </summary>
        /// <param name="targetWidth">The maximum width to scale to.</param>
        /// <returns>A <see cref="Vector2"/> containing the new width and height.</returns>
        internal Vector2 GetRatioSizeW(int targetWidth) {
            if (texture.Width < targetWidth) {
                return new Vector2(texture.Width, texture.Height);
            }
        
            var targetHeight = (int)System.Math.Round(texture.Height * (targetWidth / (double)texture.Width));

            return new Vector2(targetWidth, targetHeight);
        }

        /// <summary>
        /// Calculates the dimensions of the texture scaled to fit a target height while maintaining the aspect ratio.
        /// If the texture is smaller than the target height, the original dimensions are returned.
        /// </summary>
        /// <param name="targetHeight">The maximum height to scale to.</param>
        /// <returns>A <see cref="Vector2"/> containing the new width and height.</returns>
        internal Vector2 GetRatioSizeH(int targetHeight) {
            if (texture.Height < targetHeight) {
                return new Vector2(texture.Width, texture.Height);
            }
        
            var targetWidth = (int)System.Math.Round(texture.Width * (targetHeight / (double)texture.Height));

            return new Vector2(targetWidth, targetHeight);
        }
    }
}