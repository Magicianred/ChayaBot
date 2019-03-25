using Newtonsoft.Json;

namespace ChayaBot.Core.Music.Songs
{
    public class BaseSong
    {

        // Properties
        public string Id { get; private set; }
        public ulong Requester { get; set; }
        [JsonProperty("webpage_url")]
        public string WebpageUrl { get; private set; }
        public string Title { get; private set; }
        [JsonProperty("thumbnail")]
        public string ThumbnailUrl { get; private set; }


        // Constructor
        public BaseSong(string id, ulong requester, string webpageUrl, string title, string thumbnailUrl)
        {
            Id = id;
            Requester = requester;
            WebpageUrl = webpageUrl;
            Title = title;
            ThumbnailUrl = thumbnailUrl;
        }

    }
}
