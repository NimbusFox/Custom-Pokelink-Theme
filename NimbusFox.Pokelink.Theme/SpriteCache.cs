using System.IO.Compression;
using System.Net;
using System.Reflection;
using Pokelink.Core.Proto.V2;
using Raylib_cs;

namespace NimbusFox.Pokelink.Theme;

// Internal static class responsible for managing, caching, and retrieving Pokemon sprites.
// It implements a two-layer cache: 
// 1. In-Memory (Dictionary) for active sprites.
// 2. On-Disk (ZipArchive) for persistent storage across sessions.
internal static class SpriteCache {
    // In-memory cache mapping URLs to instantiated PokeSprite objects.
    private static readonly Dictionary<string, PokeSprite> Sprites = new();

    // Represents the persistent on-disk cache file (sprite.cache) as a Zip archive.
    // This allows storing multiple images in a single file without cluttering the directory.
    private static readonly ZipArchive ImageCache;

    // The file stream backing the ZipArchive.
    private static readonly FileStream ICF;

    private static readonly HttpClient HttpClient;

    internal static readonly PokeSprite MaleIcon;

    internal static readonly PokeSprite FemaleIcon;

    internal static readonly PokeSprite GenderlessIcon;

    static SpriteCache() {
        ICF = new FileStream("./sprite.cache", FileMode.OpenOrCreate);
        ImageCache = new ZipArchive(ICF, ZipArchiveMode.Update, true);

        HttpClient = new HttpClient();

        MaleIcon = new PokeSprite(
            Assembly.GetAssembly(typeof(SpriteCache))!.GetManifestResourceStream("Theme.Sprite.Male")!, "png");

        FemaleIcon = new PokeSprite(
            Assembly.GetAssembly(typeof(SpriteCache))!.GetManifestResourceStream("Theme.Sprite.Female")!, "png");

        GenderlessIcon = new PokeSprite(
            Assembly.GetAssembly(typeof(SpriteCache))!.GetManifestResourceStream("Theme.Sprite.Genderless")!, "png");
    }

    // Disposes of all resources, including clearing the cache and closing the file stream.
    internal static void Dispose() {
        Reset();
        ImageCache.Dispose();
        ICF.Dispose();
    }

