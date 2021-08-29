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
        // For the uptime command
        DateTimeOffset StartTime = DateTime.Now;

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

        [Command("avatar"), Aliases("pfp")]
        public async Task AvatarCommand(CommandContext ctx, DiscordMember member = null)
        {
            if(member == null)
            {
                member = (DiscordMember)ctx.User;
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"{member.DisplayName}'s avatar",
                    Description = $"[WebP]({member.GetAvatarUrl(DSharpPlus.ImageFormat.WebP)}) | [PNG]({member.GetAvatarUrl(DSharpPlus.ImageFormat.Png)}) | [JPG]({member.GetAvatarUrl(DSharpPlus.ImageFormat.Jpeg)})",
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

        [Command("serverinfo")]
        public async Task ServerinfoCommand(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(0x0080FF)
            };
            embed.WithThumbnail(ctx.Guild.IconUrl);
            embed.WithAuthor(ctx.Guild.Name, null, ctx.Guild.IconUrl);
            embed.AddField("Owner", $"{ctx.Guild.Owner.Username}#{ctx.Guild.Owner.Discriminator}");
            embed.AddField("Server ID", ctx.Guild.Id.ToString());
            string guildcreation = ctx.Guild.CreationTimestamp.ToString("G", CultureInfo.CreateSpecificCulture("es-ES"));
            embed.AddField("Server Created", guildcreation);
            embed.AddField("Number of Members", ctx.Guild.MemberCount.ToString());
            int guildrolescount = -1;
            foreach(KeyValuePair<ulong, DiscordRole> kvp in ctx.Guild.Roles)
            {
                guildrolescount = guildrolescount + 1;
            }
            embed.AddField("Number of Roles", guildrolescount.ToString());
            await ctx.RespondAsync(embed);
        }

        [Command("emoji")]
        public async Task EmojiCommand(CommandContext ctx, DiscordEmoji emoji = null)
        {
            if(emoji == null)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Missing argument",
                    Description = $"Usage:\n{ctx.Prefix}emoji <emoji>",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.RespondAsync(embed);
            }
            else
            {
                var embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor(0x0080FF)
                };
                embed.WithAuthor(emoji.Name, null, emoji.Url);
                embed.WithThumbnail(emoji.Url);
                embed.AddField("ID", emoji.Id.ToString());
                string emojicreation = emoji.CreationTimestamp.ToString("G", CultureInfo.CreateSpecificCulture("es-ES"));
                embed.AddField("Created on", emojicreation);
                embed.AddField("URL", emoji.Url);
                await ctx.RespondAsync(embed);
            }
        }

        [Command("uptime")]
        public async Task UptimeCommand(CommandContext ctx)
        {
            var uptime = (DateTime.Now - StartTime);

            var embed = new DiscordEmbedBuilder
            {
                Title = "Uptime",
                Description = $"{uptime.Days.ToString()} days, {uptime.Minutes.ToString()} minutes and {uptime.Seconds.ToString()} seconds",
                Color = new DiscordColor(0x0080FF)
            };
            await ctx.RespondAsync(embed);
        }
    }
}
