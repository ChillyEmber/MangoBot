using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Lavalink4NET.DiscordNet;
using Lavalink4NET.Player;
using Lavalink4NET.Rest;
using MangoBotStartup;
using Mono.Options;
using System;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MangoBotCore.Commands
{
    public class Music : ModuleBase<SocketCommandContext>
    {
        public async Task CheckVoiceChat(SocketCommandContext context) //Check if user is in same voice chat check
        {
            SocketGuildUser user = context.User as SocketGuildUser; // Get the user who executed the command
            IVoiceChannel channel = user.VoiceChannel;

            if (channel == null) // Check if the user is in a channel
            {
                await context.Message.Channel.SendMessageAsync("Please join a voice channel first.");
            }
            else
            {
                var clientUser = await context.Channel.GetUserAsync(context.Client.CurrentUser.Id); // Find the client's current user (I.e. this bot) in the channel the command was executed in
                if (clientUser != null)
                {
                    if (clientUser is IGuildUser bot) // Cast the client user so we can access the VoiceChannel property
                    {
                        if (bot.VoiceChannel == null)
                        {
                            Console.WriteLine("Debug: Bot is not in any channels, continuing");
                        }
                        else if (bot.VoiceChannel.Id != channel.Id)
                        {
                            await ReplyAsync($"Bot is currently in: {bot.VoiceChannel.Name}, please join the voice chat to use commands!");
                            return;
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Unable to find bot in server: {context.Guild.Name}");
                }
            }
        }

        //private readonly Program.AudioService _audioService;
        private async Task<VoteLavalinkPlayer> GetPlayerAsync(bool connectToVoiceChannel = true)
        {
            var player = Program.AudioService.GetPlayer<VoteLavalinkPlayer>(Context.Guild);

            if (player != null
                && player.State != PlayerState.NotConnected
                && player.State != PlayerState.Destroyed)
            {
                return player;
            }

            var user = Context.Guild.GetUser(Context.User.Id);

            if (!user.VoiceState.HasValue)
            {
                await ReplyAsync("You must be in a voice channel!");
                return null;
            }

            if (!connectToVoiceChannel)
            {
                await ReplyAsync("The bot is not in a voice channel!");
                return null;
            }

            return await Program.AudioService.JoinAsync<VoteLavalinkPlayer>(user.VoiceChannel);
        }

        [Command("disconnect")]
        [Alias("fuckoff", "goaway")]
        public async Task Disconnect()
        {
            var player = await GetPlayerAsync();
            //Variables? WOAHHHHHH
            var id = Context.Guild.GetUser(Context.User.Id).VoiceChannel;

            if (player == null || id == null)
            {
                return;
            }

            await CheckVoiceChat(Context);

            // when using StopAsync(true) the player also disconnects and clears the track queue.
            // DisconnectAsync only disconnects from the channel.
            await player.StopAsync(true);
            await ReplyAsync("Disconnected.");
        }

        [Command("volume")]
        [Alias("v")]
        public async Task Volume(int volume = 100)
        {
            var player = await GetPlayerAsync();
            //Variables? WOAHHHHHH
            var id = Context.Guild.GetUser(Context.User.Id).VoiceChannel;

            if (player == null || id == null)
            {
                return;
            }

            await CheckVoiceChat(Context);


            if (volume > 100 || volume < 0)
            {
                await ReplyAsync("Volume out of range: 0% - 100%! (Make sure that you don't have a % in the parameters.)");
                return;
            }

            await player.SetVolumeAsync(volume / 100f);
            await ReplyAsync($"Volume updated: {volume}%");
        }

        [Command("volumeoverride")]
        [RequireOwner]
        public async Task volumeoverride(int volume = 100)
        {
            //Declare my variable, to the flag, of the United Coders of America (I don't know if it's actually a thing or not, please don't come for me if it's some hacker corp, it's just a joke smh my head)... 
            var player = await GetPlayerAsync();

            //If the player doesn't exist... the comments; they're over 9000!!!!
            if (player == null)
            {
                return;
            }

            await player.SetVolumeAsync(volume / 100f);
            await ReplyAsync($"Volume updated: {volume}%");
        }

        [Command("play")]
        [Alias("p", "soundcloud")]
        public async Task Play([Remainder] string query)
        {
            var player = await GetPlayerAsync();
            //Variables? WOAHHHHHH
            var id = Context.Guild.GetUser(Context.User.Id).VoiceChannel;

            if (player == null || id == null)
            {
                return;
            }

            await CheckVoiceChat(Context);
            var track = await Program.AudioService.GetTrackAsync(query, SearchMode.YouTube);
            
            if (Context.Message.Content.Contains("^soundcloud"))
            {
                track = await Program.AudioService.GetTrackAsync(query, SearchMode.SoundCloud);
                await ReplyAsync("Searching on SoundCloud...");
            }
            else
            {
                /*if (Context.Message.Content.Contains("playlist"))
                {
                    
                }*/
                await ReplyAsync("Searching on YouTube...");
            }

            //If the tract wasn't found
            if (track == null)
            {
                await ReplyAsync("😖 No results.");
                return;
            }

            //If the track is greater than 90m
            /*else if (track.Duration.TotalMinutes > 90)
            {
                await ReplyAsync("That song was too long (>90m)");
                return;
            }*/

            //Some blacklist stuff.
            else if (track.Title.Contains("earrape") || track.Title.Contains("moan") || track.Title.Contains("NSFW") ||
                     track.Title.Contains("ringing") || track.Title.Contains("18+"))
            {
                await ReplyAsync("Audio returned with a blacklisted title!");
                return;
            }

            var position = await player.PlayAsync(track, enqueue: true);

            if (position == 0) //If the track is first in the queue.
            {
                await player.SetVolumeAsync(5f / 100f);
                await ReplyAsync("🔈 Playing: " + track.Title + $"\nVolume: {player.Volume * 100}%");
            }
            else //If the track is not first in queue.
            {
                await ReplyAsync("🔈 Added to queue: " + track.Title);
            }
        }

        [Command("skip")]
        [Alias("s")]
        public async Task Skip()
        {
            //4 Variables? There was 3 last command! Some declaration of variables, they declared independance from the project.
            var author = Context.Guild.GetUser(Context.User.Id);
            var player = await GetPlayerAsync();
            var results = await player.VoteAsync(Context.User.Id);
            //Variables? WOAHHHHHH
            var id = Context.Guild.GetUser(Context.User.Id).VoiceChannel;

            if (player == null || id == null)
            {
                return;
            }

            await CheckVoiceChat(Context);

            if (author.GuildPermissions.Administrator || author.GuildPermissions.ManageMessages || Context.User.Id == Program._client.GetApplicationInfoAsync().Result.Owner.Id || author.Roles.Any(r => r.Name == "DJ"))
            {
                player.ClearVotes();
                await player.SkipAsync();
                await ReplyAsync("Force skipped!");
                return;
            }

            if (results.WasSkipped)
            {
                await ReplyAsync("Skipped!");
            }
            else if (results.WasAdded)
            {
                var info = await player.GetVoteInfoAsync();
                await ReplyAsync($"Vote was added! There are {info.Votes.Count} votes, and there needs to be {info.TotalUsers / 2} votes total!");
            }
            else await ReplyAsync("You already voted!");
        }

        [Command("position")]
        public async Task Position()
        {
            var player = await GetPlayerAsync();
            //Variables? WOAHHHHHH
            var id = Context.Guild.GetUser(Context.User.Id).VoiceChannel;

            if (player == null || id == null)
            {
                return;
            }

            await CheckVoiceChat(Context);

            if (player.CurrentTrack == null)
            {
                await ReplyAsync("Nothing playing!");
                return;
            }

            await ReplyAsync($"Position: {player.TrackPosition} / {player.CurrentTrack.Duration}.");
        }

        [Command("stop")]
        public async Task Stop()
        {
            //More Variables
            var player = await GetPlayerAsync();
            var id = Context.Guild.GetUser(Context.User.Id).VoiceChannel;

            if (player == null || id == null)
            {
                return;
            }

            await CheckVoiceChat(Context);

            if (player.CurrentTrack == null)
            {
                await ReplyAsync("Nothing playing!");
                return;
            }

            await player.StopAsync();
            await ReplyAsync("Stopped playing.");
        }
        [Command("loop")]
        public async Task Loop()
        {
            //I pledge my allegience, to these variables, one project, under lXxMangoxXl... or something like that anyway.
            var player = await GetPlayerAsync();
            //Variables? WOAHHHHHH
            var id = Context.Guild.GetUser(Context.User.Id).VoiceChannel;

            if (player == null || id == null)
            {
                return;
            }

            await CheckVoiceChat(Context);


            if (player.IsLooping == false)
            {
                player.IsLooping = true;
                await ReplyAsync("Looping enabled!");
            }
            else
            {
                player.IsLooping = false;
                await ReplyAsync("Looping disabled!");
            }
        }
        [Command("queue")]
        [Alias("q")]
        public async Task Queue()
        {
            var player = await GetPlayerAsync();
            //Variables? WOAHHHHHH
            var id = Context.Guild.GetUser(Context.User.Id).VoiceChannel;

            if (player == null || id == null)
            {
                return;
            }

            await CheckVoiceChat(Context);

            if (player.Queue.Count < 1)
            {
                await ReplyAsync($"**Currently Playing:** {player.CurrentTrack.Title}\n**Nothing Else is Queued.**");
                return;
            }

            string queue = $"**Currently Playing:** {player.CurrentTrack.Title}\n**Next Up:**";
            int tracknumber = 1;
            foreach(var track in player.Queue)
            {
                queue += $"\n*{tracknumber}:* {track.Title}";
                tracknumber++;
            }

            await ReplyAsync(queue);
        }

        [Command("randomuser")]
        [Alias("ru")]
        public async Task RandomUser(ulong channelid)
        {
            var channel = Context.Guild.GetChannel(channelid);
            int funnyusernumber = new Random().Next(0, channel.Users.Count());
            SocketGuildUser user = channel.Users.ToList()[funnyusernumber];
            await ReplyAsync($"{user.Username}, {user.Id}");
        }
    }
    
}
