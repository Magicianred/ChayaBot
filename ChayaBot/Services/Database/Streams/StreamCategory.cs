using System.ComponentModel.DataAnnotations;

namespace ChayaBot.Services.Database.Streams
{
    public class StreamCategory
    {

        [Key]
        public int Id { get; set; }
        public string Title { get; set; }

    }
}
