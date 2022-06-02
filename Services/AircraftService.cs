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
        private readonly IConfiguration _configuration;
        private readonly ILogger<AircraftService> _logger;

        public AircraftService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<AircraftService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
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
            var doc = await JsonDocument.ParseAsync(contentStream);

            StateVectorsDto data = new()
            {
                Time = doc.RootElement.GetProperty("time").GetInt32(),
                States = new List<StateVector>()
            };

            var states = doc.RootElement.GetProperty("states");
            if (states.ValueKind == JsonValueKind.Null)
            {
                return null;
            }
            
            foreach (var arr in states.EnumerateArray())
            {
                var vector = new StateVector();
                var list = arr.EnumerateArray().ToList();

                var unix = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    
                vector.Icao24 = list[0].GetString();
                vector.Callsign = list[1].GetString().Trim();
                vector.OriginCountry = list[2].GetString().Trim();
                vector.LastContact = unix.AddSeconds(list[4].GetInt64());

                vector.Longitude = (float) list[5].GetDouble();
                vector.Latitude = (float) list[6].GetDouble();

                if (list[7].ValueKind != JsonValueKind.Null)
                {
                    vector.BaroAltitude = (float)list[7].GetDouble();
                }

                vector.OnGround = list[8].GetBoolean();
                vector.Velocity = (float) list[9].GetDouble();
                
                data.States.Add(vector);
            }

            return data.States[0];
        }

        public async Task HandleStateChange(Flight flight, FlightStateChange change)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var webhookDto = new WebhookDto
            {
                Content = null,
                Username = "Flight Tracker",
            };

            switch (change)
            {
                case FlightStateChange.None:
                    break;
                
                case FlightStateChange.TakeOff:
                    webhookDto.Content =
                        $"Letadlo {flight.StateVectors?.Callsign} ze země {flight.StateVectors?.OriginCountry} právě vzlétlo! Souřadnice: {flight.StateVectors.Latitude}°N, {flight.StateVectors.Longitude}°E";
                    break;
                
                case FlightStateChange.Landing:
                    webhookDto.Content =
                        $"Letadlo {flight.StateVectors?.Callsign} ze země {flight.StateVectors?.OriginCountry} právě přistálo! Souřadnice: {flight.StateVectors.Latitude}°N, {flight.StateVectors.Longitude}°E";
                    break;
                
                case FlightStateChange.GotContact:
                    webhookDto.Content =
                        $"Spojení s letadlem {flight.StateVectors?.Callsign} ze země {flight.StateVectors?.OriginCountry} navázáno! Souřadnice: {flight.StateVectors.Latitude}°N, {flight.StateVectors.Longitude}°E";
                    break;
                
                case FlightStateChange.LostContact:
                    webhookDto.Content = $"Ztratili jsme spojení s letadlem {flight.Icao24}!";
                    break;
            }

            if (webhookDto.Content != null)
            {
                await httpClient.PostAsJsonAsync(_configuration["WebhookUrl"], webhookDto);
                _logger.LogInformation(webhookDto.Content);
            }
        }
    }
}