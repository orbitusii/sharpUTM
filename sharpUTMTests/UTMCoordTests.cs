using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sharpUTMTests
{
    [TestClass]
    public class UTMCoordTests
    {
        [TestMethod]
        public void TestFormatting ()
        {
            UTMCoord coord = new UTMCoord("31N", 0, 0);

            string expected = "31N 0mE 0mN";

            Assert.AreEqual(expected, coord.ToString());
        }

        [TestMethod]
        public void TestParsing ()
        {
            UTMCoord expected = new UTMCoord("31N", 166021, 0);
            string input = "31N 166021mE 0mN";

            UTMCoord? parsed;
            Assert.IsTrue(UTMCoord.TryParse(input, out parsed));
            Assert.IsTrue(expected.Equals(parsed), $"Assert.IsTrue failed. Expected:<{expected}>. Actual:<{parsed}>.");
        }

        [TestMethod]
        public void ConvertLatLonToUTM ()
        {
            UTMCoord expected = new UTMCoord("31N", 166022, 0);
            double inputlat = 0;
            double inputlon = 0;

            UTMCoord fromLatLon = UTMCoord.FromLatLon(inputlat, inputlon);

            Assert.AreEqual(expected, fromLatLon);
        }

        [TestMethod]
        public void ConvertLatLonToUTMAtMeridian()
        {
            UTMCoord expected = new UTMCoord("31N", 500000, 0);
            double inputlat = 0;
            double inputlon = 3;

            UTMCoord fromLatLon = UTMCoord.FromLatLon(inputlat, inputlon);

            Assert.AreEqual(expected, fromLatLon);
        }

        [TestMethod]
        public void ConvertUTMToLatLon ()
        {
            (double Lat, double Lon) expected = (5, 5);

            UTMCoord input = new UTMCoord("31N", 721753.35, 553001.72);

            var actual = input.ToLatLon();

            Assert.AreEqual(expected.Lat, actual.Lat);
            Assert.AreEqual(expected.Lon, actual.Lon);
        }
    }
}
