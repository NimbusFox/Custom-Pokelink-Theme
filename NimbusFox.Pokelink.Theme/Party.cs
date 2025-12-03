using Pokelink.Core.Proto.V2;
using Raylib_cs;
using V2_Party = Pokelink.Core.Proto.V2.Party;

namespace NimbusFox.Pokelink.Theme;

/// <summary>
/// Represents a slot in the party UI, handling sprite loading, health updates, and rendering for a Pokémon.
/// </summary>
internal class Slot {
    /// <summary>
    /// The Pokémon assigned to this slot. Null if the slot is empty.
    /// </summary>
    internal Pokemon? Pokemon = null;
    /// <summary>
    /// Cached sprite for the Pokémon's appearance.
    /// </summary>
    private PokeSprite? _sprite = null;
    /// <summary>
    /// Cached party sprite (e.g., selection border) for the Pokémon.
    /// </summary>
    private PokeSprite? _partySprite = null;
    /// <summary>
    /// Cached sprite indicating the Pokémon's gender.
    /// </summary>
    private PokeSprite? _genderSprite = null;
    
    /// <summary>
    /// Updates the slot's internal state based on visibility and health.
    /// </summary>
    /// <param name="isVisible">Whether the slot should display health overlays.</param>
    public void Update(bool isVisible) {
        // If no Pokémon is assigned, clear sprites and exit early.
        if (Pokemon == null) {
            // If a sprite existed, clear both the main and party sprites.
            if (_sprite != null) {
                _sprite = null;
                _partySprite = null;
            }

            return;
        }

        // Lazily load the main sprite if it hasn't been fetched yet.
        _sprite ??= SpriteCache.FetchSprite(Pokemon);
        // Lazily load the party sprite (border) if it hasn't been fetched yet.
        _partySprite ??= SpriteCache.FetchPartySprite(Pokemon);

        // Determine the gender sprite based on the Pokémon's gender property.
        _genderSprite = Pokemon.Gender switch {
            Gender.Male => SpriteCache.MaleIcon,
            Gender.Female => SpriteCache.FemaleIcon,
            Gender.Less => SpriteCache.GenderlessIcon,
            _ => throw new ArgumentOutOfRangeException()
        };

        // Calculate the Pokémon's current health percentage.
        var healthPercentage = Math.GetPercentage(Pokemon.Hp.Current, Pokemon.Hp.Max);

        // If the slot should display health overlays, update the sprites accordingly.
        if (isVisible) {
            _sprite?.Update(healthPercentage);
            _genderSprite?.Update(healthPercentage);
        }
        
        // Always update the party sprite's health overlay.
        _partySprite?.Update(healthPercentage);
    }
    
    /// <summary>
    /// Updates the slot with a new Pokémon instance, resetting sprites when necessary.
    /// </summary>
    /// <param name="pokemon">The new Pokémon to display, or null to clear the slot.</param>
    public void Update(Pokemon? pokemon) {
        // If the incoming Pokémon is null but the slot currently holds one, reset sprites.
        if (pokemon == null && Pokemon != null) {
            Reset();
        }

        // If the slot is empty but a new Pokémon is provided, reset to clear stale sprites.
        if (pokemon != null && Pokemon == null) {
            Reset();
        }

        // If any of the key properties differ, reset to avoid visual glitches.
        if (Pokemon?.Species != pokemon?.Species || Pokemon?.Form != pokemon?.Form || Pokemon?.IsShiny != pokemon?.IsShiny ||
            Pokemon?.Gender != pokemon?.Gender) {
            Reset();
        }

        // Assign the new Pokémon to the slot.
        Pokemon = pokemon;
    }
    
    /// <summary>
    /// Clears cached sprite references for the slot.
    /// </summary>
    public void Reset() {
        _sprite = null;
        _partySprite = null;
    }

    /// <summary>
    /// Retrieves the gender sprite's texture, loading it if necessary.
    /// </summary>
    /// <returns>The texture of the gender sprite, or null if unavailable.</returns>
    public Texture2D? GetGenderSprite() {
        // Load the gender sprite texture lazily.
        if (_genderSprite?.Texture == null) {
            _genderSprite?.LoadTexture();
        }
        
        return _genderSprite?.Texture;
    }

    /// <summary>
    /// Retrieves the party sprite's texture, loading it if necessary.
    /// </summary>
    /// <returns>The texture of the party sprite, or null if unavailable.</returns>
    public Texture2D? GetPartySprite() {
        // Load the party sprite texture lazily.
        if (_partySprite?.Texture == null) {
            _partySprite?.LoadTexture();
        }

        return _partySprite?.Texture;
    }

    /// <summary>
    /// Retrieves the main sprite's texture, loading it if necessary.
    /// </summary>
    /// <returns>The texture of the main sprite, or null if unavailable.</returns>
    public Texture2D? GetSprite() {
        // Load the main sprite texture lazily.
        if (_sprite?.Texture == null) {
            _sprite?.LoadTexture();
        }

        return _sprite?.Texture;
    }
    
