using System;
using System.IO;
using System.Threading.Tasks;
using CommandLine;
using EntityGen.Models;
using Newtonsoft.Json;

namespace EntityGen
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Parser.Default.ParseArguments<Options>("-t haha".Split(" "))
                .WithParsedAsync(async opts =>
                {
                    if (!File.Exists(opts.Path))
                    {
                        Console.Write("Path to connection file not found");
                    }

                    String fileContent;

                    using (var reader = new StreamReader(opts.Path))
                        fileContent = await reader.ReadToEndAsync();

                    if (string.IsNullOrEmpty(fileContent))
                    {
                        Console.WriteLine("not able to get the file content");
                        return;
                    }

                    Connection conn = JsonConvert.DeserializeObject<Connection>(fileContent);

                    if (conn == null)
                    {
                        var writer = Console.Error;
                        await writer.WriteLineAsync("Not able to parse connection file");
                        return;
                    }

                    EntityGenerator generator = new EntityGenerator(conn);
                    await generator.CreateFileOnSystem(generator.GenerateEntity("USUARIOS_ETRACKING"));
                });
        }
    }
}
