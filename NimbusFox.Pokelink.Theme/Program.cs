using System.Diagnostics;
using System.Numerics;
using NimbusFox.Pokelink.Theme;
using Raylib_cs;
using Math = NimbusFox.Pokelink.Theme.Math;

// Register the Handlebars templates used for resolving sprite URLs.
SpriteTemplate.Register();

// Initialize the Pokelink client to connect to the local server (default port 3000).
var client = new Client("127.0.0.1", 3000);

Console.WriteLine("Connecting to Pokelink");

// Start the connection process asynchronously.
client.ConnectAsync();

// Configure Raylib window flags:
// - TransparentWindow: Allows the background to be transparent (useful for overlays).
// - VSyncHint: Attempts to sync frame rate with the monitor refresh rate.
Raylib.SetConfigFlags(ConfigFlags.TransparentWindow | ConfigFlags.VSyncHint);

// Initialize the actual window with dimensions 715x660 and a title.
Raylib.InitWindow(715, 660, "Custom Pokelink Theme");

// Define constants for font sizes.
const int nameFontSize = 75;
const int gillFontSize = 45;

// Load custom fonts from local files.
// LoadFontEx allows loading specific glyph ranges if needed (0 loads default).
var font = Raylib.LoadFontEx("./Pokemon Solid.ttf", nameFontSize, null, 0);
var gillSansFont = Raylib.LoadFontEx("./Gill Sans.otf", gillFontSize, null, 0);

// Tracks the currently selected Pokémon index in the party (0-5).
var selected = 0;

// Timer for auto-rotating the display between pages and Pokémon slots.
var wait = 3f;

// Define standard color palettes for UI elements (Experience bar colors).
var expColorLeft = new Color(0x27, 0xB6, 0xEA, 0xFF);
var expColorRight = new Color(0x8D, 0xCF, 0xE8, 0xFF);

// Health bar colors (Green, Yellow, Red) for gradient rendering.
var greenHealthLeft = new Color(0x12, 0xE8, 0x5d, 0xFF);
var greenHealthRight = new Color(0xBD, 0xF9, 0x6D, 0xFF);
var yellowHealthLeft = new Color(0xF2, 0xDE, 0x29, 0xFF);
var yellowHealthRight = new Color(0xF2, 0x79, 0x29, 0xFF);
var redHealthLeft = new Color(0xFF, 0x00, 0x00, 0xFF);
var redHealthRight = new Color(0x42, 0x15, 0x16, 0xFF);

// Tracks the current information page (0 = General Info, 1 = Stats/EVs/IVs).
var page = 0;