    /// <summary>
    /// Renders the main sprite at a given offset with a white tint.
    /// </summary>
    /// <param name="offset">The position offset to render at.</param>
    public void RenderSprite(Math.Vector2I offset) {
        // Ensure sprites use the correct animation cycle.
        _sprite?.UseCycle = 3;
        _partySprite?.UseCycle = 3;
        // Draw the main sprite.
        _sprite?.Render(offset.X, offset.Y, Color.White);
    }

    /// <summary>
    /// Renders the main sprite within a target rectangle using a specified tint.
    /// </summary>
    /// <param name="target">The rectangle area to render into.</param>
    /// <param name="tint">The color tint to apply.</param>
    public void RenderSprite(Rectangle target, Color tint) {
        _sprite?.UseCycle = 3;
        _partySprite?.UseCycle = 3;
        _sprite?.Render(target, tint);
    }

    /// <summary>
    /// Renders the main sprite at a specific coordinate with a tint.
    /// </summary>
    /// <param name="x">The X position.</param>
    /// <param name="y">The Y position.</param>
    /// <param name="tint">The color tint.</param>
    public void RenderSprite(int x, int y, Color tint) {
        _sprite?.UseCycle = 3;
        _partySprite?.UseCycle = 3;
        _sprite?.Render(x, y, tint);
    }

    /// <summary>
    /// Renders the party sprite at a given offset with a white tint.
    /// </summary>
    /// <param name="offset">The position offset to render at.</param>
    public void RenderPartySprite(Math.Vector2I offset) {
        _sprite?.UseCycle = 3;
        _partySprite?.UseCycle = 3;
        _partySprite?.Render(offset.X, offset.Y, Color.White);
    }

    /// <summary>
    /// Renders the party sprite within a target rectangle using a specified tint.
    /// </summary>
    /// <param name="target">The rectangle area to render into.</param>
    /// <param name="tint">The color tint to apply.</param>
    public void RenderPartySprite(Rectangle target, Color tint) {
        _sprite?.UseCycle = 3;
        _partySprite?.UseCycle = 3;
        _partySprite?.Render(target, tint);
    }

    /// <summary>
    /// Renders the party sprite at a specific coordinate with a tint.
    /// </summary>
    /// <param name="x">The X position.</param>
    /// <param name="y">The Y position.</param>
    /// <param name="tint">The color tint.</param>
    public void RenderPartySprite(int x, int y, Color tint) {
        _sprite?.UseCycle = 3;
        _partySprite?.UseCycle = 3;
        _partySprite?.Render(x, y, tint);
    }

    /// <summary>
    /// Renders the gender sprite at a given offset with a white tint.
    /// </summary>
    /// <param name="offset">The position offset to render at.</param>
    public void RenderGenderSprite(Math.Vector2I offset) {
        _genderSprite?.Render(offset.X, offset.Y, Color.White);
    }

    /// <summary>
    /// Renders the gender sprite within a target rectangle using a specified tint.
    /// </summary>
    /// <param name="target">The rectangle area to render into.</param>
    /// <param name="tint">The color tint to apply.</param>
    public void RenderGenderSprite(Rectangle target, Color tint) {
        _genderSprite?.Render(target, tint);
    }
    
    /// <summary>
    /// Renders the gender sprite at a specific coordinate with a tint.
    /// </summary>
    /// <param name="x">The X position.</param>
    /// <param name="y">The Y position.</param>
    /// <param name="tint">The color tint.</param>
    public void RenderGenderSprite(int x, int y, Color tint) {
        _genderSprite?.Render(x, y, tint);
    }
}

/// <summary>
/// Manages the global party state, providing thread-safe access and updates to the party slots.
/// </summary>
internal static class Party {
    /// <summary>
    /// Array of party slots representing the six Pokémon in the party.
    /// </summary>
    internal static readonly Slot[] Pokemon = [
        new(), new(), new(), new(),
        new(), new()
    ];

    /// <summary>
    /// Lock used to synchronize access to the party data.
    /// </summary>
    private static readonly Lock PartyLock = new();

    /// <summary>
    /// Updates all party slots with data from the received <see cref="V2_Party"/> message.
    /// </summary>
    /// <param name="party">The party data received from the server.</param>
    internal static void Update(V2_Party party) {
        // Acquire the lock to prevent concurrent modifications.
        PartyLock.Enter();
        // Iterate through each slot and apply the corresponding Pokémon data.
        for (var i = 0; i < Pokemon.Length; i++) {
            Pokemon[i].Update(party.Party_[i].Pokemon);
        }

        // Release the lock after updating.
        PartyLock.Exit();
    }
    
    /// <summary>
    /// Explicitly acquires the party lock for external code that needs to safely
    /// manipulate the party state.
    /// </summary>
    internal static void GetLock() {
        PartyLock.Enter();
    }

    /// <summary>
    /// Releases the party lock acquired by <see cref="GetLock"/>.
    /// </summary>
    internal static void ReleaseLock() {
        PartyLock.Exit();
    }
}
