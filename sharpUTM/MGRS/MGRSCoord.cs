using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace sharpUTM.MGRS
{
    public class MGRSCoord
    {
        /// <summary>
        /// Regular expression used for validating a string as an MGRS coordinate
        /// </summary>
        internal static Regex validator = new Regex(@"(?<utmzone>\d{0,2}\p{L}{1})?[\ ]?(?<mgrsgrid>\p{L}{2})[\ ]?((?<coordinate>\d{1,10})[\ ]?){1,2}");

        public string UTMZone = string.Empty;
        public string GridSquare = string.Empty;

        public int Precision
        {
            get => _prec;
            set
            {
                _prec = value > 5 ? 5 : value < 1 ? 1 : value;
            }
        }
        private int _prec = 1;
        public int Easting = 0;
        public int Northing = 0;

        public MGRSCoord(string grid, int east, int north, string utm = "")
        {
            if(utm != string.Empty) 
                UTMZone = utm.ToUpper();

            GridSquare = grid.ToUpper();
            Easting= east;
            Northing = north;

            Precision = assumePrecision(east, north);
        }

        private int assumePrecision (int east, int north, int place = 5)
        {
            if (place <= 1) return 1;

            int step = (int)Math.Pow(10, 6-place);

            if (east % step != 0 || north % step != 0) return place;
            else return assumePrecision(east, north, place-1);
        }

        public static bool TryParse (string value, ref MGRSCoord coord)
        {
            if (!validator.IsMatch(value))
                return false;

            var match = validator.Match(value);

            var coordinateGroup = match.Groups["coordinate"];

            string c1, c2;

            if (coordinateGroup.Captures.Count == 1)
            {   // we have a single capture for coordinates - the string will need to be split
                string rawCoord = coordinateGroup.Value;

                // If the coordinate capture is too short or is not an even number of values,
                // i.e. 2, 4, 6, 8, or 10, we can't parse it, fail the parse
                if (rawCoord.Length < 2 && rawCoord.Length % 2 != 0)
                    return false;

                int splitPos = rawCoord.Length / 2;

                c1 = rawCoord.Substring(0, splitPos);
                c2 = rawCoord.Substring(splitPos, splitPos);
            }
            else
            {   // we have two or more captures for coordinates (idk why we'd have 3 or more,
                // the regex should limit it to two captures at most)
                c1 = coordinateGroup.Captures[0].Value;
                c2 = coordinateGroup.Captures[1].Value;
            }

            // Make sure the coordinate strings are equal in length and are successfully parsed
            // before assigning them to the coordinate reference
            if (c1.Length == c2.Length)
            {
                coord.Precision = c1.Length;

                c1 = c1.PadRight(5, '0');
                c2 = c2.PadRight(5, '0');

                if (int.TryParse(c1, out int east)
                && int.TryParse(c2, out int north))
                {
                    coord.Easting = east;
                    coord.Northing = north;
                }
                else return false;
            }
            else return false;

            // Assign the UTMZone, an optional value, if it exists
            if (match.Groups["utmzone"].Value != string.Empty)
                coord.UTMZone = match.Groups["utmzone"].Value.ToUpper();
            
            // Assign the MGRSGrid to the referenced coordinate
            coord.GridSquare = match.Groups["mgrsgrid"].Value.ToUpper();

            return true;
        }

        public bool Equals(MGRSCoord? other)
        {
            if (other is null) return false;

            bool matchingZone = UTMZone == other.UTMZone;
            bool matchingGrid = GridSquare == other.GridSquare;
            bool matchEast = Easting == other.Easting;
            bool matchNorth = Northing == other.Northing;

            return matchingZone && matchingGrid && matchEast && matchNorth;
        }

        public override string ToString()
        {
            string formatString = $"{{0:D{Precision}}}";

            string c1 = string.Format(formatString, Easting).Substring(0, Precision);
            string c2 = string.Format(formatString, Northing).Substring(0, Precision);

            return $"{UTMZone}{(UTMZone != string.Empty ? " " : "")}{GridSquare} {c1} {c2}";
        }
    }
}
