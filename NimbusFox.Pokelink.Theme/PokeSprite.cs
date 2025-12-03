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

/// <summary>
/// Represents a base sprite capable of loading image data from a stream, 
/// managing GPU textures via Raylib, and rendering to the screen.
/// Implements IDisposable to manage the underlying memory stream.
/// </summary>
public class PokeSprite : IDisposable {
    /// <summary>
    /// The raw binary data of the image file.
    /// </summary>
    // Stores the full file bytes (e.g., PNG or GIF data) in memory. 
    // This allows us to reload the image if the GPU context is lost.
    protected readonly MemoryStream Data = new();

    // A shared pool for byte arrays to minimize memory allocation overhead during image processing.
    internal static readonly System.Buffers.ArrayPool<byte> ArrayPool = System.Buffers.ArrayPool<byte>.Shared;

    /// <summary>
    /// Gets or sets the cycle counter, typically used by the sprite cache to determine 
    /// if a sprite is currently in use or eligible for garbage collection.
    /// </summary>
    public byte UseCycle { get; set; }

    /// <summary>
    /// The CPU-side Raylib image data. Null if not loaded.
    /// </summary>
    // Contains metadata and potentially pixel data in system RAM.
    public Image? Image { get; protected set; }

    // The GPU-side texture handle used for rendering. Null if not uploaded to VRAM.
    public Texture2D? Texture { get; protected set; }

    // Stores the file extension (e.g., "png") to hint Raylib on how to decode the stream.
    protected readonly string Extension;

    // Constructor: Copies the input stream content into the internal MemoryStream for safe keeping.
    public PokeSprite(Stream stream, string extension) {
        stream.CopyTo(Data);
        Extension = extension;
    }

    // Loads the GPU texture from the CPU image data.
    public virtual void LoadTexture() {
        // If the CPU image hasn't been loaded yet, load it now.
        if (Image == null) {
            LoadImage();
        }

        // Convert the CPU Image into a GPU Texture.
        Texture = Raylib.LoadTextureFromImage(Image!.Value);
    }

    // Frees the GPU texture memory.
    public void UnloadTexture() {
        // If there is no texture, do nothing.
        if (Texture == null) {
            return;
        }

        // Tell Raylib to release the OpenGL texture ID.
        Raylib.UnloadTexture(Texture!.Value);
        Texture = null;
    }

    // Frees the CPU image memory.
    public virtual void UnloadImage() {
        // If there is image data, release it via Raylib.
        if (Image != null) {
            Raylib.UnloadImage(Image.Value);
        }

        Image = null;
    }

    // Renders the sprite at the given (x, y) coordinates with a tint color.
    public void Render(int x, int y, Color tint) {
        // Ensure the texture is loaded before attempting to draw.
        if (Texture == null) {
            LoadTexture();
        }

        // Draw the full texture at the specified position.
        Raylib.DrawTexture(Texture!.Value, x, y, tint);
    }

    // Renders the sprite scaled to fit the target rectangle.
    public void Render(Rectangle target, Color tint) {
        // Ensure the texture is loaded.
        if (Texture == null) {
            LoadTexture();
        }

        // DrawTexturePro allows specifying a source rectangle (the whole image) 
        // and a destination rectangle (target on screen), enabling scaling.
        Raylib.DrawTexturePro(Texture!.Value, new Rectangle(0, 0, Image!.Value.Width, Image.Value.Height), target,
            Vector2.Zero, 0, tint);
    }

    // Loads the Raylib Image struct from the raw MemoryStream data.
    public virtual void LoadImage() {
        // Reset the stream position to the beginning to read the file header.
        Data.Seek(0, SeekOrigin.Begin);
        // Load the image from memory, using the extension to determine the format.
        // If Image is already set, this line is skipped (??=).
        Image ??= Raylib.LoadImageFromMemory($".{Extension}", Data.ToArray());
    }

    // Virtual update method for animation logic. 
    // The base sprite is static, so this implementation is empty.
    public virtual void Update(float speed) { }

    // Standard object equality check.
    public override bool Equals(object? obj) {
        if (obj is PokeSprite ps) {
            return Equals(ps);
        }

        return false;
    }

    // Protected equality check using GetHashCode.
    protected bool Equals(PokeSprite other) {
        return GetHashCode() == other.GetHashCode();
    }

    // Generates a hash code based on the Data stream.
    public override int GetHashCode() {
        return HashCode.Combine(Data);
    }

