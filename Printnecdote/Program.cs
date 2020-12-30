using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Printnecdote.Properties;
using Printnecdote.Services;
using Printnecdote.Game;
using Newtonsoft.Json;

namespace Printnecdote
{
    class Program
    {
        // Bot Information
        private static string devcon_BotToken;
        public static Dictionary<ulong, string> guildPrefixes { get; set; }
        public static readonly Random _rand = new Random();
        public DiscordShardedClient client { get; private set; }
        public static Program prog { get; private set; }
        public static List<ulong> BotAdmins { get; set; }
        // Database Information
        public static readonly SqlConnectionStringBuilder conn = new SqlConnectionStringBuilder //Create internal SQL configuration
        {
            Password = Resources.SQLPass,
            UserID = "admin",
            DataSource = "cessumdb.cam7nhcocotl.us-east-1.rds.amazonaws.com",
            //InitialCatalog = "dbo.Guilds",
            TrustServerCertificate = true,
            //MultipleActiveResultSets = true
        };
        private static string devcon_DbName;
        public static string dbo { get { return devcon_DbName + ".dbo"; } }

        // Game Information
        public static readonly GameController game = new GameController();
        public static bool GameLoaded = false;
        

        // System Information
        public static bool IsDebug {
            get {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        static void Main(string[] args)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };


            // Set debug related values such as the toxen and database name
            if(IsDebug)
            {
                devcon_DbName = "PrintnecdoteDev";
                devcon_BotToken = Resources.DevToken;
            }
            else
            {
                devcon_DbName = "Printnecdote";
                devcon_BotToken = Resources.Token;
            }
            
            // Get guild prefixes and load them into the local Dictionary.
            guildPrefixes = new Dictionary<ulong, string>();
            using (SqlConnection server = new SqlConnection(conn.ConnectionString))
            {
                server.Open();
                SqlCommand cmd = new SqlCommand($"SELECT * FROM {dbo}.GuildConfig", server);
                SqlDataReader reader = cmd.ExecuteReader();
                if(reader.HasRows)
                {
                    while(reader.Read())
                    {
                        guildPrefixes.Add(Convert.ToUInt64(reader.GetInt64(0)), reader.GetString(1));
                    }
                }
            }

            
            // Sets up bot admins information
            BotAdmins = new List<ulong>();
            BotAdmins.Add(133735496479145984); // Andrew Bounds (Soyvolon). Actuall command functions to be added later

            // Starts the bot
            prog = new Program();
            prog.MainAsync().GetAwaiter().GetResult();
            prog.InitalizePlayers().GetAwaiter().GetResult();
            prog.LogAsync(new LogMessage(LogSeverity.Info, "Printnecdote", "Players Ready. System Online"));
            prog.client.SetStatusAsync(UserStatus.Online);
            GameLoaded = true;
            Task.Delay(-1).GetAwaiter().GetResult();
        }

        public async Task MainAsync()
        {

            var config = new DiscordSocketConfig
            {
                TotalShards = 1
            };

            int guildCount = 0;

            using (var services = ConfigureServices(config))
            {
                client = services.GetRequiredService<DiscordShardedClient>();

                client.ShardConnected += ReadyAsync;

                await client.LoginAsync(TokenType.Bot, devcon_BotToken);

                guildCount = client.Guilds.Count;


                await client.LogoutAsync();
            }

            config.TotalShards = guildCount / 1500;

            if (config.TotalShards <= 0) config.TotalShards = 1;

            using (var services = ConfigureServices(config))
            {
                var client = services.GetRequiredService<DiscordShardedClient>();

                await services.GetRequiredService<CommandHandlingService>().InitializeAsync();

                await client.LoginAsync(TokenType.Bot, Resources.DevToken);

                //await client.SetStatusAsync(UserStatus.DoNotDisturb);

                await client.StartAsync();

                await LogAsync(new LogMessage(LogSeverity.Info, "Startup", "Bot Started"));
            }
        }

        private ServiceProvider ConfigureServices(DiscordSocketConfig config)
        {
            return new ServiceCollection()
                .AddSingleton(new DiscordShardedClient(config))
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .BuildServiceProvider();
        }


        private Task ReadyAsync(DiscordSocketClient shard)
        {
            Console.WriteLine($"Shard Number {shard.ShardId} is connected and ready!");
            return Task.CompletedTask;
        }

        public Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        /// <summary>
        /// Adds all current players to the Player Dict.
        /// </summary>
        /// <returns></returns>
        public async Task InitalizePlayers()
        {
            using (SqlConnection server = new SqlConnection(conn.ConnectionString))
            {
                server.Open();
                SqlCommand cmd = new SqlCommand($"SELECT * FROM {dbo}.PlayerConfig", server);
                SqlDataReader reader = await cmd.ExecuteReaderAsync();
                if (reader.HasRows)
                {
                    while (await reader.ReadAsync())
                    {
                        game.InitializePlayer(Convert.ToUInt64(reader.GetInt64(0)), reader.GetString(1));
                    }
                }
            }
        }
    }
}
