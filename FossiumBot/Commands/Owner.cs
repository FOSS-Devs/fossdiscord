using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace FossiumBot.Commands
{
    public class Owner : ApplicationCommandModule
    {
        [SlashCommand("shutdown", "Shut down the bot")]
        [SlashRequireOwner]
        public async Task ShutdownCommand(InteractionContext ctx)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = $"The bot is now shut down",
                Color = new DiscordColor(0x2ECC70)
            };
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
            await ctx.Client.DisconnectAsync();
            Environment.Exit(1);
        }
        [SlashCommand("servers", "Show all the servers the bot is in")]
        [SlashRequireOwner]
        public async Task ServersCommand(InteractionContext ctx)
        {
            List<string> guildslist = new List<string>();

            foreach (DiscordGuild guild in ctx.Client.Guilds.Values)
            {
                guildslist.Add(guild.ToString());
            }
            string[] guildsarray = guildslist.ToArray();
            string guildsstring = string.Join(" ", guildsarray);
            string connectedguilds = guildsstring.Replace(" Guild ", "\nID: ").Replace("Guild ", "ID: ").Replace("; ", " Name: ");

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Connected on {ctx.Client.Guilds.Count} server(s)",
                Description = connectedguilds,
                Color = new DiscordColor(0x0080FF)
            };
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }

        [SlashCommand("leaveserver", "Leave a server the bot is in")]
        [SlashRequireOwner]
        public async Task LeaveserverCommand(InteractionContext ctx, [Option("serverid", "Server ID of the server you want the bot to leave")] long serverid)
        {
            DiscordGuild guild = await ctx.Client.GetGuildAsync((ulong)serverid);

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Left {guild.Name}",
                Description = $"ID: {guild.Id}",
                Color = new DiscordColor(0x2ECC70)
            };
            await guild.LeaveAsync();
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }
    }
}