using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using blankyBot.Handler;
using Discord.Net;
using Newtonsoft.Json;
using blankyBot.Commands;
using Victoria.WebSocket;
using Victoria;
using Victoria.Enums;
using Victoria.Rest;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using Victoria.WebSocket.EventArgs;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;

namespace blankyBot
{
    public class Program
    {

        /*APP INIT */
        static void Main()
        {
            Console.WriteLine("Blanky application start");
            //starts the discord bot
            new Program().RunBotAsync().GetAwaiter().GetResult();
        }

        /*DISCORD BOT INIT*/
        private DiscordSocketClient? client;
        private CommandService? _commands;
        private IServiceProvider? _services;
        private LavaNode<LavaPlayer<LavaTrack>, LavaTrack>? _lavaNode;
        private ResourcesCommands? resourcesCommands;
        public ILogger<LavaNode<LavaPlayer<LavaTrack>, LavaTrack>>? logger;
        public LavaQueue<LavaTrack> queue = new();

        public async Task RunBotAsync()
        {
            Console.WriteLine("Blanky Bot bot start");
            client = new DiscordSocketClient();
            var config = new DiscordSocketConfig { GatewayIntents = GatewayIntents.All };
            client = new DiscordSocketClient(config);
            _commands = new CommandService();
            _services = new ServiceCollection()
                .AddLogging()
                .AddSingleton(client)
                .AddSingleton(_commands)
                .AddSingleton<LavaNode<LavaPlayer<LavaTrack>, LavaTrack>>()
                .AddLavaNode<LavaNode<LavaPlayer<LavaTrack>, LavaTrack>, LavaPlayer<LavaTrack>, LavaTrack>()
                .BuildServiceProvider();
            // Logging
            client.Log += Client_Log;


            // Start Discord bot
            // Let's hook the ready event for creating our commands in.
            client.Ready += OnReady;
            // Command init
            RegisterCommandsAsync();

            // Bot Authentification init
            string? token = Environment.GetEnvironmentVariable("TOKEN");
            await client.LoginAsync(TokenType.Bot, token);

            // Start Discord bot
            await client.StartAsync();
            await Task.Delay(-1);
        }
        public async Task OnReady()
        {
            if (_services is null )
            {
                Console.WriteLine("Major error, the service are empty");
                return;
            }
            await _services.UseLavaNodeAsync();
            _lavaNode = _services.GetRequiredService<LavaNode<LavaPlayer<LavaTrack>, LavaTrack>>();
            if (client is null || _lavaNode is null || _commands is null)
            {
                Console.WriteLine("Major error, the client are empty");
                return;
            }
            resourcesCommands = new ResourcesCommands(_lavaNode, queue);
            await client.SetStatusAsync(UserStatus.Online);
            await client.SetGameAsync("Blanky Bot 1.0");
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            if (!_lavaNode.IsConnected)
            {
                await _lavaNode.ConnectAsync();
                Console.WriteLine($"Node connection : {_lavaNode.IsConnected}");
            }
            SlashCommandBuilder helpCommand = new SlashCommandBuilder()
                .WithName("help")
                .WithDescription("Displays all the available commands of the bot.");
            SlashCommandBuilder pingCommand = new SlashCommandBuilder()
                .WithName("ping")
                .WithDescription("Test the latency of the bot.");
            SlashCommandBuilder rollCommand = new SlashCommandBuilder()
                .WithName("roll")
                .AddOption("options", ApplicationCommandOptionType.String, "The type of roll you want. Ex: 2d6", isRequired: true)
                .WithDescription("Rolls dices like in D&D.");
            SlashCommandBuilder furryCommand = new SlashCommandBuilder()
                .WithName("furry")
                .AddOption("user", ApplicationCommandOptionType.User, "The user you want to evalute.")
                .WithDescription("Scientifically calculates how much of a furry a user is.");
            SlashCommandBuilder femboyCommand = new SlashCommandBuilder()
                .WithName("femboy")
                .AddOption("user", ApplicationCommandOptionType.User, "The user you want to evalute.")
                .WithDescription("Scientifically calculates how much of a femboy a user is.");
            SlashCommandBuilder gayCommand = new SlashCommandBuilder()
                .WithName("gay")
                .AddOption("user", ApplicationCommandOptionType.User, "The user you want to evalute.")
                .WithDescription("Scientifically calculates how gay a user is.");
            /// MUSIC BOT
            SlashCommandBuilder joinCommand = new SlashCommandBuilder()
                .WithName("join")
                .WithDescription("Makes the music bot join your channel.");
            SlashCommandBuilder leaveCommand = new SlashCommandBuilder()
                .WithName("leave")
                .WithDescription("Makes the music bot leave your channel.");
            SlashCommandBuilder playCommand = new SlashCommandBuilder()
                .WithName("play")
                .AddOption("url", ApplicationCommandOptionType.String, "The music you want.", isRequired:true)
                .AddOption("shuffle", ApplicationCommandOptionType.Boolean, "Shuffle if it's a playlist you're adding to the queue.", isRequired: false)
                .WithDescription("Plays a song from YouTube, Spotify, etc.");
            SlashCommandBuilder pauseCommand = new SlashCommandBuilder()
                .WithName("pause")
                .WithDescription("Pauses the music bot.");
            SlashCommandBuilder resumeCommand = new SlashCommandBuilder()
                .WithName("resume")
                .WithDescription("Resumes the music bot if it's paused.");
            SlashCommandBuilder stopCommand = new SlashCommandBuilder()
                .WithName("stop")
                .WithDescription("Stop the music currently played by the bot.");
            SlashCommandBuilder skipCommand = new SlashCommandBuilder()
                .WithName("skip")
                .WithDescription("Skips the first enqueued music in the music bot.");
            SlashCommandBuilder nowPlayingCommand = new SlashCommandBuilder()
                .WithName("nowplaying")
                .WithDescription("Shows the music currently played by the music bot.");
            SlashCommandBuilder queueCommand = new SlashCommandBuilder()
                .WithName("queue")
                .AddOption("page", ApplicationCommandOptionType.Integer, "Page number.", isRequired: false)
                .WithDescription("Shows the queue of songs that are about to be played by the bot.");
            SlashCommandBuilder shuffleCommand = new SlashCommandBuilder()
                .WithName("shuffle")
                .WithDescription("Shuffle the queue. (won't affect the curretly played song)");
            try
            {
                await client.CreateGlobalApplicationCommandAsync(helpCommand.Build());
                await client.CreateGlobalApplicationCommandAsync(pingCommand.Build());
                await client.CreateGlobalApplicationCommandAsync(rollCommand.Build());
                await client.CreateGlobalApplicationCommandAsync(furryCommand.Build());
                await client.CreateGlobalApplicationCommandAsync(femboyCommand.Build());
                await client.CreateGlobalApplicationCommandAsync(gayCommand.Build());
                await client.CreateGlobalApplicationCommandAsync(joinCommand.Build());
                await client.CreateGlobalApplicationCommandAsync(leaveCommand.Build());
                await client.CreateGlobalApplicationCommandAsync(playCommand.Build());
                await client.CreateGlobalApplicationCommandAsync(pauseCommand.Build());
                await client.CreateGlobalApplicationCommandAsync(resumeCommand.Build());
                await client.CreateGlobalApplicationCommandAsync(stopCommand.Build());
                await client.CreateGlobalApplicationCommandAsync(skipCommand.Build());
                await client.CreateGlobalApplicationCommandAsync(nowPlayingCommand.Build());
                await client.CreateGlobalApplicationCommandAsync(queueCommand.Build());
                await client.CreateGlobalApplicationCommandAsync(shuffleCommand.Build());
            }
            catch (HttpException exception)
            {
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
                Console.WriteLine(json);
            }
            client.SlashCommandExecuted += SlashCommandHandler;
        }