    // Clears the in-memory cache and forces garbage collection.
    internal static void Reset() {
        // Iterate over all active sprites to unload their textures and dispose of streams.
        foreach (var (_, sprite) in Sprites) {
            sprite.UnloadTexture();
            sprite.UnloadImage();
            sprite.Dispose();
        }

        Sprites.Clear();
    
        // Force GC to reclaim memory immediately, useful when reloading themes or clearing large caches.
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    // Fetches a sprite for a specific Pokemon.
    // Logic flow: Check Memory -> Check Web (Head) -> Check Disk Cache -> Download if needed -> Fallback if failed.
    internal static PokeSprite FetchSprite(Pokemon pokemon) {
        // Generate the URL for the sprite based on the Pokemon data.
        var url = SpriteTemplate.Handle(pokemon);

        // Send a request to the URL, only reading headers initially to check status.
        var response = HttpClient.Send(new HttpRequestMessage(HttpMethod.Get, url),
            HttpCompletionOption.ResponseHeadersRead);

        // 1. Check In-Memory Cache: If we already have this URL loaded, return it immediately.
        if (Sprites.TryGetValue(url, out var sprite)) {
            return sprite;
        }

        // Create a cache path key by removing protocol prefixes.
        var path = url.Replace("https://", "").Replace("http://", "");
        Stream? stream = null;

        // 2. Check Web Response: If the main URL failed (e.g. 404), switch to the fallback sprite.
        if (!response.IsSuccessStatusCode) {
            // Replace placeholder host with localhost for local fallback.
            url = pokemon.FallbackSprite.Replace("$POKELINK_HOST", "http://localhost:3000");
            // Fetch the fallback sprite.
            response = HttpClient.Send(new HttpRequestMessage(HttpMethod.Get, url));

            // Read the fallback stream directly.
            stream = response.Content.ReadAsStream();

            // Set path to null to prevent caching fallback sprites in the main Zip archive.
            path = null;
        }

        // 3. Disk Cache Logic (Only if the main URL was successful, i.e., path != null).
        if (path != null) {
            // Check if the file already exists in the ZipArchive.
            var entry = ImageCache.Entries.FirstOrDefault(x => x.FullName == path);

            if (entry == null) {
                // CASE: New File. Create a new entry in the zip.
                entry = ImageCache.CreateEntry(path, CompressionLevel.SmallestSize);
                stream = entry.Open();

                // Download the content and copy it into the zip stream.
                response.Content.ReadAsStream().CopyTo(stream);

                // Reset stream position to beginning so it can be read later for sprite creation.
                stream.Seek(0, SeekOrigin.Begin);
            
                // Ensure data is written to the zip stream.
                stream.Flush();
            
                // Flush the file stream to ensure physical write.
                ICF.Flush();
            } else {
                // CASE: Existing File. Open the entry.
                stream = entry.Open();

                // Basic cache invalidation check:
                // If the content length from the web differs from the local file, update the cache.
                if (stream.Length != response.Content.Headers.ContentLength) {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.SetLength(0); // Truncate existing data.

                    // Overwrite with new data.
                    response.Content.ReadAsStream().CopyTo(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                }
            }
        }

        // Safety check: Stream should be valid by now.
        if (stream == null) {
            throw new Exception("We shouldn't get here. But something very wrong will need of happened to get here!");
        }

        // 4. Format Detection using ImageSharp.
        // We peek at the stream headers to determine if it's a GIF or static image.
        var extensions = SixLabors.ImageSharp.Image.DetectFormat(stream).FileExtensions.ToArray();

        // 5. Instantiation.
        if (extensions.Contains("gif")) {
            // Create an animated sprite for GIFs.
            Sprites[url] = new PokeAnimatedSprite(stream);
        } else {
            // Create a standard sprite for other formats (e.g., PNG), using the first detected extension.
            Sprites[url] = new PokeSprite(stream, extensions[0]);
        }
    
        // Cleanup: Close the stream (PokeSprite made a copy internally) and dispose the response.
        stream.Close();
        response.Dispose();

        // Return the newly cached sprite.
        return Sprites[url];
    }

    // Fetches the "Party" sprite (usually smaller icons).
    // Uses a simpler logic than FetchSprite (no Zip caching shown here, purely in-memory or direct download).
    internal static PokeSprite FetchPartySprite(Pokemon pokemon) {
        // Construct the URL for the party sprite fallback (assuming local host usually).
        var url = pokemon.FallbackPartySprite.Replace("$POKELINK_HOST", "http://localhost:3000");

        // Check In-Memory cache.
        if (Sprites.TryGetValue(url, out var texture)) {
            return texture;
        }
    
        // Download the sprite.
        var response = HttpClient.Send(new HttpRequestMessage(HttpMethod.Get, url));

        var stream = response.Content.ReadAsStream();

        if (stream == null) {
            throw new Exception("We shouldn't get here. But something very wrong will need of happened to get here!");
        }

        // Detect format.
        var extensions = SixLabors.ImageSharp.Image.DetectFormat(stream).FileExtensions.ToArray();

        // Instantiate and cache.
        if (extensions.Contains("gif")) {
            Sprites[url] = new PokeAnimatedSprite(stream);
        } else {
            Sprites[url] = new PokeSprite(stream, extensions[0]);
        }

        return Sprites[url];
    }

    // Garbage Collection mechanism for the sprite cache.
    // Intended to be called periodically (e.g., every frame or every few seconds).
    internal static void ClearUnused() {
        // Iterate over a copy of the keys to avoid modification exceptions.
        foreach (var key in Sprites.Keys.ToArray()) {
            // Decrement the usage counter (TTL).
            // NOTE: Something external must be resetting 'UseCycle' to a positive value 
            // for sprites that are currently being rendered, otherwise they will eventually disappear.
            Sprites[key].UseCycle--;
            
            // If the counter hits zero, the sprite is considered unused.
            if (Sprites[key].UseCycle == 0) {
                // Unload GPU texture.
                Sprites[key].UnloadTexture();
                // Unload CPU image data.
                Sprites[key].UnloadImage();
                // Dispose stream resources.
                Sprites[key].Dispose();

                // Remove from the dictionary.
                Sprites.Remove(key);
            }
        }
    }
}
