using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Newtonsoft.Json;
using Npgsql;
using TestEnvironment.Docker;

namespace DockerEnvironmentPlayground
{
    class Program
    {
        private static void Main(string[] args)
        {
            Program.Execute().Wait();
        }

        public class TestData
        {
            public int id { get; set; }
            public string name { get; set; }
        }
        static async Task Execute()
        {
            using(var environment = new DockerEnvironmentBuilder()                
                .AddContainer(
                    "postgres", 
                    "postgres", "10.7",
                    environmentVariables: new Dictionary<string, string>
                    { 
                        { "POSTGRES_USER", "otdev" }, 
                        { "POSTGRES_PASSWORD", "letmein" },
                        { "POSTGRES_DB", "offersdb" } 
                    })
                .Build())
                {

                await environment.Up();
                var container = environment.GetContainer("postgres");

                Console.WriteLine("Container Info");
                Console.WriteLine(JsonConvert.SerializeObject(container, Formatting.Indented));
                var connectionString = $"Database=offersdb; User Id=otdev; Password=letmein;Server=localhost; Port={container.Ports.First().Value};";
                Console.WriteLine(connectionString);

                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Execute("CREATE TABLE Data ( id integer primary key, name text );");
                    conn.Open();
                    using (var trx = conn.BeginTransaction())
                    {
                        conn.Execute("CREATE INDEX CONCURRENTLY ix_name ON Data (name)");
                    }
                    conn.Execute("INSERT INTO Data (id, name) VALUES (1, 'Test')");
                    var data = conn.Query<TestData>("select * from Test");
                    foreach (var item in data)
                        Console.WriteLine(JsonConvert.SerializeObject(item, Formatting.Indented));
                }

                await environment.Down();
            }
        }
    }
}
