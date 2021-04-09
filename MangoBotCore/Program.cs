using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MangoBotCommandsNamespace;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Lavalink4NET;
using Lavalink4NET.DiscordNet;
using Lavalink4NET.Tracking;

namespace MangoBotStartup
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program().RunBot().GetAwaiter().GetResult();
        }


        // Creating the necessary variables
        public static DiscordSocketClient _client;
        public static IAudioService AudioService;
        private CommandService _commands;
        private IServiceProvider _services;
        private BotConfig config;

        // Runbot task
        public async Task RunBot()
        {
            // Config creation/reading.
            if (!File.Exists("config.json"))
            {
                config = new BotConfig()
                {
                    prefix = "^",
                    token = "",
                    game = "",
                    botowner = "",
                    disabledpenis = "1",
                    appealurl = "",
                    LavalinkPassword = "",
                    LavalinkRestURL = "",
                    LavalinkWebsocketURL = ""
                };
                File.WriteAllText("config.json", JsonConvert.SerializeObject(config, Formatting.Indented));
            }
            else
            {
                config = JsonConvert.DeserializeObject<BotConfig>(File.ReadAllText("config.json"));
            }
            if (!File.Exists("ppsize.json"))
            {
                File.WriteAllText("ppsize.json", JsonConvert.SerializeObject(new PPSize()));
            }

            _client = new DiscordSocketClient(); // Define _client
            _commands = new CommandService(); // Define _commands
            _services = new ServiceCollection() // Define _services
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton<IAudioService, LavalinkNode>()
                .AddSingleton<IDiscordClientWrapper>(new DiscordClientWrapper(_client))
                .AddSingleton(new LavalinkNodeOptions {
                    AllowResuming = true,
                    BufferSize = 1024 * 1024,
                    DisconnectOnStop = false,
                    ReconnectStrategy = ReconnectStrategies.DefaultStrategy,
                    DebugPayloads = true,
                    Password = config.LavalinkPassword,
                    RestUri = config.LavalinkRestURL,
                    WebSocketUri = config.LavalinkWebsocketURL
                })
                .AddSingleton<InactivityTrackingOptions>()
                .AddSingleton<InactivityTrackingService>()
                .BuildServiceProvider();

            AudioService = _services.GetRequiredService<IAudioService>();

            _services.GetRequiredService<InactivityTrackingService>()
            .BeginTracking();

            // Do not forget disposing the service provider!

            // I didn't forget :) (we intentionally forgot)
            //await serviceProvider.DisposeAsync();

            string botToken = config.token; // Make a string for the token

            string prefix = config.prefix;

            string disabledpenis = config.disabledpenis;

            string appealurl = config.appealurl;

            string LavalinkPassword = config.LavalinkPassword;

            string LavalinkRestURL = config.LavalinkRestURL;

            string LavalinkWebsocketURL = config.LavalinkWebsocketURL;

            //ulong botowner = config.botowner;

            _client.Log += Log; // Logging

            await RegisterCommandsAsync(); // Call registercommands

            await _client.LoginAsync(TokenType.Bot, botToken); // Log into the bot user

            await _client.StartAsync(); // Start the bot user

            await _client.SetGameAsync(config.game); // Set the game the bot is playing

            await AudioService.InitializeAsync();

            await Task.Delay(-1); // Delay for -1 to keep the console window open

            //AudioService.Dispose();
        }

        private async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync; // Messagerecieved

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), null); // Add module to _commands
        }

        private Task Log(LogMessage arg) // Logging
        {
            Console.WriteLine(arg); // Print the log to Console
            return Task.CompletedTask; // Return with completedtask
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            string messageLower = arg.Content.ToLower(); // Convert the message to a Lower
            var message = arg as SocketUserMessage; // Create a variable with the message as SocketUserMessage
            if (message is null || message.Author.IsBot || message.Content.Contains($"{config.prefix} ")) return; // Checks if the message is empty or sent by a bot
            int argumentPos = 0; // Sets the argpos to 0 (the start of the message)
            if (message.HasStringPrefix(config.prefix, ref argumentPos) & message.Author.Id != 734471872317751457 & message.Author.Id != 469974878015979520 || message.HasMentionPrefix(_client.CurrentUser, ref argumentPos)) // If the message has the prefix at the start or starts with someone mentioning the bot
            {
                var context = new SocketCommandContext(_client, message); // Create a variable called context
                var result = await _commands.ExecuteAsync(context, argumentPos, _services); // Create a veriable called result
                if (!result.IsSuccess) // If the result is unsuccessful
                {
                    Console.WriteLine($"{message.Author.Username} : {message.Author.Id} : {message.Content} : {result.ErrorReason}"); // Print the error to console
                    //await message.Channel.SendMessageAsync(result.ErrorReason);
                }
            }
        }
    }



    public class BotConfig
    {
        public string token { get; set; }
        public string prefix { get; set; }
        public string game { get; set; }
        public string botowner { get; set; }
        public string disabledpenis { get; set; }
        public string appealurl { get; set; }
        public string LavalinkPassword { get; set; }
        public string LavalinkRestURL { get; set; }
        public string LavalinkWebsocketURL { get; set; }
    }

}
