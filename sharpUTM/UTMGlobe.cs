using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sharpUTM
{
    public class UTMGlobe
    {
        public Dictionary<string, UTMZone> Zones { get; private set; }

        public UTMGlobe()
        {
            Zones = new Dictionary<string, UTMZone>();

            var rawZones = GenerateZones();

            foreach (var zone in rawZones)
            {
                Zones[zone.Name] = zone;
            }
        }

        internal static List<UTMZone> GenerateZones ()
        {
            var zones = new List<UTMZone> ();

            var PolarZoneA = UTMZone.Irregular(-90, -180, 180, 10).SetName("A");
            var PolarZoneB = UTMZone.Irregular(-90, 0, 180, 10).SetName("B");
            var PolarZoneY = UTMZone.Irregular(84, -180, 180, 6).SetName("Y");
            var PolarZoneZ = UTMZone.Irregular(84, 0, 180, 6).SetName("Z");

            // South Pole Zones
            zones.Add(PolarZoneA);
            zones.Add(PolarZoneB);

            // Add the regular zones
            for(int y = -80; y < 72; y += 8)
            {
                char latChar = GetLatChar(y);

                for (int x = 0; x < 60; x ++)
                {
                    int lon = (x - 30) * 6;
                    string zoneName = $"{x+1:d2}{latChar}";

                    UTMZone generated = GenerateRegularZones(y, lon, zoneName);

                    zones.Add(generated);
                }
            }

            // Add high latitude (72-84 N) zones, i.e. 01X through 60X
            char UpperLatChar = GetLatChar(72);
            int UpperLatStart = 72;
            
            for(int w = 0; w < 60; w++)
            {
                int lon = (w - 30) * 6;
                string zoneName = $"{w + 1}{UpperLatChar}";

                UTMZone? generated = GenerateHighLatitudeZones(UpperLatStart, lon, zoneName);

                if (generated is null) continue;

                zones.Add(generated);
            }

            // North Pole Zones
            zones.Add(PolarZoneY);
            zones.Add(PolarZoneZ);

            return zones;
        }

        /// <summary>
        /// Calculates the character suffix for a specific latitude band, exlcluding I and O. South of 80S it always returns 'A', North of 84N it always returns 'Z'.
        /// </summary>
        /// <param name="lat">The latitude in degrees</param>
        /// <returns>A character corresponding to the UTM latitude band's character suffix, excluding I and O.</returns>
        public static char GetLatChar (int lat)
        {
            if (lat < -80) return 'A';
            else if (lat > 84) return 'Z';

            int start = (int)'A';
            int offset = (lat + 80) / 8;

            int skipI = offset >= 6 ? 1 : 0;
            int skipO = offset >= 11 ? 1 : 0;

            return (char) (start + 2 + offset + skipI + skipO);
        }

        private static UTMZone GenerateRegularZones (int lat, int lon, string name)
        {
            UTMZone zone = name.ToUpper() switch
            {
                "31V" => UTMZone.Irregular(lat, lon, 3, 8),
                "32V" => UTMZone.Irregular(lat, lon - 3, 9, 8),
                _ => UTMZone.Regular(lat, lon)
            };

            return zone.SetName(name);
        }

        private static UTMZone? GenerateHighLatitudeZones (int lat, int lon, string name)
        {
            UTMZone? zone = name.ToUpper() switch
            {
                "31X" => UTMZone.Irregular(72, 0, 9, 12),
                "32X" => null,
                "33X" => UTMZone.Irregular(72, 9, 12, 12),
                "34X" => null,
                "35X" => UTMZone.Irregular(72, 21, 12, 12),
                "36X" => null,
                "37X" => UTMZone.Irregular(72, 33, 9, 12),
                _ => UTMZone.Irregular(lat, lon, 6, 12)
            };

            return zone?.SetName(name);
        }
    }
}
