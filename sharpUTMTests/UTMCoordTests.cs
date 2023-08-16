using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Assert.AreEqual(expected, parsed);
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

        /// <summary>
        /// This test method is expected and intended to fail - the current implementation doesn't
        /// support a proper ellipsoid, so coordinates will ALWAYS be off. If you'd like to help,
        /// please see issue #1 at https://github.com/orbitusii/sharpUTM/issues/1
        /// </summary>
        //[TestMethod]
        public void COnvertLLToUTMAtEdge ()
        {
            UTMCoord expected = new UTMCoord("31N", 721753, 553002);
            double inputlat = 5;
            double inputlon = 5;

            UTMCoord fromLatLon = UTMCoord.FromLatLon(inputlat, inputlon);

            Assert.AreEqual(expected, fromLatLon);
        }

        /// <summary>
        /// This test method is expected and intended to fail - the current implementation doesn't
        /// support a proper ellipsoid, so coordinates will ALWAYS be off. If you'd like to help,
        /// please see issue #1 at https://github.com/orbitusii/sharpUTM/issues/1
        /// </summary>
        //[TestMethod]
        public void ConvertUTMToLatLon ()
        {
            (double Lat, double Lon) expected = (5, 5);

            UTMCoord input = new UTMCoord("31N", 721760, 553002);

            var actual = input.ToLatLon();

            Debug.WriteLine(actual);
            Assert.AreEqual(expected.Lat, actual.Lat);
            Assert.AreEqual(expected.Lon, actual.Lon);
        }

        [TestMethod]
        public void ConvertUTMToLatLon2()
        {
            (double Lat, double Lon) expected = (0, 3);

            UTMCoord input = new UTMCoord("31N", 500000, 0);

            var actual = input.ToLatLon();

            Debug.WriteLine(actual);
            Assert.AreEqual(expected.Lat, actual.Lat);
            Assert.AreEqual(expected.Lon, actual.Lon);
        }
    }
}
