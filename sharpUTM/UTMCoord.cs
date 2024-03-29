﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace sharpUTM
{
    public class UTMCoord
    {
        internal static readonly Regex validator = new Regex(@"(?<zone>\d{2}[^IiOo])\s?(?<easting>\d+([.]\d+)?)(mE)?\s(?<northing>\d+([.]\d+)?)(mN)?");

        public UTMGlobe Globe { get; set; }
        public string ZoneName = string.Empty;
        public UTMZone Zone => Globe.Zones[ZoneName];
        public double Easting;
        public double Northing;

        public UTMCoord(string Zone, double Easting, double Northing)
        {
            this.ZoneName = Zone;
            this.Easting = Easting;
            this.Northing = Northing;
            this.Globe = UTMGlobe.Reference;
        }

        public UTMCoord(string Zone, double Easting, double Northing, UTMGlobe Globe)
        {
            this.ZoneName = Zone;
            this.Easting = Easting;
            this.Northing = Northing;
            this.Globe = Globe;
        }

        public static UTMCoord Parse(string input)
        {
            if (validator.IsMatch(input))
            {
                Match m = validator.Match(input);

                string zone = m.Groups["zone"].Value.ToUpper();
                string est_str = m.Groups["easting"].Value;
                string nor_str = m.Groups["northing"].Value;

                double est = double.TryParse(est_str, out double _est) ? _est : 0;
                double nor = double.TryParse(nor_str, out double _nor) ? _nor : 0;

                return new UTMCoord(zone, est, nor);
            }

            throw new InvalidOperationException($"The string \"{input}\" is not a valid UTM coordinate string!");
        }

        public static bool TryParse(string input, out UTMCoord? result)
        {
            try
            {
                result = Parse(input);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        public static UTMCoord FromLatLon(double Latitude, double Longitude)
        {
            return UTMCoord.FromLatLon(Latitude, Longitude, UTMGlobe.Reference);
        }

        public static UTMCoord FromLatLon(double Latitude, double Longitude, UTMGlobe Globe)
        {
            var _zone = Globe.ZoneForPoint((float)Latitude, (float)Longitude);

            double lat_rad = Trig.DegreesToRadians(Latitude);
            double lon_rad = Trig.DegreesToRadians(Longitude);
            double mer_rad = Trig.DegreesToRadians(_zone.Meridian);

            double sinLat = Math.Sin(lat_rad);
            double cosLon = Math.Cos(lon_rad - mer_rad);
            double sinLon = Math.Sin(lon_rad - mer_rad);

            double t = Math.Sinh(Trig.AtanH(sinLat));// - 2 * Trig.AtanH(2 * sinLat));
            double xiPrime = Math.Atan(t / cosLon);
            double etaPrime = Trig.AtanH(sinLon / Math.Sqrt(1 + (t * t)));

            double Easting = 500000 + (Globe.ScaleFactor * Globe.EarthRadius * etaPrime);
            double Northing = 0 + (Globe.ScaleFactor * Globe.EarthRadius * xiPrime);

            return new UTMCoord(_zone.Name, Math.Round(Easting), Math.Round(Northing), Globe);
        }

        public (double Lat, double Lon) ToLatLon()
        {
            double xi = Northing / (Globe.ScaleFactor * Globe.EarthRadius);
            double eta = (Easting - 500000) / (Globe.ScaleFactor * Globe.EarthRadius);

            double chi = Math.Asin(Math.Sin(xi) / Math.Cosh(eta));

            double Lat = Trig.RadiansToDegrees(chi);
            double Lon = Zone.Meridian + Trig.RadiansToDegrees(Math.Atan(Math.Sinh(eta) / Math.Cos(xi)));

            return (Lat, Lon);
        }

        public override string ToString()
        {
            return $"{ZoneName} {Easting}mE {Northing}mN";
        }

        public override bool Equals(object? obj)
        {
            if(obj is UTMCoord utm) return Equals(utm);
            return false;
        }

        public bool Equals(UTMCoord? other)
        {
            if (other is null) return false;

            bool matchzone = string.Equals(this.ZoneName, other.ZoneName);
            bool matchEst = this.Easting == other.Easting;
            bool matchNor = this.Northing == other.Northing;

            //Debug.WriteLine($"Zone: {matchzone}. Easting: {matchEst}. Northing: {matchNor}.");

            return matchzone && matchEst && matchNor;
        }
    }
}
