using ChayaBot.Core.Music.Songs;
using CliWrap;
using Discord.Audio;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode;

namespace ChayaBot.Core.Music
{
    public class MusicPlayer
    {

        // Properties
        public ulong GuildId { get; private set; }
        public Song CurrentSong { get; private set; }
        public bool IsPlaying => CurrentSong != null;
        public TimeSpan ElapsedDuration { get { return TimeSpan.FromSeconds(currentSecond); } }


        // Fields
        private static readonly YoutubeClient youtubeClient = new YoutubeClient();
        private IAudioClient audioClient;
        private Queue<BaseSong> songs;
        private CancellationTokenSource cancelationToken;
        private Timer songTimer;
        private int currentSecond = 0;


        // Events
        public event Action<MusicPlayer, Song> PlayerStarted;
        public event Action<MusicPlayer> PlayerStopped;
        public event Action<MusicPlayer, ulong, Song> SongQueued;
        public event Action<MusicPlayer, ulong, int> PlaylistQueued;


        // Constructor
        public MusicPlayer(ulong guildId, IAudioClient audioClient)
        {
            GuildId = guildId;
            this.audioClient = audioClient;

            cancelationToken = new CancellationTokenSource();
            songTimer = new Timer(SongTimerCallback, this, Timeout.Infinite, Timeout.Infinite);
            songs = new Queue<BaseSong>();
        }

        public async Task QueueFromUrl(ulong requester, string url)
        {
            // Check if the url is a valid youtube playlist url
            if (YoutubeClient.TryParsePlaylistId(url, out string playlistId))
            {
                var playlistInfos = await youtubeClient.GetPlaylistInfoAsync(playlistId);
                foreach (var videoSnippet in playlistInfos.Videos)
                {
                    YoutubePlaylistSong yps = new YoutubePlaylistSong(videoSnippet.Id, requester, videoSnippet.Title, videoSnippet.ImageThumbnailUrl);
                    songs.Enqueue(yps);
                }

                PlaylistQueued?.Invoke(this, requester, playlistInfos.Videos.Count);
            }
            // Check if the url is a valid youtube video url
            else if (YoutubeClient.TryParseVideoId(url, out string videoId))
            {
                Song song = await GetSongFromUrl($"https://www.youtube.com/watch?v={videoId}");
                song.Requester = requester;
                songs.Enqueue(song);

                SongQueued?.Invoke(this, requester, song);
            }
            else
            {
                throw new Exception("Invalid audio url");
            }

            // Dequeue in case the queue was empty and the player isn't playing anything
            if (!IsPlaying)
            {
                await DequeueSong();
            }
        }

        public bool SkipSong(ulong requester)
        {
            if (!IsPlaying || CurrentSong.Requester != requester)
                return false;

            StopStream();
            return true;
        }

        public async Task StopAudio()
        {
            await audioClient.StopAsync();
        }

        public int StopPlayer()
        {
            if (!IsPlaying)
                return -1;

            StopStream();

            // Clear queue
            int n = songs.Count;
            songs.Clear();
            return n;
        }

        private void StopStream()
        {
            cancelationToken.Cancel();
            cancelationToken.Dispose();
            cancelationToken = new CancellationTokenSource();
        }

        private async Task DequeueSong()
        {
            // If the player is already playing a song, return
            if (IsPlaying)
                return;

            // Check if there are still request on the queue
            if (songs.Count == 0)
            {
                PlayerStopped?.Invoke(this);
                return;
            }

            BaseSong nextSong = songs.Dequeue();
            Song songToPlay = null;

            if (nextSong is YoutubePlaylistSong yps)
            {
                songToPlay = await GetSongFromUrl(yps.WebpageUrl);
                songToPlay.Requester = nextSong.Requester;
            }
            else if (nextSong is Song song)
            {
                songToPlay = song;
            }

            PlayerStarted?.Invoke(this, songToPlay);
            var _ = PlaySong(songToPlay);
        }
        
        private void SongTimerCallback(object state)
        {
            currentSecond++;

            if (currentSecond >= CurrentSong.Duration.TotalSeconds)
                StopSongTimer();
        }

        private void StopSongTimer()
        {
            songTimer.Change(Timeout.Infinite, Timeout.Infinite);
            currentSecond = 0;
        }

        private async Task PlaySong(Song songToPlay)
        {
            if (IsPlaying)
                return;

            CurrentSong = songToPlay;
            songTimer.Change(1000, 1000);

            // Get ffmepg stream
            var ffmepgProcess = CreateFFmpegStream(CurrentSong.Url.AbsoluteUri);
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
                CurrentSong = null;
                StopSongTimer();

                // Continue queue after 1 second
                await Task.Delay(1000);
                await DequeueSong();
            }
        }

        private async Task<Song> GetSongFromUrl(string url)
        {
            var youtubedl = CreateYoutubeDlStream(url);
            string rawJson = await youtubedl.StandardOutput.ReadToEndAsync();
            return JsonConvert.DeserializeObject<Song>(rawJson);
        }

        private Process CreateFFmpegStream(string url)
        {
            var ffmpeg = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -loglevel panic -i \"{url}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            };

            return Process.Start(ffmpeg);
        }

        private Process CreateYoutubeDlStream(string url)
        {
            var youtubedl = new ProcessStartInfo
            {
                FileName = "youtube-dl",
                Arguments = $"-i --no-playlist -f bestaudio -J {url}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            };

            return Process.Start(youtubedl);
        }

    }
}
