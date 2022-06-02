using FlightTracker.Objects;
using FlightTracker.Services;

namespace FlightTracker.HostedServices;

public class AircraftMonitorHostedService : IHostedService, IDisposable
{
    private readonly ILogger<AircraftMonitorHostedService> _logger;
    private readonly IAircraftService _aircraftService;
    private Timer? _timer = null;
    private readonly IConfiguration _configuration;

    public Dictionary<string, Flight> Flights { get; set; } = new();

    public AircraftMonitorHostedService(ILogger<AircraftMonitorHostedService> logger, IAircraftService aircraftService, IConfiguration configuration)
    {
        _logger = logger;
        _aircraftService = aircraftService;
        _configuration = configuration;
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting flight monitoring service.");

        _timer = new Timer(DoWork, null, TimeSpan.Zero,
            TimeSpan.FromSeconds(5));

        return Task.CompletedTask;
    }

    private async void DoWork(object? state)
    {
        var trackedPlanes = _configuration.GetSection("TrackedPlanes").Get<List<string>>();

        foreach (var icao in trackedPlanes)
        {
            var stateVector = await _aircraftService.GetStateVectors(icao);
            if (!Flights.TryGetValue(icao, out var flight))
            {
                Flights[icao] = new Flight
                {
                    Icao24 = icao,
                    StateVectors = stateVector
                };

                flight = Flights[icao];
            }

            var stateChange = flight.CompareVectors(stateVector);
            flight.StateVectors = stateVector;

            await _aircraftService.HandleStateChange(flight, stateChange);
        }
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Stopping flight monitoring service.");
        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}