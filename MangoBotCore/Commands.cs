using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MangoBotStartup;
using Mono.Options;
using Newtonsoft.Json;
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
        private BotConfig config;
        private ulong DiscordBotOwner = 287778194977980416; // REPLACE WITH CURRENT BOT OWNER USERID
        //private int disablepenisbool;

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
        private async Task status([Remainder] string args)
        {
            if (Context.User.Id == DiscordBotOwner)
            {
                await Program._client.SetGameAsync($"{args}");
            }
            else
            {
                //await ReplyAsync("You aren't the owner silly!");
            }
        }
        [Command("help")]
        private async Task help()
        {
            config = JsonConvert.DeserializeObject<BotConfig>(File.ReadAllText("config.json"));
            string prefix = config.prefix;
            string CommandsList = $"**Commands:**\n" +
    $"***Current Prefix is {prefix}***\n" +
    $"**help:** *Displays this command.*\n" +
    $"**about:** *Displays some information about the bot!*\n" +
    $"**todo:** *Lists any upcoming commands and/or things that need to be fixed/changed.*\n" +
    $"**penis:** *Generates a penis size for the mentioned user.*\n" +
    $"**8ball:** *Read the future with this 8ball command!*\n" +
    $"**ping:** *Sends the ping of the discord bot.*\n" +
    $"**slap @user:** *Slaps specified user.*\n" +
    $"**joke:** *Tells a dad joke!*\n" +
    $"**avatar:** *Sends the avatar of the person mentioned, or yourself if nobody is mentioned.*\n" +
    $"**defaultavatar:** *Sends the default avatar of the person mentioned, or yourself if nobody is mentioned.*\n";
            if (Context.Guild.Id == 687875961995132973)
            {
                await ReplyAsync(CommandsList + "**minecraft:** *Sends the current IP of the minecraft server*\n" +
                    "**appeal:** *Sends a invite link to the appeal discord*");
            }
            else
            {
                await ReplyAsync(CommandsList);
            }
        }
        [Command("penis")]
        private async Task penis(params string[] args)
        {
            //State config.disabledpenis
            bool setpenison = true;
            config = JsonConvert.DeserializeObject<BotConfig>(File.ReadAllText("config.json"));
            string disabledpenis = config.disabledpenis;

            if (config.disabledpenis == "1" && Context.Guild.Id == 687875961995132973) 
            {
                // If pp command is disabled, and the guild is Unlimited it will return.
                await ReplyAsync("Penis commands have been disabled, sorry!");
                return;
            }

            var pp = JsonConvert.DeserializeObject<PPSize>(File.ReadAllText("ppsize.json"));

            //just generate random number and set that num to ppnum
            string[] penisquotes = { "8D", "8=D", "8==D", "8===D", "8====D", "8=====D", "8======D", "8=======D", "8========D", "8=========D", "8=========D", "8==========D" }; //All the different penises that it can send.
            Random rand = new Random(); //Creates a random veriable
            int RandomID = rand.Next(1, 11); //Select one of the two options that it can pick from
            int ppnum = RandomID; //Saves the RandomID to ppnum.

            ulong ppUserId;

            switch (args.Length) 
            {
                case 0:
                    ppUserId = Context.User.Id;
                    break;
                case 1:
                    ppUserId = MentionUtils.ParseUser(args[0]); //Takes the UserID from mention and saves it to ClientID
                    break;
                default:
                    await ReplyAsync($"Only have one input argument, you currently have {args.Length}, you're only supposed to have 1!");
                    return;
            }

            if (pp.ppsize.ContainsKey(ppUserId)) 
            {
                ppnum = pp.ppsize[ppUserId];
            } 
            else 
            {
                pp.ppsize.Add(ppUserId, ppnum);
                File.WriteAllText("ppsize.json", JsonConvert.SerializeObject(pp));
            }
            await ReplyAsync($"<@{ppUserId}>'s penis size is {penisquotes[ppnum]}");
        }


        [Command("8ball")]
        private async Task eightball(params string[] args)
        {
            string[] eightballquotes = { "As I see it, yes.", "Ask again later.", "Better not tell you now.", "Cannot predict now.", "Concentrate and ask again.", "Don’t count on it.", "Don’t count on it.", "It is certain.",
                "It is decidedly so.", "Most likely.", "My reply is no.", "My sources say no.", "Outlook not so good.", "Outlook good.", "Reply hazy, try again.", "Signs point to yes", "Very doubtful", "Without a doubt.",
            "Yes.", "Yes - definitely.", "You may rely on it"};
            Random rand = new Random();
            int index = rand.Next(eightballquotes.Length);
            if (args.Length == 0)
            {
                await ReplyAsync("You have to say something in order to recieve a prediction!");
            }
            else
            {
                await ReplyAsync($"{eightballquotes[index]}");
            }
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
                if (args[0].Contains("@everyone") | args[0].Contains("@here"))
                {
                    await ReplyAsync("Tsk Tsk");
                }
                else
                {
                    if (args[0].Contains("@"))
                    {
                        ulong CLIENTID = MentionUtils.ParseUser(args[0]);
                        string[] listofslaps = { $"<@{Context.User.Id}> just slapped {Program._client.GetUser(CLIENTID)}!", $"<@{Context.User.Id}> slaps {Program._client.GetUser(CLIENTID)} around with a large trout!" };
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
        }
        [Command("joke")]
        [Alias("dadjoke")]
        [Summary("Tells a dad joke!")]
        private async Task joke(params string[] args)
        {
            HttpResponse<string> response = Unirest.get("https://icanhazdadjoke.com/")
            .header("User-Agent", "MangoBot https://github.com/lXxMangoxXl/MangoBot")
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
            if (args.Length == 0)
            {
                string avatarurl = Context.User.GetAvatarUrl();
                await ReplyAsync($"Heres your avatar! {avatarurl}");
            }
            if (args.Length == 1)
            {
                if (args[0].Contains("@everyone") | args[0].Contains("@here"))
                {
                    await ReplyAsync("Tsk Tsk");
                }
                else
                {
                    if (args[0].Contains("@"))
                    {
                        ulong userid = MentionUtils.ParseUser(args[0]);
                        SocketUser user = Program._client.GetUser(userid);
                        if (user.GetAvatarUrl() == null)
                        {
                            await ReplyAsync($"{Program._client.GetUser(userid)}'s avatar is {user.GetDefaultAvatarUrl()}");
                        }
                        else
                        {
                            await ReplyAsync($"{Program._client.GetUser(userid)}'s avatar is {user.GetAvatarUrl()}");
                        }
                    }
                    else
                    {
                        await ReplyAsync("You have to mention someone to get their avatar!");
                    }
                }
            }
            if (args.Length == 2 | args.Length > 2)
            {
                await ReplyAsync("Mention only one user!");
            }
        }
        [Command("defaultavatar")]
        [Alias("defaultpfp")]
        private async Task defaultavatar(params string[] args)
        {
            if (args.Length == 0)
            {
                await ReplyAsync($"Heres your default avatar! {Context.User.GetDefaultAvatarUrl()}");
            }
            if (args.Length == 1)
            {
                if (args[0].Contains("@everyone") | args[0].Contains("@here"))
                {
                    await ReplyAsync("Tsk Tsk");
                }
                else
                {
                    if (args[0].Contains("@"))
                    {
                        ulong userid = MentionUtils.ParseUser(args[0]);
                        SocketUser user = Program._client.GetUser(userid);
                        await ReplyAsync($"{Program._client.GetUser(userid)}'s avatar is {user.GetDefaultAvatarUrl()}");
                    }
                    else
                    {
                        await ReplyAsync("You have to mention someone to get their avatar!");
                    }
                }
            }
            if (args.Length == 2 | args.Length > 2)
            {
                await ReplyAsync("Mention only one user!");
            }
        }
        [Command("say")]
        private async Task say([Remainder] string args)
        {
            await Context.Message.DeleteAsync();
            if (Context.User.Id == DiscordBotOwner)
            {
                await ReplyAsync(args);
            }
            else
            {
                await ReplyAsync("You must be the bot owner to execute the command!");
            }
        }

        [Command("about")]
        private async Task about()
        {
            await ReplyAsync("Made by lXxMangoxXl#8878\n" +
                "https://github.com/lXxMangoxXl/MangoBot/ \n" +
                "Made with Discord.Net, C#, and lots of love!");
        }
        [Command("minecraft")]
        private async Task minecraft()
        {
            //If you're in Unlimited, send the game address, if you aren't, spit out a generic error message.
            if (Context.Guild.Id == 687875961995132973)
            {
                await ReplyAsync("The server IP is `mc.unlimitedscp.com`");
            }
            else
            {
                await ReplyAsync("That's a Unlimited SCP only command!");
            }
        }
        [Command("appeal")]
        [Alias("appeals")]
        private async Task appeal()
        {
            //If you're in Unlimited, send the invite, if you aren't, spit out a generic error message.
            if (Context.Guild.Id == 687875961995132973)
            {
                await ReplyAsync("https://discord.gg/gfCvJ3d");
            }
            else
            {
                await ReplyAsync("That's a Unlimited SCP only command!");
            }
        }
        [Command("disablepenis")]
        private async Task disablepenis()
        {
            //label the config as a string.
            string text = File.ReadAllText("config.json");

            //Prepare the config.disabledpenis
            config = JsonConvert.DeserializeObject<BotConfig>(File.ReadAllText("config.json"));
            string disabledpenisstringg = config.disabledpenis;

            //Checks to see if your userid is either Mango's or River's
            if (Context.User.Id == 287778194977980416 | Context.User.Id == 706468176707190845)
            {
                //If it's currently disabled
                if (config.disabledpenis == "1")
                {
                    //replace the found text with the other text
                    text = text.Replace("\"disabledpenis\": \"1\"", "\"disabledpenis\": \"2\"");
                    //write it
                    File.WriteAllText("config.json", text);
                    //reply with a basic response
                    await ReplyAsync("Penis commands are now *enabled*");
                }
                //if it's not currently disabled
                else
                {
                    //replace the found text with the other text
                    text = text.Replace("\"disabledpenis\": \"2\"", "\"disabledpenis\": \"1\"");
                    //write it
                    File.WriteAllText("config.json", text);
                    //reply with a basic response
                    await ReplyAsync("Penis commands are now *disabled*");
                }
            }
            //If you aren't Mango or River
            else
            {
                await ReplyAsync("You have to be either River or Mango to execute this command!");
            }
        }
        [Command("brazil")]
        [Summary("Send someone to brazil.")]
        private async Task brazil()
        {
            await ReplyAsync("https://media1.tenor.com/images/d632412aaffe388de314b7abff9c408e/tenor.gif?itemid=17781004");
        }
    }
}
