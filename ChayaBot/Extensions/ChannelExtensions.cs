using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChayaBot.Extensions
{
    public static class ChannelExtensions
    {

        public static async Task<RestUserMessage> SendEmbedMessageAsync(this ISocketMessageChannel channel, string title, string description,
            Colors color, string thumbnailUrl = null, IEnumerable<EmbedFieldBuilder> fields = null)
        {
            var builder = new EmbedBuilder()
            {
                Title = title,
                Description = description,
                Color = new Color((uint)color),
                ThumbnailUrl = thumbnailUrl,
                Footer = new EmbedFooterBuilder() { Text = "ChayaBot" }
            };

            if (fields !=null)
                builder.Fields.AddRange(fields);

            return await channel.SendMessageAsync("", false, builder.Build());
        }

        public static async Task<IUserMessage> SendTempMessageAsync(this IMessageChannel channel, string text, uint time = 0, bool isTTS = false,
            EmbedBuilder embed = null, RequestOptions options = null)
        {
            async void deleteAfter(IUserMessage msg, uint after)
            {
                await Task.Delay((int)after);
                await msg.DeleteAsync();
            }

            var message = await channel.SendMessageAsync(text, isTTS, embed, options);

            if (time > 0)
            {
                var task = Task.Run(() => deleteAfter(message, time));
            }

            return message;
        }

    }
}
