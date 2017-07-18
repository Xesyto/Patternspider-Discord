﻿using System.Collections.Generic;

namespace PatternSpider_Discord.Plugins.Hearthstone
{
    class Tables
    {

        public static Dictionary<string, string> Block = new Dictionary<string, string>
        {
            {"BASIC", "Basic" },
            {"CLASSIC", "Classic" },
            {"REWARD", "Wild" },
            {"PROMO", "Wild" },
            {"HOF","Wild" },
            {"NAXX", "Year1" },
            {"GVG", "Year1"},
            {"BRM", "Year2" },
            {"TGT", "Year2" },
            {"LOE", "Year2" },
            {"OG", "Kraken" },
            {"MSG", "Kraken" },
            {"UNGORO","Mammoth" }
        };


        public static Dictionary<string, string> BlockNameCorrection = new Dictionary<string, string>()
        {
            {"GANGS", "MSG"},
            {"GADGET", "MSG"}
        };

        public static List<string> StandardLegal = new List<string>
        {
            "Basic",
            "Classic",
            "Kraken",
            "Mammoth"
        };
    }
}