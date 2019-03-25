using System.Collections.Generic;
using System.IO;

namespace ChayaBot
{
    public static class Configuration
    {

        // General
        public static List<ulong> Owners { get; } = new List<ulong>();
        public static string Token { get; } = "MzE5NTU5NTQ3OTUxNzc1NzY1.DCR9zw.PCBtlLxDGU8i87QLwgunKwn_iqM";


        // Music
        public static string MusicOutputDir { get; } = Path.Combine(Directory.GetCurrentDirectory(), "music_output");
        public static string DefaultThumbnailUrl { get; } = "https://cdn2.iconfinder.com/data/icons/circle-icons-1/64/music-64.png";
        public static int BaseBitrate { get; } = 128 * 1024;

    }
}
