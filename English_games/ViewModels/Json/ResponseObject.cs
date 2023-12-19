using System.Security.Cryptography.Xml;

namespace English_games.ViewModels.Json;

public class ResponseObject
{
    public int code { get; set; }
    public DataObject data { get; set; }
    public string message { get; set; }
}