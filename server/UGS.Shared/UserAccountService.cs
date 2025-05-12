using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using UGS.Shared.DbModels;

namespace UGS.Shared;

public class UserAccountService(string sessionContext)
{
    public static readonly List<string> AllowedUserStates = ["sessionStartedNotInGame", "sessionStartedGameSet", "inWorld"];

    public NewUserResult NewUser(SharedUniversalGameServerDataBaseContext dataBaseContext, ILogger logger, string username, string password, bool isAdmin = false)
    {
        logger.LogInformation("Creating new user {username}", username);
        switch (dataBaseContext.Users.Count(b => b.UserName == username))
        {
            case 0:
                User newUser = new User { UserName = username, IsAdmin = isAdmin, HashedPassword = "" };
                dataBaseContext.Users.Add(newUser);
                dataBaseContext.SaveChanges();
                HashUser hashUser = new HashUser(username, newUser.Id);
                PasswordHasher<HashUser> hasher = new PasswordHasher<HashUser>();
                string hashedPassword = hasher.HashPassword(hashUser, password);
                dataBaseContext.Users.Find(newUser.Id)!.HashedPassword = hashedPassword;
                dataBaseContext.SaveChanges();
                return NewUserResult.UserCreated;
            case 1:
                logger.LogWarning("Account already exists");
                return NewUserResult.UserAlreadyExists;
            default:
                logger.LogCritical($"Several accounts with name {username} exist!");
                return NewUserResult.Error;
        }
    }

    public (StartSessionResult, string?) StartSession(SharedUniversalGameServerDataBaseContext dataBaseContext, IDatabase redis, ILogger logger, string username, string password)
    {
        logger.LogInformation("Starting session for {username}", username);
        switch (dataBaseContext.Users.Count(b => b.UserName == username))
        {
            case 0:
                return (StartSessionResult.UserDoesNotExist, null);
            case 1:
                int id = dataBaseContext.Users.First(b => b.UserName == username).Id;
                HashUser hashUser = new HashUser(username, id);
                PasswordHasher<HashUser> hasher = new PasswordHasher<HashUser>();
                string hashedPassword = dataBaseContext.Users.Find(hashUser.Id)!.HashedPassword;
                switch (hasher.VerifyHashedPassword(hashUser, hashedPassword, password))
                {
                    case PasswordVerificationResult.Success:
                        string session = Guid.NewGuid().ToString();
                        redis.HashSet(RedisId(session), [new HashEntry("id", id), new HashEntry("state", "sessionStartedNotInGame")]);
                        redis.KeyExpire(RedisId(session), TimeSpan.FromHours(1));
                        return (StartSessionResult.SessionStarted, session);
                    case PasswordVerificationResult.Failed:
                        return (StartSessionResult.WrongPassword, null);
                    case PasswordVerificationResult.SuccessRehashNeeded:
                        throw new NotImplementedException(); // TODO: Implement rehashing
                    default:
                        throw new InvalidOperationException();
                }
            default:
                return (StartSessionResult.Error, null);
        }
    }

    public (bool success, int? userId) VerifySession(SharedUniversalGameServerDataBaseContext dataBaseContext, IDatabase redis, ILogger logger, string session)
    {
        //string? userId = redis.StringGet($"sessions:{sessionContext}:{session}");
        string? userId = redis.HashGet(RedisId(session), "id");
        if (String.IsNullOrEmpty(userId)) return (false, null);
        int userIdInt;
        if (!Int32.TryParse(userId, out userIdInt)) return (false, null);
        if (!dataBaseContext.Users.Any(b => b.Id == userIdInt)) return (false, null);
        return (true, userIdInt);
    }

    public string RedisId(string session)
    {
        return $"sessions:{sessionContext}:{session}";
    }
}

public enum NewUserResult
{
    UserCreated,
    UserAlreadyExists,
    Error
}

public enum StartSessionResult
{
    SessionStarted,
    UserDoesNotExist,
    WrongPassword,
    Error
}

record HashUser(string Username, int Id);