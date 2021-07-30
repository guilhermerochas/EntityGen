using CommandLine;

namespace EntityGen.Models
{
    public class Options
    {
        [Option('t',"tablename", Required = false, HelpText = "Name of table in Sql Server")]
        public string TableName { get; set; }
        [Option('p',"path", Required = false, HelpText = "Path for config file", Default = "./connection.json")]
        public string Path { get; set; }
    }
}