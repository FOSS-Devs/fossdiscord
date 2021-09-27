using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

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
            if (banlist.Any(x => x.User.Id == memberid))
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

        [Command("purge"), RequirePermissions(DSharpPlus.Permissions.ManageMessages), Cooldown(1, 5, CooldownBucketType.User)]
        public async Task PurgeCommands(CommandContext ctx, int amount = 10)
        {
            if (amount > 50)
            {
                var errembed = new DiscordEmbedBuilder
                {
                    Title = "Cannot purge more than 50 messages at one time",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.RespondAsync(errembed);
                return;
            }
            var messages = await ctx.Channel.GetMessagesAsync(amount + 1);
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

        [Command("autodelete"), RequirePermissions(DSharpPlus.Permissions.Administrator)]
        public async Task AutoDeleteCommand(CommandContext ctx, DiscordChannel channel, string time)
        {
            if (ctx.Guild.Id != channel.GuildId)
            {
                var em = new DiscordEmbedBuilder
                {
                    Title = $"Oops...",
                    Description = "That channel does not exist in this server",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.RespondAsync(em);
                return;
            }
            Directory.CreateDirectory(@"Settings/");
            Directory.CreateDirectory(@"Settings/lck/");
            if (time == "off" && File.Exists($"Settings/lck/{channel.Id}.lck"))
            {
                File.Delete($"Settings/lck/{channel.Id}.lck");
                return;
            }
            else if (Int16.Parse(time) >= 1 && File.Exists($"Settings/lck/{channel.Id}.lck"))
            {
                var em = new DiscordEmbedBuilder
                {
                    Title = $"Oops...",
                    Description = "That channel already configured to auto delete messages",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.RespondAsync(em);
                return;
            }
            else if (time == "off" && !File.Exists($"Settings/lck/{channel.Id}.lck"))
            {
                var em = new DiscordEmbedBuilder
                {
                    Title = $"Oops...",
                    Description = "That channel is not configured to auto delete messages",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.RespondAsync(em);
                return;
            }
            if (!File.Exists($"Settings/lck/{channel.Id}.lck"))
            {
                if (time != "off" && Int16.Parse(time) >= 1)
                {
                    var em = new DiscordEmbedBuilder
                    {
                        Title = $"Oops...",
                        Description = $"`{channel.Name}` is now configured to auto delete messages every {time} hour(s)",
                        Color = new DiscordColor(0x2ECC70)
                    };
                    await ctx.RespondAsync(em);
                    File.Create($"Settings/lck/{channel.Id}.lck").Dispose();
                    while (File.Exists($"Settings/lck/{channel.Id}.lck"))
                    {
                        var messages = await channel.GetMessagesAsync();
                        foreach (var message in messages)
                        {
                            var msgTime = message.Timestamp.UtcDateTime;
                            var sysTime = System.DateTime.UtcNow;
                            if (sysTime.Subtract(msgTime).TotalHours > Int16.Parse(time) && sysTime.Subtract(msgTime).TotalHours < 336)
                            {
                                await channel.DeleteMessageAsync(message);
                                await Task.Delay(3000);
                            }
                        }
                        await Task.Delay(1000);
                    }
                }
                else
                {
                    var em = new DiscordEmbedBuilder
                    {
                        Title = $"Oops...",
                        Description = "You command syntax is not right",
                        Color = new DiscordColor(0xFF0000)
                    };
                    await ctx.RespondAsync(em);
                }
            }
            return;
        }

        [Command("warn"), RequirePermissions(DSharpPlus.Permissions.ManageMessages)]
        public async Task WarnCommand(CommandContext ctx, DiscordMember member, [RemainingText] String reason = "none")
        {
            string file = $"Data/blacklist/{ctx.Guild.Id}.lst";
            Directory.CreateDirectory(@"Data/");
            Directory.CreateDirectory(@"Data/blacklist/");
            try
            {
                if (File.Exists(file))
                {
                    StreamReader readData = new StreamReader(file);
                    string data = readData.ReadToEnd();
                    readData.Close();
                    JObject jsonData = JObject.Parse(data);
                    if (jsonData.GetValue($"{member.Id}") != null)
                    {
                        string lastItem = ((JProperty)jsonData[$"{member.Id}"].Last()).Name;
                        int caseIncrement = int.Parse(lastItem) + 1;
                        jsonData[$"{member.Id}"][$"{caseIncrement}"] = reason;
                    }
                    else 
                    {
                        jsonData.Add(new JProperty($"{member.Id}", new JObject(new JProperty("0", reason))));
                    }
                    string dataWrite = Newtonsoft.Json.JsonConvert.SerializeObject(jsonData, Newtonsoft.Json.Formatting.Indented);
                    System.IO.File.WriteAllText(file, dataWrite);
                    var firstEM = new DiscordEmbedBuilder
                    {
                        Title = $"**{ member.DisplayName }**has been warned,",
                        Description = dataWrite,
                        Color = new DiscordColor(0xFFA500)
                    };
                    await ctx.RespondAsync(firstEM);
                }
                else
                {
                    JObject overwrite =
                    new JObject(
                        new JProperty($"{member.Id}",
                            new JObject(
                                new JProperty("1", reason)
                            )
                        )
                    );
                    string dataWrite = Newtonsoft.Json.JsonConvert.SerializeObject(overwrite, Newtonsoft.Json.Formatting.Indented);
                    System.IO.File.WriteAllText(file, dataWrite);
                    var em = new DiscordEmbedBuilder
                    {
                        Title = $"**{member.DisplayName}**has been warned",
                        Color = new DiscordColor(0xFFA500)
                    };
                    await ctx.RespondAsync(em);
                }
            }
            catch (Exception)
            {
                JObject overwrite =
                    new JObject(
                        new JProperty($"{member.Id}",
                            new JObject(
                                new JProperty("1", reason)
                            )
                        )
                    );
                string dataWrite = Newtonsoft.Json.JsonConvert.SerializeObject(overwrite, Newtonsoft.Json.Formatting.Indented);
                System.IO.File.WriteAllText(file, dataWrite);
                var emNEW = new DiscordEmbedBuilder
                {
                    Title = $"**{member.DisplayName}**has been warned",
                    Color = new DiscordColor(0xFFA500)
                };
                await ctx.RespondAsync(emNEW);
                return;
            };
        }

        [Command("warns"), Aliases("warnings"), RequirePermissions(DSharpPlus.Permissions.ManageMessages)]
        public async Task Warnings(CommandContext ctx, DiscordMember member)
        {
            try
            {
                string file = $"Data/blacklist/{ctx.Guild.Id}.lst";
                if (File.Exists(file))
                {
                    StreamReader readData = new StreamReader(file);
                    string data = readData.ReadToEnd();
                    readData.Close();
                    JObject jsonData = JObject.Parse(data);
                    if (jsonData.GetValue($"{member.Id}") != null)
                    {
                        dynamic iterData = jsonData[$"{member.Id}"];
                        var firstEM = new DiscordEmbedBuilder
                        {
                            Title = $"**{member.DisplayName}**'s warnings:",
                            Color = new DiscordColor(0xFFA500)
                        };
                        foreach (KeyValuePair<string, JToken> items in (JObject)jsonData[$"{member.Id}"])
                        {
                            string caseID = items.Key;
                            var reason = jsonData[$"{member.Id}"][caseID];
                            firstEM.AddField($"Case {caseID}",$"{reason}");
                        }
                        await ctx.RespondAsync(firstEM);
                    }
                    else
                    {
                        var nonexistEM = new DiscordEmbedBuilder
                        {
                            Title = "Awesome!",
                            Description = $"{member.DisplayName} has no warning(s)",
                            Color = new DiscordColor(0x2ECC70)
                        };
                        await ctx.RespondAsync(nonexistEM);
                    }
                }
                if (!File.Exists(file)) 
                {
                    var nonexistEM = new DiscordEmbedBuilder
                    {
                        Title = "Awesome!",
                        Description = "Nobody in this guild has been warned yet",
                        Color = new DiscordColor(0x2ECC70)
                    };
                    await ctx.RespondAsync(nonexistEM);
                }
            }
            catch (Exception ex)
            {
                var errorEM = new DiscordEmbedBuilder
                {
                    Title = "Oops",
                    Description = $"{ex}",
                    Color = new DiscordColor(0x2ECC70)
                };
                await ctx.RespondAsync(errorEM);
                return;
            };
        }



        public class warns
        {

            public int caseID { get; set; }
            public string reason { get; set; }
        }
    }
}
