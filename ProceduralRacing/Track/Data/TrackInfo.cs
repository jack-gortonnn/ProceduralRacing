using System;

public sealed class TrackInfo
{
    public string RegionName { get; private set; }
    public string Name { get; private set; }
    public string BackgroundPath { get; private set; }

    private TrackInfo() { }

    public static TrackInfo Generate(int seed)
    {
        var rng = new Random(seed);

        var region = Regions[rng.Next(Regions.Length)];

        return new TrackInfo
        {
            RegionName = region.Name,
            BackgroundPath = region.BackgroundPath,
            Name = region.Generate(rng)
        };
    }

    private sealed class Region
    {
        public string Name;
        public string BackgroundPath;
        public string[] Terms;
        public string[] Formats;
        public string[] Locations;

        public string Generate(Random rng)
        {
            string format = Formats[rng.Next(Formats.Length)];

            return format
                .Replace("{t}", Terms[rng.Next(Terms.Length)])
                .Replace("{l}", Locations[rng.Next(Locations.Length)]);
        }
    }

    private static readonly Region[] Regions =
    {
        new Region
        {
            Name = "Europe",
            BackgroundPath = "textures/backgrounds/europe",
            Formats = new[]
            {
                "{t} di {l}",
                "{t} de {l}",
                "{t} of {l}",
                "{t} {l}",
                "{l} {t}",
                "{t} Nazionale {l}",
            },
            Terms = new[] { 
                "Autodromo", "Circuit", "Circuito", "Ring", 
                "Grand Prix", "Motodromo", "Speedring" 
            },
            Locations = new[]
            {
                "Lisbon", "Madrid", "Athens", "Oslo", "Stockholm", "Helsinki", "Copenhagen", "Warsaw",
                "Prague", "Budapest", "Bucharest", "Sofia", "Belgrade", "Zurich", "Vienna", "Amsterdam",
                "Brussels", "Dublin", "Luxembourg", "Ljubljana", "Bratislava", "Talinn",
                "Riga", "Vilnius", "Bern", "Andorra", "San Marino", "Valletta", "Nicosia",
                "Skopje", "Podgorica", "Sarajevo", "Tirana", "Kyiv", "Minsk", "Chisinau", "Glasgow", "Cardiff"
            }
        },

        new Region
        {
            Name = "Middle East",
            BackgroundPath = "textures/backgrounds/middleeast",
            Formats = new[]
            {
                "{l} {t}",
                "{t} {l}",
                "{l} Circuit",
                "{l} Speedway",
                "{t} of {l}",
                "{l} Motor Park",
                "{l} Grand Prix Track"
            },
            Terms = new[]
            {
                "International Circuit", "Motor Racing Circuit", "Grand Prix Track", "Speedway", "Autodrome",
                "Racing Park", "Corniche Circuit"
            },
            Locations = new[]
            {
                "Dubai", "Muscat", "Kuwait", "Riyadh", "Beirut", "Amman", "Doha",
                "Baghdad", "Damascus", "Tehran", "Ankara", "Cairo", "Algiers", "Rabat",
                "Tunis", "Sanaa", "Nicosia", "Corfu", "Sulaymaniyah",
                "Basra", "Kandahar", "Herat", "Mazar-i-Sharif", "Jeddah", "Mecca", "Medina", "Sharjah", "Al Ain"
            }
        },

        new Region
        {
            Name = "Asia",
            BackgroundPath = "textures/backgrounds/asia",
            Formats = new[]
            {
                "{l} {t}",
                "{t} {l}",
                "{t} of {l}",

            },
            Terms = new[]
            {
                "International Circuit", "Motor Racing Circuit", "Circuit", "Track", "Speedway",
                "Autodrome", "Racing Arena"
            },
            Locations = new[]
            {
                "Bangkok", "Seoul", "Hanoi", "Jakarta", "Mumbai", "Tokyo", "Beijing", "Shanghai",
                "Delhi", "Karachi", "Dhaka", "Manila", "Kuala Lumpur", "Ho Chi Minh", "Kathmandu", "Thimphu", "Colombo",
                "Kabul", "Tashkent", "Dushanbe", "Bishkek", "Astana", "Ashgabat", "Yerevan", "Tbilisi",
                "Almaty", "Taipei", "Hong Kong", "Macau"
            }
        },

        new Region
        {
            Name = "Americas",
            BackgroundPath = "textures/backgrounds/americas",
            Formats = new[]
            {
                "{l} {t}",
                "{t} {l}",
                "{t} of {l}",
            },
            Terms = new[]
            {
                "Raceway", "Motor Speedway", "International Raceway", "Speedway", "Autodrome",
                "Grand Prix Circuit", "Motorsport Park", "Racing Complex"
            },
            Locations = new[]
            {
                "Buenos Aires", "Toronto", "Vancouver", "Santiago", "Lima", "Bogota", "Quito",
                "Caracas", "Montevideo", "Asuncion", "La Paz", "Brasilia", "Rio de Janeiro", "Sao Paulo",
                "San Francisco", "Seattle", "Denver", "Phoenix", "Houston", "Miami", "Atlanta", "Chicago",
                "Boston", "Ottawa", "Montreal", "Calgary", "Edmonton", "Kingston", "Havana", "San Juan",
                "Santo Domingo", "Managua", "San Jose", "Panama City", "Tegucigalpa"
            }
        },

        new Region
        {
            Name = "Oceania",
            BackgroundPath = "textures/backgrounds/oceania",
            Formats = new[]
            {
                "{l} {t}",
                "{t} of {l}",
            },
            Terms = new[]
            {
                "Circuit", "Raceway", "International Circuit", "Speedway", "Grand Prix Track",
                "Motorsport Park"
            },
            Locations = new[]
            {
                "Sydney", "Melbourne", "Auckland", "Christchurch", "Wellington", "Brisbane", "Perth",
                "Adelaide", "Canberra", "Hobart", "Darwin", "Suva", "Port Moresby", "Honolulu",
                "Suva", "Rarotonga", "Noumea",
            }
        }
    };
}