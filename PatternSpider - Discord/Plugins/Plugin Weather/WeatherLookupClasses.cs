﻿using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace PatternSpider_Discord.Plugins.Weather
{
    public class Currently
    {
        public int time { get; set; }
        public string summary { get; set; }
        public string icon { get; set; }
        public double precipIntensity { get; set; }
        public double precipProbability { get; set; }
        public string precipType { get; set; }
        public double temperature { get; set; }
        public double apparentTemperature { get; set; }
        public double dewPoint { get; set; }
        public double humidity { get; set; }
        public double windSpeed { get; set; }
        public double windBearing { get; set; }
        public double cloudCover { get; set; }
        public double pressure { get; set; }
        public double ozone { get; set; }
    }

    public class Hourly
    {
        public string summary { get; set; }
        public string icon { get; set; }
        public IList<Datum> data { get; set; }
    }

    public class Datum
    {
        public int time { get; set; }
        public string summary { get; set; }
        public string icon { get; set; }
        public int sunriseTime { get; set; }
        public int sunsetTime { get; set; }
        public double moonPhase { get; set; }
        public double precipIntensity { get; set; }
        public double precipIntensityMax { get; set; }
        public double precipProbability { get; set; }
        public double temperatureMin { get; set; }
        public int temperatureMinTime { get; set; }
        public double temperatureMax { get; set; }
        public int temperatureMaxTime { get; set; }
        public double apparentTemperatureMin { get; set; }
        public int apparentTemperatureMinTime { get; set; }
        public double apparentTemperatureMax { get; set; }
        public int apparentTemperatureMaxTime { get; set; }
        public double dewPoint { get; set; }
        public double humidity { get; set; }
        public double windSpeed { get; set; }
        public double windBearing { get; set; }
        public double cloudCover { get; set; }
        public double pressure { get; set; }
        public double ozone { get; set; }
        public int? precipIntensityMaxTime { get; set; }
        public string precipType { get; set; }

        private static Dictionary<double, string> MoonPhases = new Dictionary<double, string>
        {
            {0,"🌑"},
            {0.125,"🌒"},
            {0.25,"🌓"},
            {0.375,"🌔"},
            {0.5,"🌕"},
            {0.625,"🌖"},
            {0.75,"🌗"},
            {0.875,"🌘"}
        };

        public string MoonPhase()
        {           
            foreach (var p in MoonPhases)
            {
                if (moonPhase < p.Key)
                {
                    return p.Value;
                }
            }
            
            return MoonPhases.Last().Value;
        }
    }

    public class Daily
    {
        public string summary { get; set; }
        public string icon { get; set; }
        public IList<Datum> data { get; set; }
    }

    public class Flags
    {
        public IList<string> sources { get; set; }
        public string units { get; set; }
    }

    public class WeatherData
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
        public string timezone { get; set; }
        public int offset { get; set; }
        public Currently currently { get; set; }
        public Hourly hourly { get; set; }
        public Daily daily { get; set; }
        public Flags flags { get; set; }
    }
}