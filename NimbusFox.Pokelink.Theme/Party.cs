using Pokelink.Core.Proto.V2;
using V2_Party = Pokelink.Core.Proto.V2.Party;

namespace NimbusFox.Pokelink.Theme;

internal static class Party {
    internal static readonly Pokemon?[] Pokemon = new Pokemon?[6];

    internal static void Update(V2_Party party) {
        for (var i = 0; i < Pokemon.Length; i++) {
            Pokemon[i] = party.Party_[i].Pokemon;

            if (Pokemon[i] != null) {
                Console.WriteLine(SpriteTemplate.Handle(Pokemon[i]!));
            }
        }
    }
}
