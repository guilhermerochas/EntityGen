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
            await Parser.Default.ParseArguments<Options>(args)
                .WithParsedAsync(async opts =>
                {
                    var writer = Console.Error;
                    
                    if (!File.Exists(opts.Path))
                    {
                        await writer.WriteLineAsync("Path to connection file not found");
                        return;
                    }

                    string fileContent;

                    using (var reader = new StreamReader(opts.Path))
                        fileContent = await reader.ReadToEndAsync();

                    if (string.IsNullOrEmpty(fileContent))
                    {
                        await writer.WriteLineAsync("not able to get the file content");
                        return;
                    }

                    Connection conn = JsonConvert.DeserializeObject<Connection>(fileContent);

                    if (conn == null)
                    {
                        await writer.WriteLineAsync("Not able to parse connection file");
                        return;
                    }

                    EntityGenerator generator = new EntityGenerator(conn);
                    await generator.CreateFileOnSystem(generator.GenerateEntity(opts.TableName));
                });
        }
    }
}
