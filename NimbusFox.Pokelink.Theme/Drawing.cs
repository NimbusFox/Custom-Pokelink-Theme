using System.Numerics;
using Raylib_cs;

namespace NimbusFox.Pokelink.Theme;

/// <summary>
/// Provides helper methods for drawing rounded rectangles with gradient colors using Raylib.
/// </summary>
public static class Drawing {
    /// <summary>
    /// Draws a rectangle with rounded corners and a left‑to‑right gradient.
    /// The gradient uses the same left and right colors for each side.
    /// </summary>
    /// <param name="rec">The rectangle bounds.</param>
    /// <param name="roundnessLeft">The roundness factor for the left side (0–1).</param>
    /// <param name="roundnessRight">The roundness factor for the right side (0–1).</param>
    /// <param name="segments">Number of segments used to approximate the rounded corners.</param>
    /// <param name="left">The color on the left side of the gradient.</param>
    /// <param name="right">The color on the right side of the gradient.</param>
    internal static void DrawRectangleRoundedGradient(Rectangle rec, float roundnessLeft, float roundnessRight,
        int segments, Color left, Color right) {
        DrawRectangleRoundedGradient(rec, roundnessLeft, roundnessRight, segments, left, left, right, right);
    }
    

    /// <summary>
    /// Draws a rectangle with rounded corners and a per‑corner gradient.
    /// Each corner can have its own color, enabling a 4‑color gradient effect.
    /// </summary>
    /// <param name="rec">The rectangle bounds.</param>
    /// <param name="roundnessLeft">The roundness factor for the left side (0–1).</param>
    /// <param name="roundnessRight">The roundness factor for the right side (0–1).</param>
    /// <param name="segments">Number of segments used to approximate the rounded corners.</param>
    /// <param name="topLeft">Color at the top‑left corner.</param>
    /// <param name="bottomLeft">Color at the bottom‑left corner.</param>
    /// <param name="topRight">Color at the top‑right corner.</param>
    /// <param name="bottomRight">Color at the bottom‑right corner.</param>
    internal static void DrawRectangleRoundedGradient(Rectangle rec, float roundnessLeft, float roundnessRight,
        int segments, Color topLeft, Color bottomLeft, Color topRight, Color bottomRight) {
        // Early exit for degenerate rectangles or no roundness: just use the
        // built‑in Raylib gradient function which handles non‑rounded corners.
        if ((roundnessLeft <= 0.0f && roundnessRight <= 0.0f) || rec.Width < 1 || rec.Height < 1) {
            Raylib.DrawRectangleGradientEx(rec, topLeft, bottomLeft, topRight, bottomRight);
            return;
        }

        // Clamp roundness to the valid range [0, 1].
        if (roundnessLeft >= 1.0f) {
            roundnessLeft = 1.0f;
        }

        if (roundnessRight >= 1.0f) {
            roundnessRight = 1.0f;
        }

        // Determine the actual radii based on the smallest side to avoid
        // overflowing the rectangle.
        var recSize = rec.Width > rec.Height ? rec.Height : rec.Width;
        var radiusLeft = recSize * roundnessLeft / 2;
        var radiusRight = recSize * roundnessRight / 2;

        // Ensure radii are non‑negative.
        if (radiusLeft <= 0.0f) {
            radiusLeft = 0.0f;
        }

        if (radiusRight <= 0.0f) {
            radiusRight = 0.0f;
        }

        // If both radii are zero, we already handled the non‑rounded case above.
        if (radiusRight <= 0.0f && radiusLeft <= 0.0f) {
            return;
        }

        var stepLength = 90.0f / segments;

        // Compute the 12 key points that define the rectangle with rounded
        // corners. These are used both for positioning vertices and for
        // calculating the corner centers.
        var points = new Vector2[] {
            // PO, P1, P2
            new(rec.X + radiusLeft, rec.Y), new(rec.X + rec.Width - radiusRight, rec.Y),
            new(rec.X + rec.Width, rec.Y + radiusRight),
            // P3, P4
            new(rec.X + rec.Width, rec.Y + rec.Height - radiusRight),
            new(rec.X + rec.Width - radiusRight, rec.Y + rec.Height),
            // P5, P6, P7
            new(rec.X + radiusLeft, rec.Y + rec.Height), new(rec.X, rec.Y + rec.Height - radiusLeft),
            new(rec.X, rec.Y + radiusLeft),
            // P8, P9
            new(rec.X + radiusLeft, rec.Y + radiusLeft), new(rec.X + rec.Width - radiusRight, rec.Y + radiusRight),
            // P10, P11
            new(rec.X + rec.Width - radiusRight, rec.Y + rec.Height - radiusRight),
            new(rec.X + radiusLeft, rec.Y + rec.Height - radiusLeft)
        };

        // Centers of the four rounded corners for texture mapping.
        var centers = new[] { points[8], points[9], points[10], points[11] };

        // Angles that correspond to each corner in the order:
        // top‑left, top‑right, bottom‑right, bottom‑left.
        var angles = new[] { 180.0f, 270.0f, 0.0f, 90.0f };

        var texShapes = Raylib.GetShapesTexture();
        Rlgl.SetTexture(texShapes.Id);

        var shapeRect = Raylib.GetShapesTextureRectangle();

        Rlgl.Begin(DrawMode.Quads);

        // Render each corner individually, assigning the appropriate
        // gradient color and radius.
        for (var k = 0; k < 4; k++) {
            var color = Color.Blank;
            var radius = 0f;

            switch (k) {
                case 0:
                    color = topLeft;
                    radius = radiusLeft;
                    break;
                case 3:
                    color = bottomLeft;
                    radius = radiusLeft;
                    break;
                case 1:
                    color = topRight;
                    radius = radiusRight;
                    break;
                case 2:
                    color = bottomRight;
                    radius = radiusRight;
                    break;
            }

            var angle = angles[k];
            var center = centers[k];

            // For each corner, construct a fan of vertices that approximate
            // the quarter‑circle. We render the fan in two passes:
            // 1. The textured fan that draws the rounded edge.
            // 2. The solid fan that covers the corner’s interior.
            for (var i = 0; i < segments / 2; i++) {
                Rlgl.Color4ub(color.R, color.G, color.B, color.A);
                Rlgl.TexCoord2f(shapeRect.X / texShapes.Width, shapeRect.Y / texShapes.Height);
                Rlgl.Vertex2f(center.X, center.Y);

                Rlgl.TexCoord2f((shapeRect.X + shapeRect.Width) / texShapes.Width, shapeRect.Y / texShapes.Height);
                Rlgl.Vertex2f(center.X + MathF.Cos(Raylib.DEG2RAD * (angle + stepLength * 2)) * radius,
                    center.Y + MathF.Sin(Raylib.DEG2RAD * (angle + stepLength * 2)) * radius);

                Rlgl.TexCoord2f((shapeRect.X + shapeRect.Width) / texShapes.Width,
                    (shapeRect.Y + shapeRect.Height) / texShapes.Height);
                Rlgl.Vertex2f(center.X + MathF.Cos(Raylib.DEG2RAD * (angle + stepLength)) * radius,
                    center.Y + MathF.Sin(Raylib.DEG2RAD * (angle + stepLength)) * radius);

                Rlgl.TexCoord2f(shapeRect.X / texShapes.Width, (shapeRect.Y + shapeRect.Height) / texShapes.Height);
                Rlgl.Vertex2f(center.X + MathF.Cos(Raylib.DEG2RAD * angle) * radius,
                    center.Y + MathF.Sin(Raylib.DEG2RAD * angle) * radius);

                angle += stepLength * 2;

                // Handle the case where the number of segments is odd.
                if (segments % 2 != 0) {
                    Rlgl.TexCoord2f(shapeRect.X / texShapes.Width, shapeRect.Y / texShapes.Height);
                    Rlgl.Vertex2f(center.X, center.Y);

                    Rlgl.TexCoord2f((shapeRect.X + shapeRect.Width) / texShapes.Width,
                        (shapeRect.Y + shapeRect.Height) / texShapes.Height);
                    Rlgl.Vertex2f(center.X + MathF.Cos(Raylib.DEG2RAD * (angle + stepLength)) * radius,
                        center.Y + MathF.Sin(Raylib.DEG2RAD * (angle + stepLength)) * radius);

                    Rlgl.TexCoord2f(shapeRect.X / texShapes.Width, (shapeRect.Y + shapeRect.Height) / texShapes.Height);
                    Rlgl.Vertex2f(center.X + MathF.Cos(Raylib.DEG2RAD * angle) * radius,
                        center.Y + MathF.Sin(Raylib.DEG2RAD * angle) * radius);

                    Rlgl.TexCoord2f((shapeRect.X + shapeRect.Width) / texShapes.Width, shapeRect.Y / texShapes.Height);
                    Rlgl.Vertex2f(center.X, center.Y);
                }
            }
        }
        // After rendering the four rounded corners, we still need to draw the
        // flat edges (top, bottom, left, right) that connect the corners.
        // These are rendered as textured quads using the same texture
        // coordinates as the rounded corners to keep the gradient seamless.
        Rlgl.Color4ub(topLeft.R, topLeft.G, topLeft.B, topLeft.A);
        Rlgl.TexCoord2f(shapeRect.X / texShapes.Width, shapeRect.Y / texShapes.Height);
        Rlgl.Vertex2f(points[0].X, points[0].Y);
        Rlgl.TexCoord2f(shapeRect.X / texShapes.Width, (shapeRect.Y + shapeRect.Height) / texShapes.Height);
        Rlgl.Vertex2f(points[8].X, points[8].Y);

        Rlgl.Color4ub(topRight.R, topRight.G, topRight.B, topRight.A);
        Rlgl.TexCoord2f((shapeRect.X + shapeRect.Width) / texShapes.Width,
            (shapeRect.Y + shapeRect.Height) / texShapes.Height);
        Rlgl.Vertex2f(points[9].X, points[9].Y);

        Rlgl.Color4ub(topRight.R, topRight.G, topRight.B, topRight.A);
        Rlgl.TexCoord2f((shapeRect.X + shapeRect.Width) / texShapes.Width, shapeRect.Y / texShapes.Height);
        Rlgl.Vertex2f(points[1].X, points[1].Y);

        Rlgl.Color4ub(topRight.R, topRight.G, topRight.B, topRight.A);
        Rlgl.TexCoord2f(shapeRect.X / texShapes.Width, shapeRect.Y / texShapes.Height);
        Rlgl.Vertex2f(points[2].X, points[2].Y);
        Rlgl.TexCoord2f(shapeRect.X / texShapes.Width, (shapeRect.Y + shapeRect.Height) / texShapes.Height);
        Rlgl.Vertex2f(points[9].X, points[9].Y);

        Rlgl.Color4ub(bottomRight.R, bottomRight.G, bottomRight.B, bottomRight.A);
        Rlgl.TexCoord2f((shapeRect.X + shapeRect.Width) / texShapes.Width,
            (shapeRect.Y + shapeRect.Height) / texShapes.Height);
        Rlgl.Vertex2f(points[10].X, points[10].Y);
        Rlgl.TexCoord2f((shapeRect.X + shapeRect.Width) / texShapes.Width, shapeRect.Y / texShapes.Height);
        Rlgl.Vertex2f(points[3].X, points[3].Y);

        Rlgl.Color4ub(bottomLeft.R, bottomLeft.G, bottomLeft.B, bottomLeft.A);
        Rlgl.TexCoord2f(shapeRect.X / texShapes.Width, shapeRect.Y / texShapes.Height);
        Rlgl.Vertex2f(points[11].X, points[11].Y);
        Rlgl.TexCoord2f(shapeRect.X / texShapes.Width, (shapeRect.Y + shapeRect.Height) / texShapes.Height);
        Rlgl.Vertex2f(points[5].X, points[5].Y);

        Rlgl.Color4ub(bottomRight.R, bottomRight.G, bottomRight.B, bottomRight.A);
        Rlgl.TexCoord2f((shapeRect.X + shapeRect.Width) / texShapes.Width,
            (shapeRect.Y + shapeRect.Height) / texShapes.Height);
        Rlgl.Vertex2f(points[4].X, points[4].Y);
        Rlgl.TexCoord2f((shapeRect.X + shapeRect.Width) / texShapes.Width, shapeRect.Y / texShapes.Height);
        Rlgl.Vertex2f(points[10].X, points[10].Y);

        Rlgl.Color4ub(topLeft.R, topLeft.G, topLeft.B, topLeft.A);
        Rlgl.TexCoord2f(shapeRect.X / texShapes.Width, shapeRect.Y / texShapes.Height);
        Rlgl.Vertex2f(points[8].X, points[8].Y);
        Rlgl.TexCoord2f(shapeRect.X / texShapes.Width, (shapeRect.Y + shapeRect.Height) / texShapes.Height);
        Rlgl.Vertex2f(points[7].X, points[7].Y);

        Rlgl.Color4ub(bottomLeft.R, bottomLeft.G, bottomLeft.B, bottomLeft.A);
        Rlgl.TexCoord2f((shapeRect.X + shapeRect.Width) / texShapes.Width,
            (shapeRect.Y + shapeRect.Height) / texShapes.Height);
        Rlgl.Vertex2f(points[6].X, points[6].Y);
        Rlgl.TexCoord2f((shapeRect.X + shapeRect.Width) / texShapes.Width, shapeRect.Y / texShapes.Height);
        Rlgl.Vertex2f(points[11].X, points[11].Y);

        Rlgl.Color4ub(topLeft.R, topLeft.G, topLeft.B, topLeft.A);
        Rlgl.TexCoord2f(shapeRect.X / texShapes.Width, shapeRect.Y / texShapes.Height);
        Rlgl.Vertex2f(points[8].X, points[8].Y);
        
        Rlgl.Color4ub(bottomLeft.R, bottomLeft.G, bottomLeft.B, bottomLeft.A);
        Rlgl.TexCoord2f(shapeRect.X / texShapes.Width, (shapeRect.Y + shapeRect.Height) / texShapes.Height);
        Rlgl.Vertex2f(points[11].X, points[11].Y);
        
        Rlgl.Color4ub(bottomRight.R, bottomRight.G, bottomRight.B, bottomRight.A);
        Rlgl.TexCoord2f((shapeRect.X + shapeRect.Width) / texShapes.Width, shapeRect.Y / texShapes.Height);
        Rlgl.Vertex2f(points[10].X, points[10].Y);
        
        Rlgl.Color4ub(topRight.R, topRight.G, topRight.B, topRight.A);
        Rlgl.TexCoord2f((shapeRect.X + shapeRect.Width) / texShapes.Width,
            (shapeRect.Y + shapeRect.Height) / texShapes.Height);
        Rlgl.Vertex2f(points[9].X, points[9].Y);

        Rlgl.End();
        Rlgl.SetTexture(0);
    }
}
