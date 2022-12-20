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

        /// <summary>
        /// The numerical precision of this coordinate, 1 => 10,000 meters, 5 = 1 meter
        /// </summary>
        public int Precision
        {
            get => _prec;
            set
            {
                _prec = value > 5 ? 5 : value < 1 ? 1 : value;
            }
        }
        private int _prec = 1;

        /// <summary>
        /// Easting coordinate (how many meters EAST of the grid square origin)
        /// </summary>
        public int Easting = 0;
        /// <summary>
        /// Northing coordinate (how many meters NORTH of the grid square origin)
        /// </summary>
        public int Northing = 0;

        /// <summary>
        /// Initializes a new instance of MGRSCoord, using the grid square and easting/northing
        /// values in meters (i.e. "AF 12 12" will be passed as ("AF", 12000, 12000))
        /// </summary>
        /// <param name="grid">The grid square, e.g. "AF"</param>
        /// <param name="east">The Easting coordinate, in meters</param>
        /// <param name="north">The Northing coordinate, in meters</param>
        /// <param name="utm">The UTM Zone this coordinate lies within</param>
        /// <param name="padded">If true (default), takes the east/north parameters as pure meters (i.e. with leading zeros to 5 digits).
        /// If false, takes the east/north parameters as MGRS coordinate values (i.e. no leading zeros, the coordinates are passed as you would see in an MGRS string)</param>
        public MGRSCoord(string grid, int east, int north, string utm = "", bool padded = true)
        {
            if(utm != string.Empty) 
                UTMZone = utm.ToUpper();

            GridSquare = grid.ToUpper();

            if(padded)
            {   // If the input coordinates are padded (default), pass them directly - the
                // values are in raw meters
                // e.g. new MGRSCoord("AF", 12, 12) will return "AF 00012 00012"
                Easting = east;
                Northing = north;
            }
            else
            {   // If the input coordinates are not padded, pad them with zeros - the values
                // are passed as they would be seen in an MGRS string
                // e.g. new MGRSCoord ("AF", 12, 12, padded: false) will return "AF 12 12"
                Easting = PadZeros(east);
                Northing = PadZeros(north);
            }

            Precision = AssumePrecision(Easting, Northing);
        }

        /// <summary>
        /// Checks the modulus of the east and north values against successive powers of 10,
        /// i.e. 10 (10e1), 100 (10e2), 1,000 (10e3), 10,000 (10e4), 100,000 (10e5)
        /// i.e. meters, 10s of meters, 100s of meters, kilometers, 10s of kilometers, via recursion,
        /// in order to truncate trailing zeros in the final coordinate string
        /// (i.e. "AF 12 12" is preferred over "AF 12000 12000")
        /// </summary>
        /// <param name="east"></param>
        /// <param name="north"></param>
        /// <param name="place"></param>
        /// <returns></returns>
        private static int AssumePrecision (int east, int north, int place = 5)
        {   // As stated, this works by comparings the modulus of each digit in the coordinate pairs,
            // looking for the farthest-right non-zero digits.
            // e.g. 12340 and 12300 (place = 5)
            //          ^         ^
            // check these two values. They're both zero, so check the next pair leftwards...
            //      12340 and 12300 (place = 4)
            //         ^         ^
            // One of these digits is non-zero, so return the current place value (4)

            // We can't check digits any farther left than the first in each coordinate pair
            if (place <= 1) return 1;

            int step = (int)Math.Pow(10, 6-place);

            // If both digits are zero, go one step leftwards and repeat
            if (east % step == 0 && north % step == 0)
                return AssumePrecision(east, north, place - 1);

            // If not (i.e. one of the digits is non-zero), this is our precision level
            return place;
        }

        /// <summary>
        /// Pads a raw value to 5 digits long using trailing zeros.
        /// </summary>
        /// <param name="raw"></param>
        /// <returns></returns>
        private static int PadZeros (int raw)
        {
            string tostring = raw.ToString();
            return int.Parse(tostring.PadRight(5, '0'));
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
