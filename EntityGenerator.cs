﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityGen.Models;

namespace EntityGen
{
    public class EntityGenerator
    {
        private SqlConnection Conn { get; }
        private Hashtable DataTypes { get; set; }
        private string TableName { get; set; }

        public EntityGenerator(Connection connection)
        {
            Conn = new SqlConnection(connection.ToString());
            InitializeHashTable();
        }

        private void InitializeHashTable()
        {
            DataTypes = new Hashtable
            {
                {"char", "string"},
                {"varchar", "string"},
                {"bit", "bool"},
                {"int", "int"},
                {"decimal", "decimal"},
                {"money", "decimal"},
                {"date", "DateTime"},
                {"varbinary", "byte[]"},
                {"datetime", "DateTime"},
                {"smalldatetime", "DateTime"}
            };
        }

        public string GenerateEntity(string tableNameValue)
        {
            try
            {
                TableName = tableNameValue;

                using (Conn)
                {
                    Conn.Open();
                    SqlCommand tableInfo = new SqlCommand($"exec sp_help {TableName}", Conn);
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
                                values[^1].Add(reader.GetValue(i).ToString());
                            }
                        }
                    }

                    foreach (var _ in Enumerable.Range(0, 4))
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
                    Conn.Close();

                    values.ForEach(val => val.Add("no"));


                    keys?.Split(",").ToList().ForEach(keyItem =>
                    {
                        values.ForEach(val =>
                        {
                            if (val[0] == keyItem.Trim())
                                val[^1] = "yes";
                        });
                    });

                    return this.CreateModel(values);
                }
            }
            catch(Exception)
            {
                Console.WriteLine($"Não foi possivel gerar o modelo :(");
                Environment.Exit(-1);
                return String.Empty;
            }
        }

        public async Task CreateFileOnSystem(string model)
        {
            try
            {
                if (string.IsNullOrEmpty(model))
                    return;

                string fileName = $"{EntityUtils.CaptalizeString(TableName)}.cs";
                string filePath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\Desktop\{fileName}";

                if (!File.Exists(filePath))
                {
                    await using (var creator = File.Create(filePath))
                        await creator.DisposeAsync();

                    await using (StreamWriter writer = new StreamWriter(filePath))
                        await writer.WriteLineAsync(model);
                }
            }
            catch
            {
                Console.WriteLine($"Não foi possivel criar o arquivo :(");
                Environment.Exit(-1);
            }
        }

        private string CreateModel(List<List<string>> data)
        {
            try
            {
                StringBuilder model = new StringBuilder();

                model.Append($"[Table(\u0022{TableName}\u0022)]\n" +
                             $"public partial class {EntityUtils.CaptalizeString(TableName)}\n" +
                             $"{{\n");

                data.ForEach(field =>
                {
                    string isSmallDateTime = field[1] == "smalldatetime"
                        ? ", TypeName = \u0022smalldatetime\u0022"
                        : string.Empty;
                    string isKey = field[^1] == "yes" ? "[Key]" : string.Empty;
                    string nullable = (field[6].Trim() == "yes" && DataTypes[field[1].Trim()]?.ToString() != "string") ? "?" : string.Empty;
                    string stringLength = (DataTypes[field[1].Trim()]?.ToString() == "string" && field[3].Trim() != "max") ? $"[StringLength({field[3].Trim()})]" : string.Empty;

                    if (!string.IsNullOrEmpty(isKey))
                        model.Append($"     {isKey}\n");

                    model.Append($"     [Column(\u0022{field[0]}\u0022{isSmallDateTime})]\n");

                    if (!string.IsNullOrEmpty(stringLength))
                        model.Append($"     {stringLength}\n");

                    model.Append($"     public {DataTypes[field[1].Trim()]}{nullable} {EntityUtils.CaptalizeString(field[0])} {{ get; set; }}\n");
                });

                model.Append($"}}\n");
                return model.ToString();
            }
            catch
            {
                Console.WriteLine($"Erro gerado na criação do modelo :(");
                return String.Empty;
            }
        }
    }
}