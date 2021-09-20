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
using YoutubeExplode;
using YoutubeExplode.Converter;

namespace FOSSDiscord.Commands
{
    public class Music : BaseCommandModule
    {
        [Command("play")]
        public async Task PlayCommand(CommandContext ctx, string url)
        {
            var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            var vstat = ctx.Member?.VoiceState;
            var chn = vstat.Channel;
            var vnext = ctx.Client.GetVoiceNext();
            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc != null)
            {
                vnc.Dispose();
            }
            vnc = await vnext.ConnectAsync(chn);

            var youtube = new YoutubeClient();
            var video = await youtube.Videos.GetAsync(url);

            var downloadembed = new DiscordEmbedBuilder
            {
                Title = $"Downloading `{video.Title}`...",
                Description = "This can take a while...",
                Color = new DiscordColor(0xFFA500)
            };
            var embedmsg = await ctx.RespondAsync(downloadembed);

            await youtube.Videos.DownloadAsync(
                url,
                $"music.mp3",
                o => o
                    .SetFormat("mp3") // override format
                    //.SetFFmpegPath("path/to/ffmpeg") // custom FFmpeg location
            );

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
                    Arguments = $@"-i ""music.mp3"" -ac 2 -f s16le -ar 48000 pipe:1 -loglevel quiet",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };
                var ffmpeg = Process.Start(psi);
                var ffout = ffmpeg.StandardOutput.BaseStream;

                var txStream = vnc.GetTransmitSink();
                await ffout.CopyToAsync(txStream);
                await txStream.FlushAsync();
                await vnc.WaitForPlaybackFinishAsync();
            }
            finally
            {
                await vnc.SendSpeakingAsync(false);
            }
        }
    }
}
