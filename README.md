# Pokelink C# Raylib Theme

**The following documentation was created with the assistance of AI**

This project acts as a reference implementation for building a high-performance, native desktop theme for [Pokelink](https://pokelink.app/).

Unlike standard web-based themes (HTML/CSS/JS), this application runs as a standalone .NET 9 Console Application using **Raylib** for rendering. It demonstrates how to consume the Pokelink protocol, manage real-time game data, and handle advanced sprite caching and animation efficiently.

## Project Overview

The application connects to a local Pokelink session, listens for party updates, and renders a sleek UI displaying:
*   Current Party Members (Species, Form, Gender)
*   Real-time Health and Experience bars
*   Detailed Stats (EVs, IVs, Raw Stats)
*   Move sets and Held Items
*   Animated Sprites (GIF support)

## Architecture & Key Components

If you are looking to build your own C# theme, this project provides several reusable systems.

### 1. The Sprite System (`PokeSprite` & `SpriteCache`)
Handling Pokemon sprites is complex due to the sheer volume of files and formats (PNG vs GIF). This project implements a robust two-layer caching system.

*   **Memory Layer:** Sprites currently in use are kept in a `Dictionary` for instant access.
*   **Disk Layer:** To avoid thousands of small files, sprites are cached inside a single Zip archive (`sprite.cache`) managed by `SpriteCache.cs`.
*   **Format Handling:**
    *   `PokeSprite.cs`: Handles static images (PNG).
    *   `PokeAnimatedSprite.cs`: Handles GIFs. It uses **ImageSharp** to decode frames and manually updates the Raylib texture on the GPU to simulate animation.

**How to use this in your theme:**
You can copy `SpriteCache.cs`, `PokeSprite.cs`, and `PokeAnimatedSprite.cs` directly into your project to instantly get a thread-safe, persistent, and animated sprite loader.

### 2. Dynamic Sprite URL Generation (`SpriteTemplate`)
Pokemon file naming conventions vary wildly between sprite repositories (e.g. handling "Nidoran♀" or "Shellos-East").

`SpriteTemplate.cs` uses **Handlebars.net** to decouple your code from the specific URL structure. Instead of hardcoding string manipulation, you register a template string:

```csharp
// Example template used in the project
"https://assets.pokelink.xyz/.../{{toLower (noSpaces speciesName)}}{{ifElse isShiny 'shiny' 'normal'}}.png"
```


This allows theme creators to switch sprite sources (e.g., from Home sprites to Gen 5 pixel art) by changing a single string, without rewriting logic.

### 3. Data Management (`Party.cs`)
The `Party` class manages the 6 slots of a Pokemon team. It handles:
*   **Thread Safety:** Uses locks to ensure that data coming in from the network doesn't clash with the rendering loop.
*   **State Tracking:** The `Slot` class knows when a Pokemon has changed (e.g., evolution or swapping slots) and automatically discards old sprites to free memory.

### 4. Rendering (`Program.cs` & `Drawing.cs`)
The rendering loop utilizes **Raylib-cs**.
*   **Program.cs:** Contains the main loop. It handles the layout logic, determining if the user is viewing the "General Info" page or the "Stats" page.
*   **Drawing.cs:** Contains low-level OpenGL (RLGL) wrappers to draw advanced shapes, such as rounded rectangles with multi-point gradients (used for the health and type bars).

## How to Create Your Own Theme

To fork this project or build your own based on it:

### 1. Customize the Layout
Modify the `while (!Raylib.WindowShouldClose())` loop in `Program.cs`.
*   **Coordinates:** Change `Raylib.DrawTextEx` and `RenderSprite` positions to move elements.
*   **Fonts:** Load new `.ttf` or `.otf` files using `Raylib.LoadFontEx`.

### 2. Change the Visual Style
*   **Colors:** Edit `Global.cs` to change the type colors (Fire, Water, Grass, etc.) or status condition colors.
*   **Gradients:** Use the helper methods in `Drawing.cs` to create unique UI backgrounds.

### 3. Switch Sprite Packs
In `Program.cs`, you will see a call to `SpriteTemplate.Register()`. You can pass a custom Handlebars string to `RegisterTemplate()` to point to a different URL structure (e.g., Serebii, PokemonDb, or a local folder).

### 4. Add New Data
The `Pokemon` object (from `Pokelink.Core.Proto.V2`) contains much more data than currently displayed. You can easily add:
*   Friendship levels
*   Pokeball type used
*   Met location
*   Egg steps

## Prerequisites

*   **.NET 9.0 SDK**
*   **Pokelink** (running locally on port 3000)
*   An active Pokelink session

## Dependencies

*   `Raylib-cs`: Graphics and Windowing.
*   `SixLabors.ImageSharp`: Image processing and GIF decoding.
*   `Handlebars.Net`: Template engine for sprite URLs.
*   `Google.Protobuf`: For decoding Pokelink data.
