﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.WebSocket;
using PatternSpider_Discord.Config;
using PatternSpider_Discord.Plugins.Sentience.RelayChains;
using Serilog;

namespace PatternSpider_Discord.Plugins.Sentience
{
    class PluginSentience : IPatternSpiderPlugin
    {
        public string Name => "Sentience";
        public List<string> Commands => new List<string>();

        public PatternSpiderConfig ClientConfig { get; set; }
        public DiscordSocketClient DiscordClient { get; set; }

        private Chain _brain;
        private readonly object _writeLock;
        private Settings _settings;
        private StreamWriter _brainFile;

        public static string BrainPath = "Data/Sentience/Brain.txt";

        public PluginSentience()
        {
            _writeLock = new object();

            if (File.Exists(Settings.FullPath))
            {
                _settings = Settings.Load();
            }
            else
            {
                _settings = new Settings();
                _settings.Save();
            }

            var brainDirectory = Path.GetDirectoryName(BrainPath);

            if (!Directory.Exists(brainDirectory))
            {
                Directory.CreateDirectory(brainDirectory);
            }

            if (!File.Exists(BrainPath))
            {
                //This is a little ugly but i can't manually close a filestream
                using (var f = File.Create(BrainPath))
                {                    
                }
            }

            PruneBrain();
            LoadBrain();

            _brainFile = File.AppendText(BrainPath);
        }
    

        public Task Command(string command, string message, SocketMessage m)
        {
            return Task.CompletedTask;
        }

        public async Task Message(string message, SocketMessage m)
        {                        
            if (_brain == null)
            {
                return;
            }
            var brain = _brain;

            bool mentioned = false;

            foreach (var user in m.MentionedUsers)
            {
                if (user.Id == DiscordClient.CurrentUser.Id)
                {
                    mentioned = true;
                }
            }

            if (mentioned)
            {
                TextSanitizer.FixMiscelanious(message = Regex.Replace(message, "<.+>",""));                
                var response = TextSanitizer.FixInputEnds(brain.GenerateSentenceFromSentence(message));

                if (!string.IsNullOrWhiteSpace(response))
                {
                    await m.Channel.SendMessageAsync(response);
                    return;
                }

                response = RandomResponse.Reponse(m.Author.Username);
                await m.Channel.SendMessageAsync(response);
                return;
            }

            if (message[0] != ClientConfig.CommandSymbol && message.Split(' ').Length > _settings.WindowSize)
            {
                message = TextSanitizer.SanitizeInput(message);
                brain.Learn(message);

                SaveLine(message);
            }
        }

        private void SaveLine(string message)
        {
            lock (_writeLock)
            {                
                try
                {
                    _brainFile.WriteLine(message);
                }
                catch (Exception)
                {
                    Log.Warning("Patternspider - Sentience: Failed to write line to Brain File.");
                }

                _brainFile.Flush();
            }
        }

        private void LoadBrain()
        {
            using (var stream = new FileStream(BrainPath, FileMode.Open))
            using (var reader = new StreamReader(stream))
            {
                var brain = new Chain(_settings.WindowSize);
                string line;

                var i = 0;

                while ((line = reader.ReadLine()) != null)
                {                    
                    brain.Learn(TextSanitizer.SanitizeInput(line));
                    i++;
                }

                _brain = brain;
                Log.Information("Patternspider - Sentience: Loaded Brain, Parsed {i} Lines",i);
            }
        }

        private void PruneBrain()
        {
            var fileInfo = new FileInfo(BrainPath);
            if (fileInfo.Length <= _settings.LogSize)
            {
                return;
            }

            var rand = new Random();

            File.Copy(BrainPath, BrainPath + ".temp");
            File.Delete(BrainPath);

            using (var stream = new FileStream(BrainPath + ".temp", FileMode.Open))
            using (var oldBrain = new StreamReader(stream))
            {
                using (var brain = File.AppendText(BrainPath))
                {
                    string line;
                    while ((line = oldBrain.ReadLine()) != null)
                    {
                        if (rand.Next(0, 2) == 1)
                        {
                            brain.WriteLine(line);
                        }
                    }
                }                                           
            }
        }
    }
}
