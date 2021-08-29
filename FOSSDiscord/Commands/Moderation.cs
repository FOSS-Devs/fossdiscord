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
    public class Moderation : BaseCommandModule
    {
        [Command("kick"), RequirePermissions(DSharpPlus.Permissions.KickMembers)]
        public async Task KickCommand(CommandContext ctx, DiscordMember member, [RemainingText] string reason = "no reason given")
        {
            if (member.Id == ctx.Member.Id)
            {
                var fstEM = new DiscordEmbedBuilder
                {
                    Title = "You cannot kick yourself.",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.RespondAsync(fstEM);
                return;
            }

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Kicked {member.Username}#{member.Discriminator}",
                Color = new DiscordColor(0xFFA500)
            };
            embed.AddField("Reason", reason);
            await member.RemoveAsync(reason);
            await ctx.RespondAsync(embed);
        }

        [Command("ban"), RequirePermissions(DSharpPlus.Permissions.BanMembers)]
        public async Task BanCommand(CommandContext ctx, DiscordMember member, int deletemessagedays, [RemainingText] string reason = "no reason given")
        {
            var banlist = ctx.Guild.GetBansAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            if (member.Id == ctx.Member.Id)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = "You cannot ban yourself.",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.RespondAsync(embed);
                return;
            }
            if (banlist.Any(x => x.User.Id == member.Id))
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"{member.Username}#{member.Discriminator} is already banned",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.RespondAsync(embed);
            }
            else
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"Banned {member.Username}#{member.Discriminator}",
                    Color = new DiscordColor(0xFF0000)
                };
                embed.AddField("Reason", reason);
                await member.BanAsync(deletemessagedays, reason);
                await ctx.RespondAsync(embed);
            }
        }

        [Command("softban"), RequirePermissions(DSharpPlus.Permissions.BanMembers)]
        public async Task SoftbanCommand(CommandContext ctx, DiscordMember member, int deletemessagedays, [RemainingText] string reason = "no reason given")
        {
            if (member.Id == ctx.Member.Id)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = "You cannot softban yourself.",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.RespondAsync(embed);
                return;
            }
            else
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"Sofbanned {member.Username}#{member.Discriminator}",
                    Color = new DiscordColor(0xFFA500)
                };
                embed.AddField("Reason", reason);
                await member.BanAsync(deletemessagedays, reason);
                await ctx.Guild.UnbanMemberAsync(member.Id);
                await ctx.RespondAsync(embed);
            }
        }

        [Command("unban"), RequirePermissions(DSharpPlus.Permissions.BanMembers)]
        public async Task UnbanCommand(CommandContext ctx, ulong memberid)
        {
            var banlist = ctx.Guild.GetBansAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            if(banlist.Any(x => x.User.Id == memberid))
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"Unbanned {memberid}",
                    Color = new DiscordColor(0x2ECC70)
                };
                await ctx.Guild.UnbanMemberAsync(memberid);
                await ctx.RespondAsync(embed);
            }
            else
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"{memberid} is not banned",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.RespondAsync(embed);
            }
        }

        [Command("purge"), RequirePermissions(DSharpPlus.Permissions.ManageMessages)]
        public async Task PurgeCommands(CommandContext ctx, int amount = 10)
        {
            var messages = await ctx.Channel.GetMessagesAsync(amount+1);
            await ctx.Channel.DeleteMessagesAsync(messages);

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Purged {amount} messages",
                Color = new DiscordColor(0x2ECC70)
            };
            var responsemsg = await ctx.RespondAsync(embed);
            await Task.Delay(5000);
            await responsemsg.DeleteAsync();
        }
    }
}
