using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Unicode;
using Microsoft.Extensions.Logging;
using NLua;
using NLua.Exceptions;
using RabbitMQ.Client;

namespace UGS.Shared;

public class GameEngine
{
    private readonly GameEngine.GameSpec my_spec;
    private readonly Dictionary<string, Action> actions;
    private readonly IModel model;
    
    private Dictionary<string, GameUser> users;
    private Dictionary<string, string> gameData;
    private Lua lua;
    
    private DateTime lastTickTime;

    private static Mutex mut = new Mutex();

    public GameEngine(GameEngine.GameSpec spec, IModel _model)
    {
        my_spec = spec;
        actions = new Dictionary<string, Action>();
        model = _model;
        foreach (Action act in my_spec.Actions)
        {
            Console.WriteLine(act.ToString());
            actions[act.Name] = act;
        }

        lua = new Lua();
        
        lua.UseTraceback = true;
        
        gameData = new Dictionary<string, string>();
        users = new Dictionary<string, GameUser>();
        
        lastTickTime = DateTime.UtcNow;
        
        lua["Log"] = Log;
        lua["MessageUser"] = MessageUser;
        
        Action? preambleAction = FindAction("Preamble");
        if (preambleAction != null)
        {
            lua["gameData"] = gameData;
            DoLua(preambleAction.Code);
            try
            {
                gameData = (lua["gameData"] as Dictionary<string, string>)!;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        else
        {
            Console.WriteLine("No preamble action");
        }


        Action? gameStartedAction = FindAction("GameStarted");
        if (gameStartedAction != null)
        {
            lua["gameData"] = gameData;
            DoLua(gameStartedAction.Code);
            try
            {
                gameData = (lua["gameData"] as Dictionary<string, string>)!;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        else
        {
            Console.WriteLine("No game started action");
        }
    }

    public record VerifyGameResult(bool Success, string? Hash, string Message);

    public record Action(string Name, string Code);

    public record GameSpec(List<Action> Actions, string Name, string Version);

    public static byte[] HashGame(GameSpec game, ILogger logger)
    {
        using (SHA512 sha512 = SHA512.Create())
        {
            logger.LogInformation(JsonSerializer.Serialize(game));
            return sha512.ComputeHash(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(game)));
        }
    }

    public static VerifyGameResult VerifyGame(GameSpec gs, ILogger logger)
    {
        if (gs == null)
        {
            return new VerifyGameResult(false, null, "GS was null");
        }
        else
        {
            string hashGame = JsonSerializer.Serialize(HashGame(gs, logger));
            logger.LogInformation(hashGame);
            return new VerifyGameResult(true, hashGame, "Oh-Kay!");
        }
    }

    public Action? FindAction(string name)
    {
        if (actions.ContainsKey(name))
        {
            return actions[name];
        }

        return null;
    }

    private void Log(string Message)
    {
        Console.WriteLine(Message);
    }

    public bool UserJoined(string Token, int Id)
    {
        mut.WaitOne();
        if (users.ContainsKey(Token))
        {
            return false;
        }

        users[Token] = new GameUser(new Dictionary<string, string>(), Token, Id);

        model.QueueDeclare(queue: "worldOutForUser" + Token, 
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        
        
        Action? act = FindAction("UserJoined");
        if (act != null)
        {
            lua["joinedUserToken"] = Token;
            AddUsersToLua();
            lua["gameData"] = gameData;
            DoLua(act.Code);
            try
            {
                users = (lua["users"] as Dictionary<string, GameUser>)!;
                gameData = (lua["gameData"] as Dictionary<string, string>)!;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        else
        {
            Console.WriteLine("No UserJoined action found");
        }
        mut.ReleaseMutex();

        return true;
    }

    public bool UserLeft(string Token)
    {
        mut.WaitOne();
        if (users.ContainsKey(Token))
        {
            Action? act = FindAction("UserLeft");
            if (act != null)
            {
                lua["leftUserToken"] = Token;
                AddUsersToLua();
                lua["gameData"] = gameData;
                DoLua(act.Code);
                try
                {
                    users = (lua["users"] as Dictionary<string, GameUser>)!;
                    gameData = (lua["gameData"] as Dictionary<string, string>)!;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            else
            {
                Console.WriteLine("No UserLeft action found");
            }

            users.Remove(Token);
            mut.ReleaseMutex();
            return true;
        }
        mut.ReleaseMutex();
        return false;
    }

    public bool Tick()
    {
        var now = DateTime.UtcNow;
        mut.WaitOne();
        Action? act = FindAction("Tick");
        if (act != null)
        {
            AddUsersToLua();
            lua["gameData"] = gameData;
            lua["delta"] = (now - lastTickTime).TotalSeconds;
            lua["shouldExit"] = false;
            DoLua(act.Code);
            lastTickTime = now;
            try
            {
                users = (lua["users"] as Dictionary<string, GameUser>)!;
                gameData = (lua["gameData"] as Dictionary<string, string>)!;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            if ((bool)lua["shouldExit"] == true)
            {
                mut.ReleaseMutex();
                return true;
            }
        }
        else
        {
            Console.WriteLine("No Tick action found");
        }
        mut.ReleaseMutex();

        return false;
    }

    public bool ExecuteUserAction(string ActionName, string Token, string Data)
    {
        mut.WaitOne();
        if (!users.ContainsKey(Token))
        {
            Console.WriteLine("User not found");
            return false;
        }

        Action? act = FindAction("UserAction " + ActionName);
        if (act != null)
        {
            lua["actionData"] = Data;
            lua["userToken"] = Token;
            AddUsersToLua();
            lua["gameData"] = gameData;

            DoLua(act.Code);

            try
            {
                users = (lua["users"] as Dictionary<string, GameUser>)!;
                gameData = (lua["gameData"] as Dictionary<string, string>)!;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            mut.ReleaseMutex();

            return true;
        }
        else
        {
            Console.WriteLine($"Action '{ActionName}' not found");
            mut.ReleaseMutex();
            return false;
        }
    }

    bool MessageUser(string UserToken, string MsgClass, string Message)
    {
        
        if (!users.ContainsKey(UserToken))
        {
            Console.WriteLine("User not found");
            return false;
        }

        
        model.BasicPublish(string.Empty, "worldOutForUser" + UserToken, null, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new GameOutData(MsgClass, Message))));
        return true;
    }
    
    private void AddUsersToLua()
    {
        lua["users"] = users;
    }

    private void DoLua(string chunk)
    {
        try
        {
            lua.DoString(chunk);
        }
        catch (LuaScriptException e)
        {
            Console.WriteLine(chunk);
            Console.WriteLine(e);
            throw;
        }
        catch (ExecutionEngineException e)
        {
            Console.WriteLine(e);
            throw;
        }
        catch (SEHException e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    

    public record GameInData(string Token, string Message, GameInDataType MessageType);

    public enum GameInDataType
    {
        UserJoined,
        UserLeft,
        UserExecutesAction
    }

    public record UserAction(string ActionName, string Data);

    private record GameUser(Dictionary<string, string> Data, string Token, int Id);
    
    public record GameOutData(string MsgType, string Message);
}