using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using FlightTracker.Contracts;
using FlightTracker.Objects;

namespace FlightTracker.Services
{
    public class AircraftService : IAircraftService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AircraftService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<StateVector?> GetStateVectors(string icao)
        {
            var httpClient = _httpClientFactory.CreateClient("OpenSky");
            var response = await httpClient.GetAsync($"states/all?icao24={icao}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"API returned bad status code: {response.StatusCode}");
            }
            
            var contentStream = await response.Content.ReadAsStreamAsync();
            Console.WriteLine(await response.Content.ReadAsStringAsync());
            var doc = await JsonDocument.ParseAsync(contentStream);

            StateVectorsDto data = new();
            data.Time = doc.RootElement.GetProperty("time").GetInt32();
            data.States = new List<StateVector>();
            
            foreach (var arr in doc.RootElement.GetProperty("states").EnumerateArray())
            {
                var vector = new StateVector();
                var list = new List<string>();
                foreach (var st in arr.EnumerateArray())
                {
                    var unix = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

                    vector.Icao24 = st.GetString();
                    vector.Callsign = st.GetString();
                    vector.OriginCountry = st.GetProperty("origin_country").GetString();
                    vector.LastContact = unix.AddSeconds(st.GetProperty("last_contact").GetInt64());

                    vector.Longitude = (float) st.GetProperty("longitude").GetDouble();
                    vector.Latitude = (float) st.GetProperty("latitude").GetDouble();
                    vector.BaroAltitude = (float) st.GetProperty("baro_altitude").GetDouble();

                    vector.OnGround = st.GetProperty("on_ground").GetBoolean();
                    vector.Velocity = (float) st.GetProperty("velocity").GetDouble();
                }
                data.States.Add(vector);
            }

            return data.States[0];
        }
    }
}