using Newtonsoft.Json;
using System;

namespace ChayaBot.Core.Music.Songs
{
    public class Song : BaseSong
    {

        // Properties
        public string Uploader { get; private set; }
        [JsonProperty("url")]
        public Uri Url { get; private set; }
        [JsonConverter(typeof(DurationConverter))]
        public TimeSpan Duration { get; private set; }


        // Constructor
        public Song(string id, ulong requester, string webpageUrl, string title, string thumbnailUrl, 
            string uploader, Uri audioUrl, TimeSpan duration) : base(id, requester, webpageUrl, title, thumbnailUrl)
        {
            Uploader = uploader;
            Url = audioUrl;
            Duration = duration;
        }

    }
}
