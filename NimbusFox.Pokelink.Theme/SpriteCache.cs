using System.IO.Compression;
using System.Net;
using System.Reflection;
using Pokelink.Core.Proto.V2;
using Raylib_cs;

namespace NimbusFox.Pokelink.Theme;

internal static class SpriteCache {
    private static readonly Dictionary<string, PokeSprite> Sprites = new();

    private static readonly ZipArchive ImageCache;

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

    internal static void Dispose() {
        Reset();
        ImageCache.Dispose();
        ICF.Dispose();
    }

    internal static void Reset() {
        foreach (var (_, sprite) in Sprites) {
            sprite.UnloadTexture();
            sprite.UnloadImage();
            sprite.Dispose();
        }

        Sprites.Clear();
        
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    internal static PokeSprite FetchSprite(Pokemon pokemon) {
        var url = SpriteTemplate.Handle(pokemon);

        var response = HttpClient.Send(new HttpRequestMessage(HttpMethod.Get, url),
            HttpCompletionOption.ResponseHeadersRead);

        if (Sprites.TryGetValue(url, out var sprite)) {
            return sprite;
        }

        var path = url.Replace("https://", "").Replace("http://", "");
        Stream? stream = null;

        if (!response.IsSuccessStatusCode) {
            url = pokemon.FallbackSprite.Replace("$POKELINK_HOST", "http://localhost:3000");
            response = HttpClient.Send(new HttpRequestMessage(HttpMethod.Get, url));

            stream = response.Content.ReadAsStream();

            path = null;
        }

        if (path != null) {
            var entry = ImageCache.Entries.FirstOrDefault(x => x.FullName == path);

            if (entry == null) {
                entry = ImageCache.CreateEntry(path, CompressionLevel.SmallestSize);
                stream = entry.Open();

                response.Content.ReadAsStream().CopyTo(stream);

                stream.Seek(0, SeekOrigin.Begin);
                
                stream.Flush();
                
                ICF.Flush();
            } else {
                stream = entry.Open();

                if (stream.Length != response.Content.Headers.ContentLength) {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.SetLength(0);

                    response.Content.ReadAsStream().CopyTo(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                }
            }
        }

        if (stream == null) {
            throw new Exception("We shouldn't get here. But something very wrong will need of happened to get here!");
        }

        var extensions = SixLabors.ImageSharp.Image.DetectFormat(stream).FileExtensions.ToArray();

        if (extensions.Contains("gif")) {
            Sprites[url] = new PokeAnimatedSprite(stream);
        } else {
            Sprites[url] = new PokeSprite(stream, extensions[0]);
        }
        
        stream.Close();
        response.Dispose();

        return Sprites[url];
    }

    internal static PokeSprite FetchPartySprite(Pokemon pokemon) {
        var url = pokemon.FallbackPartySprite.Replace("$POKELINK_HOST", "http://localhost:3000");

        if (Sprites.TryGetValue(url, out var texture)) {
            return texture;
        }
        
        var response = HttpClient.Send(new HttpRequestMessage(HttpMethod.Get, url));

        var stream = response.Content.ReadAsStream();

        if (stream == null) {
            throw new Exception("We shouldn't get here. But something very wrong will need of happened to get here!");
        }

        var extensions = SixLabors.ImageSharp.Image.DetectFormat(stream).FileExtensions.ToArray();

        if (extensions.Contains("gif")) {
            Sprites[url] = new PokeAnimatedSprite(stream);
        } else {
            Sprites[url] = new PokeSprite(stream, extensions[0]);
        }

        return Sprites[url];
    }

    internal static void ClearUnused() {
        foreach (var key in Sprites.Keys.ToArray()) {
            Sprites[key].UseCycle--;
            if (Sprites[key].UseCycle == 0) {
                Sprites[key].UnloadTexture();
                Sprites[key].UnloadImage();
                Sprites[key].Dispose();

                Sprites.Remove(key);
            }
        }
    }
}
