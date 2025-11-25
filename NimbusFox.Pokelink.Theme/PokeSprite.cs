using System.Numerics;
using Raylib_cs;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using Color = Raylib_cs.Color;
using Image = Raylib_cs.Image;
using Rectangle = Raylib_cs.Rectangle;

namespace NimbusFox.Pokelink.Theme;

public class PokeSprite : IDisposable {
    protected readonly MemoryStream Data = new();

    internal static readonly System.Buffers.ArrayPool<byte> ArrayPool = System.Buffers.ArrayPool<byte>.Shared;

    public byte UseCycle { get; set; }

    public Image? Image { get; protected set; }

    public Texture2D? Texture { get; protected set; }

    protected readonly string Extension;

    public PokeSprite(Stream stream, string extension) {
        stream.CopyTo(Data);
        Extension = extension;
    }

    public virtual void LoadTexture() {
        if (Image == null) {
            LoadImage();
        }

        Texture = Raylib.LoadTextureFromImage(Image!.Value);
    }

    public void UnloadTexture() {
        if (Texture == null) {
            return;
        }

        Raylib.UnloadTexture(Texture!.Value);
        Texture = null;
    }

    public virtual void UnloadImage() {
        if (Image != null) {
            Raylib.UnloadImage(Image.Value);
        }

        Image = null;
    }

    public void Render(int x, int y, Color tint) {
        if (Texture == null) {
            LoadTexture();
        }

        Raylib.DrawTexture(Texture!.Value, x, y, tint);
    }

    public void Render(Rectangle target, Color tint) {
        if (Texture == null) {
            LoadTexture();
        }

        Raylib.DrawTexturePro(Texture!.Value, new Rectangle(0, 0, Image!.Value.Width, Image.Value.Height), target,
            Vector2.Zero, 0, tint);
    }

    public virtual void LoadImage() {
        Data.Seek(0, SeekOrigin.Begin);
        Image ??= Raylib.LoadImageFromMemory($".{Extension}", Data.ToArray());
    }

    public virtual void Update(float speed) { }

    public override bool Equals(object? obj) {
        if (obj is PokeSprite ps) {
            return Equals(ps);
        }

        return false;
    }

    protected bool Equals(PokeSprite other) {
        return GetHashCode() == other.GetHashCode();
    }

    public override int GetHashCode() {
        return HashCode.Combine(Data);
    }

    /// <inheritdoc />
    public virtual void Dispose() {
        Data.SetLength(0);
        Data.Dispose();
        GC.SuppressFinalize(this);
    }
}

public class PokeAnimatedSprite : PokeSprite {
    private SixLabors.ImageSharp.Image? _info;
    private int _currentFrame;
    private float _sinceLastFrame;
    private byte[]? _pixels;

    public PokeAnimatedSprite(Stream stream) : base(stream, "gif") { }

    public override void UnloadImage() {
        _info?.Dispose();

        if (_pixels != null) {
            ArrayPool.Return(_pixels, true);
        }
        
        _info = null;
        _pixels = null;
    }

    public override void LoadImage() {
        Data.Seek(0, SeekOrigin.Begin);
        _info = GifDecoder.Instance.Decode(new DecoderOptions() { Configuration = new Configuration() }, Data);

        _pixels = ArrayPool.Rent(_info.Width * _info.Height * 4);

        var currentFrame = (ImageFrame<Rgba32>)_info.Frames[0];

        for (var y = 0; y < currentFrame.Height; y++) {
            for (var x = 0; x < currentFrame.Width; x++) {
                var index = (y * currentFrame.Width + x) * 4;
                _pixels![index] = currentFrame.PixelBuffer[x, y].R;
                _pixels[index + 1] = currentFrame.PixelBuffer[x, y].G;
                _pixels[index + 2] = currentFrame.PixelBuffer[x, y].B;
                _pixels[index + 3] = currentFrame.PixelBuffer[x, y].A;
            }
        }

        var image = new Image {
            Format = PixelFormat.UncompressedR8G8B8A8, Mipmaps = 1, Height = _info!.Height, Width = _info.Width
        };

        Image = image;
    }

    /// <inheritdoc />
    public override void LoadTexture() {
        if (_pixels == null) {
            LoadImage();
        }

        var texture = Raylib.LoadTextureFromImage(Image!.Value);

        Raylib.GenTextureMipmaps(ref texture);

        Texture = texture;
    }

    public override void Update(float speed) {
        if (Texture == null) {
            LoadTexture();
        }

        if (speed <= 0) {
            return;
        }

        _sinceLastFrame += Raylib.GetFrameTime();

        var delay = _info!.Frames[_currentFrame].Metadata.GetGifMetadata().FrameDelay;

        if (speed < 30) {
            speed = 30;
        }

        if (delay / speed < _sinceLastFrame) {
            _sinceLastFrame = 0;
            _currentFrame++;

            if (_currentFrame >= _info.Frames.Count) {
                _currentFrame = 0;
            }

            var currentFrame = (ImageFrame<Rgba32>)_info.Frames[_currentFrame];

            for (var y = 0; y < currentFrame.Height; y++) {
                for (var x = 0; x < currentFrame.Width; x++) {
                    var index = (y * currentFrame.Width + x) * 4;
                    _pixels![index] = currentFrame.PixelBuffer[x, y].R;
                    _pixels[index + 1] = currentFrame.PixelBuffer[x, y].G;
                    _pixels[index + 2] = currentFrame.PixelBuffer[x, y].B;
                    _pixels[index + 3] = currentFrame.PixelBuffer[x, y].A;
                }
            }

            Raylib.UpdateTexture(Texture!.Value, _pixels);
        }
    }
}
