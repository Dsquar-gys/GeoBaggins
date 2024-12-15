using GeoBaggins.AdminApp.Models;

namespace GeoBaggins.Server;

public static class Extends
{
    public static bool IsWithinGeoZone(double userLatitude, double userLongitude, AnchorZone geoZone)
    {
        const double earthRadius = 6371000;

        var lat1 = userLatitude * Math.PI / 180;
        var lon1 = userLongitude * Math.PI / 180;
        var lat2 = geoZone.Latitude * Math.PI / 180;
        var lon2 = geoZone.Longitude * Math.PI / 180;

        // Разница между координатами
        var dlat = lat2 - lat1;
        var dlon = lon2 - lon1;

        // Применяем формулу Haversine
        var a = Math.Sin(dlat / 2) * Math.Sin(dlat / 2) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                Math.Sin(dlon / 2) * Math.Sin(dlon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        // Расстояние между точками
        var distance = earthRadius * c;

        return distance <= geoZone.Radius * 1000;
    }

}