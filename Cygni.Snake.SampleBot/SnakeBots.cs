using System;
using System.Collections.Generic;
using Cygni.Snake.Client;
using Cygni.Snake.SampleBot;

public class SnakeBots
{
    private Dictionary<string, Func<string, SnakeBot>> snakes = new Dictionary<string, Func<string, SnakeBot>>();

    public IEnumerable<string> Keys => snakes.Keys;

    public void Register(string key, Func<string, SnakeBot> botCreator)
    {
        snakes.Add(key, botCreator);
    }

    public SnakeBot Create(string key, string name)
    {
        return snakes[key](name);
    }

    public bool Contains(string snakeKey)
    {
        return snakes.ContainsKey(snakeKey);
    }
}
