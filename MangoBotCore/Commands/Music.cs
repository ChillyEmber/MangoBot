using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Lavalink4NET.DiscordNet;
using Lavalink4NET.Player;
using Lavalink4NET.Rest;
using MangoBotStartup;
using Mono.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using unirest_net.http;

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
            var player = await GetPlayerAsync();

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

            //player.VoiceChannelId
            var id = Context.Guild.GetUser(Context.User.Id).VoiceChannel;

            if (id == null || player.VoiceChannelId != id.Id)
            {
                await ReplyAsync("Join the voice chat I'm in first!");
                return;
            }

            var track = await Program.AudioService.GetTrackAsync(query, SearchMode.YouTube);

            if (track == null)
            {
                await ReplyAsync("😖 No results.");
                return;
            }

            else if (track.Duration.TotalMinutes > 90)
            {
                await ReplyAsync("That song was too long (>90m)");
                return;
            }

            else if(track.Title.Contains("earrape") || track.Title.Contains("moan") || track.Title.Contains("NSFW") || track.Title.Contains("ringing") || track.Title.Contains("18+"))
            {
                await ReplyAsync("Audio returned with a blacklisted title!");
                return;
            }

            var position = await player.PlayAsync(track, enqueue: true);

            if (position == 0)
            {
                await ReplyAsync("🔈 Playing: " + track.Title);
            }
            else
            {
                await ReplyAsync("🔈 Added to queue: " + track.Title);
            }
        }

        [Command("skip")]
        [Alias("s")]
        public async Task Skip()
        {
            var author = Context.Guild.GetUser(Context.User.Id);
            var player = await GetPlayerAsync();
            var results = await player.VoteAsync(Context.User.Id);
            var id = Context.Guild.GetUser(Context.User.Id).VoiceChannel;

            if (player == null)
            {
                return;
            }

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
            var player = await GetPlayerAsync();
            var id = Context.Guild.GetUser(Context.User.Id).VoiceChannel;

            if (player == null)
            {
                return;
            }

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
            var player = await GetPlayerAsync();
            var id = Context.Guild.GetUser(Context.User.Id).VoiceChannel;

            if (player == null)
            {
                return;
            }

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
    }
}
