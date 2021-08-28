using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace FOSSDiscord.Commands
{
    public class Utils : BaseCommandModule
    {
        [Command("avatar")]
        public async Task AvatarCommand(CommandContext ctx, DiscordMember member = null)
        {
            if(member == null)
            {
                member = (DiscordMember)ctx.User;
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"{member.DisplayName}'s avatar",
                    ImageUrl = member.AvatarUrl,
                    Color = new DiscordColor(0x0080FF)
                };
                await ctx.RespondAsync(embed);
            }
            else
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"{member.DisplayName}'s avatar",
                    ImageUrl = member.AvatarUrl,
                    Color = new DiscordColor(0x0080FF)
                };
                await ctx.RespondAsync(embed);
            }
        }
    }
}
