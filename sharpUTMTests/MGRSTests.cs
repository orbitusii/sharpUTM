﻿using sharpUTM.MGRS;

namespace sharpUTMTests
{
    [TestClass]
    public class MGRSTests
    {
        internal static Dictionary<string, mgrsTestOutput> samples = new Dictionary<string, mgrsTestOutput>
        {
            { "QK 4527 0745", new mgrsTestOutput{valid = true, coord = new MGRSCoord("QK", 45270, 07450) } },
            { "48T CQ 18749 17382", new mgrsTestOutput { valid = true, coord = new MGRSCoord("CQ", 18749, 17382, "48T") } },
            { "31S XH 6910 1957", new mgrsTestOutput { valid = true, coord = new MGRSCoord("XH", 69100, 19570, "31S") } },
            { "69F DN 69696 96969", new mgrsTestOutput { valid = true, coord = new MGRSCoord("DN", 69696, 96969, "69F") } },
            { "01P FF 1234512345", new mgrsTestOutput { valid = true, coord = new MGRSCoord("FF", 12345, 12345, "01P") } },
            { "60X FF 12341234", new mgrsTestOutput { valid = true, coord = new MGRSCoord("FF", 12340, 12340, "60X") } },
            { "30U YB 05054 53454", new mgrsTestOutput { valid = true, coord = new MGRSCoord("YB", 05054, 53454, "30U") } },
            { "13T DE 93164 91705", new mgrsTestOutput { valid = true, coord = new MGRSCoord("DE", 93164, 91705, "13T") } },
            { "aa 1212", new mgrsTestOutput{valid = true, coord = new MGRSCoord("AA", 12000, 12000)} },
            { "ab1212", new mgrsTestOutput{valid = true, coord = new MGRSCoord("Ab", 12000, 12000)} },
            { "af121", new mgrsTestOutput{valid = false } }, // invalid because we don't have an even number of digits in the coordinate pair!
            { "af 12 123", new mgrsTestOutput{valid = false } }, // invalid because the number of digits isn't equal!
            { "q", new mgrsTestOutput{valid = false} }, // invalid because it doesn't match the regex
            { "q 12 12", new mgrsTestOutput{valid = false } }, // invalid because the grid square isn't two characters long

        };

        [TestInitialize]
        public void Init()
        {
        }

        [TestMethod]
        public void TestMGRSParsing()
        {
            MGRSCoord coord = new MGRSCoord("", 0, 0);

            foreach (var sample in samples)
            {
                coord.UTMZone = string.Empty;

                Assert.AreEqual(sample.Value.valid, MGRSCoord.TryParse(sample.Key, ref coord));

                if (sample.Value.coord is not null)
                {
                    Assert.IsTrue(sample.Value.coord.Equals(coord),
                        $"Expected {sample.Value.coord} with precision {sample.Value.coord.Precision}, " +
                        $"got {coord} with precision {coord.Precision}");
                }
            }
        }

        [TestMethod]
        public void TestMGRSToString()
        {
            MGRSCoord coord = new MGRSCoord("AF", 12340, 12300, "12P");
            string expected = "12P AF 1234 1230";

            Console.WriteLine($"Expected : {expected}");
            Console.WriteLine($"Actual   : {coord}");

            Assert.AreEqual(expected, coord.ToString());
        }

        [TestMethod]
        public void TestPrecisionChange()
        {
            MGRSCoord coord = new MGRSCoord("AF", 1, 1, padded: false);
            string expect0 = "AF 1 1";

            Assert.AreEqual(1, coord.Precision);
            Assert.AreEqual(expect0, coord.ToString());
            Console.WriteLine($"Expected : {expect0}");
            Console.WriteLine($"Actual   : {coord}");

            // Increase the coordinate's precision to 5 by force
            coord.Precision = 5;
            string expect1 = "AF 10000 10000";
            Console.WriteLine("Precision changed!");

            Assert.AreEqual(5, coord.Precision);
            Assert.AreEqual(expect1, coord.ToString());
            Console.WriteLine($"Expected : {expect1}");
            Console.WriteLine($"Actual   : {coord}");
        }

        [TestMethod]
        public void TestMGRSUnPadded()
        {
            MGRSCoord coord = new MGRSCoord("AF", 12, 12, padded: false);
            string expected = "AF 12 12";

            Assert.AreEqual(expected, coord.ToString());
        }

        [TestMethod]
        public void ConvertFromUTM()
        {
            MGRSCoord expected = new MGRSCoord("BA", 12, 12, "31N");

            UTMCoord input = new UTMCoord("31N", 200012, 12);

            Assert.AreEqual(expected, MGRSCoord.FromUTM(input));
        }

        [TestMethod]
        public void ConvertFromLatLon ()
        {
            MGRSCoord expected = new MGRSCoord("BA", 12, 12, "31N");

            double lat = 0.00012;
            double lon = 0.30509;

            Assert.AreEqual(expected, MGRSCoord.FromLatLon(lat, lon));
        }

        [TestMethod]
        public void ConvertToUTM()
        {
            MGRSCoord input = new MGRSCoord("BA", 12, 12, "31N");

            UTMCoord expected = new UTMCoord("31N", 200012, 12);

            Assert.AreEqual(expected, input.ToUTM());
        }

        [TestMethod]
        public void ConvertToLatLon()
        {
            MGRSCoord input = new MGRSCoord("BA", 12, 12, "31N");

            double expectedlat = 0.00012;
            double expectedlon = 0.30509;

            var result = input.ToLatLon();

            Assert.AreEqual(expectedlat, result.Lat);
            Assert.AreEqual(expectedlon, result.Lon);
        }
    }

    internal class mgrsTestOutput
    {
        internal bool valid = false;
        internal MGRSCoord? coord = null;
    }
}
