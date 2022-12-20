using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sharpUTM.MGRS;

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
            { "q", new mgrsTestOutput{valid = false} },
            { "aa 1212", new mgrsTestOutput{valid = true, coord = new MGRSCoord("AA", 12000, 12000)} },
            { "ab1212", new mgrsTestOutput{valid = true, coord = new MGRSCoord("Ab", 12000, 12000)} },
        };

        [TestInitialize]
        public void Init ()
        {
        }

        [TestMethod]
        public void TestMGRSParsing ()
        {
            MGRSCoord coord = new MGRSCoord("", 0, 0);

            foreach (var sample in samples)
            {
                coord.UTMZone = string.Empty;

                Assert.AreEqual(sample.Value.valid, MGRSCoord.TryParse(sample.Key, ref coord));

                if(sample.Value.coord is not null)
                {
                    Assert.IsTrue(sample.Value.coord.Equals(coord),
                        $"Expected {sample.Value.coord} with precision {sample.Value.coord.Precision}, " +
                        $"got {coord} with precision {coord.Precision}");
                }
            }
        }

        [TestMethod]
        public void TestMGRSToString ()
        {
            MGRSCoord coord = new MGRSCoord("AF", 12340, 12340, "12P");
            string expected = "12P AF 1234 1234";

            Console.WriteLine($"Expected : {expected}");
            Console.WriteLine($"Actual   : {coord}");

            Assert.AreEqual(expected, coord.ToString());
        }
    }

    internal class mgrsTestOutput
    {
        internal bool valid = false;
        internal MGRSCoord? coord = null;
    }
}
