# Cygni C# Snake Bot Client

[![Build Status](http://jenkins.snake.cygni.se/buildStatus/icon?job=snakebot-client-dotnet)](http://jenkins.snake.cygni.se/job/snakebot-client-dotnet/)

## Baby, I'm just gonna snake, snake, snake, snake, snake. I snake it off, I snake it off

I would like to begin this section with some wise words from a great and wise man.

> I didn’t want to be just a *snake* building champion; I wanted to be the best *snake* builder of all times. <br /> **Arnold Schwarzenegger**

Within this repository you can find the beginning of the same journey that Mr. Schwarzenegger walked so many years ago. 
A journey that took him, an insignificant young boy from Steiermark in Austria, all the way to the podium at not one but **seven** Mr. Snake Olympia events.

Here you can find a Snake Client written in the C# language for the .Net Core platform.

I would also like to leave you with a qoute from the aforementioned great man

> Anything I’ve ever attempted, I was always willing to fail. So you can’t always win, but don’t be afraid of making decisions. You can’t be paralyzed by the fear of failure or you will never push yourself. You keep pushing because you believe in yourself and in your vision and you know that it is the right thing to do, and success will come. So don’t be afraid to fail. <br /> **Arnold Schwarzenegger**

## System Requirements
- Operating system supported by .NET Core (https://www.microsoft.com/net/core), or docker.
- Your favourite text-editor. Although we would recommend using either 'Visual Studio Code' or 'Visual Studio'.

## Project structure
The solution contains three projects
- Cygni.Snake.Client
- Cygni.Snake.Client.Tests
- Cygni.Snake.SampleBot

#### Cygni.Snake.Client
This project contains among other things, the SnakeClient, SnakeBot and Map classes.

- SnakeClient: Provides the communication with the Cygni Snake server.
- SnakeBot: Provides an abstract base class for snake bots.
- Map: Provides a way to examine the state of the snake world.
- IGameObserver: Interface for types that can observe games.

#### Cygni.Snake.Client.Tests
Contains unit tests for the Cygni.Snake.Client library.

#### Cygni.Snake.SampleBot
This project provides a command line application that connects to the Cygni Snake Server using the SnakeClient and a SnakeBot implementation of your choice.

- Program: The main entry point. Connects to the server and requests a new game.
- MySnakeBot: The sample SnakeBot implementation.
- GamePrinter: An implementation of IGameObserver for printing snake updates to console.

## Get started
Get source latest source from http://github.com/cygni/snakebot-client-dotnet.

### Building and running using Visual Studio
Prerequisites:
- Visual Studio with .NET Core tooling

### Building and running using Docker
Make sure your current directory is the repository root, then build a new image from the Dockerfile in the root directory. This will compile and run the unit tests.

    docker build -t snake .

Start container:
    
    docker run -i --rm snake

Print usage options using:

    docker run -i --rm snake -- --help

### Building and running using .NET CLI
Make sure your current directory is the repository root, i.e. the same directory as the `Cygni.Snake.sln` file, then restore all dependencies:

```
dotnet restore
```
    
Run unit tests (optional):

```
$ dotnet test Cygni.Snake.Client.Tests/
```
    
Change directory into the sample bot CLI client and run:

```bash
$ cd Cygni.Snake.SampleBot
$ dotnet run
```

Show usage options using (note the '--' before '--help' to avoid printing dotnet CLI help information):

```bash
$ dotnet run -- --help
```

### Implementing a SnakeBot

The file Cygni.Snake.SampleBot/MySnakeBot.cs contains a skeleton SnakeBot implementation. All you need to do is to implement the GetNextMove()-method to return the direction of choice for your next move. The parameter map represents the current state of the world. It exposes a property called MySnake which represents your snake. Other than that, use the intellisense to examine its API.

```csharp
public class MySnakeBot : SnakeBot
{
    public MySnakeBot(string name) : base(name)
    {
    }
    
    public override Direction GetNextMove(Map map)
    {
        // figure out a good move
        
        // do calculated move
        return Direction.Down;
    }
}
```

### Switch between SnakeBots

If you prefer, you can create several SnakeBot types and easily switch between them using the CLI-options and the SnakeBots.Register method.

For example, if there is another SnakeBot implementation called `CustomSnakeBot`, it can be registered as follows:

```csharp
public static void Main(string[] args)
{
    var bots = new SnakeBots();
    bots.Register("default", name => new MySnakeBot(name));
    bots.Register("custom", name => new CustomSnakeBot(name));

    ...
}
```

It can then be selected when running the application through the `--snake` option:

```
dotnet run -- --snake custom
```

### Execution

The Execute method in Program.cs wires up the WebSocket connection with the SnakeClient and the SnakeBot of your choice. You can choose to omit the observer parameter in SnakeClient. Or, if you prefer, you can provide another implementation to log or do whatever cool stuff you like.

```csharp
public class Program
{
    ...

    private static int Execute(SnakeBotOptions options)
    {
        if (!options.ValidateOptions())
        {
            return 1;
        }

        var snake = options.CreateSnakeBot();
        var url = options.GetServerUrl();
        var observer = options.CreateObserver();

        Console.WriteLine($"Connecting to {url}");

        var client = SnakeClient.Connect(new Uri(url), observer));
        client.Start(snake);
        Console.ReadLine();
        return 0;
    }
}
```
