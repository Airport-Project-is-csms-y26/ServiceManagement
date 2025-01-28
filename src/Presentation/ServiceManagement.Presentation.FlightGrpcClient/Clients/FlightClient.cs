using Flights.FlightsService.Contracts;
using Google.Protobuf.WellKnownTypes;
using ServiceManagement.Presentation.FlightGrpcClient.Clients.Interfaces;
using System.Runtime.CompilerServices;

namespace ServiceManagement.Presentation.FlightGrpcClient.Clients;

public class FlightClient : IFlightClient
{
    private readonly FlightsService.FlightsServiceClient _client;

    public FlightClient(FlightsService.FlightsServiceClient client)
    {
        _client = client;
    }

    public async Task<CreateFlightResponse> CreateFlight(
        string from,
        string toPlace,
        long planeNumber,
        DateTimeOffset departureTime,
        CancellationToken cancellationToken)
    {
        var request = new CreateFlightRequest
        {
            From = from,
            To = toPlace,
            PlaneNumber = planeNumber,
            DepartureTime = Timestamp.FromDateTime(departureTime.UtcDateTime),
        };

        return await _client.CreateAsync(request, cancellationToken: cancellationToken);
    }

    public async Task<ChangeFlightStatusResponse> ChangeFlightStatus(
        long flightId,
        FlightStatus status,
        CancellationToken cancellationToken)
    {
        var request = new ChangeFlightStatusRequest()
        {
            FlightId = flightId,
            Status = status,
        };

        return await _client.ChangeFlightStatusAsync(request, cancellationToken: cancellationToken);
    }

    public async IAsyncEnumerable<Flight> GetFlights(
        int pageSize,
        int cursor,
        long[] ids,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var request = new GetFlightsRequest
        {
            Cursor = cursor,
            PageSize = pageSize,
            FlightsIds = { ids },
        };

        GetFlightsResponse response =
            await _client.GetFlightsAsync(request, cancellationToken: cancellationToken);

        foreach (Flight passenger in response.Flights)
        {
            yield return passenger;
        }
    }
}