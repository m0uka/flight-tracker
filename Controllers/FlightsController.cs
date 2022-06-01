using System.Collections.Generic;
using System.Threading.Tasks;
using FlightTracker.Objects;
using FlightTracker.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace FlightTracker.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FlightsController : ControllerBase
    {
        private readonly IAircraftService _aircraftService;
        private readonly IConfiguration _configuration;

        public FlightsController(IAircraftService aircraftService, IConfiguration configuration)
        {
            _aircraftService = aircraftService;
            _configuration = configuration;
        }

        public async Task<List<StateVector>> Get()
        {
            var trackedPlanes = _configuration.GetSection("TrackedPlanes").Get<List<string>>();
            var stateVectors = new List<StateVector>();

            foreach (var plane in trackedPlanes)
            {
                var stateVector = await _aircraftService.GetStateVectors(plane);
                if (stateVector != null) stateVectors.Add(stateVector);
            }

            return stateVectors;
        }
    }
}