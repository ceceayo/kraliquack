using System.Text.Json.Nodes;

namespace UGS.Shared.DbModels;

public class GameSpec
{
    public int Id { get; set; }
    public required string Hash { get; set; }
    public required string Data { get; set; }
}