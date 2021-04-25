using Discord.Commands;
using Lavalink4NET.DiscordNet;
using Lavalink4NET.Player;
using Lavalink4NET.Rest;
using MangoBotStartup;
using Mono.Options;
using System.Linq;
using System.Threading.Tasks;

namespace MangoBotCore.Commands
{
    public class Music : ModuleBase<SocketCommandContext>
    {
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
            //Variables... Variables... VARIABLES!!!!
            var player = await GetPlayerAsync();
            
            //Check if the player exists... who's the player and what is he playing??
            if (player == null)
            {
                return;
            }

            var id = Context.Guild.GetUser(Context.User.Id).VoiceChannel;

            if (id == null || player.VoiceChannelId != id.Id)
            {
                await ReplyAsync("Join the voice chat I'm in first!");
                return;
            }

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

            if (player == null)
            {
                return;
            }

            var id = Context.Guild.GetUser(Context.User.Id).VoiceChannel;

            if (id == null || player.VoiceChannelId != id.Id)
            {
                await ReplyAsync("Join the voice chat I'm in first!");
                return;
            }

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
        [Alias("p")]
        public async Task Play([Remainder] string query)
        {
            var player = await GetPlayerAsync();

            if (player == null)
            {
                return;
            }

            //Variables? WOAHHHHHH
            var id = Context.Guild.GetUser(Context.User.Id).VoiceChannel;

            //Makes sure that the bot is in the same voice chat as the author.
            if (id == null || player.VoiceChannelId != id.Id)
            {
                await ReplyAsync("Join the voice chat I'm in first!");
                return;
            }
            
            //Searches for the track on YouTube.
            var track = await Program.AudioService.GetTrackAsync(query, SearchMode.YouTube);

            //If the tract wasn't found
            if (track == null)
            {
                await ReplyAsync("😖 No results.");
                return;
            }

            //If the track is greater than 90m
            else if (track.Duration.TotalMinutes > 90)
            {
                await ReplyAsync("That song was too long (>90m)");
                return;
            }

            //Some blacklist stuff.
            else if (track.Title.Contains("earrape") || track.Title.Contains("moan") || track.Title.Contains("NSFW") || track.Title.Contains("ringing") || track.Title.Contains("18+"))
            {
                await ReplyAsync("Audio returned with a blacklisted title!");
                return;
            }

            //Start playing the song/video/whatever, none of my business.
            var position = await player.PlayAsync(track, enqueue: true);

            if (position == 0) //If the track is first in the queue.
            {
                await ReplyAsync("🔈 Playing: " + track.Title);
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
            var id = Context.Guild.GetUser(Context.User.Id).VoiceChannel;

            //Checking if the player is null.
            if (player == null)
            {
                return;
            }

            //Makes sure the user is in the same VC as the bot.
            if (id == null || player.VoiceChannelId != id.Id)
            {
                await ReplyAsync("Join the voice chat I'm in first!");
                return;
            }

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
            //More variables...
            var player = await GetPlayerAsync();
            var id = Context.Guild.GetUser(Context.User.Id).VoiceChannel;

            //Make sure the player exists, or something like that
            if (player == null)
            {
                return;
            }

            //Check to make sure that the bot is in the same voice chat as the author
            if (id == null || player.VoiceChannelId != id.Id)
            {
                await ReplyAsync("Join the voice chat I'm in first!");
                return;
            }

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

            //Make sure that the player actually exists...
            if (player == null)
            {
                return;
            }

            //Hello? Can you hear me? This is to make sure that the author is actually in the same VC as the bot.
            if (id == null || player.VoiceChannelId != id.Id)
            {
                await ReplyAsync("Join the voice chat I'm in first!");
                return;
            }

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
            var id = Context.Guild.GetUser(Context.User.Id).VoiceChannel;

            //Make sure that the player exists
            if (player == null)
            {
                return;
            }

            //Make sure that the user is in the same VC as the bot
            if (id == null || player.VoiceChannelId != id.Id)
            {
                await ReplyAsync("Join the voice chat I'm in first!");
                return;
            }

            
            if(player.IsLooping == false)
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
            var id = Context.Guild.GetUser(Context.User.Id).VoiceChannel;

            //Make sure that the player exists
            if (player == null)
            {
                return;
            }

            //Make sure that the user is in the same VC as the bot
            if (id == null || player.VoiceChannelId != id.Id)
            {
                await ReplyAsync("Join the voice chat I'm in first!");
                return;
            }

            if (player.Queue.Count < 1)
            {
                await ReplyAsync($"Currently Playing: {player.CurrentTrack.Title}, Nothing Else is Queued.");
                return;
            }

            string queue = $"Currently Playing: {player.CurrentTrack.Title}\nNext Up:";
            int tracknumber = 1;
            foreach(var track in player.Queue)
            {
                queue += $"\n{tracknumber.ToString()}: {track.Title}";
                tracknumber++;
            }

            await ReplyAsync(queue);
        }
    }
}
