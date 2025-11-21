using System.Numerics;
using Pokelink.Core.Proto.V2;
using Raylib_cs;
using V2_Party = Pokelink.Core.Proto.V2.Party;

namespace NimbusFox.Pokelink.Theme;

internal class Slot {
    internal Pokemon? Pokemon = null;
    private PokeSprite? _sprite = null;
    private PokeSprite? _partySprite = null;

    public void Update(bool isVisible) {
        if (Pokemon == null) {
            if (_sprite != null) {
                _sprite = null;
                _partySprite = null;
            }

            return;
        }

        _sprite ??= SpriteCache.FetchSprite(Pokemon);
        _partySprite ??= SpriteCache.FetchPartySprite(Pokemon);

        if (isVisible) {
            _sprite?.Update();
        }
        _partySprite?.Update();
    }

    public void Update(Pokemon? pokemon) {
        if (pokemon == null && Pokemon != null) {
            Reset();
            Pokemon = null;
            return;
        }

        if (pokemon != null && Pokemon == null) {
            Reset();
            Pokemon = pokemon;
            return;
        }

        if (pokemon == null || Pokemon == null) {
            return;
        }

        if (Pokemon.Species != pokemon.Species || Pokemon.Form != pokemon.Form || Pokemon.IsShiny != pokemon.IsShiny ||
            Pokemon.Gender != pokemon.Gender) {
            Reset();
        }

        Pokemon = pokemon;
    }

    public void Reset() {
        _sprite = null;
        _partySprite = null;
    }

    public Texture2D? GetPartySprite() {
        if (_partySprite?.Texture == null) {
            _partySprite?.LoadTexture();
        }

        return _partySprite?.Texture;
    }

    public Texture2D? GetSprite() {
        if (_sprite?.Texture == null) {
            _sprite?.LoadTexture();
        }

        return _sprite?.Texture;
    }

    public void RenderSprite(Math.Vector2I offset) {
        _sprite?.UseCycle = 3;
        _partySprite?.UseCycle = 3;
        _sprite?.Render(offset.X, offset.Y, Color.White);
    }

    public void RenderSprite(Rectangle target, Color tint) {
        _sprite?.UseCycle = 3;
        _partySprite?.UseCycle = 3;
        _sprite?.Render(target, tint);
    }

    public void RenderSprite(int x, int y, Color tint) {
        _sprite?.UseCycle = 3;
        _partySprite?.UseCycle = 3;
        _sprite?.Render(x, y, tint);
    }

    public void RenderPartySprite(Math.Vector2I offset) {
        _sprite?.UseCycle = 3;
        _partySprite?.UseCycle = 3;
        _partySprite?.Render(offset.X, offset.Y, Color.White);
    }

    public void RenderPartySprite(Rectangle target, Color tint) {
        _sprite?.UseCycle = 3;
        _partySprite?.UseCycle = 3;
        _partySprite?.Render(target, tint);
    }

    public void RenderPartySprite(int x, int y, Color tint) {
        _sprite?.UseCycle = 3;
        _partySprite?.UseCycle = 3;
        _partySprite?.Render(x, y, tint);
    }
}

internal static class Party {
    internal static readonly Slot[] Pokemon = [
        new(), new(), new(), new(),
        new(), new()
    ];

    private static readonly Lock PartyLock = new();

    internal static void Update(V2_Party party) {
        PartyLock.Enter();
        for (var i = 0; i < Pokemon.Length; i++) {
            Pokemon[i].Update(party.Party_[i].Pokemon);
        }

        PartyLock.Exit();
    }

    internal static void GetLock() {
        PartyLock.Enter();
    }

    internal static void ReleaseLock() {
        PartyLock.Exit();
    }
}
