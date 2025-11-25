// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Numerics;
using NimbusFox.Pokelink.Theme;
using Raylib_cs;

SpriteTemplate.Register();

var client = new Client("127.0.0.1", 3000);

Console.WriteLine("Connecting to Pokelink");

client.ConnectAsync();

Raylib.SetConfigFlags(ConfigFlags.TransparentWindow | ConfigFlags.VSyncHint);

Raylib.InitWindow(715, 660, "Custom Pokelink Theme");

const int nameFontSize = 75;

const int gillFontSize = 45;

var font = Raylib.LoadFontEx("./Pokemon Solid.ttf", nameFontSize, null, 0);

var gillSansFont = Raylib.LoadFontEx("./Gill Sans.otf", gillFontSize, null, 0);

var selected = 0;

var wait = 5f;

while (!Raylib.WindowShouldClose()) {
    var fpsX = Raylib.MeasureText($"{Raylib.GetFPS()} FPS", 20);
    
    if (client is { IsConnected: false, IsConnecting: false }) {
        client.ConnectAsync();
    }
    
    Raylib.BeginDrawing();

    Raylib.ClearBackground(Color.Blank);

    Party.GetLock();

    var partyOffset = 5;

    wait -= Raylib.GetFrameTime();

    if (wait <= 0) {
        selected++;

        wait = 5;
    }

    var max = Party.Pokemon.Count(x => x.Pokemon != null);

    if (selected >= max) {
        selected = 0;
    }

    var index = 0;

    const int startX = 115;
    const int middleX = startX + 250;

    for (byte i = 0; i < Party.Pokemon.Length; i++) {
        try {
            var slot = Party.Pokemon[i];

            slot.Update(selected == index);

            if (slot.Pokemon == null) {
                continue;
            }

            var type1Color = Global.Colors.Types.GetColor(slot.Pokemon.Translations.English.Types_[0]);
            var type2Color = Global.Colors.Types.GetColor(slot.Pokemon.Translations.English.Types_.Count < 2
                ? slot.Pokemon.Translations.English.Types_[0]
                : slot.Pokemon.Translations.English.Types_[1]);

            Drawing.DrawRectangleRoundedGradient(new Rectangle(5, partyOffset, selected == index ? 120 : 100, 100), 0.5f,
                selected == index ? 0 : 0.5f, 20, type1Color, type2Color, type1Color, type2Color);

            var pSpriteSize = slot.GetPartySprite()!.Value.GetRatioSizeH(50);

            slot.RenderPartySprite(
                new Rectangle(new Vector2(55 - (pSpriteSize.X / 2), (50 - (pSpriteSize.Y / 2)) + partyOffset),
                    pSpriteSize), Color.White);
                
            slot.RenderGenderSprite(new Rectangle(5 + 70, partyOffset + 25, 20, 20), Color.White);

            if (index == selected) {
                Drawing.DrawRectangleRoundedGradient(new Rectangle(115, 5, 500, 650), 0f, 0.5f, 20, type1Color,
                    type2Color, type2Color, type1Color);

                var name = slot.Pokemon.HasNickname
                    ? slot.Pokemon.Nickname
                    : slot.Pokemon.Translations.Locale.SpeciesName;

                var nameSize = Raylib.MeasureTextEx(font, name, nameFontSize, 1f);

                Raylib.DrawTextEx(font, name, new Vector2(middleX - nameSize.X / 2, 15), nameFontSize, 1f, Color.Black);

                var spriteSize = slot.GetSprite()!.Value.GetRatioSizeH(125);

                var heightOffset = 125 - spriteSize.Y;

                if (heightOffset < 0) {
                    heightOffset = 0;
                }

                slot.RenderSprite(new Rectangle(middleX - spriteSize.X / 2, nameSize.Y + heightOffset, spriteSize),
                    Color.White);

                var levelText = $"Lv. {slot.Pokemon.Level}";

                var levelSize = Raylib.MeasureTextEx(gillSansFont, levelText, gillFontSize, 1f);

                var levelOffset = levelSize.X / 2;

                Raylib.DrawTextEx(gillSansFont, levelText,
                    new Vector2((middleX / 2f - levelOffset) + 5, nameSize.Y + 60 - (levelSize.Y / 2)), gillFontSize, 1f,
                    Color.Black);
            }

            partyOffset += 110;
        } catch (Exception ex) when (!Debugger.IsAttached) {
            // ignore
            if (Debugger.IsAttached) {
                Console.Error.WriteLine($"{ex}");
            }
        }

        index++;
    }

    Party.ReleaseLock();

    Raylib.DrawFPS((Raylib.GetScreenWidth()) - 10 - fpsX, 10);

    Raylib.EndDrawing();
    
    SpriteCache.ClearUnused();
}


SpriteCache.Dispose();
