// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Logging;
using MyOtherOtherGame;

Console.WriteLine("Hello, Game!");

var app = new MyGame(
    new LoggerFactory().CreateLogger<MyGame>(),
    "My Brand New Game"
);
app.Run();

Console.WriteLine("Bye, Game!");