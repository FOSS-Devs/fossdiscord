using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VideoLibrary;
using System.Text.RegularExpressions;
using DSharpPlus.Lavalink;

namespace FossiumBot.Commands
{
    public class Music : ApplicationCommandModule
    {
        int ffmpegpid = 0;

        [SlashCommand("play", "Play audio from a YouTube video")]
        public async Task PlayCommand(InteractionContext ctx, [Option("url", "YouTube video url")] string url)
        {
            /*await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource);
            var vstat = ctx.Member?.VoiceState;
            if (vstat == null)
            {
                var errorembed = new DiscordEmbedBuilder
                {
                    Title = $"You are not in a voice channel",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(errorembed));
                return;
            }
            string videoid = String.Empty;
            Match match1 = Regex.Match(url, @"(?:https?:\/{2})?(?:w{3}\.)?youtu(?:be)?\.(?:com|be)(?:\/watch\?v=|\/)([^\s&]+)");
            if (match1.Success)
            {
                videoid = match1.Groups[1].Value;
            }
            else
            {
                var errorembed = new DiscordEmbedBuilder
                {
                    Title = $"That isn't a YouTube url",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(errorembed));
                return;
            }
            var chn = vstat.Channel;
            var vnext = ctx.Client.GetVoiceNext();
            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc != null)
            {
                vnc.Dispose();
            }
            vnc = await vnext.ConnectAsync(chn);
            var youTube = YouTube.Default;
            var video = youTube.GetVideo(url);
            var downloadembed = new DiscordEmbedBuilder
            {
                Title = $"Downloading `{video.Title}`...",
                Description = "This can take a while...",
                Color = new DiscordColor(0xFFA500)
            };
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(downloadembed));

            JObject data = new JObject(
                new JProperty("name", $"{video.Title}"),
                new JProperty("videoid", $"{videoid}")
                );

            string json = JsonConvert.SerializeObject(data);
            Directory.CreateDirectory(@"Music/");
            string path = @"Music/nowplaying.json";
            using (TextWriter tw = new StreamWriter(path))
            {
                tw.WriteLine(json);
            };
            Process ps = Process.GetProcessById(ffmpegpid);
            if (ffmpegpid != 0)
            {
                ps.Kill();
                ps.WaitForExit();
            }
            foreach (string file in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.mp3").Where(item => item.EndsWith(".mp3")))
            {
                File.Delete(file);
            }
            foreach (string file in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.mp4").Where(item => item.EndsWith(".mp4")))
            {
                File.Delete(file);
            }

            File.WriteAllBytes($"{video.Title}.mp4", video.GetBytes());

            var playingembed = new DiscordEmbedBuilder
            {
                Title = $"Playing `{video.Title}`",
                Color = new DiscordColor(0x2ECC70)
            };

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(playingembed));*/

            //testing

            string urltype = string.Empty;

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
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(errorembed));
                    return;
                }
            }
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                var test = new DiscordEmbedBuilder
                {
                    Title = "You are not in a voice channel.",
                    //Description = "",
                    Color = new DiscordColor(0xFFA500)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(test));
                return;;
            }
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var vstat = ctx.Member?.VoiceState;
            var channel = vstat.Channel;
            await node.ConnectAsync(channel);
            var connection = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            dynamic loadResult = null;
            if (urltype == "YouTube")
            {
                loadResult = await node.Rest.GetTracksAsync(url, LavalinkSearchType.Youtube);
            }
            else if (urltype == "SoundCloud")
            {
                loadResult = await node.Rest.GetTracksAsync(url, LavalinkSearchType.SoundCloud);
            }
            
            var track = loadResult.Tracks.First();
            var musicEmbed = new DiscordEmbedBuilder
            {
                Title = $"Now playing {track.Title}",
                Description = $"{track.Uri}",
                Color = new DiscordColor(0xFFA500)
            };
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(musicEmbed));
            await connection.PlayAsync(track);
            Console.WriteLine(connection.CurrentState.PlaybackPosition);

            //var loadResult = await lava.Rest.GetTracksAsync(search, LavalinkSearchType.Youtube);
            //

            /*try
            {
                await vnc.SendSpeakingAsync(true);

                var psi = new ProcessStartInfo
                {
                    FileName = "ffmpeg.exe",
                    Arguments = $@"-i ""{video.Title}"".mp4 -ac 2 -f s16le -ar 48000 pipe:1 -loglevel quiet",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };
                var ffmpeg = Process.Start(psi);
                ffmpegpid = ffmpeg.Id;
                var ffout = ffmpeg.StandardOutput.BaseStream;

                var txStream = vnc.GetTransmitSink();
                await ffout.CopyToAsync(txStream);
                await txStream.FlushAsync();
                await vnc.WaitForPlaybackFinishAsync();
            }
            finally
            {
                await vnc.SendSpeakingAsync(false);

                foreach (string file in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.mp3").Where(item => item.EndsWith(".mp3")))
                {
                    File.Delete(file);
                }
                foreach (string file in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.mp4").Where(item => item.EndsWith(".mp4")))
                {
                    File.Delete(file);
                }
                Directory.CreateDirectory(@"Music/");
                if (File.Exists(@"Music/nowplaying.json"))
                {
                    File.Delete(@"Music/nowplaying.json");
                }
                var finishedembed = new DiscordEmbedBuilder
                {
                    Title = $"Finished playing {video.Title}",
                    Color = new DiscordColor(0x2ECC70)
                };
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(finishedembed));
            }*/
        }

        [SlashCommand("stop", "Stop playing and leave the voice channel")]
        public async Task StopCommand(InteractionContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNext();
            var vnc = vnext.GetConnection(ctx.Guild);

            if (vnc != null)
            {
                vnc.Dispose();
                Process ps = Process.GetProcessById(ffmpegpid);
                if (ffmpegpid != 0)
                {
                    ps.Kill();
                    ps.WaitForExit();
                }
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"Stopped playing and left the channel",
                    Color = new DiscordColor(0x2ECC70)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
                return;
            }
            else
            {
                var errorembed = new DiscordEmbedBuilder
                {
                    Title = $"Nothing is playing",
                    Color = new DiscordColor(0xFFA500)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(errorembed));
                return;
            }
        }

        [SlashCommand("nowplaying", "Show what's currently playing")]
        public async Task NowplayingCommand(InteractionContext ctx)
        {
            Directory.CreateDirectory(@"Music/");
            if (!File.Exists(@"Music/nowplaying.json"))
            {
                var nothingplayingembed = new DiscordEmbedBuilder
                {
                    Title = "Nothing is playing",
                    Color = new DiscordColor(0xFFA500)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(nothingplayingembed));
                return;
            }

            JObject jsonData = JObject.Parse(File.ReadAllText(@"Music/nowplaying.json"));

            var embed = new DiscordEmbedBuilder 
            { 
                Title = jsonData["name"].ToString(),
                Color = new DiscordColor(0x0080FF)
            };
            string videoid = jsonData["videoid"].ToString();
            embed.WithThumbnail($"http://i3.ytimg.com/vi/{videoid}/maxresdefault.jpg");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }
    }
}
