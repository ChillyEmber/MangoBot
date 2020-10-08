using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using MangoBotStartup;
using System.IO;
using Newtonsoft.Json;
using System;
using unirest_net;
using unirest_net.http;
using unirest_net.request;
using System.Net.NetworkInformation;
using MangoBotCommandsNamespace;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Mono;
using Mono.Options;
using System.Linq;

namespace MangoBotCommandsNamespace
{
    public class PPSize
    {
        public Dictionary<ulong, int> ppsize { get; set; } = new Dictionary<ulong, int>();
    }
    public class Commands : ModuleBase<SocketCommandContext>
    {
        //Some Strings that need to be changed per-bot owner
        private ulong DiscordBotOwner = 287778194977980416; // REPLACE WITH CURRENT BOT OWNER USERID
        public string prefixx = "^"; //REPLACE WITH CURRENT PREFIX (ALSO MUST BE CHANGED IN CONFIG)
        private BotConfig config;


        [Command("ping")]
        private async Task Ping()
        {
            await ReplyAsync("Pong! 🏓 **" + Program._client.Latency + "ms**");
        }
        [Command("stinky")]
        private async Task Dylan(ulong args)
        {
            if (Context.User.Id == DiscordBotOwner)
            {
                int stinky = 0;
                while (stinky < 6)
                {
                    SocketUser user = Program._client.GetUser(args);
                    await user.GetOrCreateDMChannelAsync();
                    await user.SendMessageAsync("Hey Stinky");
                    stinky = stinky + 1;
                }
                await ReplyAsync("Done!");
            }
            else
            {
                await ReplyAsync("You're not the owner, you're the stinky one!");
            }
        }
        [Command("status")]
        private async Task status(string args)
        {
            if (Context.User.Id == DiscordBotOwner)
            {
                string status = args;
                await Program._client.SetGameAsync($"{status}");
            }
            else
            {
                await ReplyAsync("You aren't the owner silly!");
            }
        }
        [Command("help")]
        private async Task help()
        {
            config = JsonConvert.DeserializeObject<BotConfig>(File.ReadAllText("config.json"));
            await ReplyAsync($"**Commands:**\n" +
                $"***Current Prefix is {prefixx}***\n" +
                $"**Help:** *Displays this command.*\n" +
                $"**penis:** *Generates a penis size for the mentioned user.*\n" +
                $"**8ball:** *Read the future with this 8ball command!*\n" +
                $"**ping:** *Sends the ping of the discord bot.*\n" +
                $"**slap @user:** *Slaps specified user.*\n" +
                $"**joke:** *Tells a dad joke!*\n" +
                $"**todo:** *Lists any upcoming commands and/or things that need to be fixed/changed.*\n");
        }
        [Command("penis")]
        private async Task penis(string args)
        {
            /*string[] penisquotes = { "8=D", "8==D", "8===D", "8====D", "8=====D", "8======D", "8=======D", "8========D", "8=========D", "8=========D"};
            Random rand = new Random();
            int index = rand.Next(penisquotes.Length);
            await ReplyAsync($"{args}'s penis length is {penisquotes[index]}");*/

            var pp = JsonConvert.DeserializeObject<PPSize>(File.ReadAllText("ppsize.json"));

            //just  generate random number and set that num to ppnum
            string[] penisquotes = { "8D", "8=D", "8==D", "8===D", "8====D", "8=====D", "8======D", "8=======D", "8========D", "8=========D", "8=========D", "8==========D" };
            Random rand = new Random();
            int RandomID = rand.Next(1, 11); //Under the assumption that ^ comment is what this means
            int ppnum = RandomID;
            ulong CLIENTID = MentionUtils.ParseUser(args);
            if (pp.ppsize.ContainsKey(CLIENTID))
            {
                ppnum = pp.ppsize[CLIENTID];
            }
            else
            {
                pp.ppsize.Add(CLIENTID, ppnum);
                File.WriteAllText("ppsize.json", JsonConvert.SerializeObject(pp));
            }
            await ReplyAsync($"{args} penis size is {penisquotes[ppnum]}");
        }
        [Command("8ball")]
        private async Task eightball()
        {
            string[] eightballquotes = { "As I see it, yes.", "Ask again later.", "Better not tell you now.", "Cannot predict now.", "Concentrate and ask again.", "Don’t count on it.", "Don’t count on it.", "It is certain.",
                "It is decidedly so.", "Most likely.", "My reply is no.", "My sources say no.", "Outlook not so good.", "Outlook good.", "Reply hazy, try again.", "Signs point to yes", "Very doubtful", "Without a doubt.",
            "Yes.", "Yes - definitely.", "You may rely on it"};
            Random rand = new Random();
            int index = rand.Next(eightballquotes.Length);
            await ReplyAsync($"{eightballquotes[index]}");
        }
        [Command("slap")]
        private async Task slap(string args)
        {
            if (args.Contains("@"))
            {
                if (args.Contains(" "))
                {
                    await ReplyAsync("Only mention one user!"); //Doesn't work because the command handler is a tad wonky
                }
                else
                {
                    string[] listofslaps = { $"<@{Context.User.Id}> just slapped {args}!", $"<@{Context.User.Id}> slaps {args} around with a large trout!" };
                    Random rand = new Random();
                    int index = rand.Next(listofslaps.Length);
                    await ReplyAsync($"{listofslaps[index]}");
                }
            }
            else
            {
                await ReplyAsync("You need to mention someone to slap them!");
            }
        }
        [Command("joke")]
        [Alias("dadjoke")]
        [Summary("Tells a dad joke!")]
        private async Task joke()
        {
            HttpResponse<string> response = Unirest.get("https://icanhazdadjoke.com/")
              .header("User-Agent", "MangoBot lXxMangoxXl@gmail.com (Github coming soon)")
              .header("Accept", "text/plain")
              .asString();
            await ReplyAsync(response.Body.ToString());
        }
        [Command("todo")]
        private async Task todo()
        {
            await ReplyAsync("**1.** Edit command handler for better error messages.");
        }
    }
}
