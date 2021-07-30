using Newtonsoft.Json;

namespace EntityGen.Models
{
    public class Connection
    {
        [JsonProperty(Required = Required.Always)]
        string Server { get; set; }
        [JsonProperty(Required = Required.Always)]
        string Database { get; set; }
        [JsonProperty("user_id",Required = Required.Always)]
        string UserId { get; set; }
        [JsonProperty(Required = Required.Always)]
        string Password { get; set; }

        public override string ToString() 
            => $"Server={Server};Database={Database};User ID={UserId};Password={Password}";
    }
}