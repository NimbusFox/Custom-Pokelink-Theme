using System.Numerics;
using Raylib_cs;

namespace NimbusFox.Pokelink.Theme;

public static class Drawing {
    /// <summary>
    /// Ported from https://www.raylib.com/examples/shapes/loader.html?name=shapes_rectangle_advanced
    /// </summary>
    /// <param name="rec"></param>
    /// <param name="roundnessLeft"></param>
    /// <param name="roundnessRight"></param>
    /// <param name="segments"></param>
    /// <param name="left"></param>
    /// <param name="right"></param>
    internal static void DrawRectangleRoundedGradient(Rectangle rec, float roundnessLeft, float roundnessRight,
        int segments, Color left, Color right) {
        DrawRectangleRoundedGradient(rec, roundnessLeft, roundnessRight, segments, left, left, right, right);
    }

    /// <summary>
    /// Ported from https://www.raylib.com/examples/shapes/loader.html?name=shapes_rectangle_advanced
    /// </summary>
    /// <param name="rec"></param>
    /// <param name="roundnessLeft"></param>
    /// <param name="roundnessRight"></param>
    /// <param name="segments"></param>
    /// <param name="topLeft"></param>
    /// <param name="bottomLeft"></param>
    /// <param name="topRight"></param>
    /// <param name="bottomRight"></param>
    internal static void DrawRectangleRoundedGradient(Rectangle rec, float roundnessLeft, float roundnessRight,
        int segments, Color topLeft, Color bottomLeft, Color topRight, Color bottomRight) {
        if ((roundnessLeft <= 0.0f && roundnessRight <= 0.0f) || rec.Width < 1 || rec.Height < 1) {
            Raylib.DrawRectangleGradientEx(rec, topLeft, bottomLeft, topRight, bottomRight);
            return;
        }

        if (roundnessLeft >= 1.0f) {
            roundnessLeft = 1.0f;
        }

        if (roundnessRight >= 1.0f) {
            roundnessRight = 1.0f;
        }

        var recSize = rec.Width > rec.Height ? rec.Height : rec.Width;
        var radiusLeft = recSize * roundnessLeft / 2;
        var radiusRight = recSize * roundnessRight / 2;

        if (radiusLeft <= 0.0f) {
            radiusLeft = 0.0f;
        }

        if (radiusRight <= 0.0f) {
            radiusRight = 0.0f;
        }

        if (radiusRight <= 0.0f && radiusLeft <= 0.0f) {
            return;
        }

        var stepLength = 90.0f / segments;

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

        var centers = new Vector2[] { points[8], points[9], points[10], points[11] };

        var angles = new float[] { 180.0f, 270.0f, 0.0f, 90.0f };

        var texShapes = Raylib.GetShapesTexture();
        Rlgl.SetTexture(texShapes.Id);

        var shapeRect = Raylib.GetShapesTextureRectangle();

        Rlgl.Begin(DrawMode.Quads);

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
