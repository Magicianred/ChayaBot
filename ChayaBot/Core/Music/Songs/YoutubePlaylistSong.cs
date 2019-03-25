namespace ChayaBot.Core.Music.Songs
{
    public class YoutubePlaylistSong : BaseSong
    {

        // Constructor
        public YoutubePlaylistSong(string id, ulong requester, string title, string thumbnailUrl)
            : base(id, requester, $"https://www.youtube.com/watch?v={id}", title, thumbnailUrl)
        {

        }

    }
}
