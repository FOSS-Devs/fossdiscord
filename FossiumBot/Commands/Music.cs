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
            Directory.CreateDirectory(@"Data/");
            Directory.CreateDirectory(@"Data/playback/");
            string file = $"Data/playback/{ctx.Guild.Id}.json";
            LavalinkLoadResult loadResult = null;
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var vstat = ctx.Member?.VoiceState;
            var preparingembed = new DiscordEmbedBuilder
            {
                Title = $"Preparing for playing...",
                Color = new DiscordColor(0xFFA500)
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
                JObject playlist =
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
                                ),
                            }
                        )
                    );
                string playlistWrite = JsonConvert.SerializeObject(playlist, Formatting.Indented);
                File.WriteAllText(file, playlistWrite);
            }
            else
            {
                string read = File.ReadAllText(file);
                JObject jsonData = JObject.Parse(read);
                jsonData["playlist"][$"{jsonData["playlist"].Count() + 1}"] = new JObject(new JProperty("title", track.Title), new JProperty("urltype", urltype), new JProperty("url", url), new JProperty("time", track.Length), new JProperty("thumbnail", thumbnail));
                string playlistAdd = JsonConvert.SerializeObject(jsonData, Formatting.Indented);
                File.WriteAllText(file, playlistAdd);
                var playingembed = new DiscordEmbedBuilder
                {
                    Title = "Added to queue...",
                    Description = $"{track.Title}",
                    Color = new DiscordColor(0x0080FF)
                };
                if (urltype == "YouTube")
                {
                    playingembed.WithThumbnail(thumbnail);
                }
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(playingembed));
                return;
            }
            await node.ConnectAsync(vstat.Channel);
            var connection = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
            int lastplaybackIndex = 0;
            while (connection != null && File.Exists(file))
            {
                string getPlaylist = File.ReadAllText(file);
                JObject playlistCurrent = JObject.Parse(getPlaylist);
                if (lastplaybackIndex == 0)
                {
                    for (int i = 1; i <= JObject.Parse(File.ReadAllText(file))["playlist"].Count(); i++)
                    {
                        getPlaylist = File.ReadAllText(file);
                        playlistCurrent = JObject.Parse(getPlaylist);
                        url = playlistCurrent["playlist"][$"{i}"]["url"].ToString();
                        urltype = playlistCurrent["playlist"][$"{i}"]["urltype"].ToString();
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
                            Color = new DiscordColor(0x0080FF)
                        };
                        if (urltype == "YouTube")
                        {
                            playingembed.WithThumbnail((string)playlistCurrent["playlist"][$"{i}"]["thumbnail"]);
                        }
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(playingembed));
                        DateTime nextTrackTime = DateTime.UtcNow.Add(track.Length);
                        while (DateTime.UtcNow < nextTrackTime)
                        {
                            Thread.Sleep(1000);
                        }

                        lastplaybackIndex = (int) i;
                    }
                }
                else
                {
                    getPlaylist = File.ReadAllText(file);
                    playlistCurrent = JObject.Parse(getPlaylist);
                    url = playlistCurrent["playlist"][$"{lastplaybackIndex}"]["url"].ToString();
                    urltype = playlistCurrent["playlist"][$"{lastplaybackIndex}"]["urltype"].ToString();
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
                        Color = new DiscordColor(0x0080FF)
                    };
                    if (urltype == "YouTube")
                    {
                        playingembed.WithThumbnail((string)playlistCurrent["playlist"][$"{lastplaybackIndex}"]["thumbnail"]);
                    }
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(playingembed));
                    DateTime nextTrackTime = DateTime.UtcNow.Add(track.Length);
                    lastplaybackIndex += 1;
                    if (playlistCurrent["playlist"][$"{lastplaybackIndex}"] == null)
                    {
                        lastplaybackIndex = 0;
                    }
                    while (DateTime.UtcNow < nextTrackTime)
                    {
                        Thread.Sleep(1000);
                    }
                } 
            }
            
            
            
            
            
                /*if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
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
                            Thread.Sleep(1000);
                        }
                        await connection.PlayAsync(track);
                    };
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
                }*/
        }

        [SlashCommand("stop", "Stop playing and leave the voice channel")]
        public async Task StopCommand(InteractionContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var connection = node.GetGuildConnection(ctx.Guild);
            string file = $"Data/playback/{ctx.Guild.Id}.json";
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
    }
}
