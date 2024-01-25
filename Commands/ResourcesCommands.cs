using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;
using Victoria;
using Victoria.Rest.Search;
using blankyBot;
using static blankyBot.PublicFunction;
using static System.Net.WebRequestMethods;

namespace blankyBot.Commands
{
    public class ResourcesCommands(LavaNode<LavaPlayer<LavaTrack>, LavaTrack> lavaNode)
    {
        private readonly LavaNode<LavaPlayer<LavaTrack>, LavaTrack> _lavaNode = lavaNode;
        public readonly EmbedBuilder embedHelp = new EmbedBuilder()
            .WithTitle("Commands list:")
            .AddField($"{prefix}femboy", "Display the seeded femboy percentage rating of the a user. Can accept one paramater.")
            .AddField($"{prefix}furry", "Display the seeded furry percentage rating of the a user. Can accept one paramater.")
            .AddField($"{prefix}gay", "Display the seeded gay percentage rating of the a user. Can accept one paramater.")
            .AddField($"{prefix}help", "Displays help related to the bot!")
            .AddField($"{prefix}roll", "Rolls the dice. Ex: /roll 2d6+2")
            .AddField($"{prefix}ping", "Replies with the ping of the bot")
            .WithFooter(footer => footer.Text = "Page 1 out of 1.")
            .WithColor(Color.Blue)
            .WithCurrentTimestamp();

        public static Embed RollCommand(string param)
        {

            EmbedBuilder embed = new();
            Embed embedResult;
            string paramFormatted = "";
            try
            {
                foreach (char item in param)
                {
                    if (item == '+' || item == '-' || item == '*')
                    {
                        paramFormatted += $" {item} ";
                    }
                    else
                    {
                        paramFormatted += item;
                    }
                }
                string[] listParams = paramFormatted.Split(' ');
                for (int indexParam = 0; indexParam < listParams.Length; indexParam++)
                {
                    if (listParams[indexParam].Contains('d'))
                    {
                        string[] diceParams = listParams[indexParam].Split("d");
                        if (diceParams[0] == "") diceParams[0] = "1";
                        if (Convert.ToInt32(diceParams[0]) > 100) throw new ArgumentException("You can roll a dice 100 max");
                        if (Convert.ToInt32(diceParams[0]) < 1 || Convert.ToInt32(diceParams[0]) > 100) throw new ArgumentException("The dice can only have 1 to 100 faces max!");
                        Random getrandom = new();
                        string result = "(";
                        for (int i = 0; i < Convert.ToInt32(diceParams[0]); i++)
                            result += $"{getrandom.Next(1, Convert.ToInt32(diceParams[1]))}+";
                        result = result.Remove(result.Length - 1);
                        listParams[indexParam] = $"{result})";
                    }
                }
                string resultCmd = "";
                foreach (string item in listParams)
                {
                    resultCmd += $"{item}";
                }
                object? value = new DataTable().Compute(resultCmd, null);
                embedResult = embed.WithDescription($"Your roll is {value}.\n{resultCmd}")
                    .WithColor(Color.Purple)
                    .Build();
            }
            catch (Exception)
            {
                embedResult =  embed.WithDescription($"Error ! Bad formatting")
                    .WithColor(Color.Purple)
                    .Build();
            }
            return embedResult;
        }

        public static Embed RandomCommand(SocketUser user, string name, ulong randomModifier)
        {
            Random rnd;
            // id function
            rnd = new Random((int)(user.Id + randomModifier % 10000000));
            EmbedBuilder embed = PostEmbedPercent(user.Username, rnd.Next(101), name);
            return embed.WithAuthor(user.Username, user.GetAvatarUrl()).Build();
        }

        public static Embed RandomTextCommand(string user, string name, int randomModifier)
        {
            Random rnd;
            rnd = new Random(randomModifier);
            return PostEmbedPercent(user, rnd.Next(101), name).Build();
        }

        // MUSIC BOT RESSOURCES

