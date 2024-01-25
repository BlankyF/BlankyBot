using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Victoria;
using static blankyBot.PublicFunction;

namespace blankyBot.Commands
{
    public class TextCommands(ResourcesCommands resourcesCommands) : ModuleBase<SocketCommandContext>
    {
        private readonly ResourcesCommands ressources = resourcesCommands;

        [Command("femboy")]
        public async Task FemboyCommand() => await Context.Channel.SendMessageAsync(embed: ResourcesCommands.RandomCommand(Context.User, "femboy", 0));

        [Command("femboy")]
        public async Task FemboyCommand([Remainder] string param) => await Context.Channel.SendMessageAsync(embed: ResourcesCommands.RandomTextCommand(param, "femboy", 0));

        [Command("furry")]
        public async Task RealFurryCommand() => await Context.Channel.SendMessageAsync(embed: ResourcesCommands.RandomCommand(Context.User,  "furry", 2));

        [Command("furry")]
        public async Task RealFurryCommand([Remainder] string param) => await Context.Channel.SendMessageAsync(embed: ResourcesCommands.RandomTextCommand(param, "furry", 2));

        [Command("gay")]
        public async Task GayCommand() => await Context.Channel.SendMessageAsync(embed: ResourcesCommands.RandomCommand(Context.User, "gay", 1));

        [Command("gay")]
        public async Task GayCommand([Remainder] string param) => await Context.Channel.SendMessageAsync(embed: ResourcesCommands.RandomTextCommand(param, "gay", 1));

        [Command("help")]
        public async Task HelpCommand() => await ReplyAsync(embed: ressources.embedHelp.WithAuthor(Context.Client.CurrentUser).Build());

        [Command("roll")]
        public async Task RollCommand([Remainder] string param) => await Context.Channel.SendMessageAsync(embed: ResourcesCommands.RollCommand(param));

        [Command("ping")]
        public async Task Ping()
        {
            TimeSpan ping = TimeZoneInfo.ConvertTimeToUtc(DateTime.Now) - TimeZoneInfo.ConvertTimeToUtc(Context.Message.Timestamp.DateTime);
            await ReplyAsync($"Pong: {ping.TotalMilliseconds} ms");
        }

        [Command("pong")]
        public async Task Pong() => await ReplyAsync("Ping");

        // MUSIC BOT
        [Command("join")]
        public async Task JoinCommand() => await Context.Channel.SendMessageAsync(embed: await ressources.Join(Context.Guild.Id,Context.User,Context.Channel));

        [Command("leave")]
        public async Task LeaveCommand() => await Context.Channel.SendMessageAsync(embed: await ressources.Leave(Context.Guild.Id, Context.User));

        [Command("Play")]
        public async Task PlayCommand() => await Context.Channel.SendMessageAsync(embed: new EmbedBuilder().WithDescription($"Error : not enough argument.")
                    .WithColor(Color.Red)
                    .Build());
        [Command("Play")]
        public async Task PlayCommand([Remainder] string param) => await Context.Channel.SendMessageAsync(embed: await ressources.Play(Context.Guild.Id, Context.User, Context.Channel, param));


        [Command("Pause")]
        public async Task PauseCommand() => await Context.Channel.SendMessageAsync(embed: await ressources.Pause(Context.Guild.Id, Context.User));

        [Command("Resume")]
        public async Task ResumeCommand() => await Context.Channel.SendMessageAsync(embed: await ressources.Resume(Context.Guild.Id, Context.User));

        [Command("Stop")]
        public async Task StopAsync() => await Context.Channel.SendMessageAsync(embed: await ressources.Stop(Context.Guild.Id, Context.User));

        [Command("Skip")]
        public async Task SkipAsync() => await Context.Channel.SendMessageAsync(embed: await ressources.Skip(Context.Guild.Id, Context.User));

        [Command("NowPlaying"), Alias("Np")]
        public async Task NowPlayingAsync() => await Context.Channel.SendMessageAsync(embed: await ressources.NowPlaying(Context.Guild.Id, Context.User));
        [Command("queue")]
        public async Task QueueAsync()
        {
            await Context.Channel.SendMessageAsync(embed: await ressources.Queue(Context.Guild.Id, Context.User, 1));
        }
        [Command("queue")]
        public async Task QueueAsync([Remainder] string param)
        {
            bool isNumeric = Int32.TryParse(param, out int pageNumber);
            if (isNumeric)
            {
                if ( pageNumber > 0 )
                {
                    await Context.Channel.SendMessageAsync(embed: await ressources.Queue(Context.Guild.Id, Context.User, pageNumber));
                    return;
                }
            }
            await Context.Channel.SendMessageAsync(embed: await ressources.Queue(Context.Guild.Id, Context.User, 1));
        }
    }
}
