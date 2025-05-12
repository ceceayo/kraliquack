using System.Text.Json;
using Carter;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using UGS.Shared;
using UGS.Shared.DbModels;

namespace UGS.ApiService;

public class GamesModule : ICarterModule
{
    public record GetGamesResult(List<string> Games, bool AutomaticallyAllowNewGames);

    public record AddGameResult(GameEngine.VerifyGameResult? VerificiationResult, bool Success, string Message);

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/games", (
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
                return Results.Unauthorized();
            }
            else
            {
                
                
                return Results.Ok(new GetGamesResult(
                    db.GameSpecs.ToList().Select(x => x.Hash).ToList(), 
                    config.GetOrSetConfigKey("allowAutoAddGames", "false") == "true"
                    ));
            }
        }).WithName("GetGames");

        app.MapPost("/games", (
            [FromServices] IDatabase redis,
            [FromServices] SharedUniversalGameServerDataBaseContext db,
            [FromServices] UserAccountService uas,
            [FromServices] ILogger<GamesModule> logger,
            [FromServices] ConfigService config,
            [FromHeader] string token,
            [FromBody] GameEngine.GameSpec game) =>
        {
            (bool Success, int? UserId) = uas.VerifySession(db, redis, logger, token);
            if (!Success)
            {
                return new AddGameResult(null, false, "Please use another token");
            }
            else if (redis.HashGet(uas.RedisId(token), "state") == "sessionStartedNotInGame")
            {
                GameEngine.VerifyGameResult result = GameEngine.VerifyGame(game, logger);
                if (!result.Success)
                {
                    return new AddGameResult(result, false, "Imparsable game");
                }

                if (db.GameSpecs.Where(b => b.Hash == result.Hash).Count() == 1)
                {
                    if (config.GetOrSetConfigKey(
                            "allowGame_" + result.Hash!, config.GetOrSetConfigKey("allowAutoAddGames", "false")) ==
                        "true")
                    {
                        redis.HashSet(uas.RedisId(token),
                            [new HashEntry("state", "sessionStartedGameSet"), new HashEntry("game", result.Hash)]);
                        return new AddGameResult(result, true, "Welcome");
                    }
                    else
                    {
                        return new AddGameResult(result, false, "Waiting for server admin to allow game.");
                    }
                }
                else
                {

                    if (config.GetOrSetConfigKey(
                            "allowGame_" + result.Hash!, config.GetOrSetConfigKey("allowAutoAddGames", "false")) ==
                        "true")
                    {
                        GameSpec spec = new GameSpec { Hash = result.Hash!, Data = JsonSerializer.Serialize(game) };
                        db.GameSpecs.Add(spec);
                        db.SaveChanges();
                        redis.HashSet(uas.RedisId(token),
                            [new HashEntry("state", "sessionStartedGameSet"), new HashEntry("game", result.Hash)]);
                        return new AddGameResult(result, true, "Game was added! Welcome.");
                    }
                    else
                    {
                        return new AddGameResult(result, false, "Game not allowed by server admin.");
                    }
                }

            }

            return new AddGameResult(null, false, "QHAR?!");
        }).WithName("CreateGame");
    }
}