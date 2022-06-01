namespace FlightTracker.Objects;

public class Flight
{
    public string Icao24 { get; set; }
    public bool HasContact => StateVectors != null;
    public StateVector? StateVectors { get; set; }

    public FlightStateChange CompareVectors(StateVector? newVectors)
    {
        if (newVectors == null)
        {
            return FlightStateChange.LostContact;
        }

        if (StateVectors == null)
        {
            return FlightStateChange.GotContact;
        }
        
        if (StateVectors.OnGround && !newVectors.OnGround)
        {
            // take off
            return FlightStateChange.TakeOff;
        }

        if (!StateVectors.OnGround && newVectors.OnGround)
        {
            // landing
            return FlightStateChange.Landing;
        }

        return FlightStateChange.None;
    }
}