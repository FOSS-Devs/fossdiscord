using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VideoLibrary;
using System.Text.RegularExpressions;

namespace FossiumBot.Commands
{
    public class Help : BaseCommandModule
    {
        [Command("help")]
        public async Task HelpCommand(CommandContext ctx)
        {
            var myButton = new DiscordButtonComponent(ButtonStyle.Primary, "help_moderation", "Moderation", false);
            var embed = new DiscordEmbedBuilder
            {
                Title = $"Help",
                Color = new DiscordColor(0x0080FF)
            };

            var builder = new DiscordMessageBuilder()
                .AddEmbed(embed)
                .AddComponents(new DiscordComponent[]
                {
                    new DiscordButtonComponent(ButtonStyle.Primary, "help_moderation", "Moderation", false),
                    new DiscordButtonComponent(ButtonStyle.Primary, "help_utils", "Utils", false),
                    new DiscordButtonComponent(ButtonStyle.Primary, "help_fun", "Fun", false),
                    new DiscordButtonComponent(ButtonStyle.Primary, "help_music", "Music", false),
                })
                .AddComponents(new DiscordComponent[]
                {
                    new DiscordButtonComponent(ButtonStyle.Primary, "help_owner", "Owner", false),
                    new DiscordButtonComponent(ButtonStyle.Primary, "help_settings", "Settings", false)
                });
            await ctx.RespondAsync(builder);

        }
    }
}
