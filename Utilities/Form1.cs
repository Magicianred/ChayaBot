using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utilities.YoutubeExtractor;
using VideoLibrary;
using YoutubeExplode;
using YoutubeExtractor;

namespace Utilities
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnCalcAvgExp_Click(object sender, EventArgs e)
        {
            List<int> exps = rtbExpList.Lines.Select(f => int.Parse(f)).ToList();

            int total = 0;
            total += exps[0];
            for (int i = 0; i < exps.Count - 1; i++)
            {
                int diff = exps[i + 1] - exps[i];
                // Console.WriteLine(diff);
                total += diff;
            }

            int avg = 123;
            Console.WriteLine("Average: " + avg);

            int totalMsgs = 0;
            for (int i = 0; i < exps.Count; i++)
            {
                Console.WriteLine($"{i + 1}, {exps[i]} wille take {exps[i] / avg} messages to reach");
                totalMsgs += exps[i] / avg;
            }

            Console.WriteLine("Total messages: " + totalMsgs);
        }

        private void btnWhatever_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();

            // Generate sql query
            sb.Append("INSERT INTO RankLevels VALUES ");
            foreach (string line in rtbWhatever.Lines)
            {
                var s = line.Split('\t').Select(f =>
                {
                    int x;
                    return int.TryParse(f, out x) ? f : $"'{f}'";
                });
                sb.Append($"({string.Join(", ", s)}), ");
            }


            Console.WriteLine(sb.ToString());
        }

        private void btnExpGen_Click(object sender, EventArgs e)
        {
            int levels = 15;
            int total = 100;

            for (int i = 2; i <= levels; i++)
            {
                total += (int)(total * 1.1);
                Console.WriteLine(total);
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            //var videos = DownloadUrlResolver.GetDownloadUrls("https://www.youtube.com/watch?v=rTKHB_tVYpk");
            //VideoInfo video = videos
            //            .OrderByDescending(info => info.AudioBitrate)
            //            .First();

            //await Task.Factory.StartNew(() =>
            //{
            //    var audioDownload = new AudioDownloader(video, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "test.mp3"));
            //    audioDownload.DownloadProgressChanged += (s, args) => Console.WriteLine(args.ProgressPercentage * 0.85);
            //    audioDownload.AudioExtractionProgressChanged += (s, args) => Console.WriteLine(85 + args.ProgressPercentage * 0.15);
            //    audioDownload.Execute();
            //});

            string path = Path.Combine(@"D:\Recordings\Obs", "Haytam.flv");
            FlvFile flvFile = new FlvFile(path, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "test.mp3"));
            flvFile.ConversionProgressChanged += (s, ea) => Console.WriteLine(ea.ProgressPercentage);
            flvFile.ExtractStreams();
            flvFile.Dispose();
        }

        private async void btnPlaylist_Click(object sender, EventArgs e)
        {
            YoutubeClient youtubeClient = new YoutubeClient();

            if (YoutubeClient.TryParsePlaylistId(tbPlaylistUrl.Text, out string playlistId))
            {
                var playlistInfos = await youtubeClient.GetPlaylistInfoAsync(playlistId);
                var firstVideo = await youtubeClient.GetVideoInfoAsync(playlistInfos.Videos[0].Id);
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            YoutubeClient youtubeClient = new YoutubeClient();

            if (YoutubeClient.TryParseVideoId(tbLive.Text, out string videoId))
            {
                var videoInfo = await youtubeClient.GetVideoInfoAsync(videoId);

                HttpClient httpClient = new HttpClient();
                HttpResponseMessage response = await httpClient.GetAsync($"https://www.youtube.com/get_video_info?&video_id={videoId}&el=info&ps=default&eurl=&gl=US&hl=en");
                string raw = await response.Content.ReadAsStringAsync();

                // Parsing
                var dict = raw.Split('&')
                            .Select(p => p.Split('='))
                            .ToDictionary(p => p[0], p => p.Length > 1 ? Uri.UnescapeDataString(p[1]) : null);
                Console.WriteLine(dict["hlsvp"]);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string path = "https://www.youtube.com/watch?v=cvaIgq5j2Q8&list=PLge2NVqdJnpDoUhLAdnaQiUCylTnMpWJQ";

            var ffmpeg = new ProcessStartInfo
            {
                FileName = "youtube-dl",
                Arguments = $"-i -j {path}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            };

            string s = Process.Start(ffmpeg).StandardOutput.ReadToEnd();
            Clipboard.SetText(s);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            StringBuilder s = new StringBuilder();



            Clipboard.SetText(s.ToString());
        }
    }
}
