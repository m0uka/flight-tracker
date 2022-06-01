using System;
using System.Text.Json.Serialization;
using FlightTracker.JsonConverters;

namespace FlightTracker.Objects
{
    public class StateVector
    {
        public string Icao24 { get; set; }
        public string Callsign { get; set; }
        
        [JsonPropertyName("origin_country")]
        public string OriginCountry { get; set; }
        
        [JsonPropertyName("time_position")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime TimePosition { get; set; }
        
        [JsonPropertyName("last_contact")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime LastContact { get; set; }
        
        public float Longitude { get; set; }
        public float Latitude { get; set; }
        
        [JsonPropertyName("baro_altitude")]
        public float BaroAltitude { get; set; }
        
        [JsonPropertyName("on_ground")]
        public bool OnGround { get; set; }
        
        public float Velocity { get; set; }
        
        [JsonPropertyName("true_track")]
        public float TrueTrack { get; set; }
        
        [JsonPropertyName("vertical_rate")]
        public float VerticalRate { get; set; }
        
        public int[] Sensors { get; set; }
        
        [JsonPropertyName("geo_altitude")]
        public float GeoAltitude { get; set; }
        
        public string Squawk { get; set; }
        public bool Spi { get; set; }
        
        [JsonPropertyName("position_source")]
        public int PositionSource { get; set; }
    }
}