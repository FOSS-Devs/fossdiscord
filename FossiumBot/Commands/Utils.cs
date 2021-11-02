using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.Entities;

namespace FossiumBot.Commands
{
    public class Utils : ApplicationCommandModule
    {
        [SlashCommand("ping", "Get the ping of the bot")]
        public async Task PingCommand(InteractionContext ctx)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = $"Pong! `{ctx.Client.Ping}ms`",
                Color = new DiscordColor(0x0080FF)
            };
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }

        [SlashCommand("avatar", "Get the avatar from a user")]
        public async Task AvatarCommand(InteractionContext ctx, [Option("user", "The user get the avatar from")] DiscordUser user = null)
        {
            if(user == null)
            {
                DiscordMember member = (DiscordMember)ctx.User;
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"{member.DisplayName}'s avatar",
                    Description = $"[WebP]({member.GetAvatarUrl(DSharpPlus.ImageFormat.WebP)}) | [PNG]({member.GetAvatarUrl(DSharpPlus.ImageFormat.Png)}) | [JPG]({member.GetAvatarUrl(DSharpPlus.ImageFormat.Jpeg)})",
                    ImageUrl = member.AvatarUrl,
                    Color = new DiscordColor(0x0080FF)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
            }
            else
            {
                DiscordMember member = (DiscordMember)user;
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"{member.DisplayName}'s avatar",
                    ImageUrl = member.AvatarUrl,
                    Color = new DiscordColor(0x0080FF)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
            }
        }

        [SlashCommand("userinfo", "Get information about a user")]
        public async Task UserinfoCommand(InteractionContext ctx, [Option("user", "User to get information")] DiscordUser user = null)
        {
            if (user == null)
            {
                DiscordMember member = (DiscordMember)ctx.User;
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
                long memberjoinedat = member.JoinedAt.ToUnixTimeSeconds();
                embed.AddField("Joined Server", $"<t:{memberjoinedat}:F>");
                long membercreation = member.CreationTimestamp.ToUnixTimeSeconds();
                embed.AddField("Registered", $"<t:{membercreation}:F>");
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
            }
            else
            {
                DiscordMember member = (DiscordMember)user;
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
                long memberjoinedat = member.JoinedAt.ToUnixTimeSeconds();
                embed.AddField("Joined Server", $"<t:{memberjoinedat}:F>");
                long membercreation = member.CreationTimestamp.ToUnixTimeSeconds();
                embed.AddField("Registered", $"<t:{membercreation}:F>");
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
            }
        }

        [SlashCommand("serverinfo", "Get information about the server")]
        public async Task ServerinfoCommand(InteractionContext ctx)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(0x0080FF)
            };
            embed.WithThumbnail(ctx.Guild.IconUrl);
            embed.WithAuthor(ctx.Guild.Name, null, ctx.Guild.IconUrl);
            embed.AddField("Owner", $"{ctx.Guild.Owner.Username}#{ctx.Guild.Owner.Discriminator}");
            embed.AddField("Server ID", ctx.Guild.Id.ToString());
            long guildcreation = ctx.Guild.CreationTimestamp.ToUnixTimeSeconds();
            embed.AddField("Server Created", $"<t:{guildcreation}:F>");
            embed.AddField("Number of Members", ctx.Guild.MemberCount.ToString());
            embed.AddField("Number of Roles", ctx.Guild.Roles.Count.ToString());
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }

        [SlashCommand("emojiinfo", "Get information about an emoji")]
        public async Task EmojiinfoCommand(InteractionContext ctx, [Option("emoji", "Emoji to get information")] DiscordEmoji emoji)
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
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }

        [SlashCommand("poll", "Make a poll with multiple options")]
        public async Task PollCommand(InteractionContext ctx, [Option("poll", "The poll, split the title and the different questions with \" | \"")] string poll)
        {

            string[] pollsplit = poll.Split(" | ");
            string[] polls = pollsplit.Skip(1).ToArray();

            if (polls.Count() > 10)
            {
                var errorembed = new DiscordEmbedBuilder
                {
                    Title = "Oops...",
                    Description = "You can only have up to 10 questions in a poll",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(errorembed));
                return;
            }

            string[] number = new string[] { "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "keycap_ten"};

            long repeating = 0;
            string pollquestions = String.Empty;

            foreach (string question in polls)
            {
                if (repeating == 0)
                {
                    pollquestions = pollquestions + $":one: - {question}";
                }
                else
                {
                    pollquestions = pollquestions + $"\n\n:{number[repeating]}: - {question}";
                }
                repeating = repeating + 1;
            }

            var embed = new DiscordEmbedBuilder
            {
                Title = pollsplit[0],
                Description = pollquestions,
                Color = new DiscordColor(0x0080FF)
            };

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
            var embedmsg = await ctx.GetOriginalResponseAsync();

            repeating = 0;

            foreach (string question in polls)
            {
                
                await embedmsg.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, $":{number[repeating]}:"));
                repeating = repeating + 1;
            }
        }

        [SlashCommand("quickpoll", "Make a yes/no poll")]
        public async Task QuickpollCommand(InteractionContext ctx, [Option("poll", "The poll/question")] string poll)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = $"{ctx.Member.DisplayName} asks:",
                Description = poll,
                Color = new DiscordColor(0x0080FF)
            };
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
            var embedmsg = await ctx.GetOriginalResponseAsync();
            await embedmsg.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":thumbsup:"));
            await embedmsg.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":thumbsdown:"));
        }

        [SlashCommand("uptime", "Get the uptime of the bot")]
        public async Task UptimeCommand(InteractionContext ctx)
        {
            var uptime = (DateTime.Now - Program.StartTime);
            long onlinesince = Program.StartTime.ToUnixTimeSeconds();
            var embed = new DiscordEmbedBuilder
            {
                Title = "Uptime",
                Description = $"Online since <t:{onlinesince}:F>\n({uptime.Days} days, {uptime.Minutes} minutes and {uptime.Seconds} seconds)",
                Color = new DiscordColor(0x0080FF)
            };
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }
    }
}
