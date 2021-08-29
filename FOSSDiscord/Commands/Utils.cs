using System;
using System.Collections.Generic;
using System.Globalization;
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
        [Command("ping")]
        public async Task PingCommand(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = $"Pong! `{ctx.Client.Ping}ms`",
                Color = new DiscordColor(0x0080FF)
            };
            await ctx.RespondAsync(embed);
        }

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

        [Command("userinfo")]
        public async Task UserinfoCommand(CommandContext ctx, DiscordMember member = null)
        {
            if (member == null)
            {
                member = (DiscordMember)ctx.User;
                var embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor(0x0080FF)
                };
                embed.WithAuthor(member.Username, null, member.AvatarUrl);
                embed.WithThumbnail(member.AvatarUrl);
                embed.AddField("ID", member.Id.ToString());
                if (member.Nickname == null)
                {
                    embed.AddField("Nickname", "none");
                }
                else
                {
                    embed.AddField("Nickname", member.Nickname);
                }
                string memberjoinedat = member.JoinedAt.ToString("G", CultureInfo.CreateSpecificCulture("es-ES"));
                embed.AddField("Joined Server", memberjoinedat, false);
                string membercreation = member.CreationTimestamp.ToString("G", CultureInfo.CreateSpecificCulture("es-ES"));
                embed.AddField("Account Created", membercreation);
                await ctx.RespondAsync(embed);
            }
            else
            {
                var embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor(0x0080FF)
                };
                embed.WithAuthor(member.Username, null, member.AvatarUrl);
                embed.WithThumbnail(member.AvatarUrl);
                embed.AddField("ID", member.Id.ToString());
                if (member.Nickname == null)
                {
                    embed.AddField("Nickname", "none");
                }
                else
                {
                    embed.AddField("Nickname", member.Nickname);
                }
                string memberjoinedat = member.JoinedAt.ToString("G", CultureInfo.CreateSpecificCulture("es-ES"));
                embed.AddField("Joined Server", memberjoinedat, false);
                string membercreation = member.CreationTimestamp.ToString("G", CultureInfo.CreateSpecificCulture("es-ES"));
                embed.AddField("Account Created", membercreation);
                await ctx.RespondAsync(embed);
            }
        }
    }
}
