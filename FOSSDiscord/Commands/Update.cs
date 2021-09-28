using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;

namespace FossiumBot.Commands
{
    public class Update : BaseCommandModule
    {
        [Command("updatecheck"), Aliases("checkupdate"), RequireOwner]
        public async Task UpdatecheckCommand(CommandContext ctx)
        {
            var checkingembed = new DiscordEmbedBuilder
            {
                Title = $"{ctx.Client.CurrentUser.Username} is checking for updates...",
                Color = new DiscordColor(0xFFA500)
            };
            var embedmsg = await ctx.RespondAsync(checkingembed);

            string content = String.Empty;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("FossiumBot", Program.localversion));
                content = await client.GetStringAsync("https://api.github.com/repos/SKBotNL/FossiumBot-new/releases/latest");
            }
            JObject jsonData = JObject.Parse(content);
            var latestversion = jsonData["tag_name"];


            var embed = new DiscordEmbedBuilder
            {
                Title = $"{latestversion}",
                Color = new DiscordColor(0xFF0000)
            };
            await embedmsg.ModifyAsync(null, (DiscordEmbed)embed);
        }

        [Command("updatebot"), Aliases("update"), RequireOwner]
        public async Task UpdatebotCommand(CommandContext ctx)
        {
            var checkingembed = new DiscordEmbedBuilder
            {
                Title = $"Updating...",
                Color = new DiscordColor(0xFFA500)
            };
            var embedmsg = await ctx.RespondAsync(checkingembed);

            string content = String.Empty;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("FossiumBot", Program.localversion));
                content = await client.GetStringAsync("https://api.github.com/repos/SKBotNL/FossiumBot-updater/releases/latest");
            }
            string downloadurl = string.Empty;
            JObject jsonData = JObject.Parse(content);
            var latestversion = jsonData["tag_name"];

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                downloadurl = $"https://github.com/SKBotNL/FossiumBot-updater/releases/download/{latestversion}/FossiumBot-Updater-Windows.zip";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                downloadurl = $"https://github.com/SKBotNL/FossiumBot-updater/releases/download/{latestversion}/FossiumBot-Updater-Linux.zip";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var macosembed = new DiscordEmbedBuilder
                {
                    Title = $"Updater is not (yet) available for MacOS",
                    Color = new DiscordColor(0xFF0000)
                };

                await embedmsg.ModifyAsync(null, (DiscordEmbed)macosembed);
            }

            Directory.CreateDirectory(@"updatertemp/");
            using (var client = new WebClient())
            {
                client.DownloadFile(downloadurl, @"updatertemp/FossiumBot.zip");
            }
            ZipFile.ExtractToDirectory(@"updatertemp/FossiumBot.zip", @"updatertemp/updaterzip/");
            Directory.CreateDirectory(@"backup/");
            File.Move($"{System.Diagnostics.Process.GetCurrentProcess().ProcessName}.exe", @"backup/", true);
            File.Move("libopus.dll", @"backup/libopus.dll", true);
            File.Move("libsodium.dll", @"backup/libsodium.dll", true);
            File.Move(@"updatertemp/zip/FossiumBot.exe", @"FossiumBot.exe", true);
            File.Move(@"updatertemp/zip/libopus.dll", @"libopus.dll", true);
            File.Move(@"updatertemp/zip/libsodium.dll", @"libsodium.dll", true);
            File.Move(@"updatertemp/zip/no-u.dll", @"no-u.dll", true);
            File.Move(@"updatertemp.zip/no-u.runtimeconfig.json", @"no-u.runtimeconfig.json", true);
        }
    }
}