        private async Task SlashCommandHandler(SocketSlashCommand command)
        {
            if (_lavaNode is null || client is null || resourcesCommands is null)
            {
                Console.WriteLine("Major error: the node or client or resourcesCommands are empty.");
                return;
            }
            SlashCommands slashCommands = new (resourcesCommands);
            switch (command.Data.Name)
            {
                case "help":
                    await slashCommands.HelpCommand(command);
                    break;
                case "ping":
                    await SlashCommands.PingCommand(command);
                    break;
                case "roll":
                    await SlashCommands.RollCommand(command);
                    break;
                case "furry":
                    await SlashCommands.FurryCommand(command);
                    break;
                case "femboy":
                    await SlashCommands.FemboyCommand(command);
                    break;
                case "gay":
                    await SlashCommands.GayCommand(command);
                    break;
                // MUSIC BOT
                case "join":
                    await slashCommands.JoinCommand(command);
                    break;
                case "leave":
                    await slashCommands.LeaveCommand(command);
                    break;
                case "play":
                    await slashCommands.PlayCommand(command);
                    break;
                case "pause":
                    await slashCommands.PauseCommand(command);
                    break;
                case "resume":
                    await slashCommands.ResumeCommand(command);
                    break;
                case "stop":
                    await slashCommands.StopCommand(command);
                    break;
                case "skip":
                    await slashCommands.SkipCommand(command);
                    break;
                case "nowplaying":
                    await slashCommands.NowPlaying(command);
                    break;
                case "queue":
                    await slashCommands.Queue(command);
                    break;
                case "shuffle":
                    await slashCommands.Shuffle(command);
                    break;
            }
        }

        // Logging
        private Task Client_Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        // Command init
        public void RegisterCommandsAsync()
        {
            if (client is null || _commands is null || _services is null || _lavaNode is null)
            {
                Console.WriteLine("Major error, the bot will not load due to a null client, command or services");
                return;
            }
            ReactionHandler reactionHandler = new(client);
            FiregatorHandler firegatorHandler = new(client);
            MessageEditedHandler editedHandler = new(client);
            MessageDeleteHandler deleteHandler = new(client);
            MessageAddedHandler messageHandler = new(client, _commands, _services);
            FireGatorTracker FireGator = new();
            client.ReactionAdded += reactionHandler.HandleReactionAsync;
            client.ReactionRemoved += reactionHandler.HandleReactionAsync;
            client.ReactionsCleared += reactionHandler.HandleReactionClearAsync;
            client.MessageReceived += messageHandler.HandleCommandAsync;
            client.MessageDeleted += deleteHandler.HandleDeleteAsync;
            client.MessageUpdated += editedHandler.HandleEditAsync;
            _lavaNode.OnTrackEnd += Autoplay;
            FireGator.Start();
            FireGator.OnThursday += firegatorHandler.HandleFiregatorAsync;
        }

        private async Task Autoplay(TrackEndEventArg endTrackArg)
        {
            if (client is null || _commands is null || _services is null || _lavaNode is null)
            {
                Console.WriteLine("Major error, the bot will not load due to a null client, command or services");
                return;
            }
            if (_lavaNode == null) { return; }
            ITextChannel textChannel = (ITextChannel)client.GetChannel(1199393888989892729);
            LavaPlayer<LavaTrack> player = await _lavaNode.GetPlayerAsync(endTrackArg.GuildId);
            if (queue.Count > 0)
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
                await player.PlayAsync(lavaNode: _lavaNode, lavaTrack: queue.First());
                queue.RemoveAt(0);
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
    }
}