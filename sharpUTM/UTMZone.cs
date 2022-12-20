namespace sharpUTM
{
    public class UTMZone
    {
        public string Name { get; private set; } = string.Empty;
        public float Left { get; private set; }
        public float Right { get; private set; }
        public float Top { get; private set; }
        public float Bottom { get; private set; }
        public float Width => Right - Left;
        public float Height => Top - Bottom;

        public bool IsRegular => Width == 6.0f && Height == 8.0f;

        public bool Contains(float lat, float lon)
        {
            bool inLat = lat < Top && lat >= Bottom;
            bool inLon = lon < Right && lon >= Left;

            return inLat && inLon;
        }

        private UTMZone(float startLat, float startLon, float width = 6f, float height = 8f)
        {
            this.Bottom = startLat;
            this.Top = Bottom + height;

            this.Left = startLon;
            this.Right = Left + width;
        }

        internal UTMZone SetName(string name)
        {
            this.Name = name;
            return this;
        }

        internal static UTMZone Regular(float startLat, float startLon)
        {
            return new UTMZone(startLat, startLon);
        }

        internal static UTMZone Irregular(float startLat, float startLon, float width, float height)
        {
            return new UTMZone(startLat, startLon, width, height);
        }
    }
}