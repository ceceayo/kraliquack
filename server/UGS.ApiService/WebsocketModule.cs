using Carter;
using static UGS.ApiService.WorldModule;
using UGS.Shared;
using StackExchange.Redis;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using RabbitMQ.Client;
using UGS.Shared.DbModels;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client.Events;

namespace UGS.ApiService
{
    public class WebsocketModule : ICarterModule
    {
        public record WebsocketComResponse(bool Success, string? Message);

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.Map("/{world}.w/ws", async context =>
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var redis = context.RequestServices.GetRequiredService<IDatabase>();
                    var db = context.RequestServices.GetRequiredService<SharedUniversalGameServerDataBaseContext>();
                    var uas = context.RequestServices.GetRequiredService<UserAccountService>();
                    var logger = context.RequestServices.GetRequiredService<ILogger<GamesModule>>();
                    var config = context.RequestServices.GetRequiredService<ConfigService>();
                    var msgqueue = context.RequestServices.GetRequiredService<RabbitMQ.Client.IModel>();
                    var token = context.Request.Headers["token"].ToString();
                    Console.WriteLine(redis.HashGetAll(uas.RedisId(token)));
                    if (!int.TryParse((string)context.Request.RouteValues["world"]!, out int world))
                    {
                        context.Response.StatusCode = 400;
                        return;
                    }

                    (bool Success, int? UserId) = uas.VerifySession(db, redis, logger, token);
                    if (!Success)
                    {
                        context.Response.StatusCode = 400;
                    }
                    else if (redis.HashGet(uas.RedisId(token), "state") != "sessionStartedGameSet")
                    {
                        context.Response.StatusCode = 400;
                    }
                    else if (db.Worlds.Count(b => b.Id == world) != 1)
                    {
                        context.Response.StatusCode = 400;
                    }
                    else if (redis.HashGet(uas.RedisId(token), "game") !=
                             db.Worlds.Include(b => b.Game).First(b => b.Id == world)!.Game.Hash)
                    {
                        context.Response.StatusCode = 400;
                    }
                    else
                    {
                        int uid = (int)redis.HashGet(uas.RedisId(token), "id");
                        User u = db.Users.First(b => b.Id == uid);
                        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        await RunWebsocket(webSocket, msgqueue, world, token, u);
                    }
                }
                else
                {
                    context.Response.StatusCode = 400;
                }
            });
        }

        private async Task RunWebsocket(WebSocket webSocket, RabbitMQ.Client.IModel msgqueue, int world, string uToken, User u)
        {
            msgqueue.QueueDeclare(queue: "worldInForId" + world.ToString(),
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            msgqueue.QueueDeclare(queue: "worldOutForUser" + uToken,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var buffer = new byte[1024 * 128];
            var consumer = new EventingBasicConsumer(msgqueue);
            var queueName = "worldOutForUser" + uToken;

            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var bodyMsg = Encoding.UTF8.GetString(body);

                if (webSocket.State == WebSocketState.Open)
                    await webSocket.SendAsync(Encoding.UTF8.GetBytes(bodyMsg), WebSocketMessageType.Text, true, CancellationToken.None);
            };

            msgqueue.BasicPublish(string.Empty, "worldInForId" + world.ToString(), null,
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new GameEngine.GameInData(uToken, u.Id.ToString(),
                    GameEngine.GameInDataType.UserJoined))));

            msgqueue.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var receiveTask = webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    Task<WebSocketReceiveResult>? completedTask;
                    try
                    {
                        completedTask = await Task.WhenAny(receiveTask);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        break;
                    }

                    if (completedTask == null)
                    {
                        Console.WriteLine("Completed task is null");
                    }
                    else if (completedTask == receiveTask)
                    {
                        if (receiveTask.Status == TaskStatus.RanToCompletion)
                        {
                            var result = receiveTask.Result;
                            if (result.CloseStatus.HasValue)
                            {
                                break;
                            }

                            var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                            var messageObject = new GameEngine.GameInData(uToken, receivedMessage,
                                GameEngine.GameInDataType.UserExecutesAction);
                            var messageJson = JsonSerializer.Serialize(messageObject);
                            var body = Encoding.UTF8.GetBytes(messageJson);

                            msgqueue.BasicPublish(string.Empty, "worldInForId" + world.ToString(), null, body);
                        }
                        else
                        {
                            Console.WriteLine("HUH?!");
                        }
                    }
                }
            }
            finally
            {
                // Ensure the UserLeft event is sent when the WebSocket disconnects
                msgqueue.BasicPublish(string.Empty, "worldInForId" + world.ToString(), null,
                    Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new GameEngine.GameInData(uToken,
                        u.Id.ToString(), GameEngine.GameInDataType.UserLeft))));

                if (webSocket.State == WebSocketState.Open)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "User disconnected", CancellationToken.None);
                }
            }
        }

    }
}