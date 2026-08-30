using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static blankyBot.PublicFunction;

namespace blankyBot
{
    public class MessageAddedHandler
    {
        public MessageAddedHandler(DiscordSocketClient _client, CommandService _commands, IServiceProvider _services)
        {
            this._client = _client;
            this._commands = _commands;
            this._services = _services;
        }

        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;

        private readonly ulong altheaReadChannel = 1543370962727211141;


        private readonly ulong altheaSpeakChannel = 1543371692137644132;

        /*----------------------------*/
        /*  MESSAGE CONTENT HANDLER   */
        /*----------------------------*/

        // Handle each message recieved into the right command (if it exists)
        public async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(_client, message);

            // if the message is from bot then ignore
            if (message is null || message.Author.IsBot) return;

            // althea speaks
            if (context.Channel.Id == altheaReadChannel) await AltheaCommand(altheaSpeakChannel, context);

            // command prompt
            int argPos = 0;
            if (message.HasStringPrefix(prefix, ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _services);
                if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
            }
        }

        private async Task AltheaCommand(ulong speakChannelId, SocketCommandContext context)
        {
            ISocketMessageChannel speakChannel = (ISocketMessageChannel)await _client.GetChannelAsync(speakChannelId);
            string messageContent = context.Message.Content;
            List<string> urls = [];

            // get urls from text and remove them from the 
            MatchCollection ms = Regex.Matches(messageContent, @"(www.+|http.+)([\s]|$)");
            foreach (Match match in ms)
            {
                messageContent = messageContent.Replace(match.Value,"");
            }

            // get all the embeds pictures
            foreach (Embed? embed in context.Message.Embeds)
            {
                if (embed is not null)
                {
                    urls.Add(embed.Url);
                }
            }

            foreach (Attachment? attachment in context.Message.Attachments)
            {
                if (attachment is not null)
                {
                    urls.Add(attachment.Url);
                }
            }

            // makes the non url text sss
            messageContent = messageContent.Replace("s", "sss");
            if (messageContent is not null and not "")
            {
                await speakChannel.SendMessageAsync(messageContent);
            }
            // add the pictures after
            foreach (string url in urls)
            {
                await speakChannel.SendMessageAsync(url);
            }
        }
    }
}
