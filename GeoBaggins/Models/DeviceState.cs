using System;

namespace GeoBaggins.Models;

public class DeviceState
{
    public int Id { get; init; }
    public string DeviceId { get; init; }
    public int? GeoZoneId { get; set; }
    public DateTime? LastNotificationTime { get; set; }
}