    /// <inheritdoc />
    // Clean up resources.
    public virtual void Dispose() {
        // Clear the internal buffer.
        Data.SetLength(0);
        // Dispose the stream.
        Data.Dispose();
        // Suppress finalization as we've manually cleaned up.
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Represents an animated sprite (GIF).
/// Uses ImageSharp to decode frames and manually updates the Raylib texture to simulate animation.
/// </summary>
public class PokeAnimatedSprite : PokeSprite {
    // Stores the loaded GIF image information (frames, metadata) from ImageSharp.
    private SixLabors.ImageSharp.Image? _info;
    // The index of the currently displayed frame.
    private int _currentFrame;
    // Accumulator for time elapsed since the last frame switch.
    private float _sinceLastFrame;
    // A buffer holding the raw pixel bytes (RGBA) for the current frame.
    private byte[]? _pixels;

    /// <summary>
    /// Initializes a new instance of the <see cref="PokeAnimatedSprite"/> class.
    /// </summary>
    /// <param name="stream">The stream containing GIF data.</param>
    public PokeAnimatedSprite(Stream stream) : base(stream, "gif") { }

    /// <inheritdoc />
    // Custom unload logic to handle ImageSharp resources and array pooling.
    public override void UnloadImage() {
        // Dispose the ImageSharp object.
        _info?.Dispose();

        // If we rented a pixel array, return it to the shared pool.
        if (_pixels != null) {
            ArrayPool.Return(_pixels, true);
        }

        _info = null;
        _pixels = null;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Decodes the GIF using ImageSharp, rents a pixel buffer from the shared pool, 
    /// and prepares the initial frame data.
    /// </remarks>
    public override void LoadImage() {
        // Reset stream to start.
        Data.Seek(0, SeekOrigin.Begin);
        // Decode GIF stream using ImageSharp's GifDecoder.
        _info = GifDecoder.Instance.Decode(new DecoderOptions() { Configuration = new Configuration() }, Data);

        // Rent a byte array large enough to hold all pixels (Width * Height * 4 bytes for RGBA).
        _pixels = ArrayPool.Rent(_info.Width * _info.Height * 4);

        // Access the first frame of the GIF.
        var currentFrame = (ImageFrame<Rgba32>)_info.Frames[0];

        // Loop through every pixel in the frame to populate our flat byte array.
        for (var y = 0; y < currentFrame.Height; y++) {
            for (var x = 0; x < currentFrame.Width; x++) {
                // Calculate the linear index in the byte array.
                var index = (y * currentFrame.Width + x) * 4;
                // Copy R, G, B, A channels manually.
                _pixels![index] = currentFrame.PixelBuffer[x, y].R;
                _pixels[index + 1] = currentFrame.PixelBuffer[x, y].G;
                _pixels[index + 2] = currentFrame.PixelBuffer[x, y].B;
                _pixels[index + 3] = currentFrame.PixelBuffer[x, y].A;
            }
        }

        // Create a Raylib Image structure manually. 
        // We only set dimensions and format here; the actual pixel data is managed via _pixels and UpdateTexture.
        var image = new Image {
            Format = PixelFormat.UncompressedR8G8B8A8, Mipmaps = 1, Height = _info!.Height, Width = _info.Width
        };

        Image = image;
    }

    /// <inheritdoc />
    public override void LoadTexture() {
        // Ensure pixels are decoded from the GIF.
        if (_pixels == null) {
            LoadImage();
        }

        // Create a base texture from the Image struct. 
        // (Note: Initial pixel content might be updated shortly after by UpdateTexture).
        var texture = Raylib.LoadTextureFromImage(Image!.Value);

        // Generate mipmaps for the texture to improve rendering at smaller scales.
        Raylib.GenTextureMipmaps(ref texture);

        Texture = texture;
    }

    /// <summary>
    /// Updates the animation frame based on elapsed time and the provided speed factor.
    /// </summary>
    /// <param name="speed">
    /// The speed factor (often representing health percentage). 
    /// Higher values result in faster animation (lower delay).
    /// </param>
    public override void Update(float speed) {
        // Ensure texture is ready.
        if (Texture == null) {
            LoadTexture();
        }

        // If speed is 0 or less, the animation is paused.
        if (speed <= 0) {
            return;
        }

        // Add the time elapsed since the last frame render (delta time).
        _sinceLastFrame += Raylib.GetFrameTime();

        // Retrieve the specific delay for the current frame from the GIF metadata.
        var delay = _info!.Frames[_currentFrame].Metadata.GetGifMetadata().FrameDelay;

        // Enforce a minimum speed clamp to prevent division by zero or extremely long delays.
        if (speed < 30) {
            speed = 30;
        }

        // Calculate if enough time has passed to advance the frame.
        // Logic: delay is divided by speed, so higher speed = smaller threshold = faster updates.
        if (delay / speed < _sinceLastFrame) {
            // Reset the timer.
            _sinceLastFrame = 0;
            // Move to the next frame index.
            _currentFrame++;

            // Loop the animation back to the first frame if we reach the end.
            if (_currentFrame >= _info.Frames.Count) {
                _currentFrame = 0;
            }

            // Get the ImageSharp frame for the new index.
            var currentFrame = (ImageFrame<Rgba32>)_info.Frames[_currentFrame];

            // Update the reusable pixel buffer with the new frame's data.
            for (var y = 0; y < currentFrame.Height; y++) {
                for (var x = 0; x < currentFrame.Width; x++) {
                    var index = (y * currentFrame.Width + x) * 4;
                    _pixels![index] = currentFrame.PixelBuffer[x, y].R;
                    _pixels[index + 1] = currentFrame.PixelBuffer[x, y].G;
                    _pixels[index + 2] = currentFrame.PixelBuffer[x, y].B;
                    _pixels[index + 3] = currentFrame.PixelBuffer[x, y].A;
                }
            }

            // Upload the new pixel data directly to the GPU texture.
            // This is what actually changes the visual on screen.
            Raylib.UpdateTexture(Texture!.Value, _pixels);
        }
    }
}