        public async Task<Embed> Join(ulong guildId, SocketUser user, ISocketMessageChannel channel)
        {
            EmbedBuilder embed = new();
            IVoiceState? voiceState = user as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                return embed.WithDescription("Error : You must be connected to a voice channel!")
                    .WithColor(Color.Red)
                    .Build();
            }
            try
            {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, channel as ITextChannel);
                return embed.WithDescription($"Joined the channel : {voiceState.VoiceChannel.Name}!")
                    .WithColor(Color.Purple)
                    .WithAuthor(user)
                    .WithTitle("Bot joined the channel!")
                    .Build();
            }
            catch (Exception exception)
            {
                return embed.WithDescription($"Error : {exception}!")
                    .WithColor(Color.Red)
                    .Build();
            }

        }
        public async Task<Embed> Leave(ulong guildId, SocketUser user)
        {
            EmbedBuilder embed = new();
            IVoiceChannel voiceChannel = ((IVoiceState)user).VoiceChannel;
            if (voiceChannel == null)
            {
                return embed.WithDescription("Error : Not sure which voice channel to disconnect from.")
                    .WithColor(Color.Red)
                    .Build();
            }

            try
            {
                await _lavaNode.LeaveAsync(voiceChannel);
                return embed.WithDescription($"I've left the channel \"{voiceChannel.Name}\"!")
                    .WithColor(Color.Purple)
                    .WithAuthor(user)
                    .WithTitle("Bot left!")
                    .Build();
            }
            catch (Exception exception)
            {
                return embed.WithDescription($"Error : {exception}!")
                    .WithColor(Color.Red)
                    .Build();
            }
        }
        public async Task<Embed> Play(ulong guildId, SocketUser user, ISocketMessageChannel channel, string searchQuery, bool isShuffle = false)
        {
            EmbedBuilder embed = new();
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                return embed.WithDescription($"Error : Please provide search terms.")
                    .WithColor(Color.Red)
                    .Build();
            }

            LavaPlayer<LavaTrack>? player = await _lavaNode.TryGetPlayerAsync(guildId);
            if (player == null)
            {
                IVoiceState? voiceState = user as IVoiceState;
                if (voiceState?.VoiceChannel == null)
                {
                    return embed.WithDescription($"Error : You must be connected to a voice channel!")
                        .WithColor(Color.Red)
                        .Build();
                }

                try
                {
                    player = await _lavaNode.JoinAsync(voiceState.VoiceChannel, channel as ITextChannel);
                    await channel.SendMessageAsync(embed: embed.WithDescription($"Joined {voiceState.VoiceChannel.Name}!")
                        .WithColor(Color.Purple)
                        .Build());
                }
                catch (Exception exception)
                {
                    return embed.WithDescription($"Error : {exception.Message}")
                        .WithColor(Color.Red)
                        .Build();
                }
            }

            SearchResponse searchResponse = await _lavaNode.LoadTrackAsync(searchQuery);
            if (searchResponse.Type is SearchType.Empty or SearchType.Error)
            {
                return embed.WithDescription($"Error : I wasn't able to find anything for `{searchQuery}`.")
                    .WithColor(Color.Red)
                    .Build();
            }

            LavaTrack? track = searchResponse.Tracks.FirstOrDefault();
            if (track is null)
            {
                return embed.WithDescription($"Error : track is null.")
                    .WithColor(Color.Red)
                    .Build();
            }
            await player.PlayAsync(lavaNode: _lavaNode, lavaTrack: track);
            return embed.WithDescription($"{user.Username} enqueued :\n[{track?.Title}]({track?.Url})")
                    .WithColor(Color.Purple)
                    .WithAuthor(user: user)
                    .WithTitle("Music Added!")
                    .WithImageUrl(imageUrl: track.Artwork)
                    .Build(); ;
        }
        public async Task<Embed> Pause(ulong guildId, SocketUser user)
        {
            EmbedBuilder embed = new();
            LavaPlayer<LavaTrack> player = await _lavaNode.TryGetPlayerAsync(guildId);
            if (player == null)
            {
                return embed.WithDescription("Error : I'm not connected to a voice channel.")
                    .WithColor(Color.Red)
                    .WithAuthor(user)
                    .Build();
            }

            if (player.IsPaused)
            {
                return embed.WithDescription("Error : I cannot pause when I'm not playing anything!")
                    .WithColor(Color.Red)
                    .WithAuthor(user)
                    .Build();
            }

            try
            {
                await player.PauseAsync(_lavaNode);
                return embed.WithDescription($"Paused: {player.Track.Title}")
                    .WithColor(Color.Purple)
                    .WithAuthor(user)
                    .WithTitle("Music paused!")
                    .Build();
            }
            catch (Exception exception)
            {
                return embed.WithDescription($"Error : {exception.Message}")
                    .WithColor(Color.Red)
                    .Build();
            }
        }
        public async Task<Embed> Resume(ulong guildId, SocketUser user)
        {
            EmbedBuilder embed = new();
            LavaPlayer<LavaTrack> player = await _lavaNode.TryGetPlayerAsync(guildId);
            if (player == null)
            {
                return embed.WithDescription("Error : I'm not connected to a voice channel.")
                    .WithColor(Color.Red)
                    .WithAuthor(user)
                    .Build();
            }

            if (!player.IsPaused)
            {
                return embed.WithDescription("Error : I cannot resume when I'm not paused!")
                    .WithColor(Color.Red)
                    .WithAuthor(user)
                    .Build();
            }

            try
            {
                await player.ResumeAsync(_lavaNode);
                return embed.WithDescription($"Resume: {player.Track.Title}")
                    .WithColor(Color.Purple)
                    .WithAuthor(user)
                    .WithTitle("Music resumed!")
                    .Build();
            }
            catch (Exception exception)
            {
                return embed.WithDescription($"Error : {exception.Message}")
                    .WithColor(Color.Red)
                    .Build();
            }
        }
        public async Task<Embed> Stop(ulong guildId, SocketUser user)
        {
            EmbedBuilder embed = new();
            LavaPlayer<LavaTrack> player = await _lavaNode.TryGetPlayerAsync(guildId);
            if (player == null)
            {
                return embed.WithDescription("Error : I'm not connected to a voice channel.")
                    .WithColor(Color.Red)
                    .WithAuthor(user)
                    .Build();
            }

            if (player.IsPaused)
            {
                return embed.WithDescription("Error : I cannot stop when I'm not playing anything!")
                    .WithColor(Color.Red)
                    .WithAuthor(user)
                    .Build();
            }

            try
            {
                await player.StopAsync(_lavaNode);
                return embed.WithDescription($"Paused: {player.Track.Title}")
                    .WithColor(Color.Purple)
                    .WithAuthor(user)
                    .WithTitle("Music stopped!")
                    .Build();
            }
            catch (Exception exception)
            {
                return embed.WithDescription($"Error : {exception.Message}")
                    .WithColor(Color.Red)
                    .Build();
            }
        }
        public async Task<Embed> Skip(ulong guildId, SocketUser user)
        {
            EmbedBuilder embed = new();

            LavaPlayer<LavaTrack> player = await _lavaNode.TryGetPlayerAsync(guildId);
            if (player == null)
            {
                return embed.WithDescription("Error : I'm not connected to a voice channel.")
                    .WithColor(Color.Red)
                    .WithAuthor(user)
                    .Build();
            }

            if (player.IsPaused)
            {
                return embed.WithDescription($"Error : I can't skip when nothing is playing.")
                    .WithColor(Color.Red)
                    .Build();
            }

            try
            {
                string skippedTrack = player.Track.Title;
                if (player.Queue.Count > 0 && player.Queue.First() is not null)
                {
                    Embed embedResult = embed.WithDescription(description: $"Skipped: {skippedTrack}\nNow Playing: {player.Queue.First().Title}")
                        .WithColor(Color.Purple)
                        .WithAuthor(user)
                        .WithTitle("Song skipped!")
                        .Build();
                    await player.SeekAsync(_lavaNode, player.Track.Duration);
                    return embedResult;
                } 
                else
                {
                    await player.StopAsync(_lavaNode);
                    return embed.WithDescription($"Skipped: {skippedTrack}")
                        .WithColor(Color.Purple)
                        .WithAuthor(user)
                        .WithTitle("Song skipped!")
                        .Build();
                }
            }
            catch (Exception exception)
            {
                return embed.WithDescription($"Error : {exception.Message}")
                    .WithColor(Color.Red)
                    .Build();
            }
        }
        public async Task<Embed> NowPlaying(ulong guildId, SocketUser user)
        {
            EmbedBuilder embed = new();
            LavaPlayer<LavaTrack> player = await _lavaNode.TryGetPlayerAsync(guildId);
            if (player == null)
            {
                return embed.WithDescription("Error : I'm not connected to a voice channel.")
                    .WithColor(Color.Red)
                    .WithAuthor(user)
                    .Build();
            }

            if (player.IsPaused)
            {
                return embed.WithDescription($"Error : I'm not playing any tracks.")
                    .WithColor(Color.Red)
                    .Build();
            }

            LavaTrack track = player.Track;
            string time = $"{track.Position:mm\\:ss}/{track.Duration:mm\\:ss}";
            return embed.WithAuthor(track.Author, user.GetAvatarUrl(), track.Url)
                .WithTitle($"Now Playing: ")
                .WithDescription($"[{track.Title}]({track.Url})")
                .WithImageUrl(track.Artwork)
                .WithFooter(time)
                .Build();
        }

        public async Task<Embed> Shuffle(ulong guildId, SocketUser user)
        {
            EmbedBuilder embed = new();

            LavaPlayer<LavaTrack> player = await _lavaNode.TryGetPlayerAsync(guildId);
            if (player == null)
            {
                return embed.WithDescription("Error : I'm not connected to a voice channel.")
                    .WithColor(Color.Red)
                    .WithAuthor(user)
                    .Build();
            }

            if (player.Queue.Count == 0)
            {
                return embed.WithDescription($"Error : Queue is empty.")
                    .WithColor(Color.Red)
                    .Build();
            }
            player.Queue.Shuffle();

            string result = "";
            for (int trackNumber = 0 ; trackNumber < player.Queue.Count; trackNumber++)
            {
                if (trackNumber >= player.Queue.Count)
                {
                    break;
                }
                string title = player.Queue.ElementAt(trackNumber).Title;
                if (title.Length > 80)
                {
                    title = $"{title[..77]}...";
                }
                result += $"\n{trackNumber + 1} : [{title}]({player.Queue.ElementAt(trackNumber).Url}) [{player.Queue.ElementAt(trackNumber).Duration:mm\\:ss}]";
            }
            return embed.WithAuthor(user)
                .WithTitle($"Queue shuffled! ")
                .WithDescription(result)
                .Build();
        }
        public async Task<Embed> Queue(ulong guildId, SocketUser user, int pageNumber)
        {
            EmbedBuilder embed = new();
            LavaPlayer<LavaTrack> player = await _lavaNode.TryGetPlayerAsync(guildId);
            if (player == null)
            {
                return embed.WithDescription("Error : I'm not connected to a voice channel.")
                    .WithColor(Color.Red)
                    .WithAuthor(user)
                    .Build();
            }
            if (player.Queue.Count == 0)
            {
                return embed.WithDescription($"Error : Queue is empty.")
                    .WithColor(Color.Red)
                    .Build();
            }
            string result = "";
            for (int trackNumber = 0 + ( ( pageNumber - 1 ) * 10 ); trackNumber < 10 + ( ( pageNumber - 1 ) * 10); trackNumber++)
            {
                if ( trackNumber >= player.Queue.Count )
                {
                    break;
                }
                string title = player.Queue.ElementAt(trackNumber).Title;
                if (title.Length > 60)
                {
                    title = $"{title[..57]}...";
                }
                result += $"\n{ trackNumber + 1 } : [{ title }]({ player.Queue.ElementAt(trackNumber).Url }) [{player.Queue.ElementAt(trackNumber).Duration:mm\\:ss}]";
            }
            if ( result == "" )
            {
                return embed.WithDescription($"Error : There is nothing on this page!.")
                    .WithColor(Color.Red)
                    .Build();
            }
            int PageTotal = player.Queue.Count / 10;
            if(player.Queue.Count % 10 != 0)
            {
                PageTotal++;
            }
            return embed.WithAuthor(user)
                .WithTitle($"List of queued music :")
                .WithDescription(result)
                .WithFooter($"Page { pageNumber }/{ PageTotal }. { player.Queue.Count } tracks enqueued")
                .Build();
        }

        public static TimeSpan StripMilliseconds(TimeSpan time)
        {
            return new TimeSpan(time.Days, time.Hours, time.Minutes, time.Seconds);
        }
    }
}
