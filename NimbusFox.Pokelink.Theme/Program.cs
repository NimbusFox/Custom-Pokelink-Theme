// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Numerics;
using NimbusFox.Pokelink.Theme;
using Raylib_cs;
using Math = NimbusFox.Pokelink.Theme.Math;

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

var wait = 3f;

var expColorLeft = new Color(0x27, 0xB6, 0xEA, 0xFF);
var expColorRight = new Color(0x8D, 0xCF, 0xE8, 0xFF);

var greenHealthLeft = new Color(0x12, 0xE8, 0x5d, 0xFF);
var greenHealthRight = new Color(0xBD, 0xF9, 0x6D, 0xFF);
var yellowHealthLeft = new Color(0xF2, 0xDE, 0x29, 0xFF);
var yellowHealthRight = new Color(0xF2, 0x79, 0x29, 0xFF);
var redHealthLeft = new Color(0xFF, 0x00, 0x00, 0xFF);
var redHealthRight = new Color(0x42, 0x15, 0x16, 0xFF);

var page = 0;

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
        if (page == 1) {
            page = 0;
            selected++;
        } else {
            page++;
        }

        wait = 3;
    }

    var max = Party.Pokemon.Count(x => x.Pokemon != null);

    if (selected >= max) {
        page = 0;
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

            var smallSpriteWidth = selected == index ? 120 : 100;

            var smallSpriteRect = new Rectangle(5, partyOffset, smallSpriteWidth, 100);

            var healthPercent = Math.GetPercentage(slot.Pokemon.Hp.Current, slot.Pokemon.Hp.Max);

            var healthWidth = (int)System.Math.Floor(Math.GetPercentageOf(smallSpriteWidth, healthPercent));

            var healthColorLeft = healthPercent > 56 ? greenHealthLeft : healthPercent > 21 ? yellowHealthLeft
                : redHealthLeft;
            var healthColorRight = healthPercent > 56 ? greenHealthRight : healthPercent > 21 ? yellowHealthRight
                : redHealthRight;

            var expWidth =
                (int)System.Math.Floor(Math.GetPercentageOf(smallSpriteWidth, (float)slot.Pokemon.ExpPercentage));

            var roundedness = selected == index ? 0 : 0.5f;

            Drawing.DrawRectangleRoundedGradient(smallSpriteRect, 0.5f, roundedness, 20, type1Color, type2Color,
                type1Color, type2Color);

            Raylib.BeginScissorMode(5, partyOffset, healthWidth, 13);

            Drawing.DrawRectangleRoundedGradient(smallSpriteRect, 0.5f, roundedness, 20, healthColorLeft,
                healthColorLeft, healthColorRight, healthColorRight);

            Raylib.EndScissorMode();

            Raylib.BeginScissorMode(5, partyOffset + 87, expWidth, 13);

            Drawing.DrawRectangleRoundedGradient(smallSpriteRect, 0.5f, roundedness, 20, expColorLeft, expColorLeft,
                expColorRight, expColorRight);

            Raylib.EndScissorMode();


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
                    new Vector2((middleX / 2f - levelOffset) + 5, nameSize.Y + 60 - (levelSize.Y / 2)), gillFontSize,
                    1f, Color.Black);

                if (page == 0) {
                    if (slot.Pokemon.Translations.Locale.HasFormName) {
                        Raylib.DrawTextEx(gillSansFont, $"Form: ({slot.Pokemon.Translations.Locale.FormName})",
                            new Vector2(startX + 5, nameSize.Y + 135), gillFontSize, 1f, Color.Black);
                    }

                    Raylib.DrawTextEx(gillSansFont, $"HP: {slot.Pokemon.Hp.Current} / {slot.Pokemon.Hp.Max}",
                        new Vector2(startX + 5, nameSize.Y + 135 + gillFontSize), gillFontSize, 1f, Color.Black);
                    if (slot.Pokemon.Level == 100) {
                        Raylib.DrawTextEx(gillSansFont, $"Max Level",
                            new Vector2(startX + 5, nameSize.Y + 135 + gillFontSize * 2), gillFontSize, 1f,
                            Color.Black);
                    } else {
                        Raylib.DrawTextEx(gillSansFont,
                            $"Lv. {(slot.Pokemon.Level == 100 ? 100 : slot.Pokemon.Level + 1)}: {slot.Pokemon.ExpToNextLevel - slot.Pokemon.Exp}",
                            new Vector2(startX + 5, nameSize.Y + 135 + gillFontSize * 2), gillFontSize, 1f,
                            Color.Black);
                    }

                    Raylib.DrawTextEx(gillSansFont, $"Item: {slot.Pokemon.Translations.Locale.HeldItemName}",
                        new Vector2(startX + 5, nameSize.Y + 135 + gillFontSize * 3), gillFontSize, 1f, Color.Black);

                    for (var move = 0; move < slot.Pokemon.Moves.Count; move++) {
                        Raylib.DrawTextEx(gillSansFont,
                            $"{slot.Pokemon.Moves[move].Locale.Name}: {slot.Pokemon.Moves[move].Pp} / {slot.Pokemon.Moves[move].MaxPP}",
                            new Vector2(startX + 5, nameSize.Y + 135 + gillFontSize * (move + 4)), gillFontSize, 1f,
                            Color.Black);
                    }
                } else {
                    const string statsText = "Stats:";
                    var statsSize = Raylib.MeasureTextEx(gillSansFont, statsText, gillFontSize, 1f);
                    
                    Raylib.DrawTextEx(gillSansFont, statsText,
                        new Vector2(middleX - statsSize.X / 2, nameSize.Y + 135), gillFontSize, 1f, Color.Black);
                    
                    Raylib.DrawTextEx(gillSansFont, "EVs    IVs",
                        new Vector2(startX + 175, nameSize.Y + 135 + gillFontSize), gillFontSize, 1f,
                        Color.Black);
                    
                    Raylib.DrawTextEx(gillSansFont, "HP", new Vector2(startX + 5, nameSize.Y + 135 + gillFontSize * 2), gillFontSize, 1f, Color.Black);
                    Raylib.DrawTextEx(gillSansFont, "Attack", new Vector2(startX + 5, nameSize.Y + 135 + gillFontSize * 3), gillFontSize, 1f, Color.Black);
                    Raylib.DrawTextEx(gillSansFont, "Defense", new Vector2(startX + 5, nameSize.Y + 135 + gillFontSize * 4), gillFontSize, 1f, Color.Black);
                    Raylib.DrawTextEx(gillSansFont, "Sp.Atk", new Vector2(startX + 5, nameSize.Y + 135 + gillFontSize * 5), gillFontSize, 1f, Color.Black);
                    Raylib.DrawTextEx(gillSansFont, "Sp.Def", new Vector2(startX + 5, nameSize.Y + 135 + gillFontSize * 6), gillFontSize, 1f, Color.Black);
                    Raylib.DrawTextEx(gillSansFont, "Speed", new Vector2(startX + 5, nameSize.Y + 135 + gillFontSize * 7), gillFontSize, 1f, Color.Black);
                    
                    Raylib.DrawTextEx(gillSansFont, $"{slot.Pokemon.Evs.Hp}", new Vector2(startX + 190, nameSize.Y + 135 + gillFontSize * 2), gillFontSize, 1f, Color.Black);
                    Raylib.DrawTextEx(gillSansFont, $"{slot.Pokemon.Evs.Atk}", new Vector2(startX + 190, nameSize.Y + 135 + gillFontSize * 3), gillFontSize, 1f, Color.Black);
                    Raylib.DrawTextEx(gillSansFont, $"{slot.Pokemon.Evs.Def}", new Vector2(startX + 190, nameSize.Y + 135 + gillFontSize * 4), gillFontSize, 1f, Color.Black);
                    Raylib.DrawTextEx(gillSansFont, $"{slot.Pokemon.Evs.Spatk}", new Vector2(startX + 190, nameSize.Y + 135 + gillFontSize * 5), gillFontSize, 1f, Color.Black);
                    Raylib.DrawTextEx(gillSansFont, $"{slot.Pokemon.Evs.Spdef}", new Vector2(startX + 190, nameSize.Y + 135 + gillFontSize * 6), gillFontSize, 1f, Color.Black);
                    Raylib.DrawTextEx(gillSansFont, $"{slot.Pokemon.Evs.Spd}", new Vector2(startX + 190, nameSize.Y + 135 + gillFontSize * 7), gillFontSize, 1f, Color.Black);
                    
                    Raylib.DrawTextEx(gillSansFont, $"{slot.Pokemon.Ivs.Hp}", new Vector2(startX + 300, nameSize.Y + 135 + gillFontSize * 2), gillFontSize, 1f, Color.Black);
                    Raylib.DrawTextEx(gillSansFont, $"{slot.Pokemon.Ivs.Atk}", new Vector2(startX + 300, nameSize.Y + 135 + gillFontSize * 3), gillFontSize, 1f, Color.Black);
                    Raylib.DrawTextEx(gillSansFont, $"{slot.Pokemon.Ivs.Def}", new Vector2(startX + 300, nameSize.Y + 135 + gillFontSize * 4), gillFontSize, 1f, Color.Black);
                    Raylib.DrawTextEx(gillSansFont, $"{slot.Pokemon.Ivs.Spatk}", new Vector2(startX + 300, nameSize.Y + 135 + gillFontSize * 5), gillFontSize, 1f, Color.Black);
                    Raylib.DrawTextEx(gillSansFont, $"{slot.Pokemon.Ivs.Spdef}", new Vector2(startX + 300, nameSize.Y + 135 + gillFontSize * 6), gillFontSize, 1f, Color.Black);
                    Raylib.DrawTextEx(gillSansFont, $"{slot.Pokemon.Ivs.Spd}", new Vector2(startX + 300, nameSize.Y + 135 + gillFontSize * 7), gillFontSize, 1f, Color.Black);
                    
                    Raylib.DrawTextEx(gillSansFont, $"{slot.Pokemon.Stats.Hp}", new Vector2(startX + 400, nameSize.Y + 135 + gillFontSize * 2), gillFontSize, 1f, Color.Black);
                    Raylib.DrawTextEx(gillSansFont, $"{slot.Pokemon.Stats.Atk}", new Vector2(startX + 400, nameSize.Y + 135 + gillFontSize * 3), gillFontSize, 1f, Color.Black);
                    Raylib.DrawTextEx(gillSansFont, $"{slot.Pokemon.Stats.Def}", new Vector2(startX + 400, nameSize.Y + 135 + gillFontSize * 4), gillFontSize, 1f, Color.Black);
                    Raylib.DrawTextEx(gillSansFont, $"{slot.Pokemon.Stats.Spatk}", new Vector2(startX + 400, nameSize.Y + 135 + gillFontSize * 5), gillFontSize, 1f, Color.Black);
                    Raylib.DrawTextEx(gillSansFont, $"{slot.Pokemon.Stats.Spdef}", new Vector2(startX + 400, nameSize.Y + 135 + gillFontSize * 6), gillFontSize, 1f, Color.Black);
                    Raylib.DrawTextEx(gillSansFont, $"{slot.Pokemon.Stats.Spd}", new Vector2(startX + 400, nameSize.Y + 135 + gillFontSize * 7), gillFontSize, 1f, Color.Black);
                }
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
