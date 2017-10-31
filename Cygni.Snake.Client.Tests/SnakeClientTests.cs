﻿using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Cygni.Snake.Client.Tests
{
    public class StubGameObserver : IGameObserver
    {
        public void OnSnakeDied(string reason, string snakeId)
        {
            SnakeDiedCalls++;
        }

        public void OnGameStart()
        {
            GameStartCalls++;
        }

        public void OnGameEnd(Map map)
        {
            GameEndCalls++;
        }

        public void OnUpdate(Map map)
        {
            UpdateCalls++;
        }

        public void OnGameLink(string url)
        {
            GameLinkCalls++;
        }

        public int GameLinkCalls { get; private set; }

        public int GameStartCalls { get; private set; }
        public int GameEndCalls { get; private set; }
        public int UpdateCalls { get; private set; }
        public int SnakeDiedCalls { get; private set; }
    }

    public class SnakeClientTests
    {
        private readonly JObject _sampleMapJson = new JObject
        {
            {"width", 3},
            {"height", 3},
            {"worldTick", 1 },
            {"snakeInfos", new JArray(new JObject
            {
                {"id", "snake-id"},
                {"positions", new JArray(0, 1)},
                {"name", "snake"},
                {"points", 3}
            })},
            {"foodPositions", new JArray(2) },
            {"obstaclePositions", new JArray(3) }
        };

        [Fact]
        public void Start_ThrowsArgumentNullWhenBotIsNull()
        {
            var socket = new StubWebSocket(WebSocketState.Open);
            var client = new SnakeClient(socket);
            Assert.Throws<ArgumentNullException>(() => client.Start(null));
        }

        [Theory]
        [InlineData(WebSocketState.None)]
        [InlineData(WebSocketState.Aborted)]
        [InlineData(WebSocketState.CloseReceived)]
        [InlineData(WebSocketState.CloseSent)]
        [InlineData(WebSocketState.Closed)]
        [InlineData(WebSocketState.Connecting)]
        public void Start_ThrowsInvalidOperationWhenSocketIsNotOpen(WebSocketState state)
        {
            var socket = new StubWebSocket(state);
            var client = new SnakeClient(socket);
            Assert.Throws<InvalidOperationException>(() => client.Start(new StubSnakeBot()));
        }

        [Fact]
        public void Start_ThrowsInvalidOperationWhenServerSaysPlayerNameIsInvalid()
        {
            var socket = new StubWebSocket(WebSocketState.Open);
            socket.IncomingJson.Enqueue(new JObject { { "type", MessageType.InvalidPlayerName }, { "reason", "taken" } });
            var client = new SnakeClient(socket);

            Assert.Throws<InvalidOperationException>(() => client.Start(new StubSnakeBot()));
        }

        [Fact]
        public void Start_SendsRegisterPlayerRequest()
        {
            var socket = new StubWebSocket(WebSocketState.Open);
            socket.IncomingJson.Enqueue(new JObject { { "type", MessageType.PlayerRegistered } });
            var client = new SnakeClient(socket, new StubGameObserver());

            var bot = new StubSnakeBot();
            client.Start(bot);

            var registerMessage = socket.OutgoingJson[0];
            Assert.Equal("se.cygni.snake.api.request.RegisterPlayer", (string)registerMessage["type"]);
            Assert.Equal(bot.Name, (string)registerMessage["playerName"]);
        }

        [Fact]
        public void Start_NotifiesObserverWhenGameIsStarting()
        {
            var socket = new StubWebSocket(WebSocketState.Open);
            socket.IncomingJson.Enqueue(new JObject { { "type", MessageType.PlayerRegistered } });
            socket.IncomingJson.Enqueue(new JObject { { "type", MessageType.GameStarting } });

            var observer = new StubGameObserver();
            var client = new SnakeClient(socket, observer);
            client.Start(new StubSnakeBot());

            Assert.Equal(1, observer.GameStartCalls);
        }

        [Fact]
        public void Start_NotifiesObserverWhenMapHasUpdated()
        {
            var socket = new StubWebSocket(WebSocketState.Open);
            socket.IncomingJson.Enqueue(new JObject
            {
                { "type", MessageType.MapUpdated },
                { "map", _sampleMapJson }
            });


            var observer = new StubGameObserver();

            var client = new SnakeClient(socket, observer);
            client.Start(new StubSnakeBot());

            Assert.Equal(1, observer.UpdateCalls);
        }

        [Fact]
        public void Start_SendsStartGameRequestAfterServerHasConfirmedRegistrationWhenAutoStartEnabled()
        {
            var socket = new StubWebSocket(WebSocketState.Open);
            socket.IncomingJson.Enqueue(new JObject { { "type", MessageType.PlayerRegistered } });
            var client = new SnakeClient(socket, new StubGameObserver());

            client.Start(new StubSnakeBot() {AutoStart = true});

            var startGameMessage = socket.OutgoingJson[2];
            Assert.Equal("se.cygni.snake.api.request.StartGame", (string)startGameMessage["type"]);
        }

        [Fact]
        public void Start_DoesNotSendStartGameRequestAfterServerHasConfirmedRegistrationWhenAutoStartDisabled()
        {
            var socket = new StubWebSocket(WebSocketState.Open);
            socket.IncomingJson.Enqueue(new JObject { { "type", MessageType.PlayerRegistered } });
            var client = new SnakeClient(socket, new StubGameObserver());

            client.Start(new StubSnakeBot() {AutoStart = false});

            Assert.Equal(2, socket.OutgoingJson.Count);
        }

        [Fact]
        public void Start_NotifiesObserverWhenGameHasEnded()
        {
            var socket = new StubWebSocket(WebSocketState.Open);
            socket.IncomingJson.Enqueue(new JObject
            {
                { "type", MessageType.GameEnded },
                { "map", _sampleMapJson }
            });


            var observer = new StubGameObserver();

            var client = new SnakeClient(socket, observer);
            client.Start(new StubSnakeBot());

            Assert.Equal(observer.GameEndCalls, 1);
        }

        [Fact]
        public void Start_NotifiesObserverWhenSnakeHasDied()
        {
            var socket = new StubWebSocket(WebSocketState.Open);
            socket.IncomingJson.Enqueue(new JObject
            {
                { "type", MessageType.SnakeDead },
                { "playerId", "snake-id" },
                { "deathReason", "CollisionWithWall" },
            });

            var observer = new StubGameObserver();

            var client = new SnakeClient(socket, observer);
            client.Start(new StubSnakeBot());

            Assert.Equal(1, observer.SnakeDiedCalls);
        }

        [Fact]
        public void Start_RequestsMoveFromSnakeBotOnMapUpdate()
        {
            var socket = new StubWebSocket(WebSocketState.Open);
            socket.IncomingJson.Enqueue(new JObject { { "type", MessageType.PlayerRegistered } });
            socket.IncomingJson.Enqueue(new JObject { { "type", MessageType.GameStarting } });
            socket.IncomingJson.Enqueue(new JObject
            {
                { "type", MessageType.MapUpdated },
                { "map", _sampleMapJson }
            });

            bool receivedCall = false;
            var bot = new StubSnakeBot(map => { receivedCall = true; return Direction.Down; });

            var client = new SnakeClient(socket);
            client.Start(bot);

            Assert.True(receivedCall);
        }

        [Fact]
        public void Start_RequestsMoveFromBotEventIfNoGameStartingMessageHasBeenReceived()
        {
            // Not an actual requirement, but this test is here for completness.
            var socket = new StubWebSocket(WebSocketState.Open);
            socket.IncomingJson.Enqueue(JObject.Parse(TestResources.GetResourceText("map-update.json", Encoding.UTF8)));

            bool receivedCall = false;
            var bot = new StubSnakeBot(map => { receivedCall = true; return Direction.Down; });

            var client = new SnakeClient(socket);
            client.Start(bot);

            Assert.True(receivedCall);
        }

        [Theory]
        [InlineData(Direction.Down, "DOWN")]
        [InlineData(Direction.Up, "UP")]
        [InlineData(Direction.Left, "LEFT")]
        [InlineData(Direction.Right, "RIGHT")]
        public void Start_SendsRegisterMoveRequestFromBot(Direction direction, string expectedDirectionString)
        {
            var socket = new StubWebSocket(WebSocketState.Open);
            socket.IncomingJson.Enqueue(new JObject { { "type", MessageType.PlayerRegistered } });
            socket.IncomingJson.Enqueue(new JObject { { "type", MessageType.GameStarting } });
            socket.IncomingJson.Enqueue(new JObject
            {
                { "type", MessageType.MapUpdated },
                { "map", _sampleMapJson }
            });

            var client = new SnakeClient(socket, new StubGameObserver());
            client.Start(new StubSnakeBot(direction));

            var moveMessage = socket.OutgoingJson.Last();
            Assert.Equal("se.cygni.snake.api.request.RegisterMove", (string)moveMessage["type"]);
            Assert.Equal(expectedDirectionString, (string)moveMessage["direction"]);
            Assert.Equal(_sampleMapJson["worldTick"], (string)moveMessage["gameTick"]);
        }
    }
}
