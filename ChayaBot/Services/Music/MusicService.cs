using System;
using Discord;
using System.Threading.Tasks;
using Discord.WebSocket;
using ChayaBot.Core.Music;
using Discord.Commands;
using System.Collections.Concurrent;
using ChayaBot.Extensions;
using ChayaBot.Core.Music.Songs;
using System.Linq;
using ChayaBot.Services.Database;
using ChayaBot.Services.Database.Streams;
using System.Collections.Generic;
using System.Text;
using Discord.Rest;

namespace ChayaBot.Services.Music
{
    public class MusicService
    {

        // Fields
        private DiscordSocketClient client;
        private DatabaseContext database;
        private ConcurrentDictionary<ulong, MusicEntry> entries;
        private bool plannedStop;


        // Constructor
        public MusicService(DiscordSocketClient client, DatabaseService databaseService)
        {
            this.client = client;
            database = databaseService.GetContext();

            entries = new ConcurrentDictionary<ulong, MusicEntry>();

            client.UserVoiceStateUpdated += Client_UserVoiceStateUpdated;
        }

        private async Task Client_UserVoiceStateUpdated(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {
            var guser = user as IGuildUser;

            if (guser == null || guser.IsBot || oldState.VoiceChannel == null)
                return;

            // If there is a player in the guild and the user is in the same voice channel as the player
            if (entries.TryGetValue(guser.GuildId, out MusicEntry entry) && entry.VoiceChannel == oldState.VoiceChannel)
            {
                // If the user left the channel (disconnected or left to another one)
                if (newState.VoiceChannel != entry.VoiceChannel)
                {
                    // If Chaya is the only one left
                    if (oldState.VoiceChannel.Users.Count == 1 && oldState.VoiceChannel.Users.First().Id == client.CurrentUser.Id)
                    {
                        await entry.MessageChannel.SendEmbedMessageAsync("Music Player", "Getting left behind is fun.. Goodbye.", Colors.BLACK);
                        await LeaveChannelAsync(guser.GuildId);
                    }
                }
            }
        }

        public async Task<JoinChannelResult> JoinChannelAsync(SocketCommandContext context, IVoiceChannel voiceChannel)
        {
            if (entries.TryGetValue(context.Guild.Id, out MusicEntry entry))
                return JoinChannelResult.ALREADY_CONNECTED;

            if (context.Guild.Id != voiceChannel.GuildId)
                return JoinChannelResult.DIFFERENT_GUILD;

            var audioClient = await voiceChannel.ConnectAsync();
            MusicEntry newentry = new MusicEntry(context.Guild.Id, voiceChannel, audioClient, context.Channel, database.GetStreams());

            if (entries.TryAdd(context.Guild.Id, newentry))
            {
                // Register event handlers
                newentry.MusicPlayer.PlayerStarted += Player_PlayerStarted;
                newentry.MusicPlayer.PlayerStopped += Player_PlayerStopped;
                newentry.MusicPlayer.SongQueued += Player_SongQueued;
                newentry.MusicPlayer.PlaylistQueued += Player_PlaylistQueued;
                newentry.StreamPlayer.PlayerStarted += StreamPlayer_PlayerStarted;
                newentry.StreamPlayer.PlayerStopped += StreamPlayer_PlayerStopped;

                return JoinChannelResult.JOINED;
            }

            return JoinChannelResult.NONE;
        }

        private async void StreamPlayer_PlayerStopped(StreamPlayer player)
        {
            if (!entries.TryGetValue(player.GuildId, out MusicEntry entry))
                return;

            if (plannedStop)
            {
                plannedStop = false;
                return;
            }

            await entry.MessageChannel.SendEmbedMessageAsync("Stream Player", "Player stopped.", Colors.BLACK);
        }

        private async void StreamPlayer_PlayerStarted(StreamPlayer player, Stream currentStream)
        {
            if (!entries.TryGetValue(player.GuildId, out MusicEntry entry))
                return;

            await entry.MessageChannel.SendEmbedMessageAsync("Stream Player", $"Currently streaming: **{currentStream.Title}**.", Colors.GREEN);
        }

        private async void Player_PlaylistQueued(MusicPlayer player, ulong requester, int count)
        {
            if (!entries.TryGetValue(player.GuildId, out MusicEntry entry))
                return;

            var user = client.GetUser(requester);
            await entry.MessageChannel.SendEmbedMessageAsync("Music Player", $"**{count}** songs queued by {user.Mention}.", Colors.GREEN);
        }

        private async void Player_SongQueued(MusicPlayer player, ulong requester, Song song)
        {
            if (!entries.TryGetValue(player.GuildId, out MusicEntry entry))
                return;

            var user = client.GetUser(requester);
            await entry.MessageChannel.SendEmbedMessageAsync("Music Player", $"**[{song.Title}]({song.WebpageUrl})** queued by {user.Mention}",
                Colors.GREEN, song.ThumbnailUrl);
        }

        private async void Player_PlayerStopped(MusicPlayer player)
        {
            if (!entries.TryGetValue(player.GuildId, out MusicEntry entry))
                return;

            if (plannedStop)
            {
                plannedStop = false;
                return;
            }

            await entry.MessageChannel.SendEmbedMessageAsync("Music Player", "Finished playing all songs.", Colors.BLACK);
        }

        private async void Player_PlayerStarted(MusicPlayer player, Song currentSong)
        {
            if (!entries.TryGetValue(player.GuildId, out MusicEntry entry))
                return;

            EmbedFieldBuilder fb1 = new EmbedFieldBuilder()
            {
                //IsInline = true,
                Name = "Channel",
                Value = currentSong.Uploader
            };
            EmbedFieldBuilder fb2 = new EmbedFieldBuilder()
            {
                //IsInline = true,
                Name = "Duration",
                Value = currentSong.Duration.ToString("hh':'mm':'ss")
            };

            await entry.MessageChannel.SendEmbedMessageAsync("Music Player", $"Now playing: **[{currentSong.Title}]({currentSong.WebpageUrl})**.",
                Colors.BLACK, currentSong.ThumbnailUrl, new[] { fb1, fb2 });
        }

        public async Task PrintCurrentSong(SocketCommandContext context)
        {
            if (!entries.TryGetValue(context.Guild.Id, out MusicEntry entry) || entry.StreamPlayer.IsPlaying)
                return;

            if (!entry.MusicPlayer.IsPlaying)
            {
                await entry.MessageChannel.SendEmbedMessageAsync("Music Player", "The player isn't playing anything.", Colors.RED);
            }
            else
            {
                Song currentSong = entry.MusicPlayer.CurrentSong;
                var requester = client.GetUser(currentSong.Requester);

                EmbedFieldBuilder fb1 = new EmbedFieldBuilder()
                {
                    //IsInline = true,
                    Name = "Channel",
                    Value = currentSong.Uploader
                };
                EmbedFieldBuilder fb2 = new EmbedFieldBuilder()
                {
                    //IsInline = true,
                    Name = "Time Elapsed",
                    Value = $"{entry.MusicPlayer.ElapsedDuration.ToString("hh':'mm':'ss")}/{currentSong.Duration.ToString("hh':'mm':'ss")}"
                };

                await entry.MessageChannel.SendEmbedMessageAsync("Music Player", $"Currently playing: **[{currentSong.Title}]({currentSong.WebpageUrl})**." +
                    Environment.NewLine + $"Requested by {requester.Mention}.", Colors.BLACK, currentSong.ThumbnailUrl, new[] { fb1, fb2 });
            }
        }

        public async Task<RestUserMessage> PrintStreams(ISocketMessageChannel channel)
        {
            Dictionary<string, List<string>> streams = new Dictionary<string, List<string>>();

            foreach (Stream stream in database.GetStreams())
            {
                if (!streams.ContainsKey(stream.Category.Title))
                    streams.Add(stream.Category.Title, new List<string>());

                streams[stream.Category.Title].Add($"{stream.Id}: [{stream.Title}]({stream.WebsiteUrl})");
            }

            List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>();
            foreach (var kvp in streams)
            {
                EmbedFieldBuilder builder = new EmbedFieldBuilder() { Name = kvp.Key };
                StringBuilder sb = new StringBuilder();
                kvp.Value.ForEach(f => sb.AppendLine(f));
                fields.Add(builder.WithValue(sb.ToString()));
            }

            return await channel.SendEmbedMessageAsync("Stream Player", "Please choose a stream to play:", Colors.BLACK, null, fields);
        }

        public async Task PlayStream(SocketCommandContext context, int streamId)
        {
            if (!entries.TryGetValue(context.Guild.Id, out MusicEntry entry))
            {
                await context.Channel.SendEmbedMessageAsync("Stream Player", "Please use the join command first.", Colors.RED);
                return;
            }

            // Stop music if its playing
            if (entry.MusicPlayer.IsPlaying)
            {
                plannedStop = true;
                entry.MusicPlayer.StopPlayer();
            }

            try
            {
                await entry.StreamPlayer.PlayStream(streamId);
            }
            catch (Exception e)
            {
                await context.Channel.SendEmbedMessageAsync("Stream Player", e.Message, Colors.RED);
            }
        }

        public async Task QueueSongFromUrl(SocketCommandContext context, string url)
        {
            if (!entries.TryGetValue(context.Guild.Id, out MusicEntry entry))
            {
                await context.Channel.SendEmbedMessageAsync("Music Player", "Please use the join command first.", Colors.RED);
                return;
            }

            // Stop stream if its playing
            if (entry.StreamPlayer.IsPlaying)
            {
                plannedStop = true;
                entry.StreamPlayer.StopPlayer();
            }

            try
            {
                await entry.MusicPlayer.QueueFromUrl(context.User.Id, url);
            }
            catch
            {
                await context.Channel.SendEmbedMessageAsync("Music Player", $"Unable to get audio from url: **{url}**.", Colors.RED);
            }
        }

        public async Task SkipSong(SocketCommandContext context)
        {
            if (!entries.TryGetValue(context.Guild.Id, out MusicEntry entry))
                return;

            if (!entry.MusicPlayer.IsPlaying)
            {
                await entry.MessageChannel.SendEmbedMessageAsync("Music Player", "The player isn't playing anything.", Colors.RED);
            }
            else
            {
                if (entry.MusicPlayer.SkipSong(context.User.Id))
                {
                    await entry.MessageChannel.SendEmbedMessageAsync("Music Player", $"Current song skipped by {context.User.Mention}.", Colors.GREEN);
                }
                else
                {
                    await entry.MessageChannel.SendEmbedMessageAsync("Music Player", $"You cannot skip a song you didn't request.", Colors.RED);
                }
            }
        }

        public async Task StopPlayer(ulong guildId, SocketUser user)
        {
            if (!entries.TryGetValue(guildId, out MusicEntry entry))
                return;

            if (entry.MusicPlayer.IsPlaying)
            {
                int clearedRequests = entry.MusicPlayer.StopPlayer() + 1;
                if (clearedRequests > 0 && user != null)
                {
                    await entry.MessageChannel.SendEmbedMessageAsync("Music Player", $"Player stopped by {user.Mention}, **{clearedRequests}** songs cleared.", Colors.GREEN);
                }
            }
            else if (entry.StreamPlayer.IsPlaying)
            {
                entry.StreamPlayer.StopPlayer();
                if (user != null)
                {
                    await entry.MessageChannel.SendEmbedMessageAsync("Stream Player", $"Player stopped by {user.Mention}.", Colors.GREEN);
                }
            }
        }

        public async Task<bool> LeaveChannelAsync(ulong guildId)
        {
            // Stop the entry.Player in case the music is still streaming
            await StopPlayer(guildId, null);

            // Leave the channel
            if (entries.TryRemove(guildId, out MusicEntry entry))
            {
                // Remove event handlers
                entry.MusicPlayer.PlayerStarted -= Player_PlayerStarted;
                entry.MusicPlayer.PlayerStopped -= Player_PlayerStopped;
                entry.MusicPlayer.SongQueued -= Player_SongQueued;
                entry.MusicPlayer.PlaylistQueued -= Player_PlaylistQueued;

                await entry.MusicPlayer.StopAudio();
                return true;
            }

            return false;
        }

    }
}
