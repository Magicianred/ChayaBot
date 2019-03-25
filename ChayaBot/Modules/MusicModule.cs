using ChayaBot.Core.Music;
using ChayaBot.Extensions;
using ChayaBot.Services;
using ChayaBot.Services.Music;
using Discord;
using Discord.Commands;
using Discord.Rest;
using System;
using System.Threading.Tasks;

namespace ChayaBot.Modules
{
    public class MusicModule : ModuleBase<SocketCommandContext>
    {

        // Fields
        private MusicService musicService;
        private InteractiveService interactiveService;


        // Constructor
        public MusicModule(MusicService mService, InteractiveService interactiveService)
        {
            musicService = mService;
            this.interactiveService = interactiveService;
        }


        [Command("join", RunMode = RunMode.Async)]
        public async Task JoinCommand()
        {
            var voiceChannel = (Context.User as IGuildUser)?.VoiceChannel;

            if (voiceChannel == null)
            {
                await Context.Channel.SendEmbedMessageAsync("Music Player", "Please join a voice channel first.", Colors.RED);
                return;
            }

            JoinChannelResult result = await musicService.JoinChannelAsync(Context, voiceChannel);
            switch (result)
            {
                case JoinChannelResult.ALREADY_CONNECTED:
                    await Context.Channel.SendEmbedMessageAsync("Music Player", "I am already connected to a voice channel.", Colors.RED);
                    break;
                case JoinChannelResult.DIFFERENT_GUILD:
                    await Context.Channel.SendEmbedMessageAsync("Music Player", "How am i supposed to join you?", Colors.RED);
                    break;
                case JoinChannelResult.JOINED:
                    await Context.Channel.SendEmbedMessageAsync("Music Player", $"Joined channel **{voiceChannel.Name}**.", Colors.GREEN);
                    break;
            }
        }


        [Command("play", RunMode = RunMode.Async)]
        public async Task PlayCommand(string url)
        {
            await musicService.QueueSongFromUrl(Context, url);
        }


        [Command("stream", RunMode = RunMode.Async)]
        public async Task StreamCommand(int num)
        {
            await musicService.PlayStream(Context, num);
        }


        [Command("stream", RunMode = RunMode.Async)]
        public async Task StreamCommand()
        {
            var streamsMsg = await musicService.PrintStreams(Context.Channel);

            // Wait for the user's response
            var userMsg = await interactiveService.WaitForMessageAsync(Context.User, Context.Channel, 4000);

            if (userMsg == null)
            {
                await streamsMsg.DeleteAsync();
                return;
            }

            if (int.TryParse(userMsg.Content, out int streamId))
            {
                await streamsMsg.DeleteAsync();
                await musicService.PlayStream(Context, streamId);
            }
            else
            {
                await streamsMsg.ModifyEmbedMessageAsync(null, "Wrong number of the stream.", Colors.RED);
            }
        }


        [Command("skip", RunMode = RunMode.Async)]
        public async Task SkipCommand()
        {
            await musicService.SkipSong(Context);
        }


        [Command("stop", RunMode = RunMode.Async)]
        public async Task StopCommand()
        {
            await musicService.StopPlayer(Context.Guild.Id, Context.User);
        }


        [Command("current song", RunMode = RunMode.Async)]
        public async Task CurrentSongCommand()
        {
            await musicService.PrintCurrentSong(Context);
        }


        [Command("leave", RunMode = RunMode.Async)]
        public async Task LeaveCommand()
        {
            if (await musicService.LeaveChannelAsync(Context.Guild.Id))
            {
                await Context.Channel.SendEmbedMessageAsync("Music Player", "Voice channel left.", Colors.GREEN);
            }
        }

    }
}
