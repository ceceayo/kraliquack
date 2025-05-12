using System.Diagnostics;
using Carter;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.CodeAnalysis.CSharp;
using NSwag.AspNetCore;
using StackExchange.Redis;
using UGS.Shared;
using static UGS.Shared.UserAccountService;

namespace UGS.ApiService;

public class SeshModule : ICarterModule
{
    public record SessionStarted(string SessionId);
    public record SessionStatus(string RedisKey, int UserId, string State);
    
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/session/start", (
                [FromServices] UserAccountService userAccountService, 
                [FromServices] SharedUniversalGameServerDataBaseContext dbContext,
                [FromServices] IDatabase redis,
                [FromServices] ILogger<UGS.ApiService.SeshModule> logger,
                string userName, 
                string password) =>
            {
                var (result, session) = userAccountService.StartSession(dbContext, redis, logger, userName, password);
                switch (result)
                {
                    case StartSessionResult.WrongPassword:
                        return Results.BadRequest("Wrong password");
                    case StartSessionResult.UserDoesNotExist:
                        return Results.BadRequest("User does not exist");
                    case StartSessionResult.SessionStarted:
                        return Results.Ok(new SessionStarted(session));
                    default:
                        return Results.InternalServerError();
                    case StartSessionResult.Error:
                        return Results.InternalServerError();
                }
                
            })
            .WithName("StartSession");

        app.MapGet("/session/status", (
            [FromServices] IDatabase redis,
            [FromServices] ILogger<SeshModule> logger,
            [FromServices] SharedUniversalGameServerDataBaseContext dbContext,
            [FromServices] UserAccountService userAccountService,
            [FromHeader] string token
            ) =>
        {
            (bool Success, int? UserId) = userAccountService.VerifySession(dbContext, redis, logger, token);
            if (!Success)
            {
                return Results.Unauthorized();
            }
            else
            {
                string? state = redis.HashGet(userAccountService.RedisId(token), "state");
                if (string.IsNullOrWhiteSpace(state))
                {
                    logger.LogCritical("User has empty (or whitespace) state");
                    return Results.InternalServerError("Invalid state for user.");
                }

                if (!UserAccountService.AllowedUserStates.Contains(state))
                {
                    logger.LogCritical("User has invalid state");
                    return Results.InternalServerError("Invalid state for user.");
                }
                
                return Results.Ok(new SessionStatus(userAccountService.RedisId(token), UserId.Value, state));
            }
        }).WithName("SessionStatus");
    }
    
}