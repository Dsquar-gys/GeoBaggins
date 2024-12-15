using System;
using System.Runtime.Serialization;

namespace GeoBaggins.Models;

[DataContract]
public record LocationDto
{
    [DataMember]
    public double Latitude { get; init; }
    
    [DataMember]
    public double Longitude { get; init; }
    
    [DataMember]
    public DateTime TimeStamp { get; init; }
}