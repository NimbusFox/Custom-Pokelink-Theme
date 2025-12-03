using HandlebarsDotNet;
using Pokelink.Core.Proto.V2;
using V2_Party = Pokelink.Core.Proto.V2.Party;

namespace NimbusFox.Pokelink.Theme;

// Static utility class responsible for generating sprite URLs using Handlebars templates.
// It bridges the gap between raw Pokemon data and the file naming conventions used by sprite repositories.
internal static class SpriteTemplate {
    // Holds the compiled Handlebars function that transforms a Pokemon object into a URL string.
    private static HandlebarsTemplate<object, object>? _template;

    // Registers all custom Handlebars helpers required for string manipulation and logic.
    internal static void Register() {
        // Helper: 'isDefined'
        // Returns true if the first argument is not null or empty whitespace.
        Handlebars.RegisterHelper("isDefined", (context, arguments) => 
            !string.IsNullOrWhiteSpace(arguments[0]?.ToString())
        );

        // Helper: 'ifElse'
        // Acts like a ternary operator: if arg[0] is true, return arg[1], else return arg[2].
        Handlebars.RegisterHelper("ifElse", (context, arguments) => arguments[0] is true ? arguments[1] : arguments[2]);

        // Helper: 'concat'
        // Joins all arguments into a single string.
        Handlebars.RegisterHelper("concat", (context, arguments) => string.Join("", arguments));

        // Helper: 'toLower'
        // Converts the argument to lowercase.
        Handlebars.RegisterHelper("toLower", (context, arguments) => arguments[0]?.ToString()?.ToLower());

        // Helper: 'noSpaces'
        // Removes all whitespace from the string.
        Handlebars.RegisterHelper("noSpaces", (context, arguments) => {
            if (string.IsNullOrWhiteSpace(arguments[0]?.ToString())) {
                return arguments[0];
            }

            var value = arguments[0].ToString()!;

            // Iteratively remove spaces.
            while (value.Contains(' ')) {
                value = value.Replace(" ", "");
            }

            return value;
        });

        // Helper: 'underscoreSpaces'
        // Replaces all spaces with underscores (often used in file names).
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

        // Helper: 'remove'
        // Removes occurrences of string arg[1] from string arg[0].
        Handlebars.RegisterHelper("remove",
            (context, arguments) => string.IsNullOrWhiteSpace(arguments[0]?.ToString())
                ? arguments[0]
                : arguments[0].ToString()!.Replace(arguments[1]?.ToString() ?? string.Empty, ""));

        // Helper: 'nidoranGender'
        // Special case handling for "Nidoran♀" and "Nidoran♂".
        // File systems often don't like symbols, so this converts them to text suffixes based on arguments.
        Handlebars.RegisterHelper("nidoranGender", (context, arguments) => {
            var name = arguments[0]?.ToString()?.ToLower();
            
            // Check if the pokemon is actually a Nidoran.
            if (name?.StartsWith("nidoran") == true) {
                // Take the base "nidoran" string (first 7 chars).
                var text = name[..7];

                // If it ends in female symbol or -f, append the female suffix arg (arg[2]).
                if (name.EndsWith('♀') || name.ToLower().EndsWith("-f") &&
                    !string.IsNullOrWhiteSpace(arguments[2].ToString())) {
                    text += arguments[2].ToString();
                }

                // If it ends in male symbol or -m, append the male suffix arg (arg[1]).
                if (name.EndsWith('♂') || name.ToLower().EndsWith("-m") &&
                    !string.IsNullOrWhiteSpace(arguments[1].ToString())) {
                    text += arguments[1].ToString();
                }

                return text;
            }

            // If not nidoran, return original name.
            return arguments[0];
        });

        // Helper: 'addFemaleTag'
        // Appends a specific tag (arg[1]) if the pokemon is female AND the species actually has a distinct female sprite.
        Handlebars.RegisterHelper("addFemaleTag", (context, arguments) => {
            if (arguments[0] is not Pokemon pokemon) {
                return "";
            }

            return pokemon is { Gender: Gender.Female, HasFemaleSprite: true } ? arguments[1]?.ToString() : "";
        });
    
        // Helper: 'isZero'
        // Simple equality check for 0.
        Handlebars.RegisterHelper("isZero", (context, arguments) => arguments[0] is 0);
    
        // Register the default template URL structure.
        // Logic:
        // 1. Base URL: https://assets.pokelink.xyz/v2/sprites/pokemon/home/
        // 2. Shiny Check: Adds "shiny" or "normal" folder.
        // 3. Filename: 
        //    - Normalizes Nidoran names.
        //    - Removes spaces.
        //    - Lowercases everything.
        //    - Appends form name (if defined) prefixed with "-".
        //    - Appends "-f" if it's a female variant with a unique sprite.
        //    - Ends with .png.
        RegisterTemplate("https://assets.pokelink.xyz/v2/sprites/pokemon/home/{{ifElse isShiny \"shiny\" \"normal\"}}/{{toLower (noSpaces (nidoranGender translations.english.speciesName \"\" \"-f\"))}}{{ifElse (isDefined translations.english.formName) (concat \"-\" (toLower (noSpaces translations.english.formName))) \"\"}}{{addFemaleTag this \"-f\"}}.png");
    }

    // Compiles the provided string template and resets caches.
    // This allows the theme to change the sprite source dynamically at runtime.
    internal static void RegisterTemplate(string template) {
        _template = Handlebars.Compile(template);
    
        // Clear the sprite cache because the URLs for every Pokemon have effectively changed.
        SpriteCache.Reset();

        // Reset cached data on the Party objects to force them to re-fetch sprites on next update.
        for (var i = 0; i < Party.Pokemon.Length; i++) {
            Party.Pokemon[i].Reset();
        }
    }

    // Executes the compiled template against a Pokemon instance to generate the final URL string.
    internal static string Handle(Pokemon pokemon) {
        return _template!(pokemon);
    }
}
