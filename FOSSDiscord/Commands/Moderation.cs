using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO
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
                var errembed = new DiscordEmbedBuilder
                {
                    Title = "You cannot kick yourself",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.RespondAsync(errembed);
                return;
            }
            else if (ctx.Member.Hierarchy <= member.Hierarchy)
            {
                var errembed = new DiscordEmbedBuilder
                {
                    Title = "Oops...",
                    Description = "Your role is too low in the role hierarchy to do that",
                    Color = new DiscordColor(0xFF0000),
                };
                await ctx.RespondAsync(errembed);
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
        public async Task BanCommand(CommandContext ctx, DiscordMember member, int deletemessagedays = 5, [RemainingText] string reason = "no reason given")
        {
            if (member.Id == ctx.Member.Id)
            {
                var errembed = new DiscordEmbedBuilder
                {
                    Title = "You cannot ban yourself",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.RespondAsync(errembed);
                return;
            }
            else if (ctx.Member.Hierarchy <= member.Hierarchy)
            {
                var errembed = new DiscordEmbedBuilder
                {
                    Title = "Oops...",
                    Description = "Your role is too low in the role hierarchy to do that",
                    Color = new DiscordColor(0xFF0000),
                };
                await ctx.RespondAsync(errembed);
                return;
            }
            var banlist = ctx.Guild.GetBansAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            if (banlist.Any(x => x.User.Id == member.Id))
            {
                var errembed = new DiscordEmbedBuilder
                {
                    Title = $"{member.Username}#{member.Discriminator} is already banned",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.RespondAsync(errembed);
                return;
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
        public async Task SoftbanCommand(CommandContext ctx, DiscordMember member, int deletemessagedays = 5, [RemainingText] string reason = "no reason given")
        {
            int userPerms = member.Hierarchy;
            int authorPerms = ctx.Member.Hierarchy;
            if (member.Id == ctx.Member.Id)
            {
                var errembed = new DiscordEmbedBuilder
                {
                    Title = "You cannot softban yourself",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.RespondAsync(errembed);
                return;
            }
            else if (ctx.Member.Hierarchy <= member.Hierarchy)
            {
                var errembed = new DiscordEmbedBuilder
                {
                    Title = "Oops...",
                    Description = "Your role is too low in the role hierarchy to do that",
                    Color = new DiscordColor(0xFF0000),
                };
                await ctx.RespondAsync(errembed);
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
                var errembed = new DiscordEmbedBuilder
                {
                    Title = $"{memberid} is not banned",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.RespondAsync(errembed);
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

        [Command("autodelete"), RequirePermissions(DSharpPlus.Permissions.ManageMessages)]
        public async Task AutoDeleteCommand(CommandContext ctx, DiscordChannel channel, int time = 1)
        {
            if (ctx.Guild.Id != channel.GuildId)
            {
                var em = new DiscordEmbedBuilder
                {
                    Title = $"Oops...",
                    Description = "That channel is not in this server",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.RespondAsync(em);
                return;
            }
            if (!Directory.Exists(@"Settings"))
            {
                Directory.CreateDirectory(@"Settings/");
            }
            if (!Directory.Exists(@"Settings/lck/"))
            {
                Directory.CreateDirectory(@"Settings/lck");
            }
            if (!File.Exists($"Settings/lck/{channel.Id}.lck"))
                File.Create($"Settings/lck/{channel.Id}.lck").Dispose();
            else if (File.Exists($"Settings/lck/{channel.Id}.lck"))
            {
                var em = new DiscordEmbedBuilder
                {
                    Title = $"Oops...",
                    Description = "That channel already configured to auto delete message.",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.RespondAsync(em);
                return;
            }
            while (true) {
                var messages = await channel.GetMessagesAsync();
                foreach (var message in messages)
                {
                    var msgTime = message.Timestamp.UtcDateTime;
                    var sysTime = System.DateTime.UtcNow;
                        if (sysTime.Subtract(msgTime).TotalHours > time)
                        {
                            await channel.DeleteMessageAsync(message);
                            await Task.Delay(2000);
                        }
                        else if(!File.Exists($"Settings/lck/{channel.Id}.lck"))
                        {
                            return;
                        }
                }
            }
        }
    }
}