// Main Game Loop: Runs until the window close button is pressed.
while (!Raylib.WindowShouldClose()) {
    // Calculate width of FPS text for positioning.
    var fpsX = Raylib.MeasureText($"{Raylib.GetFPS()} FPS", 20);

    // Auto-reconnect logic: If disconnected, try to connect again.
    if (client is { IsConnected: false, IsConnecting: false }) {
        client.ConnectAsync();
    }

    // Begin the frame drawing sequence.
    Raylib.BeginDrawing();

    // Clear the entire buffer to transparent.
    Raylib.ClearBackground(Color.Blank);

    // Thread safety: Lock the party data while reading/rendering to prevent race conditions 
    // if the network thread updates data simultaneously.
    Party.GetLock();

    // Vertical offset counter for drawing the party list on the left side.
    var partyOffset = 5;

    // Update the rotation timer using the time elapsed since the last frame.
    wait -= Raylib.GetFrameTime();

    // Logic to switch pages or selected Pokémon when timer expires.
    if (wait <= 0) {
        if (page == 1) {
            // If on Stats page (1), go back to Info page (0) and move to next Pokémon.
            page = 0;
            selected++;
        } else {
            // If on Info page (0), show Stats page (1) for the same Pokémon.
            page++;
        }

        // Reset timer to 3 seconds.
        wait = 3;
    }

    // Calculate how many valid Pokémon are currently in the party.
    var max = Party.Pokemon.Count(x => x.Pokemon != null);

    // Loop the selection index back to 0 if it exceeds the party count.
    if (selected >= max) {
        page = 0;
        selected = 0;
    }

    // Current index iterator for the loop below.
    var index = 0;

    // Layout constants for the detailed view on the right.
    const int startX = 115;
    const int middleX = startX + 250;

    // Iterate through all 6 possible party slots.
    for (byte i = 0; i < Party.Pokemon.Length; i++) {
        try {
            var slot = Party.Pokemon[i];

            // Update sprite animation state (only the selected one might need specific updates if logic dictates).
            slot.Update(selected == index);

            // Skip empty slots.
            if (slot.Pokemon == null) {
                continue;
            }

            // Determine background colors based on Pokémon types.
            // If dual type, gradient goes Type1 -> Type2. If single type, Type1 -> Type1.
            var type1Color = Global.Colors.Types.GetColor(slot.Pokemon.Translations.English.Types_[0]);
            var type2Color = Global.Colors.Types.GetColor(slot.Pokemon.Translations.English.Types_.Count < 2
                ? slot.Pokemon.Translations.English.Types_[0]
                : slot.Pokemon.Translations.English.Types_[1]);

            // Highlight the selected Pokémon in the side list by making it wider.
            var smallSpriteWidth = selected == index ? 120 : 100;

            // Define the rectangle for the side list entry.
            var smallSpriteRect = new Rectangle(5, partyOffset, smallSpriteWidth, 100);

            // Calculate health percentage (0.0 to 1.0).
            var healthPercent = Math.GetPercentage(slot.Pokemon.Hp.Current, slot.Pokemon.Hp.Max);

            // Calculate pixel width of the health bar.
            var healthWidth = (int)System.Math.Floor(Math.GetPercentageOf(smallSpriteWidth, healthPercent));

            // Determine health bar color (Green > 56%, Yellow > 21%, otherwise Red).
            var healthColorLeft = healthPercent > 56 ? greenHealthLeft : healthPercent > 21 ? yellowHealthLeft
                : redHealthLeft;
            var healthColorRight = healthPercent > 56 ? greenHealthRight : healthPercent > 21 ? yellowHealthRight
                : redHealthRight;

            // Calculate XP bar width.
            var expWidth =
                (int)System.Math.Floor(Math.GetPercentageOf(smallSpriteWidth, (float)slot.Pokemon.ExpPercentage));

            // Rounded corners logic: Selected item is square on the right side (0 roundedness), unselected is rounded (0.5).
            var roundedness = selected == index ? 0 : 0.5f;

            // Draw the background card for the list item.
            Drawing.DrawRectangleRoundedGradient(smallSpriteRect, 0.5f, roundedness, 20, type1Color, type2Color,
                type1Color, type2Color);

            // Draw Health Bar using Scissor Mode (masks the drawing area).
            // 1. Set drawing mask to the calculated health width.
            Raylib.BeginScissorMode(5, partyOffset, healthWidth, 13);
            // 2. Draw the full health gradient (it will be clipped by the scissor).
            Drawing.DrawRectangleRoundedGradient(smallSpriteRect, 0.5f, roundedness, 20, healthColorLeft,
                healthColorLeft, healthColorRight, healthColorRight);
            Raylib.EndScissorMode();

            // Draw Experience Bar (similar masking technique).
            Raylib.BeginScissorMode(5, partyOffset + 87, expWidth, 13);
            Drawing.DrawRectangleRoundedGradient(smallSpriteRect, 0.5f, roundedness, 20, expColorLeft, expColorLeft,
                expColorRight, expColorRight);
            Raylib.EndScissorMode();

            // Fetch and render the party icon (small sprite).
            var pSpriteSize = slot.GetPartySprite()!.Value.GetRatioSizeH(50);

            slot.RenderPartySprite(
                new Rectangle(new Vector2(55 - (pSpriteSize.X / 2), (50 - (pSpriteSize.Y / 2)) + partyOffset),
                    pSpriteSize), Color.White);

            // Render gender icon next to the party sprite.
            slot.RenderGenderSprite(new Rectangle(5 + 70, partyOffset + 25, 20, 20), Color.White);

            // --- DETAILED VIEW RENDERING (Right Side) ---
            if (index == selected) {
                // Draw the large background card for the selected Pokémon details.
                Drawing.DrawRectangleRoundedGradient(new Rectangle(115, 5, 500, 650), 0f, 0.5f, 20, type1Color,
                    type2Color, type2Color, type1Color);

                // Get display name (Nickname if exists, otherwise Species name).
                var name = slot.Pokemon.HasNickname
                    ? slot.Pokemon.Nickname
                    : slot.Pokemon.Translations.Locale.SpeciesName;

                // Measure text size to center it.
                var nameSize = Raylib.MeasureTextEx(font, name, nameFontSize, 1f);

                // Draw Name.
                Raylib.DrawTextEx(font, name, new Vector2(middleX - nameSize.X / 2, 15), nameFontSize, 1f, Color.Black);

                // Render the main large sprite (optionally animated).
                var spriteSize = slot.GetSprite()!.Value.GetRatioSizeH(125);

                // Ensure sprite aligns properly even if short.
                var heightOffset = 125 - spriteSize.Y;
                if (heightOffset < 0) {
                    heightOffset = 0;
                }

                slot.RenderSprite(new Rectangle(middleX - spriteSize.X / 2, nameSize.Y + heightOffset, spriteSize),
                    Color.White);

                // Draw Level.
                var levelText = $"Lv. {slot.Pokemon.Level}";
                var levelSize = Raylib.MeasureTextEx(gillSansFont, levelText, gillFontSize, 1f);
                var levelOffset = levelSize.X / 2;

                Raylib.DrawTextEx(gillSansFont, levelText,
                    new Vector2((middleX / 2f - levelOffset) + 5, nameSize.Y + 60 - (levelSize.Y / 2)), gillFontSize,
                    1f, Color.Black);

                // --- PAGE 0: GENERAL INFO ---
                if (page == 0) {
                    // Form name (e.g., "Alolan", "Mega").
                    if (slot.Pokemon.Translations.Locale.HasFormName) {
                        Raylib.DrawTextEx(gillSansFont, $"Form: ({slot.Pokemon.Translations.Locale.FormName})",
                            new Vector2(startX + 5, nameSize.Y + 135), gillFontSize, 1f, Color.Black);
                    }

                    // Exact HP values.
                    Raylib.DrawTextEx(gillSansFont, $"HP: {slot.Pokemon.Hp.Current} / {slot.Pokemon.Hp.Max}",
                        new Vector2(startX + 5, nameSize.Y + 135 + gillFontSize), gillFontSize, 1f, Color.Black);
                    
                    // Experience to next level.
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

                    // Held Item.
                    Raylib.DrawTextEx(gillSansFont, $"Item: {slot.Pokemon.Translations.Locale.HeldItemName}",
                        new Vector2(startX + 5, nameSize.Y + 135 + gillFontSize * 3), gillFontSize, 1f, Color.Black);

                    // Moveset (Loop through up to 4 moves).
                    for (var move = 0; move < slot.Pokemon.Moves.Count; move++) {
                        Raylib.DrawTextEx(gillSansFont,
                            $"{slot.Pokemon.Moves[move].Locale.Name}: {slot.Pokemon.Moves[move].Pp} / {slot.Pokemon.Moves[move].MaxPP}",
                            new Vector2(startX + 5, nameSize.Y + 135 + gillFontSize * (move + 4)), gillFontSize, 1f,
                            Color.Black);
                    }
                } 
                // --- PAGE 1: STATS & EVS/IVS ---
                else {
                    const string statsText = "Stats:";
                    var statsSize = Raylib.MeasureTextEx(gillSansFont, statsText, gillFontSize, 1f);
                
                    // Header.
                    Raylib.DrawTextEx(gillSansFont, statsText,
                        new Vector2(middleX - statsSize.X / 2, nameSize.Y + 135), gillFontSize, 1f, Color.Black);
                
                    // Column Headers.
                    Raylib.DrawTextEx(gillSansFont, "EVs    IVs",
                        new Vector2(startX + 175, nameSize.Y + 135 + gillFontSize), gillFontSize, 1f,
                        Color.Black);
                
                    // Row Headers (Stat Names).
                    Raylib.DrawTextEx(gillSansFont, "HP", new Vector2(startX + 5, nameSize.Y + 135 + gillFontSize * 2), gillFontSize, 1f, Color.Black);
                    Raylib.DrawTextEx(gillSansFont, "Attack", new Vector2(startX + 5, nameSize.Y + 135 + gillFontSize * 3), gillFontSize, 1f, Color.Black);
                    Raylib.DrawTextEx(gillSansFont, "Defense", new Vector2(startX + 5, nameSize.Y + 135 + gillFontSize * 4), gillFontSize, 1f, Color.Black);
                    Raylib.DrawTextEx(gillSansFont, "Sp.Atk", new Vector2(startX + 5, nameSize.Y + 135 + gillFontSize * 5), gillFontSize, 1f, Color.Black);
                    Raylib.DrawTextEx(gillSansFont, "Sp.Def", new Vector2(startX + 5, nameSize.Y + 135 + gillFontSize * 6), gillFontSize, 1f, Color.Black);
                    Raylib.DrawTextEx(gillSansFont, "Speed", new Vector2(startX + 5, nameSize.Y + 135 + gillFontSize * 7), gillFontSize, 1f, Color.Black);
                
                    // EV Values Column.
                    Raylib.DrawTextEx(gillSansFont, $"{slot.Pokemon.Evs.Hp}", new Vector2(startX + 190, nameSize.Y + 135 + gillFontSize * 2), gillFontSize, 1f, Color.Black);
                    Raylib.DrawTextEx(gillSansFont, $"{slot.Pokemon.Evs.Atk}", new Vector2(startX + 190, nameSize.Y + 135 + gillFontSize * 3), gillFontSize, 1f, Color.Black);
                    Raylib.DrawTextEx(gillSansFont, $"{slot.Pokemon.Evs.Def}", new Vector2(startX + 190, nameSize.Y + 135 + gillFontSize * 4), gillFontSize, 1f, Color.Black);
                    Raylib.DrawTextEx(gillSansFont, $"{slot.Pokemon.Evs.Spatk}", new Vector2(startX + 190, nameSize.Y + 135 + gillFontSize * 5), gillFontSize, 1f, Color.Black);
                    Raylib.DrawTextEx(gillSansFont, $"{slot.Pokemon.Evs.Spdef}", new Vector2(startX + 190, nameSize.Y + 135 + gillFontSize * 6), gillFontSize, 1f, Color.Black);
                    Raylib.DrawTextEx(gillSansFont, $"{slot.Pokemon.Evs.Spd}", new Vector2(startX + 190, nameSize.Y + 135 + gillFontSize * 7), gillFontSize, 1f, Color.Black);
                
                    // IV Values Column.
                    Raylib.DrawTextEx(gillSansFont, $"{slot.Pokemon.Ivs.Hp}", new Vector2(startX + 300, nameSize.Y + 135 + gillFontSize * 2), gillFontSize, 1f, Color.Black);
                    Raylib.DrawTextEx(gillSansFont, $"{slot.Pokemon.Ivs.Atk}", new Vector2(startX + 300, nameSize.Y + 135 + gillFontSize * 3), gillFontSize, 1f, Color.Black);
                    Raylib.DrawTextEx(gillSansFont, $"{slot.Pokemon.Ivs.Def}", new Vector2(startX + 300, nameSize.Y + 135 + gillFontSize * 4), gillFontSize, 1f, Color.Black);
                    Raylib.DrawTextEx(gillSansFont, $"{slot.Pokemon.Ivs.Spatk}", new Vector2(startX + 300, nameSize.Y + 135 + gillFontSize * 5), gillFontSize, 1f, Color.Black);
                    Raylib.DrawTextEx(gillSansFont, $"{slot.Pokemon.Ivs.Spdef}", new Vector2(startX + 300, nameSize.Y + 135 + gillFontSize * 6), gillFontSize, 1f, Color.Black);
                    Raylib.DrawTextEx(gillSansFont, $"{slot.Pokemon.Ivs.Spd}", new Vector2(startX + 300, nameSize.Y + 135 + gillFontSize * 7), gillFontSize, 1f, Color.Black);
                
                    // Total Stats Values Column.
                    Raylib.DrawTextEx(gillSansFont, $"{slot.Pokemon.Stats.Hp}", new Vector2(startX + 400, nameSize.Y + 135 + gillFontSize * 2), gillFontSize, 1f, Color.Black);
                    Raylib.DrawTextEx(gillSansFont, $"{slot.Pokemon.Stats.Atk}", new Vector2(startX + 400, nameSize.Y + 135 + gillFontSize * 3), gillFontSize, 1f, Color.Black);
                    Raylib.DrawTextEx(gillSansFont, $"{slot.Pokemon.Stats.Def}", new Vector2(startX + 400, nameSize.Y + 135 + gillFontSize * 4), gillFontSize, 1f, Color.Black);
                    Raylib.DrawTextEx(gillSansFont, $"{slot.Pokemon.Stats.Spatk}", new Vector2(startX + 400, nameSize.Y + 135 + gillFontSize * 5), gillFontSize, 1f, Color.Black);
                    Raylib.DrawTextEx(gillSansFont, $"{slot.Pokemon.Stats.Spdef}", new Vector2(startX + 400, nameSize.Y + 135 + gillFontSize * 6), gillFontSize, 1f, Color.Black);
                    Raylib.DrawTextEx(gillSansFont, $"{slot.Pokemon.Stats.Spd}", new Vector2(startX + 400, nameSize.Y + 135 + gillFontSize * 7), gillFontSize, 1f, Color.Black);
                }
            }

            // Increase offset for next party list item.
            partyOffset += 110;
        } catch (Exception ex) when (!Debugger.IsAttached) {
            // General exception handling block. 
            // If debugger is NOT attached, swallow exceptions to keep the app running.
            if (Debugger.IsAttached) {
                Console.Error.WriteLine($"{ex}");
            }
        }

        index++;
    }

    // Release the thread lock for the party data.
    Party.ReleaseLock();

    // Draw current FPS counter in top right corner.
    Raylib.DrawFPS((Raylib.GetScreenWidth()) - 10 - fpsX, 10);

    // Submit the frame for rendering.
    Raylib.EndDrawing();

    // Perform cycle-based garbage collection for sprites.
    SpriteCache.ClearUnused();
}

// Cleanup when exiting the application loop.
SpriteCache.Dispose();
