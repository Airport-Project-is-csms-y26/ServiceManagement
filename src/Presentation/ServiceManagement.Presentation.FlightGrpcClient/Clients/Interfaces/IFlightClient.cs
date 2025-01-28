using Flights.FlightsService.Contracts;

namespace ServiceManagement.Presentation.FlightGrpcClient.Clients.Interfaces;

public interface IFlightClient
{
    Task<CreateFlightResponse> CreateFlight(
        string from,
        string toPlace,
        long planeNumber,
        DateTimeOffset departureTime,
        CancellationToken cancellationToken);

    Task<ChangeFlightStatusResponse> ChangeFlightStatus(long flightId, FlightStatus status,  CancellationToken cancellationToken);

    IAsyncEnumerable<Flight> GetFlights(int pageSize, int cursor, long[] ids, CancellationToken cancellationToken);
}