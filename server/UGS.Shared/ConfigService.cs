using UGS.Shared.DbModels;

namespace UGS.Shared;

public class ConfigService(SharedUniversalGameServerDataBaseContext db)
{
    public string GetOrSetConfigKey(string key, string defaultValue)
    {
        if (db.ConfigKeys.Any(b => b.Key == key))
        {
            return db.ConfigKeys.First(b => b.Key == key).Value;
        }
        else
        {
            db.ConfigKeys.Add(new ConfigKey {Key = key, Value = defaultValue});
            db.SaveChanges();
            return defaultValue;
        }
    }

    public void OverWriteConfigKey(string key, string newValue)
    {
        if (db.ConfigKeys.Any(b => b.Key == key))
        {
            db.ConfigKeys.First(b => b.Key == key).Value = newValue;
        }
        else
        {
            db.ConfigKeys.Add(new ConfigKey {Key = key, Value = newValue});
        }

        db.SaveChanges();
    }
    
    public Dictionary<string, string> GetAllConfigKeys()
    {
        Dictionary<string, string> result = new();
        foreach (ConfigKey configKey in db.ConfigKeys)
        {
            result[configKey.Key] = configKey.Value;
        }
        return result;
    }

}