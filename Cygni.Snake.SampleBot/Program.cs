using System;
using Cygni.Snake.Client;
using Microsoft.Extensions.CommandLineUtils;

namespace Cygni.Snake.SampleBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var bots = new SnakeBots();
            bots.Register("default", name => new MySnakeBot(name));
            // register additional bot types here if you need to switch easily
            // bots.Register("custom", name => new CustomSnakeBot(name));

            var app = new CommandLineApplication();
            var options = new SnakeBotOptions(app, bots);

            app.OnExecute(() => Execute(options));
            app.Execute(args);
        }

        private static int Execute(SnakeBotOptions options)
        {
            if (!options.ValidateOptions())
            {
                return 1;
            }

            var snake = options.CreateSnakeBot();
            var url = options.GetServerUrl();

            Console.WriteLine($"Connecting to {url}");

            var client = SnakeClient.Connect(new Uri(url), new GamePrinter());
            client.Start(snake);
            Console.ReadLine();
            return 0;
        }
    }
}