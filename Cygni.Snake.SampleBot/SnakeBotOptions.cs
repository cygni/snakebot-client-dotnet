using System;
using System.Collections.Generic;
using Cygni.Snake.Client;
using Microsoft.Extensions.CommandLineUtils;

namespace Cygni.Snake.SampleBot
{
    public class SnakeBotOptions
    {
        private const string TrainingMode = "training";
        private const string TournamentMode = "tournament";
        private readonly CommandOption userOption;
        private readonly CommandOption modeOption;
        private readonly CommandOption autoOption;
        private readonly CommandOption snakeOption;
        private readonly CommandOption printOption;
        private readonly CommandLineApplication app;
        private readonly SnakeBots bots;

        public SnakeBotOptions(CommandLineApplication app, SnakeBots bots)
        {
            this.app = app;
            this.bots = bots;
            app.HelpOption("-h|--help");
            userOption = app.Option(
                "-u|--user",
                "Specifies the user name for this bot. This will be the name shown in the SnakeBot arena.",
                CommandOptionType.SingleValue);

            modeOption = app.Option(
                "-m|--mode",
                "Specifies the play mode. Can be 'training' or 'tournament', default is 'training'.",
                CommandOptionType.SingleValue);

            autoOption = app.Option(
                "-a|--auto",
                "If provided, this option ensures that the game is started automatically when connected. Only applicable for training mode.",
                CommandOptionType.NoValue);

            snakeOption = app.Option(
                "-s|--snake",
                "Specifies the snake bot implementation as registered in Program.cs. The default value is 'default'.",
                CommandOptionType.SingleValue
            );

            printOption = app.Option(
                "-p|--print-game",
                "If provided, the game is rendered on the standard output",
                CommandOptionType.NoValue
            );
        }


        public bool ValidateOptions()
        {
            var mode = Mode;
            if (!(mode == TrainingMode || mode == TournamentMode))
            {
                app.Out.WriteLine($"Invalid mode '{mode}', valid values are '{TrainingMode}' and '{TournamentMode}'");
                return false;
            }

            var name = UserName;
            if (String.IsNullOrWhiteSpace(name))
            {
                app.Out.WriteLine($"Invalid name '{name}'");
                return false;
            }

            var snakeKey = SnakeBotKey;
            if (!bots.Contains(snakeKey))
            {
                Console.WriteLine($"Failed to instantiate snake bot '{snakeKey}', available snakes are:");
                foreach (var key in bots.Keys)
                {
                    app.Out.WriteLine(key);
                }
                return false;
            }
            return true;
        }

        private string Mode => modeOption.Value() ?? TrainingMode;

        private string UserName => userOption.Value() ?? "DotNetSnake";

        private string SnakeBotKey => snakeOption.Value() ?? "default";

        private bool AutoStart => autoOption.HasValue() && (Mode == TrainingMode);

        public SnakeBot CreateSnakeBot()
        {
            var snake = bots.Create(SnakeBotKey, UserName);
            snake.AutoStart = AutoStart;
            return snake;
        }

        public string GetServerUrl()
        {
            var url = $"ws://snake.cygni.se:80/{Mode}";
            return url;
        }

        public IGameObserver CreateObserver()
        {
            return new GamePrinter()
            {
                ShouldPrintMap = printOption.HasValue(),
                ShouldPrintScores = printOption.HasValue()
            };
        }
    }
}