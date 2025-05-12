using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.Kestrel.Core.Features;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;
using UGS.Shared;
using UGS.Shared.DbModels;

namespace UGS.Worker
    
{
    
    public class GameServerWorker
    {
        private record JoinedUser(
            Dictionary<String, String> data
        )
        {
            public void Save(IDatabase redis, int worldId)
            {
                // TODO: implement
            }
        };

        //private readonly ILogger<GameServerWorker> _logger;
        private readonly int _worldId;
        private readonly IModel _model;
        private readonly SharedUniversalGameServerDataBaseContext _db;
        private readonly IDatabase _redis;
        private readonly String _worldInQueueName;

        public GameServerWorker(
            //ILogger<GameServerWorker> logger, 
            IModel model,
            SharedUniversalGameServerDataBaseContext db,
            IDatabase redis,
            int worldId
        )
        {
            //_logger = logger;
            _worldId = worldId;
            _model = model;
            _db = db;
            _redis = redis;
            _worldInQueueName = "worldInForId" + _worldId.ToString();
            Console.WriteLine("GameServerWorker created for world " + _worldId.ToString());
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _model.QueueDeclare(queue: _worldInQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            World world = _db.Worlds.Include(w => w.Game).First(w => w.Id == _worldId);
            GameEngine engine = new GameEngine(JsonSerializer.Deserialize<GameEngine.GameSpec>(world.Game.Data)!, _model);
            var consumer = new EventingBasicConsumer(_model);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                String value = Encoding.UTF8.GetString(body);
                GameEngine.GameInData data = JsonSerializer.Deserialize<GameEngine.GameInData>(value);
                //Console.WriteLine(data.ToString());
                switch (data.MessageType)
                {
                    case GameEngine.GameInDataType.UserJoined:
                        engine.UserJoined(data.Token, int.Parse(data.Message));
                        break;
                    case GameEngine.GameInDataType.UserLeft:
                        engine.UserLeft(data.Token);
                        break;
                    case GameEngine.GameInDataType.UserExecutesAction:
                        GameEngine.UserAction action = JsonSerializer.Deserialize<GameEngine.UserAction>(data.Message);
                        engine.ExecuteUserAction(action.ActionName, data.Token, action.Data);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            };

            var consumerTag = _model.BasicConsume(queue: _worldInQueueName, autoAck: true, consumer: consumer);

            while (!stoppingToken.IsCancellationRequested)
            {
                //Console.WriteLine("Now Ticking for world {0}", _worldId);
                bool shouldExit = engine.Tick();
                await Task.Delay(20, stoppingToken);
                if (shouldExit)
                {
                    break;
                }
            }
            _model.BasicCancel(consumerTag);
        }
    }
}
