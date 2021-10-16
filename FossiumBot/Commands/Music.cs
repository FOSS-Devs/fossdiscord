using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using System.Text.RegularExpressions;
using DSharpPlus.Lavalink;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FossiumBot.Commands
{
    public class Music : ApplicationCommandModule
    {
        //int ffmpegpid = 0;

        [SlashCommand("play", "Play audio from a YouTube video")]
        public async Task PlayCommand(InteractionContext ctx, [Option("url", "YouTube video url")] string url)
        {
            Directory.CreateDirectory(@"Settings/");
            Directory.CreateDirectory(@"Settings/playback/");
            string file = $"Settings/playback/{ctx.Guild.Id}.json";
            var preparingembed = new DiscordEmbedBuilder
            {
                Title = $"Preparing for playing...",
                Color = new DiscordColor(0xFFA500)
            };
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(preparingembed));

            string urltype;
            Match youtubematch = Regex.Match(url, @"(?:https?:\/{2})?(?:w{3}\.)?youtu(?:be)?\.(?:com|be)(?:\/watch\?v=|\/)([^\s&]+)");
            if (youtubematch.Success)
            {
                urltype = "YouTube";
            }
            else
            {
                Match soundcloudmatch = Regex.Match(url, @"^(https?:\/\/)?(www.)?(m\.)?soundcloud\.com\/[\w\-\.]+(\/)+[\w\-\.]+/?$");
                if (soundcloudmatch.Success)
                {
                    urltype = "SoundCloud";
                }
                else
                {
                    var errorembed = new DiscordEmbedBuilder
                    {
                        Title = $"That isn't a YouTube or SoundCloud url",
                        Color = new DiscordColor(0xFF0000)
                    };
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(errorembed));
                    return;
                }
            }
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                var voicestatenull = new DiscordEmbedBuilder
                {
                    Title = "You are not in a voice channel.",
                    //Description = "",
                    Color = new DiscordColor(0xFFA500)
                };
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(voicestatenull));
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var vstat = ctx.Member?.VoiceState;
            await node.ConnectAsync(vstat.Channel);
            var connection = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
            try {
                if (connection == null)
                {
                    var lavalinkerror = new DiscordEmbedBuilder
                    {
                        Title = "Something went wrong while trying to connect to Lavalink",
                        Color = new DiscordColor(0xFF0000)
                    };
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(lavalinkerror));
                    return;
                }
                LavalinkLoadResult loadResult = null;
                if (urltype == "YouTube")
                {
                    loadResult = await node.Rest.GetTracksAsync(url, LavalinkSearchType.Youtube);
                }
                else if (urltype == "SoundCloud")
                {
                    loadResult = await node.Rest.GetTracksAsync(url, LavalinkSearchType.SoundCloud);
                }
                var track = loadResult.Tracks.First();
                if (connection.CurrentState.CurrentTrack == null)
                {
                    DateTime nextTrackTime = DateTime.UtcNow;
                    String parseNextTrackTime = nextTrackTime.Add(track.Length).ToString();
                    JObject trackTime = new JObject(new JProperty("time", parseNextTrackTime));
                    if(File.Exists(file))
                    {
                        File.Delete(file);
                    }
                    string timeData = JsonConvert.SerializeObject(trackTime, Formatting.Indented);
                    File.WriteAllText(file, timeData);
                    var playingembed = new DiscordEmbedBuilder
                    {
                        Title = $"Now playing {track.Title}",
                        Description = $"{track.Uri}",
                        Color = new DiscordColor(0x0080FF)
                    };
                    if (urltype == "YouTube")
                    {
                        playingembed.WithThumbnail($"http://i3.ytimg.com/vi/{youtubematch.Groups[1].Value}/maxresdefault.jpg");
                    }
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(playingembed));
                    await connection.PlayAsync(track);
                }
                else
                {
                    String fileData = File.ReadAllText(file);
                    JObject jsonData = JObject.Parse(fileData);
                    DateTime thisTrack = DateTime.Parse((string)jsonData["time"]);
                    jsonData["time"] = thisTrack.Add(track.Length);
                    string timeData = JsonConvert.SerializeObject(jsonData, Formatting.Indented);
                    File.WriteAllText(file, timeData);
                    var playingembed = new DiscordEmbedBuilder
                    {
                        Title = "Added to queue...",
                        Description = $"{track.Title}",
                        Color = new DiscordColor(0x0080FF)
                    };
                    if (urltype == "YouTube")
                    {
                        playingembed.WithThumbnail($"http://i3.ytimg.com/vi/{youtubematch.Groups[1].Value}/maxresdefault.jpg");
                    }
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(playingembed));
                    while (DateTime.UtcNow < thisTrack)
                    {
                        //Console.WriteLine($"Now: {DateTime.UtcNow}\nTarget: {thisTrack}");
                        Thread.Sleep(1000);
                    }
                    //int trackLength = (int)connection.CurrentState.CurrentTrack.Length.TotalMilliseconds;
                    //int currentPosition = (int)connection.CurrentState.PlaybackPosition.Milliseconds;
                    //int threadSleepTimer = trackLength - currentPosition;
                    //Thread.Sleep(threadSleepTimer - 1000);
                    await connection.PlayAsync(track);
                    /*var playingembed = new DiscordEmbedBuilder
                    {
                        Title = "Testing",
                        Description = $"{threadSleepTimer}",
                        Color = new DiscordColor(0x0080FF)
                    };
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(playingembed));*/
                };
                /*else
                {
                    var alreadyplayingembed = new DiscordEmbedBuilder
                    {
                        Title = $"The bot is already in {connection.Channel.Mention} playing music",
                        Description = $"Current track: `{connection.CurrentState.CurrentTrack.Title}`",
                        Color = new DiscordColor(0xFFA500)
                    };
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(alreadyplayingembed));
                    return;
                };*/
            }
            catch (Exception ex)
            {
                var errorEM = new DiscordEmbedBuilder
                {
                    Title = "Something Went Wrong...",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(errorEM));
                await connection.DisconnectAsync();
                Console.WriteLine(ex);
                return;
            }
        }

        [SlashCommand("stop", "Stop playing and leave the voice channel")]
        public async Task StopCommand(InteractionContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var connection = node.GetGuildConnection(ctx.Guild);
            string file = $"Settings/playback/{ctx.Guild.Id}.json";
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                var voicestatenull = new DiscordEmbedBuilder
                {
                    Title = "You are not in a voice channel.",
                    Color = new DiscordColor(0xFFA500)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(voicestatenull));
                return;
            }
            if (connection == null)
            {
                var lavalinkerror = new DiscordEmbedBuilder
                {
                    Title = "Something went wrong while trying to connect to Lavalink",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(lavalinkerror));
                return;
            }
            if (connection.CurrentState.CurrentTrack == null)
            {
                var nothingplayingembed = new DiscordEmbedBuilder
                {
                    Title = "Nothing is playing",
                    Color = new DiscordColor(0xFFA500)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(nothingplayingembed));
                return;
            }
            await connection.DisconnectAsync();
            if(File.Exists(file))
            {
                File.Delete(file);
            }
            //await node.StopAsync();
            var embed = new DiscordEmbedBuilder
            {
                Title = $"Stopped playing and left the channel",
                Color = new DiscordColor(0x2ECC70)
            };
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
            return;
        }

        [SlashCommand("nowplaying", "Show what's currently playing")]
        public async Task NowplayingCommand(InteractionContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var connection = node.GetGuildConnection(ctx.Member.Guild);

            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                var voicestatenull = new DiscordEmbedBuilder
                {
                    Title = "You are not in a voice channel.",
                    //Description = "",
                    Color = new DiscordColor(0xFFA500)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(voicestatenull));
                return;
            }

            if (connection == null)
            {
                var lavalinkerror = new DiscordEmbedBuilder
                {
                    Title = "Something went wrong while trying to connect to Lavalink",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(lavalinkerror));
                return;
            }

            if (connection.CurrentState.CurrentTrack == null)
            {
                var nothingplayingembed = new DiscordEmbedBuilder
                {
                    Title = "Nothing is playing",
                    Color = new DiscordColor(0xFFA500)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(nothingplayingembed));
                return;
            }

            var embed = new DiscordEmbedBuilder
            {
                Title = connection.CurrentState.CurrentTrack.Title,
                Description = $"{connection.CurrentState.PlaybackPosition.ToString("mm\\:ss")}/{connection.CurrentState.CurrentTrack.Length.ToString("mm\\:ss")}",
                Color = new DiscordColor(0x0080FF)
            };
            Match youtubematch = Regex.Match(connection.CurrentState.CurrentTrack.Uri.ToString(), @"(?:https?:\/{2})?(?:w{3}\.)?youtu(?:be)?\.(?:com|be)(?:\/watch\?v=|\/)([^\s&]+)");
            if (youtubematch.Success)
            {
                embed.WithThumbnail($"http://i3.ytimg.com/vi/{youtubematch.Groups[1].Value}/maxresdefault.jpg");
            }
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }
    }
}
