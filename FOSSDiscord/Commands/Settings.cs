using System;
using System.Collections.Generic;
using System.IO;
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


namespace FOSSDiscord.Commands
{
    public class Settings : BaseCommandModule
    {
        [Group("settings")]
        public class SettingsGroup : BaseCommandModule
        {
            [GroupCommand]
            public async Task SettingsCommand(CommandContext ctx)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Settings",
                    Color = new DiscordColor(0x0080FF)
                };
                embed.AddField($"`{ctx.Prefix}settings loggingchannel <mention channel>`", "Set the channel to log events to, disable logging by running the command without any arguments");
                await ctx.RespondAsync(embed);
            }

            [Command("loggingchannel")]
            public async Task LoggingchannelCommand(CommandContext ctx, DiscordChannel loggingchannel = null)
            {
                if (loggingchannel == null)
                {
                    FOSSDiscord.Properties.Settings.Default.loggingchannelid = 0;
                    var rmembed = new DiscordEmbedBuilder
                    {
                        Title = $"Disabled logging",
                        Color = new DiscordColor(0x2ECC70)
                    };
                    await ctx.RespondAsync(rmembed);
                    return;
                }
                else
                {
                    FOSSDiscord.Properties.Settings.Default.loggingchannelid = loggingchannel.Id;
                }
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"Set #{loggingchannel.Name} as the logging channel",
                    Color = new DiscordColor(0x2ECC70)
                };
                await ctx.RespondAsync(embed);
            }
        }
    }
}