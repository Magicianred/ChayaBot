using ChayaBot.Core.Music;
using ChayaBot.Services.Database.Streams;
using Discord;
using Discord.Audio;
using Discord.WebSocket;
using System.Collections.Generic;

namespace ChayaBot.Services.Music
{
    public class MusicEntry
    {

        // Properties
        public MusicPlayer MusicPlayer { get; private set; }
        public StreamPlayer StreamPlayer { get; private set; }
        public ISocketMessageChannel MessageChannel { get; private set; }
        public IVoiceChannel VoiceChannel { get; private set; }


        // Constructor
        public MusicEntry(ulong guildId, IVoiceChannel voiceChannel, IAudioClient audioClient, ISocketMessageChannel messageChannel, List<Stream> streams)
        {
            MessageChannel = messageChannel;
            VoiceChannel = voiceChannel;

            MusicPlayer = new MusicPlayer(guildId, audioClient);
            StreamPlayer = new StreamPlayer(guildId, audioClient, streams);
        }

    }
}
