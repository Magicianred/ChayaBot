using ChayaBot.Services.Database;
using ChayaBot.Services.Database.Streams;
using Discord.Audio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ChayaBot.Core.Music
{
    public class StreamPlayer
    {

        // Properties
        public ulong GuildId { get; private set; }
        public Stream CurrentStream { get; private set; }
        public bool IsPlaying => CurrentStream != null;

        // Fields
        private IAudioClient audioClient;
        private List<Stream> streams;
        private CancellationTokenSource cancelationToken;
        private bool nextStream;


        // Events
        public event Action<StreamPlayer, Stream> PlayerStarted;
        public event Action<StreamPlayer> PlayerStopped;


        // Constructor
        public StreamPlayer(ulong guildId, IAudioClient audioClient, List<Stream> streams)
        {
            GuildId = guildId;
            this.audioClient = audioClient;
            this.streams = streams;

            cancelationToken = new CancellationTokenSource();
        }


        public async Task PlayStream(int id)
        {
            // If a stream is already playing 
            if (IsPlaying)
            {
                StopStream(true);
                await Task.Delay(2000);
            }

            Stream stream = streams.FirstOrDefault(f => f.Id == id);
            
            if (stream == null)
            {
                throw new Exception("Stream not found.");
            }

            PlayerStarted?.Invoke(this, stream);
            await PlayCurrentStream(stream);
        }

        public void StopPlayer()
        {
            StopStream(false);
        }

        private void StopStream(bool nextStream)
        {
            this.nextStream = nextStream;

            cancelationToken.Cancel();
            cancelationToken.Dispose();
            cancelationToken = new CancellationTokenSource();
        }

        private async Task PlayCurrentStream(Stream streamToPlay)
        {
            CurrentStream = streamToPlay;

            // Get ffmepg stream
            var ffmepgProcess = CreateFFmpegStream(CurrentStream.StreamUrl);
            var stream = ffmepgProcess.StandardOutput.BaseStream;
            var discord = audioClient.CreatePCMStream(AudioApplication.Music, Configuration.BaseBitrate);

            // Start streaming
            try
            {
                await stream.CopyToAsync(discord, 81920, cancelationToken.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException e)
            {

            }
            finally
            {
                // Flush buffers / clean
                await discord.FlushAsync().ConfigureAwait(false);
                CurrentStream = null;

                if (!nextStream)
                {
                    PlayerStopped?.Invoke(this);
                }
                else
                {
                    nextStream = false;
                }
            }
        }

        private Process CreateFFmpegStream(string url)
        {
            var ffmpeg = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-reconnect 1 -reconnect_streamed 1 -reconnect_delay_max 5 -hide_banner -loglevel panic -i \"{url}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            };

            return Process.Start(ffmpeg);
        }

    }
}
