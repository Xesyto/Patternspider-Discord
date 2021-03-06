﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using PatternSpider_Discord.Config;
using PatternSpider_Discord.Plugins.MTG;
using Serilog;

namespace PatternSpider_Discord.Plugins.Hearthstone
{
    class PluginHearthstone : IPatternSpiderPlugin
    {
        private HttpClient _httpClient;

        public string Name => "Hearthstone";
        public List<string> Commands => new List<string> { "hs" };

        public PatternSpiderConfig ClientConfig { get; set; }
        public DiscordSocketClient DiscordClient { get; set; }

        public PluginHearthstone()
        {
            _httpClient = new HttpClient();

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.Timeout = TimeSpan.FromSeconds(2);
        }

        public async Task Command(string command, string message, SocketMessage m)
        {
            var text = message.Trim();
            var messageParts = text.Split(' ');
            var searchString = string.Join(" ", messageParts.Skip(1));

            var resultMesage = await SearchHearthHead(searchString);

            await resultMesage.SendMessageToChannel(m.Channel);
        }

        public Task Message(string message, SocketMessage m)
        {
            return Task.CompletedTask;
        }

        private async Task<DiscordMessage> SearchHearthHead(string searchString)
        {
            List<Card> cards;
            string searchUrl = $"http://hearthstone.services.zam.com/v1/card?sort=cost,name&search={searchString}&cost=0,1,2,3,4,5,6,7,8,9,10&type=MINION,SPELL,WEAPON,HERO&collectible=true";
            string jsonData;

            var stringTask = _httpClient.GetStringAsync(searchUrl);

            try
            {
                jsonData = await stringTask;
            }
            catch
            {                
                return new DiscordMessage("Error Occured trying to search for card.");
            }

            if (string.IsNullOrWhiteSpace(jsonData))
            {
                return new DiscordMessage("No Results found for: " + searchString);
            }                

            try
            {
                cards = ParseJson(jsonData);
            }
            catch (Exception e)
            {
                Log.Warning("Plugin-Hearthstone: Cannot parse Hearthead API Response. Request: {requestSTring}", searchUrl);
                Log.Warning("Plugin-Hearthstone: Cannot parse Hearthead API Response. Request: {requestSTring}", searchUrl);
                Log.Debug(e, "Plugin-Hearthstone: Cannot parse Hearthead API Response. Request: {requestSTring}", searchUrl);
                throw;
            }

            if (cards.Count == 0)
            {                
                return new DiscordMessage($"could not find any cards named: {searchString}");
            }

            if (cards.Count == 1)
            {
                var card = cards[0];

                return new DiscordMessage(CardToEmbed(card));
            }
           
            return new DiscordMessage($"[<https://www.hearthpwn.com/cards?filter-name={searchString}&filter-premium=1&display=3>] Found {cards.Count} results.");
        }

        private static List<Card> ParseJson(string data)
        {
            return JsonConvert.DeserializeObject<List<Card>>(data);
        }

        private static Embed CardToEmbed(Card card)
        {
            var cardEmbed = new EmbedBuilder();

            cardEmbed.Title = card.name;

            var zamName = card.name.ToLower().Replace(" ", "-");
            cardEmbed.Url = $"http://www.hearthhead.com/cards/{zamName}";

            cardEmbed.ThumbnailUrl = card.cardImage;

            var setField = new EmbedFieldBuilder
            {
                Name = "Set",
                Value = card.set.CapitalizeOnlyFirstLetter(),
                IsInline = true
            };

            var rarityField = new EmbedFieldBuilder
            {
                Name = "Rarity",
                Value = card.rarity.CapitalizeOnlyFirstLetter(),
                IsInline = true

            };

            var classField = new EmbedFieldBuilder
            {
                Name = "Class",
                Value = card.card_class.CapitalizeOnlyFirstLetter(),
                IsInline = true

            };

            var typeField = new EmbedFieldBuilder
            {
                Name = "Type",
                Value = card.type.CapitalizeOnlyFirstLetter(),
                IsInline = true
            };


            cardEmbed.Fields.Add(setField);
            cardEmbed.Fields.Add(rarityField);
            cardEmbed.Fields.Add(classField);
            cardEmbed.Fields.Add(typeField);
            
            if (card.type == "MINION")
            {
                //card is a minion
                var raceField = new EmbedFieldBuilder
                {
                    Name = "Tribe",
                    Value = (card.card_class ?? "Neutral").CapitalizeOnlyFirstLetter()
                };

                var attackField = new EmbedFieldBuilder
                {
                    Name = "Attack",
                    Value = card.attack,
                    IsInline = true,                    
                };

                var healthField = new EmbedFieldBuilder
                {
                    Name = "Health",
                    Value = card.health,
                    IsInline = true
                };

                if (card.race != null)
                {
                    cardEmbed.Fields.Add(raceField);
                }

                cardEmbed.Fields.Add(attackField);
                cardEmbed.Fields.Add(healthField);
            }

            if (card.type == "WEAPON")
            {
                //card is a minion
                var attackField = new EmbedFieldBuilder
                {
                    Name = "Attack",
                    Value = card.attack,
                    IsInline = true
                };

                var healthField = new EmbedFieldBuilder
                {
                    Name = "Durability",
                    Value = card.durability,
                    IsInline = true
                };

                cardEmbed.Fields.Add(attackField);
                cardEmbed.Fields.Add(healthField);
            }

            var costField = new EmbedFieldBuilder
            {
                Name = "Cost",
                Value = MTG_EmoteTable.ReplaceSymbols(card.cost.ToString()),

            };

            card.text = CorrectCardText(card.text);
            card.text = card.text.Replace("\n", " ");

            var textField = new EmbedFieldBuilder
            {
                Name = "Text",
                Value = MTG_EmoteTable.ReplaceSymbols(card.text),
            };

            cardEmbed.Fields.Add(costField);
            cardEmbed.Fields.Add(textField);

            cardEmbed.Color = Color.Teal;

            return cardEmbed;
        }

        private static string CorrectCardText(string text)
        {
            //Insert points for details
            var newText = Regex.Replace(text, "{.}", string.Empty);

            //Markup
            newText = Regex.Replace(newText, "<.>", string.Empty);
            newText = Regex.Replace(newText, "</.>", string.Empty);

            //@ Symbols
            newText = newText.Replace("@", " ");

            return newText;
        }
    }
}
