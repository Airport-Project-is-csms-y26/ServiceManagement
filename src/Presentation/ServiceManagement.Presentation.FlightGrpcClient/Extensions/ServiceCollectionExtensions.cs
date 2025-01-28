using Flights.FlightsService.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ServiceManagement.Presentation.FlightGrpcClient.Clients;
using ServiceManagement.Presentation.FlightGrpcClient.Clients.Interfaces;
using ServiceManagement.Presentation.FlightGrpcClient.Options;

namespace ServiceManagement.Presentation.FlightGrpcClient.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFlightServiceGrpcClient(
        this IServiceCollection collection)
    {
        collection.AddGrpcClient<FlightsService.FlightsServiceClient>((sp, o) =>
        {
            IOptions<FlightServiceClientOptions> options = sp.GetRequiredService<IOptions<FlightServiceClientOptions>>();
            o.Address = new Uri(options.Value.GrpcServerUrl);
        });

        collection.AddScoped<IFlightClient, FlightClient>();
        return collection;
    }
}