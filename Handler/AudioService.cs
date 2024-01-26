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
using System.Collections;

namespace blankyBot.Handler
{
    public sealed class AudioService
    {
        private readonly LavaNode<LavaPlayer<LavaTrack>, LavaTrack> _lavaNode;
        private readonly ILogger _logger;

        public AudioService(LavaNode<LavaPlayer<LavaTrack>, LavaTrack> lavaNode, ILoggerFactory loggerFactory)
        {
            _lavaNode = lavaNode;
            _logger = loggerFactory.CreateLogger<LavaNode<LavaPlayer<LavaTrack>, LavaTrack>>();
            _lavaNode.OnWebSocketClosed += OnWebSocketClosedAsync;
        }
        private Task OnWebSocketClosedAsync(WebSocketClosedEventArg arg)
        {
            _logger.LogCritical($"Log {arg.Code} {arg.Reason}");
            return Task.CompletedTask;
        }
    }
}
