using Discord;
using Discord.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChayaBot.Extensions
{
    public static class MessageExtensions
    {

        public static async Task ModifyEmbedMessageAsync(this RestUserMessage message, string newTitle, string newDescription, Colors? newColor = null,
            string newThumbnailUrl = null, bool appendDescription = false, IEnumerable<EmbedFieldBuilder> newFields = null, bool appendFields = false)
        {
            var oldBuilder = message.Embeds.FirstOrDefault();
            if (oldBuilder == null)
                return;

            string description = newDescription == null ? oldBuilder.Description : (appendDescription ? (oldBuilder.Description + Environment.NewLine + newDescription) : newDescription);
            List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>();
            if (appendFields)
            {
                foreach (var field in oldBuilder.Fields)
                {
                    fields.Add(new EmbedFieldBuilder()
                    {
                        IsInline = field.Inline,
                        Name = field.Name,
                        Value = field.Value
                    });
                }
            }
            if (newFields != null)
            {
                fields.AddRange(newFields);
            }

            var builder = new EmbedBuilder()
            {
                Title = newTitle == null ? oldBuilder.Title : newTitle,
                Description = description,
                Color = newColor == null ? oldBuilder.Color : new Color((uint)newColor),
                ThumbnailUrl = newThumbnailUrl == null ? oldBuilder.Thumbnail?.Url : newThumbnailUrl,
                Footer = new EmbedFooterBuilder() { Text = oldBuilder.Footer.Value.Text }
            };

            builder.Fields.AddRange(fields);
            await message.ModifyAsync(f => f.Embed = builder.Build());
        }

    }
}
