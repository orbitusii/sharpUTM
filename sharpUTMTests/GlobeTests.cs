namespace sharpUTMTests
{
    [TestClass]
    public class GlobeTests
    {
        UTMGlobe globe = new UTMGlobe();
        List<(float lat, float lon, string zone)> testSamples = new List<(float, float, string)>();

        float sampleStepSize = 1f;

        [TestInitialize]
        public void Init()
        {
            GenerateSamples();
        }

        public void GenerateSamples()
        {
            foreach (var zone in globe.Zones.Values)
            {
                for (float y = zone.Bottom; y < zone.Top; y += sampleStepSize)
                {
                    for (float x = zone.Left; x < zone.Right; x += sampleStepSize)
                    {
                        testSamples.Add((y, x, zone.Name));
                    }
                }
            }
        }

        [TestMethod]
        public void TestLatChars()
        {
            char[] validChars = "CDEFGHJKLMNPQRSTUVWX".ToCharArray();

            for (int i = 0; i < validChars.Length; i++)
            {
                int lat = (i * 8) - 80;

                char expected = validChars[i];
                char actual = UTMGlobe.GetLatChar(lat);

                Assert.IsTrue(actual == expected, $"[{i}] Expected {expected} but got {actual}");
            }
        }

        [TestMethod]
        public void TestPolarLatChars()
        {
            char actualA = UTMGlobe.GetLatChar(-81);
            char actualZ = UTMGlobe.GetLatChar(85);

            Assert.IsTrue(actualA == 'A');
            Assert.IsTrue(actualZ == 'Z');
        }

        [TestMethod]
        public void TestRegularZones()
        {
            List<UTMZone> zones = globe.Zones.Values.ToList();
            int count = zones.Count;
            int index = 2;

            Console.WriteLine($"Skipping zones {zones[0].Name} and {zones[1].Name}");
            Console.WriteLine($"Total Zones: {count}, should be {60 * 20 + 1}");

            Console.WriteLine($"First zone: {zones[2].Name}, starting at point {zones[2].Left}, {zones[2].Bottom}");
            for (int y = 0; y < 19; y++)
            {
                char expectedChar = UTMGlobe.GetLatChar((y * 8) - 80);

                for (int x = 0; x < 60; x++)
                {
                    string expectedName = $"{x + 1:d2}{expectedChar}";
                    UTMZone zone = zones[index++];

                    Assert.IsTrue(zone.Name == expectedName, $"[{x}, {y}] Expected {expectedName}, got {zone.Name}");
                }
            }

            Console.WriteLine($"Last Regular zone checked: [{index - 1}]{zones[index - 1].Name}, ending at point {zones[index - 1].Top}, {zones[index - 1].Right}");
            Console.WriteLine($"Zones left over: {count - index}");
        }

        [TestMethod]
        public void Test31VAnd31VWidth()
        {
            Assert.IsTrue(globe.Zones["31V"].Width == 3.0f, $"31V is not 3 degrees wide, is actually {globe.Zones["31V"].Width}");

            Assert.IsTrue(globe.Zones["32V"].Left == 3.0f, $"Left edge of 32V should be at 3.0 E, is actually at {globe.Zones["32V"].Left}");
            Assert.IsTrue(globe.Zones["32V"].Width == 9.0f, $"32V is not 9 degrees wide, is actually {globe.Zones["32V"].Width}");
        }

        [TestMethod]
        public void TestInvalidHighLatZones()
        {
            Assert.ThrowsException<KeyNotFoundException>(() => { var z = globe.Zones["32X"]; });
            Assert.ThrowsException<KeyNotFoundException>(() => { var z = globe.Zones["34X"]; });
            Assert.ThrowsException<KeyNotFoundException>(() => { var z = globe.Zones["36X"]; });
        }

        [TestMethod]
        public void TestHighLatChar()
        {
            char expected = 'X';
            char actual = UTMGlobe.GetLatChar(72);

            Assert.IsTrue(expected == actual, $"Expected 'X', got '{actual}'");
        }

        [TestMethod]
        public void TestWideHighLatZones()
        {
            Assert.IsTrue(globe.Zones["31X"].Width == 9f, $"Zone 31X is not 9 degrees wide, is actually {globe.Zones["31X"].Width}");
            Assert.IsTrue(globe.Zones["33X"].Width == 12f, $"Zone 31X is not 12 degrees wide, is actually {globe.Zones["33X"].Width}");
            Assert.IsTrue(globe.Zones["35X"].Width == 12f, $"Zone 31X is not 12 degrees wide, is actually {globe.Zones["35X"].Width}");
            Assert.IsTrue(globe.Zones["37X"].Width == 9f, $"Zone 31X is not 9 degrees wide, is actually {globe.Zones["37X"].Width}");
            Assert.IsTrue(globe.Zones["38X"].Left == 42f);
        }

        [TestMethod]
        public void TestZonesForManyPoints()
        {
            int count = testSamples.Count;
            Console.WriteLine($"Testing for {count} points...");

            foreach (var sample in testSamples)
            {
                string expected = sample.zone;
                string actual = globe.ZoneDesignatorForPoint(sample.lat, sample.lon);

                Assert.IsTrue(expected.Equals(actual), $"Expected <{expected}>, got <{actual}> for ({sample.lat}, {sample.lon})");
            }

            Console.WriteLine("Success!");
        }

        [TestMethod]
        public void TestZoneForEdges()
        {
            float lat1 = -90;
            float lat2 = 90;

            float lon1 = -180;
            float lon2 = 180;

            Assert.AreEqual("B", globe.ZoneDesignatorForPoint(lat1, 0));
            Assert.AreEqual("Z", globe.ZoneDesignatorForPoint(lat2, 0));

            Assert.AreEqual("01N", globe.ZoneDesignatorForPoint(0, lon1));
            Assert.AreEqual("01N", globe.ZoneDesignatorForPoint(0, lon2));
        }

        [TestMethod]
        public void TestZoneReferences()
        {
            int count = testSamples.Count;
            Console.WriteLine($"Testing for {count} points...");

            foreach (var sample in testSamples)
            {
                Assert.IsNotNull(globe.ZoneForPoint(sample.lat, sample.lon));
            }

            Console.WriteLine("Success!");
        }

        [TestMethod]
        public void DumpZonesToFile()
        {
            string path = Path.Combine(Environment.CurrentDirectory, "Zones.txt");
            DateTime dateTime = DateTime.Now;

            Console.WriteLine($"Dumping zones to {path}");
            Console.WriteLine($"{dateTime:yyyy-mm-dd @ HH:mm:ss}");

            if (!File.Exists(path))
            {
                File.Create(path).Close();
            }

            using var stream = new FileStream(path, FileMode.Truncate);
            using var writer = new StreamWriter(stream);

            writer.WriteLine($"UTM Globe Zones ({dateTime:yyyy-mm-dd @ HH:mm:ss})");

            foreach (var pair in globe.Zones)
            {
                UTMZone zone = pair.Value;

                writer.WriteLine($"{zone.Name} => Lat: {zone.Bottom} to {zone.Top} / Lon: {zone.Left} to {zone.Right} / {(zone.IsRegular ? "Regular" : "Irregular")}");
            }
        }
    }
}