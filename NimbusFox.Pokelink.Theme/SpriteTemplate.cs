using HandlebarsDotNet;
using Pokelink.Core.Proto.V2;
using V2_Party = Pokelink.Core.Proto.V2.Party;

namespace NimbusFox.Pokelink.Theme;

internal static class SpriteTemplate {
    private static HandlebarsTemplate<object, object>? _template;
    
    internal static void Register() {
        Handlebars.RegisterHelper("isDefined", (context, arguments) => 
            !string.IsNullOrWhiteSpace(arguments[0]?.ToString())
        );

        Handlebars.RegisterHelper("ifElse", (context, arguments) => arguments[0] is true ? arguments[1] : arguments[2]);

        Handlebars.RegisterHelper("concat", (context, arguments) => string.Join("", arguments));

        Handlebars.RegisterHelper("toLower", (context, arguments) => arguments[0]?.ToString()?.ToLower());

        Handlebars.RegisterHelper("noSpaces", (context, arguments) => {
            if (string.IsNullOrWhiteSpace(arguments[0]?.ToString())) {
                return arguments[0];
            }

            var value = arguments[0].ToString()!;

            while (value.Contains(' ')) {
                value = value.Replace(" ", "");
            }

            return value;
        });

        Handlebars.RegisterHelper("underscoreSpaces", (context, arguments) => {
            if (string.IsNullOrWhiteSpace(arguments[0]?.ToString())) {
                return arguments[0];
            }

            var value = arguments[0].ToString()!;

            while (value.Contains(' ')) {
                value = value.Replace(' ', '_');
            }

            return value;
        });

        Handlebars.RegisterHelper("remove",
            (context, arguments) => string.IsNullOrWhiteSpace(arguments[0]?.ToString())
                ? arguments[0]
                : arguments[0].ToString()!.Replace(arguments[1]?.ToString() ?? string.Empty, ""));

        Handlebars.RegisterHelper("nidoranGender", (context, arguments) => {
            var name = arguments[0]?.ToString()?.ToLower();
            if (name?.StartsWith("nidoran") == true) {
                var text = name[..7];

                if (name.EndsWith('♀') || name.ToLower().EndsWith("-f") &&
                    !string.IsNullOrWhiteSpace(arguments[2].ToString())) {
                    text += arguments[2].ToString();
                }

                if (name.EndsWith('♂') || name.ToLower().EndsWith("-m") &&
                    !string.IsNullOrWhiteSpace(arguments[1].ToString())) {
                    text += arguments[1].ToString();
                }

                return text;
            }

            return arguments[0];
        });

        Handlebars.RegisterHelper("addFemaleTag", (context, arguments) => {
            if (arguments[0] is not Pokemon pokemon) {
                return "";
            }

            return pokemon is { Gender: Gender.Female, HasFemaleSprite: true } ? arguments[1]?.ToString() : "";
        });
        
        Handlebars.RegisterHelper("isZero", (context, arguments) => arguments[0] is 0);
        
        RegisterTemplate("https://assets.pokelink.xyz/v2/sprites/pokemon/home/{{ifElse isShiny \"shiny\" \"normal\"}}/{{toLower (noSpaces (nidoranGender translations.english.speciesName \"\" \"-f\"))}}{{ifElse (isDefined translations.english.formName) (concat \"-\" (toLower (noSpaces translations.english.formName))) \"\"}}{{addFemaleTag this \"-f\"}}.png");
    }

    internal static void RegisterTemplate(string template) {
        _template = Handlebars.Compile(template);
        
        SpriteCache.Reset();

        for (var i = 0; i < Party.Pokemon.Length; i++) {
            Party.Pokemon[i].Reset();
        }
    }

    internal static string Handle(Pokemon pokemon) {
        return _template!(pokemon);
    }
}
