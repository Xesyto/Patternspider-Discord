﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace PatternSpider_Discord.Plugins
{
    class PluginChatReactions : IPatternSpiderPlugin
    {
        public string Name => "Chat Reactions";
        public List<string> Commands => new List<string>();

        public Task Command(string command, string messsage, SocketMessage m)
        {
            return Task.CompletedTask;
        }

        public async Task Message(string messsage, SocketMessage m)
        {
            if (messsage.Contains("(╯°□°)╯︵ ┻━┻"))
                await m.Channel.SendMessageAsync("┬──┬◡ﾉ(° -°ﾉ)" );            
        }
    }
}