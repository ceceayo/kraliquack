using System.Text;
using System.Threading.Channels;
using Carter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using StackExchange.Redis;
using UGS.Shared;
using UGS.Shared.DbModels;

namespace UGS.ApiService;

public class WorldModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/worlds", (
            [FromServices] IDatabase redis,
            [FromServices] SharedUniversalGameServerDataBaseContext db,
            [FromServices] UserAccountService uas,
            [FromServices] ILogger<GamesModule> logger,
            [FromServices] ConfigService config,
            [FromHeader] string token) =>
        {
            (bool Success, int? UserId) = uas.VerifySession(db, redis, logger, token);
            if (!Success)
            {
                return new GetWorldsResult(false, "Bogus user session.", null);
            }

            if (redis.HashGet(uas.RedisId(token), "state") != "sessionStartedGameSet")
            {
                return new GetWorldsResult(false, "User in wrong state", null);
            }

            List<GetWorldsResultWord> worlds = new List<GetWorldsResultWord>();
            String gameHash = redis.HashGet(uas.RedisId(token), "game");
            foreach (World w in db.Worlds.Include(b => b.Game).Where(b => b.Game.Hash == gameHash))
            {
                worlds.Add(new GetWorldsResultWord(w.Name, w.Id));
            }

            return new GetWorldsResult(true, "Listing of worlds", worlds);

        }).WithName("GetWorlds");

        app.MapPost("/worlds", ([FromServices] IDatabase redis,
            [FromServices] SharedUniversalGameServerDataBaseContext db,
            [FromServices] UserAccountService uas,
            [FromServices] ILogger<GamesModule> logger,
            [FromServices] ConfigService config,
            [FromServices] RabbitMQ.Client.IModel msgqueue,
            [FromHeader]
            string token,
            [FromBody] StartWorldRequest worldRequest) =>
        {

            (bool Success, int? UserId) = uas.VerifySession(db, redis, logger, token);
            if (!Success)
            {
                return new StartWorldResult(false, "Bogus user session.");
            }

            if (redis.HashGet(uas.RedisId(token), "state") != "sessionStartedGameSet")
            {
                return new StartWorldResult(false, "User in wrong state");
            }

            String gameHash = redis.HashGet(uas.RedisId(token), "game");

            logger.LogInformation("Set game to {gameHash}", gameHash);

            UGS.Shared.DbModels.World w = new World { Game = db.GameSpecs.First(b => b.Hash == gameHash), Name = worldRequest.WorldName };
            db.Worlds.Add(w);
            db.SaveChanges();
            msgqueue.QueueDeclare(queue: "worldsToStart",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var body = Encoding.UTF8.GetBytes(w.Id.ToString());

            msgqueue.BasicPublish(exchange: string.Empty,
                routingKey: "worldsToStart",
                basicProperties: null,
                body: body);

            return new StartWorldResult(true, w.Id.ToString());
        }).WithName("StartGame");
    }



    public record GetWorldsResult(bool Success, string Message, List<GetWorldsResultWord>? Worlds);
    public record GetWorldsResultWord(string title, int id);

    public record StartWorldRequest(string WorldName);
    public record StartWorldResult(bool Success, string Message);

}