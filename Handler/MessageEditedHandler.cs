using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static blankyBot.PublicFunction;

namespace blankyBot
{
    public class MessageEditedHandler
    {
        public MessageEditedHandler(DiscordSocketClient _client)
        {
            this._client = _client;
        }

        private readonly DiscordSocketClient _client;

        public async Task HandleEditAsync(Cacheable<IMessage, ulong> message, SocketMessage socketMessage, ISocketMessageChannel channel)
        {
            if (channel.Id == galleryId) await HandleGalleryEdit(socketMessage, channel, message);
        }

        private async Task HandleGalleryEdit(SocketMessage socketMessage, ISocketMessageChannel galleryChannel, Cacheable<IMessage, ulong> message)
        {
            ITextChannel galleryTalkChannel = (ITextChannel)_client.GetChannel(galleryTalkId);
            if (galleryTalkChannel is null || galleryChannel is not ITextChannel) return;
            IReadOnlyCollection<IMessage>? messageList = await galleryTalkChannel.GetMessagesAsync(message.Id, Direction.After, 10).LastOrDefaultAsync();
            // get all media urls of the message
            List<string> UrlList = new ();
            if (socketMessage.Content != null)
                UrlList = GetAllUrlFromString(socketMessage.Content);
            foreach (var attachment in socketMessage.Attachments) UrlList.Add(attachment.Url);
            // Delete the message if it's null
            if ((UrlList.Count == 0)&&(socketMessage.Content!=null))
            {

                Console.WriteLine($"deleted {socketMessage.Attachments.Count} attachment and {UrlList.Count} URLs");
                Console.WriteLine($"deleted {socketMessage.Content}");
                await socketMessage.DeleteAsync();
                await galleryTalkChannel.SendMessageAsync(
                    $"{socketMessage.Author.Username} do not edit messages so it doesn't return any media!",
                    embed: PostEmbedText(socketMessage.Author.Username, socketMessage.Author.GetAvatarUrl(), "Deleted message content:", socketMessage.Content));
                return;
            }

            IMessage originalMessage = await galleryChannel.GetMessageAsync(socketMessage.Id);
            if (messageList is null)
            {
                return;
            }

            foreach (var item in messageList.Reverse())
            {
                // only tests message with the bot
                if (item.Author.IsBot == false) continue;
                //if no url in message return
                if (item.Content.Contains(socketMessage.Id.ToString()))
                {
                    if (socketMessage.Content is null || item is not IUserMessage userMessageToEdit) continue;
                    string messageStringContent = Regex.Replace(socketMessage.Content.Replace("|", ""), @"http[^\s]+", "");
                    string messageContent = $"{messageStringContent}\nUrl link: {GetAllUrlFromString(socketMessage.Content).First()}\nDiscord link: https://discord.com/channels/{serverId}/{galleryId}/{message.Id}";
                    await userMessageToEdit.ModifyAsync(editMessage => editMessage.Content= messageContent);
                    break;
                }
                // if no embed return
                if (item.Embeds.Count == 0) continue;
                //test if the embed contains 
                if (item.Embeds.First().Description.Contains(socketMessage.Id.ToString()))
                {
                    if (item is not IUserMessage userMessageToEdit) continue;
                    await userMessageToEdit.ModifyAsync(editMessage => editMessage.Embed = EditEmbed(originalMessage, userMessageToEdit));
                }
            }
        }

        private static Embed EditEmbed(IMessage originalMessage, IUserMessage userMessageToEdit)
        {
            string cleanDescription = Regex.Replace(originalMessage.Content, @"http[^\s]+", "");
            Console.WriteLine($"Number of reaction {originalMessage.Reactions.Count}");
            foreach (var emoteItem in originalMessage.Reactions)
            {
                // For basic Emojis
                if (emoteItem.Key is Emoji)
                {
                    cleanDescription += $"\n{emoteItem.Key}x{emoteItem.Value.ReactionCount} ";
                }
                //for Custom Emojis.
                else
                {
                    Emote customeEmoji = (Emote)emoteItem.Key;
                    if (customeEmoji.Animated)
                        cleanDescription += $"\n<a:{emoteItem.Key.Name}:{customeEmoji.Id}> x {emoteItem.Value.ReactionCount}";
                    else
                        cleanDescription += $"\n<:{emoteItem.Key.Name}:{customeEmoji.Id}> x {emoteItem.Value.ReactionCount}";
                }
            }
            EmbedImage? image = userMessageToEdit.Embeds.First().Image;
            string url;
            if (image is not null)
            {
                EmbedImage value = image.Value;
                url = value.Url;
            } else
            {
                url = "https://miro.medium.com/v2/resize:fit:2000/format:webp/1*zwUCapFGHln9VKG-LSvWlw.jpeg";
            }
            return PostEmbedImage(
                originalMessage.Author.Username,
                originalMessage.Author.Id,
                cleanDescription,
                originalMessage.Author.GetAvatarUrl(),
                url,
                originalMessage.Id);
        }
    }
}