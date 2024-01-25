using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Victoria.WebSocket.EventArgs;
using Victoria;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;

namespace blankyBot.Handler
{
    public sealed class AudioService
    {
        private readonly DiscordSocketClient _client;
        private readonly LavaNode<LavaPlayer<LavaTrack>, LavaTrack>? _lavaNode;
        private readonly ILogger _logger;

        public AudioService(LavaNode<LavaPlayer<LavaTrack>, LavaTrack> lavaNode, ILoggerFactory loggerFactory, DiscordSocketClient client)
        {
            _client = client;
            _lavaNode = lavaNode;
            _logger = loggerFactory.CreateLogger<LavaNode<LavaPlayer<LavaTrack>, LavaTrack>>();
            _lavaNode.OnTrackEnd += Autoplay;
            _lavaNode.OnWebSocketClosed += OnWebSocketClosedAsync;
        }

        private async Task Autoplay(TrackEndEventArg arg)
        {
            if (_lavaNode == null) { return; }

            ITextChannel textChannel = (ITextChannel)_client.GetChannel(1199393888989892729);
            LavaPlayer<LavaTrack> player = await _lavaNode.GetPlayerAsync(arg.GuildId);
            if (player.Queue.Count > 0)
            {
                // queues up the next track by skipping if the queue isn't empty
                await textChannel.SendMessageAsync(embed: new EmbedBuilder()
                    .WithTitle($"Finished playing {player.Track}.")
                    .WithColor(Color.Purple)
                    .Build());
                await textChannel.SendMessageAsync(embed: new EmbedBuilder()
                    .WithTitle($"Now playing {player.Queue.First().Title}.")
                    .WithColor(Color.Purple)
                    .Build());
                await player.SeekAsync(_lavaNode, player.Track.Duration);
            }
            else
            {
                // if playlist empty
                await textChannel.SendMessageAsync(embed: new EmbedBuilder()
                    .WithTitle($"Finished playing {player.Track}.")
                    .WithDescription($"This was the last track of the playlist.\nUse {PublicFunction.prefix}play or the slash command /play to queue up more songs!")
                    .WithColor(Color.Purple)
                    .Build());
            }
        }
        private Task OnWebSocketClosedAsync(WebSocketClosedEventArg arg)
        {
            _logger.LogCritical($"{arg.Code} {arg.Reason}");
            return Task.CompletedTask;
        }
    }
}
