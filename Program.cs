using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityGen
{
    class Program
    {
        public class EntityGenerator 
        {
            private readonly string conn_string = @"Server=<SERVER>;
                                                    Database=<TABLE>;
                                                    User ID=<Username>; 
                                                    Password=<Password>";
            public SqlConnection conn { get; private set; }
            public Hashtable dataTypes { get; private set; }
            public string tableName { get; private set; }

            public EntityGenerator() 
            {
                conn = new SqlConnection(conn_string);
                InitializeHashTable();
            }

            public void InitializeHashTable()
            {
                dataTypes = new Hashtable();

                dataTypes.Add("char", "string");
                dataTypes.Add("varchar", "string");
                dataTypes.Add("bit", "bool");
                dataTypes.Add("int", "int");
                dataTypes.Add("decimal", "decimal");
                dataTypes.Add("money", "decimal");
                dataTypes.Add("date", "DateTime");
                dataTypes.Add("varbinary", "byte[]");
                dataTypes.Add("datetime", "DateTime");
            }

            public string GenerateEntity(string tableName)
            {
                try
                {
                    this.tableName = tableName;

                    using (conn)
                    {
                        conn.Open();
                        SqlCommand tableInfo = new SqlCommand($"exec sp_help {tableName}", conn);
                        SqlDataReader reader = tableInfo.ExecuteReader();
                        reader.NextResult();

                        List<List<string>> values = new List<List<string>>();
                        string keys = "";

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                values.Add(new List<string>());

                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    values[values.Count - 1].Add(reader.GetValue(i).ToString());
                                }
                            }
                        }

                        foreach (var i in Enumerable.Range(0, 4))
                            reader.NextResult();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    keys = reader.GetValue(i).ToString();
                                }
                            }
                        }
                        conn.Close();

                        values.ForEach(val => val.Add("no"));


                        keys.Split(",").ToList().ForEach(keyItem =>
                        {
                            values.ForEach(val =>
                            {
                                if (val[0] == keyItem.Trim())
                                    val[val.Count() - 1] = "yes";
                            });
                        });

                        return this.CreateModel(values, tableName);
                    }
                }
                catch(Exception)
                {
                    Console.WriteLine($"Não foi possivel gerar o modelo :(");
                    return "";
                }
            }

            public async Task CreateFileOnSystem(string model)
            {
                try
                {
                    if (string.IsNullOrEmpty(model))
                        throw new Exception();

                    string fileName = $"{tableName.Replace("_", "")}.cs";
                    string filePath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\Desktop\{fileName}";

                    if (!File.Exists(filePath))
                    {
                        using (var creator = File.Create(filePath))
                            creator.Dispose();

                        using (StreamWriter writer = new StreamWriter(filePath))
                        {
                            await writer.WriteLineAsync(model);
                        }
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine($"Não foi possivel criar o arquivo :(");
                }
            }

            private string CreateModel(List<List<string>> data, string tableName)
            {
                try
                {
                    StringBuilder model = new StringBuilder();

                    model.Append($"[Table(\u0022{tableName}\u0022)]\n" +
                                 $"public partial class {tableName.Replace("_", "")}\n" +
                                 $"{{\n");

                    data.ForEach(field =>
                    {
                        string is_key = field[field.Count() - 1] == "yes" ? "[Key]" : string.Empty;
                        string nullable = (field[6].Trim() == "yes" && dataTypes[field[1].Trim()].ToString() != "string") ? "?" : string.Empty;
                        string capitalizedString = (field[0].Substring(0, 1).ToUpper() + field[0].Substring(1)).Replace("_", "");
                        string stringLength = (dataTypes[field[1].Trim()].ToString() == "string" && field[3].Trim() != "max") ? $"[StringLength({field[3].Trim()})]" : string.Empty; 

                        model.Append($"     {is_key}\n" +
                                     $"     [Column(\u0022{field[0]}\u0022)]\n" +
                                     $"     {stringLength}\n" +
                                     $"     public {dataTypes[field[1].Trim()]}{nullable} {capitalizedString} {{ get; set; }}\n");
                    });

                    model.Append($"}}\n");

                    return model.ToString();
                }
                catch(Exception)
                {
                    Console.WriteLine($"Erro gerado na criação do modelo :(");
                    return "";
                }
            }
        }

        static async Task Main(string[] args)
        {
            var entityGen = new EntityGenerator();
            if (args.Count() != 0)
                await entityGen.CreateFileOnSystem(entityGen.GenerateEntity(args[0]));
            else
                Console.WriteLine("É necessario passar o nome da tabela como parametro");
        }
    }
}
