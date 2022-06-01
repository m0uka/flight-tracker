using System.Threading.Tasks;
using FlightTracker.Objects;

namespace FlightTracker.Services
{
    public interface IAircraftService
    {
        Task<StateVector?> GetStateVectors(string icao);
    }
}