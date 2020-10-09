using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using MangoBotStartup;
using Microsoft.VisualBasic;
using Mono.Options;
using Newtonsoft.Json;
using Spectacles.NET.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using unirest_net.http;

namespace MangoBotCommandsNamespace
{
    public class PPSize
    {
        public Dictionary<ulong, int> ppsize { get; set; } = new Dictionary<ulong, int>();
    }
    public class Commands : ModuleBase<SocketCommandContext>
    {
        private ulong DiscordBotOwner = 287778194977980416; // REPLACE WITH CURRENT BOT OWNER USERID
        private BotConfig config;

        [Command("ping")]
        private async Task Ping(params string[] args)
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
        private async Task status([Remainder]string args)
        {
            if (Context.User.Id == DiscordBotOwner)
            {
                await Program._client.SetGameAsync($"{args}");
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
            string prefix = config.prefix;
            await ReplyAsync($"**Commands:**\n" +
                $"***Current Prefix is {prefix}***\n" +
                $"**Help:** *Displays this command.*\n" +
                $"**penis:** *Generates a penis size for the mentioned user.*\n" +
                $"**8ball:** *Read the future with this 8ball command!*\n" +
                $"**ping:** *Sends the ping of the discord bot.*\n" +
                $"**slap @user:** *Slaps specified user.*\n" +
                $"**joke:** *Tells a dad joke!*\n" +
                $"**todo:** *Lists any upcoming commands and/or things that need to be fixed/changed.*\n" +
                $"**avatar:** *Sends the avatar of the person mentioned, or yourself if nobody is mentioned.*");
        }
        [Command("penis")]
        private async Task penis(params string[] args)
        {
            /*string[] penisquotes = { "8=D", "8==D", "8===D", "8====D", "8=====D", "8======D", "8=======D", "8========D", "8=========D", "8=========D"};
            Random rand = new Random();
            int index = rand.Next(penisquotes.Length);
            await ReplyAsync($"{args}'s penis length is {penisquotes[index]}");*/

            var pp = JsonConvert.DeserializeObject<PPSize>(File.ReadAllText("ppsize.json"));

            //just generate random number and set that num to ppnum
            string[] penisquotes = { "8D", "8=D", "8==D", "8===D", "8====D", "8=====D", "8======D", "8=======D", "8========D", "8=========D", "8=========D", "8==========D" }; //All the different penises that it can send.
            Random rand = new Random(); //Creates a random veriable
            int RandomID = rand.Next(1, 11); //Select one of the two options that it can pick from
            int ppnum = RandomID; //Saves the RandomID to ppnum.
            if (args.Length == 0)
            {
                ulong authorid = Context.User.Id;
                if (pp.ppsize.ContainsKey(authorid))
                {
                    ppnum = pp.ppsize[authorid];
                }
                else
                {
                    pp.ppsize.Add(authorid, ppnum);
                    File.WriteAllText("ppsize.json", JsonConvert.SerializeObject(pp));
                }
                await ReplyAsync($"<@{authorid}>'s penis size is {penisquotes[ppnum]}");
            }
            if (args.Length == 1)
            {
                ulong CLIENTID = MentionUtils.ParseUser(args[0]); //Takes the UserID from mention and saves it to ClientID
                if (pp.ppsize.ContainsKey(CLIENTID))
                {
                    ppnum = pp.ppsize[CLIENTID];
                }
                else
                {
                    pp.ppsize.Add(CLIENTID, ppnum);
                    File.WriteAllText("ppsize.json", JsonConvert.SerializeObject(pp));
                }
                await ReplyAsync($"{args[0]}'s penis size is {penisquotes[ppnum]}");
            }
            if (args.Length > 2 | args.Length == 2)
            {
                await ReplyAsync($"Only have one input argument, you currently have {args.Length}, you're only supposed to have 1!");
            }
        }


        [Command("8ball")]
        private async Task eightball(params string[] args)
        {
            string[] eightballquotes = { "As I see it, yes.", "Ask again later.", "Better not tell you now.", "Cannot predict now.", "Concentrate and ask again.", "Don’t count on it.", "Don’t count on it.", "It is certain.",
                "It is decidedly so.", "Most likely.", "My reply is no.", "My sources say no.", "Outlook not so good.", "Outlook good.", "Reply hazy, try again.", "Signs point to yes", "Very doubtful", "Without a doubt.",
            "Yes.", "Yes - definitely.", "You may rely on it"};
            Random rand = new Random();
            int index = rand.Next(eightballquotes.Length);
            await ReplyAsync($"{eightballquotes[index]}");
        }
        [Command("slap")]
        private async Task slap(params string[] args)
        {
            if (args.Length > 2 | args.Length == 2)
            {
                await ReplyAsync("Only mention one user!");
            }
            if (args.Length == 0)
            {
                await ReplyAsync($"<@{Context.User.Id}>... Slapped themself?");
            }
            if (args.Length == 1)
            {
                if (args[0].Contains("@"))
                {
                    string[] listofslaps = { $"<@{Context.User.Id}> just slapped {args[0]}!", $"<@{Context.User.Id}> slaps {args[0]} around with a large trout!" };
                    Random rand = new Random();
                    int index = rand.Next(listofslaps.Length);
                    await ReplyAsync($"{listofslaps[index]}");
                }
                else
                {
                    await ReplyAsync("You need to mention someone to slap them!");
                }
            }
        }
        [Command("joke")]
        [Alias("dadjoke")]
        [Summary("Tells a dad joke!")]
        private async Task joke(params string[] args)
        {
            HttpResponse<string> response = Unirest.get("https://icanhazdadjoke.com/")
            .header("User-Agent", "MangoBot lXxMangoxXl@gmail.com (Github coming soon)")
            .header("Accept", "text/plain")
            .asString();
            await ReplyAsync(response.Body.ToString());
        }
        [Command("todo")]
        private async Task todo(params string[] args)
        {
            await ReplyAsync("**1.** Edit command handler for better error messages. **FIXED**");
        }
        [Command("avatar")]
        [Alias("pfp")]
        private async Task avatar(params string[] args)
        {
            if(args.Length == 0)
            {
                string avatarurl = Context.User.GetAvatarUrl();
                await ReplyAsync($"Heres your avatar! {avatarurl}");
            }
            if(args.Length == 1)
            {
                ulong ulonguserid = Convert.ToUInt64(args[0]);
                if (args[0].Contains("@"))
                {
                    ulong userid = MentionUtils.ParseUser(args[0]);
                    Discord.WebSocket.SocketUser user = Program._client.GetUser(userid);
                    await ReplyAsync($"<@{userid}>'s avatar is {user.GetAvatarUrl()}");
                }
                else
                {
                    await ReplyAsync("You have to mention someone to get their avatar!");
                }
            }
            if(args.Length == 2 | args.Length > 2)
            {
                await ReplyAsync("Mention only one user!");
            }
        }
    }
}
