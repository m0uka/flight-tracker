using System.Collections.Generic;
using System.Text.Json.Serialization;
using FlightTracker.Objects;

namespace FlightTracker.Contracts
{
    public class StateVectorsDto
    {
        public int Time { get; set; }
        public List<StateVector> States { get; set; }
    }
}