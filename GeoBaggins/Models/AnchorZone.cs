using System;

namespace GeoBaggins.AdminApp.Models;

public class AnchorZone
{
    public int Id { get; init; }
    public string Address { get; set; } = string.Empty;
    public double Longitude { get; set; }
    public double Latitude { get; set; }
    public double Radius { get; set; }
    public string Message { get; set; } = string.Empty;
}