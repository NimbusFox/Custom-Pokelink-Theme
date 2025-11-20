// See https://aka.ms/new-console-template for more information

using NimbusFox.Pokelink.Theme;

SpriteTemplate.Register();

var client = new Client("127.0.0.1", 3000);

Console.WriteLine("Connecting to Pokelink");

client.ConnectAsync();

Console.ReadKey();