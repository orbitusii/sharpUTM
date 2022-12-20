# sharpUTM
## A Universal Transverse Mercator (UTM) coordinate implementation in C#

## Summary
sharpUTM is a quick and dirty UTM implementation primarily aimed at supporting LOKI-BMS.
Create an instance of the `UTMGlobe` class to get started.
- This instance contains a dictionary of UTMZones, which are compliant with the modern zone layouts and sizes.
- UTMZones can be referenced by their zone designation, e.g. "18T" for the zone that contains New York City, NY, USA.
- UTMZones have a Contains() method to check if a specific point (in Latitude & Longitude) is contained within the zone.

### NOTE ON ELLIPSOIDS
Until further notice, sharpUTM will assume Earth is a **sphere.** In the future I may implement support for the WGS-84 Ellipsoid, but for now sharpUTM only uses a spherical earth model.