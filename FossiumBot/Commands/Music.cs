using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using System.Text.RegularExpressions;
using DSharpPlus.Lavalink;
//using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FossiumBot.Commands
{
    public class Music : ApplicationCommandModule
    {
        //int ffmpegpid = 0;
        public JObject playlist = new JObject();

        [SlashCommand("play", "Play audio from a YouTube video")]
        public async Task PlayCommand(InteractionContext ctx, [Option("url", "YouTube video url")] string url)
        {
            Directory.CreateDirectory(@"Data/");
            Directory.CreateDirectory(@"Data/playback/");
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
            //string file = $"Data/playback/{ctx.Guild.Id}.json";
            string file = $"Data/playback/{ctx.Guild.Id}.lck";
            LavalinkLoadResult loadResult = null;
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var connection = node.GetGuildConnection(ctx.Guild);
            var vstat = ctx.Member?.VoiceState;
            var preparingembed = new DiscordEmbedBuilder
            {
                Title = $"Preparing for playing...",
                Color = new DiscordColor(0x0080FF)
            };
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(preparingembed));
            string urltype;
            string thumbnail = "";
            Match youtubematch = Regex.Match(url, @"(?:https?:\/{2})?(?:w{3}\.)?youtu(?:be)?\.(?:com|be)(?:\/watch\?v=|\/)([^\s&]+)");
            if (youtubematch.Success)
            {
                urltype = "YouTube";
                thumbnail = $"http://i3.ytimg.com/vi/{youtubematch.Groups[1].Value}/maxresdefault.jpg";
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
            if (urltype == "YouTube")
            { 
                loadResult = await node.Rest.GetTracksAsync(url, LavalinkSearchType.Youtube);
            }
            else if (urltype == "SoundCloud")
            {
                loadResult = await node.Rest.GetTracksAsync(url, LavalinkSearchType.SoundCloud);
            }
            LavalinkTrack track = loadResult.Tracks.First();
            if (!File.Exists(file))
            {
                playlist[$"{ctx.Guild.Id}"] = 
                    new JObject(
                        new JProperty("playlist",
                            new JObject {
                                new JProperty("1",
                                    new JObject(
                                        new JProperty("title", track.Title),
                                        new JProperty("urltype", urltype),
                                        new JProperty("url", url),
                                        new JProperty("time", track.Length),
                                        new JProperty("thumbnail", thumbnail)
                                    )
                                )
                            }
                        )
                    );
                File.Create(file).Dispose();
                //string playlistWrite = JsonConvert.SerializeObject(playlist, Formatting.Indented);
                //await File.WriteAllTextAsync(file, playlistWrite);
            }
            else
            {
                if (connection == null)
                {
                    File.Delete(file);
                    var lavalinkerror = new DiscordEmbedBuilder
                    {
                        Title = "Something went wrong at our end, please try again.",
                        Color = new DiscordColor(0xFF0000)
                    };
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(lavalinkerror));
                    return;
                }
                //string read = await File.ReadAllTextAsync(file);
                //JObject jsonData = JObject.Parse(read);
                playlist[$"{ctx.Guild.Id}"]["playlist"][$"{playlist[$"{ctx.Guild.Id}"]["playlist"].Count() + 1}"] =
                        new JObject(
                                new JProperty("title", track.Title),
                                new JProperty("urltype", urltype),
                                new JProperty("url", url),
                                new JProperty("time", track.Length),
                                new JProperty("thumbnail", thumbnail)
                        );
                //string playlistAdd = JsonConvert.SerializeObject(jsonData, Formatting.Indented);
                //await File.WriteAllTextAsync(file, playlistAdd);
                var newEmbed = new DiscordEmbedBuilder
                {
                    Title = "Added to queue...",
                    Description = $"{track.Title}",
                    Color = new DiscordColor(0x2ECC70)
                };
                if (urltype == "YouTube")
                {
                    newEmbed.WithThumbnail(thumbnail);
                }
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(newEmbed));
                return;
            }
            await node.ConnectAsync(vstat.Channel);
            connection = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
            int lastplaybackIndex = 0;
            while (connection != null && File.Exists(file))
            {
                //string getPlaylist = await File.ReadAllTextAsync(file);
                //JObject playlistCurrent = JObject.Parse(getPlaylist);
                //JObject playlistCurrent = playlist;
                if (lastplaybackIndex == 0)
                {
                    for (int i = 1; i <= playlist[$"{ctx.Guild.Id}"]["playlist"].Count(); i++)
                    {
                        //getPlaylist = await File.ReadAllTextAsync(file);
                        //playlistCurrent = JObject.Parse(getPlaylist);
                        url = playlist[$"{ctx.Guild.Id}"]["playlist"][$"{i}"]["url"].ToString();
                        urltype = playlist[$"{ctx.Guild.Id}"]["playlist"][$"{i}"]["urltype"].ToString();
                        if (urltype == "YouTube")
                        {
                            loadResult = await node.Rest.GetTracksAsync(url, LavalinkSearchType.Youtube);
                        }
                        else if (urltype == "SoundCloud")
                        {
                            loadResult = await node.Rest.GetTracksAsync(url, LavalinkSearchType.SoundCloud);
                        }
                        track = loadResult.Tracks.First();
                        await connection.PlayAsync(track);
                        var playingembed = new DiscordEmbedBuilder
                        {
                            Title = "Now playing",
                            Description = $"{track.Title}",
                            Color = new DiscordColor(0x2ECC70)
                        };
                        if (urltype == "YouTube")
                        {
                            playingembed.WithThumbnail((string)playlist[$"{ctx.Guild.Id}"]["playlist"][$"{i}"]["thumbnail"]);
                        }
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(playingembed));
                        await Task.Delay(TimeSpan.FromMilliseconds(track.Length.TotalMilliseconds));
                        //getPlaylist = await File.ReadAllTextAsync(file);
                        //playlistCurrent = JObject.Parse(getPlaylist);
                        if (playlist[$"{ctx.Guild.Id}"]["playlist"][$"{i+1}"] == null)
                        {
                            await connection.DisconnectAsync();
                            if (File.Exists(file))
                            {
                                File.Delete(file);
                            }
                            break;
                        }
                        lastplaybackIndex = (int) i;
                    }
                }
                else
                {
                    //getPlaylist = await File.ReadAllTextAsync(file);
                    //playlistCurrent = playlist;
                    url = playlist[$"{ctx.Guild.Id}"]["playlist"][$"{lastplaybackIndex}"]["url"].ToString();
                    urltype = playlist[$"{ctx.Guild.Id}"]["playlist"][$"{lastplaybackIndex}"]["urltype"].ToString();
                    if (urltype == "YouTube")
                    { 
                        loadResult = await node.Rest.GetTracksAsync(url, LavalinkSearchType.Youtube);
                    }
                    else if (urltype == "SoundCloud")
                    {
                        loadResult = await node.Rest.GetTracksAsync(url, LavalinkSearchType.SoundCloud);
                    }
                    track = loadResult.Tracks.First();
                    await connection.PlayAsync(track);
                    var playingembed = new DiscordEmbedBuilder
                    {
                        Title = "Now playing",
                        Description = $"{track.Title}",
                        Color = new DiscordColor(0x2ECC70)
                    };
                    if (urltype == "YouTube")
                    {
                        playingembed.WithThumbnail((string)playlist[$"{ctx.Guild.Id}"]["playlist"][$"{lastplaybackIndex}"]["thumbnail"]);
                    }
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(playingembed));
                    lastplaybackIndex += 1;
                    await Task.Delay(TimeSpan.FromMilliseconds(track.Length.TotalMilliseconds));
                    //getPlaylist = await File.ReadAllTextAsync(file);
                    //playlistCurrent = JObject.Parse(getPlaylist);
                    //playlistCurrent = playlist;
                    if (playlist[$"{ctx.Guild.Id}"]["playlist"][$"{lastplaybackIndex}"] == null)
                    {
                        //lastplaybackIndex = 0;
                        await connection.DisconnectAsync();
                        if (File.Exists(file))
                        {
                            File.Delete(file);
                        }
                        break;
                    }
                } 
            }
        }

        [SlashCommand("stop", "Stop playing and leave the voice channel")]
        public async Task StopCommand(InteractionContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var connection = node.GetGuildConnection(ctx.Guild);
            string file = $"Data/playback/{ctx.Guild.Id}.lck";
            if (File.Exists(file))
            {
                File.Delete(file);
            }
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
                var nothingplayingembed = new DiscordEmbedBuilder
                {
                    Title = "Nothing is playing",
                    Color = new DiscordColor(0xFFA500)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(nothingplayingembed));
                return;
            }
            await connection.DisconnectAsync();
            var embed = new DiscordEmbedBuilder
            {
                Title = $"Stopped playing and left the channel",
                Color = new DiscordColor(0x2ECC70)
            };
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
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

        [SlashCommand("playlist", "Show current playlist")]
        public async Task PlaylistCommand(InteractionContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var connection = node.GetGuildConnection(ctx.Member.Guild);
            Directory.CreateDirectory(@"Data/");
            Directory.CreateDirectory(@"Data/playback/");
            string file = $"Data/playback/{ctx.Guild.Id}.json";
            if (connection == null | !File.Exists(file))
            {
                var lavalinkerror = new DiscordEmbedBuilder
                {
                    Title = "Nothing is playing right now",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(lavalinkerror));
                return;
            }
            string read = await File.ReadAllTextAsync(file);
            JObject jsonData = JObject.Parse(read);
            var embed = new DiscordEmbedBuilder
            {
                Title = "Current Playlist:",
                Color = new DiscordColor(0x0080FF)
            };
            for (int i = 1; i <= jsonData["playlist"].Count(); i++)
            {
                if (jsonData["playlist"].Any())
                {
                    string title = (string)jsonData["playlist"][$"{i}"]["title"];
                    string length = (string)jsonData["playlist"][$"{i}"]["time"];
                    string thumbnail = (string)jsonData["playlist"][$"{i}"]["thumbnail"];
                    string urltype = (string)jsonData["playlist"][$"{i}"]["urltype"];
                    string url = (string)jsonData["playlist"][$"{i}"]["url"];
                    embed.AddField(title, $"Link: {url}\n{length}\n", false);
                    if (title == connection.CurrentState.CurrentTrack.Title)
                    {
                        embed.WithThumbnail(thumbnail);
                    }
                }
            }
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }
    }
}
