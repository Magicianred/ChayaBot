using System;
using System.ComponentModel.DataAnnotations;

namespace ChayaBot.Services.Database.Streams
{
    public class Stream
    {

        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string WebsiteUrl { get; set; }
        public string StreamUrl { get; set; }

        public int CategoryId { get; set; }
        public StreamCategory Category { get; set; }

    }
}
