namespace sharpUTM
{
    public class UTMGlobe
    {
        public static UTMGlobe Reference = new UTMGlobe();

        public Dictionary<string, UTMZone> Zones { get; private set; }
        /// <summary>
        /// Scale factor for the mercator projection. Used to correct for some error in the math used during coordinate conversion.
        /// </summary>
        public double ScaleFactor = 0.9996;
        /// <summary>
        /// Earth radius at the equator, in meters.
        /// </summary>
        public double EarthRadius = 6378137.0;

        /// <summary>
        /// Gets the UTMZone that a point lies within
        /// </summary>
        /// <param name="lat">Latitude, in decimal degrees</param>
        /// <param name="lon">Longitude, in decimal degrees</param>
        /// <returns>A reference to the UTMZone that contains this point</returns>
        public UTMZone ZoneForPoint(float lat, float lon)
        {
            string designator = ZoneDesignatorForPoint(lat, lon);
            UTMZone zone = Zones[designator];

            if (!zone.Contains(lat, lon))
                throw new ArgumentException($"The point specified does not lie within the bounds of the Zone retrieved!\n" +
                    $"Expected Lat {zone.Bottom} to {zone.Top}, got {lat}\n" +
                    $"Expected Lon {zone.Left} to {zone.Right}, got {lon}\n" +
                    $"This should never happen. Please create an Issue on Github at https://github.com/orbitusii/sharpUTM/issues");

            return zone;
        }

        /// <summary>
        /// Gets the Zone designator (or name) for a lat-lon point.
        /// </summary>
        /// <param name="Lat">Latitude, in decimal degrees</param>
        /// <param name="Lon">Longitude, in decimal degrees</param>
        /// <returns>The designator, or name, of the zone this point lies within (a string)</returns>
        public string ZoneDesignatorForPoint(float Lat, float Lon)
        {
            // Wrap and clamp Longitude and Latitude values to avoid issues
            Lon = Math.Clamp(Lon >= 180 ? Lon - 360 : Lon, -180, 180);
            Lat = Math.Clamp(Lat, -90, 90);

            int floorLat = (int)Math.Floor(Lat);
            int floorLon = (int)Math.Floor(Lon / 6);

            char latChar = GetLatChar(floorLat);

            // This nested switch accounts for irregular zone sizing in the latitude bands
            // X, from 72 to 84 North, and V, from 56 to 64 North
            int lonIndex = latChar switch
            {
                'V' => Lon switch
                {
                    >= 0 and < 3 => 31,
                    >= 3 and < 12 => 32,
                    _ => floorLon + 31,
                },
                'X' => Lon switch
                {
                    >= 0 and < 9 => 31,
                    >= 9 and < 21 => 33,
                    >= 21 and < 33 => 35,
                    >= 33 and < 42 => 37,
                    _ => floorLon + 31,
                },
                _ => floorLon + 31,
            };

            // This switch accounts for irregular zone sizing and naming at the poles
            // when compared to all other zones.
            return latChar switch
            {
                // Correct the character used for naming polar zones based on the point's longitude
                'A' => $"{(char)(latChar + (Lon < 0 ? 0 : 1))}",
                'Z' => $"{(char)(latChar + (Lon < 0 ? -1 : 0))}",
                _ => $"{lonIndex:d2}{latChar}",
            };
        }

        /// <summary>
        /// Initializes a new instance of UTMGlobe
        /// </summary>
        public UTMGlobe()
        {
            Zones = new Dictionary<string, UTMZone>();

            var rawZones = GenerateZones();

            foreach (var zone in rawZones)
            {
                Zones[zone.Name] = zone;
            }
        }

        private static List<UTMZone> GenerateZones()
        {
            var zones = new List<UTMZone>();

            var PolarZoneA = UTMZone.Irregular(-90, -180, 180, 10, 0).SetName("A");
            var PolarZoneB = UTMZone.Irregular(-90, 0, 180, 10, 0).SetName("B");
            var PolarZoneY = UTMZone.Irregular(84, -180, 180, 6, 0).SetName("Y");
            var PolarZoneZ = UTMZone.Irregular(84, 0, 180, 6, 0).SetName("Z");

            // South Pole Zones
            zones.Add(PolarZoneA);
            zones.Add(PolarZoneB);

            // Add the regular zones
            for (int y = -80; y < 72; y += 8)
            {
                char latChar = GetLatChar(y);

                for (int x = 0; x < 60; x++)
                {
                    int lon = (x - 30) * 6;
                    string zoneName = $"{x + 1:d2}{latChar}";

                    UTMZone generated = GenerateRegularZones(y, lon, zoneName);

                    zones.Add(generated);
                }
            }

            // Add high latitude (72-84 N) zones, i.e. 01X through 60X
            char UpperLatChar = GetLatChar(72);
            int UpperLatStart = 72;

            for (int w = 0; w < 60; w++)
            {
                int lon = (w - 30) * 6;
                string zoneName = $"{w + 1:d2}{UpperLatChar}";

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
        /// Calculates the character suffix for a specific latitude band, excluding I and O. South of 80S it always returns 'A', North of 84N it always returns 'Z'.
        /// </summary>
        /// <param name="lat">The latitude in degrees</param>
        /// <returns>A character corresponding to the UTM latitude band's character suffix, excluding I and O.</returns>
        public static char GetLatChar(int lat)
        {
            int start = (int)'A';
            int offset = (lat + 80) / 8;

            //I and O are skipped in UTM due to their similarity to one and zero
            int skipI = offset >= 6 ? 1 : 0;
            int skipO = offset >= 11 ? 1 : 0;

            return lat switch
            {
                < -80 => 'A',
                >= 84 => 'Z',
                >= 72 => 'X',
                _ => (char)(start + 2 + offset + skipI + skipO)
            };
        }

        private static UTMZone GenerateRegularZones(int lat, int lon, string name)
        {
            UTMZone zone = name.ToUpper() switch
            {
                "31V" => UTMZone.Irregular(lat, lon, 3, 8, 3),
                "32V" => UTMZone.Irregular(lat, lon - 3, 9, 8, 9),
                _ => UTMZone.Regular(lat, lon)
            };

            return zone.SetName(name);
        }

        private static UTMZone? GenerateHighLatitudeZones(int lat, int lon, string name)
        {
            UTMZone? zone = name.ToUpper() switch
            {
                "32X" or "34X" or "36X" => null,
                "31X" => UTMZone.Irregular(72, 0, 9, 12, 3),
                "33X" => UTMZone.Irregular(72, 9, 12, 12, 15),
                "35X" => UTMZone.Irregular(72, 21, 12, 12, 27),
                "37X" => UTMZone.Irregular(72, 33, 9, 12, 39),
                _ => UTMZone.Irregular(lat, lon, 6, 12, lon+3)
            };

            return zone?.SetName(name);
        }
    }
}
