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
    public class Fun : BaseCommandModule
    {
        [Command("rate")]
        public async Task RateCommand(CommandContext ctx, [RemainingText] string thing)
        {
            Random r = new Random();
            int randomnum = r.Next(0, 10);
            var embed = new DiscordEmbedBuilder
            {
                Title = $"I rate `{thing}` a {randomnum}/10",
                Color = new DiscordColor(0x0080FF)
            };
            await ctx.RespondAsync(embed);
        }

        [Command("cat")]
        public async Task CatCommand(CommandContext ctx)
        {
            string URL;
            URL = "https://api.thecatapi.com/v1/images/search";
            WebRequest wrREQUEST;
            wrREQUEST = WebRequest.Create(URL);
            wrREQUEST.Proxy = null;
            wrREQUEST.Method = "GET";
            //wrREQUEST.ContentType = "application/json";
            WebResponse response = wrREQUEST.GetResponse();
            StreamReader streamReader = new StreamReader(response.GetResponseStream());
            string responseData = streamReader.ReadToEnd();
            streamReader.Close();

            JArray jsonData = JArray.Parse(responseData);
            var caturl = jsonData[0]["url"];
            string catpic = (string)caturl;

            var embed = new DiscordEmbedBuilder
            {
                Title = "Cat Picture",
                ImageUrl = catpic,
                Color = new DiscordColor(0x0080FF)
            };
            await ctx.RespondAsync(embed);
        }

        [Command("dog"), Aliases("doggo")]
        public async Task DogCommand(CommandContext ctx)
        {
            string URL;
            URL = "https://api.thedogapi.com/v1/images/search";
            WebRequest wrREQUEST;
            wrREQUEST = WebRequest.Create(URL);
            wrREQUEST.Proxy = null;
            wrREQUEST.Method = "GET";
            //wrREQUEST.ContentType = "application/json";
            WebResponse response = wrREQUEST.GetResponse();
            StreamReader streamReader = new StreamReader(response.GetResponseStream());
            string responseData = streamReader.ReadToEnd();
            streamReader.Close();

            JArray jsonData = JArray.Parse(responseData);
            var dogurl = jsonData[0]["url"];
            string dogpic = (string)dogurl;

            var embed = new DiscordEmbedBuilder
            {
                Title = "Dog Picture",
                ImageUrl = dogpic,
                Color = new DiscordColor(0x0080FF)
            };
            await ctx.RespondAsync(embed);
        }
        
        [Command("wikipedia"), Aliases("wiki")]
        public async Task WikiCommand(CommandContext ctx, [RemainingText] string query)
        {
            //string URL = $"https://en.wikipedia.org/w/api.php?action=query&origin=*&format=json&generator=search&gsrnamespace=0&gsrlimit=1&gsrsearch={query}";
            string URL = $"https://en.wikipedia.org/w/api.php?action=query&prop=extracts&exintro&explaintext&origin=*&format=json&generator=search&gsrnamespace=0&gsrlimit=1&gsrsearch={query}";
            WebRequest wrREQUEST;
            wrREQUEST = WebRequest.Create(URL);
            wrREQUEST.Proxy = null;
            wrREQUEST.Method = "GET";

            WebResponse response = wrREQUEST.GetResponse();
            StreamReader streamReader = new StreamReader(response.GetResponseStream());
            string responseData = streamReader.ReadToEnd();
            streamReader.Close();
            JObject jsonData = JObject.Parse(responseData);
            string pageID = ((JProperty)jsonData["query"]["pages"].First()).Name;
            if (pageID == "-1")
            {
                var errEmbed = new DiscordEmbedBuilder
                {
                    Title = "Oops...",
                    Description = "The page you've requested might not exist.",
                    Color = new DiscordColor(0xFF0000)
                };
                await ctx.RespondAsync(errEmbed);
                return;
            }
            else
            {
                string pageTitle = (string)jsonData["query"]["pages"][pageID]["title"];
                string extract = (string)jsonData["query"]["pages"][pageID]["extract"];
                string brief = extract.Substring(0, 260);
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"{pageTitle}",
                    Description = $"{brief}...",
                    Color = new DiscordColor(0x0080FF)
                };
                await ctx.RespondAsync(embed);
            };
                
        }
    }
}
