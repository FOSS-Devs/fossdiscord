using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VideoLibrary;
using System.Text.RegularExpressions;

namespace FOSSDiscord.Commands
{
    public class Music : BaseCommandModule
    {
        int ffmpegpid = 0;

        [Command("play"), Cooldown(1, 5, CooldownBucketType.User)]
        public async Task PlayCommand(CommandContext ctx, string url)
        {
            var vstat = ctx.Member?.VoiceState;
            if (vstat == null)
            {
                var errorembed = new DiscordEmbedBuilder
                {
                    Title = $"You are not in a voice channel",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.RespondAsync(errorembed);
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
                await ctx.RespondAsync(errorembed);
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
            var embedmsg = await ctx.RespondAsync(downloadembed);

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

            await embedmsg.ModifyAsync(null, (DiscordEmbed)playingembed);

            try
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
            }
        }

        [Command("stop"), Aliases("leave")]
        public async Task StopCommand(CommandContext ctx)
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
                await ctx.RespondAsync(embed);
                return;
            }
            else
            {
                var errorembed = new DiscordEmbedBuilder
                {
                    Title = $"Nothing is playing",
                    Color = new DiscordColor(0xFFA500)
                };
                await ctx.RespondAsync(errorembed);
                return;
            }
        }

        [Command("nowplaying"), Aliases("np")]
        public async Task NowplayingCommand(CommandContext ctx)
        {
            Directory.CreateDirectory(@"Music/");
            if (!File.Exists(@"Music/nowplaying.json"))
            {
                var nothingplayingembed = new DiscordEmbedBuilder
                {
                    Title = "Nothing is playing",
                    Color = new DiscordColor(0xFFA500)
                };
                await ctx.RespondAsync(nothingplayingembed);
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
            await ctx.RespondAsync(embed);
        }
    }
}
