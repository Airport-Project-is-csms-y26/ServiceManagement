#pragma warning disable CA1506

using PassengerService.Infrastructure.Persistence.Options;
using ServiceManagement.Application.BackgroundServices;
using ServiceManagement.Application.Extensions;
using ServiceManagement.Infrastructure.Persistence.Extensions;
using ServiceManagement.Presentation.FlightGrpcClient.Extensions;
using ServiceManagement.Presentation.FlightGrpcClient.Options;
using ServiceManagement.Presentation.Grpc.Controllers;
using ServiceManagement.Presentation.Grpc.Interceptors;
using ServiceManagement.Presentation.Kafka.Configuration;
using ServiceManagement.Presentation.Kafka.Extensions;
using ServiceManagement.Presentation.Kafka.Models;
using Tasks.Kafka.Contracts;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel();

builder.Services.Configure<PostgresOptions>(builder.Configuration.GetSection("Persistence:Postgres"));
builder.Services.Configure<FlightServiceClientOptions>(builder.Configuration.GetSection("FlightClient"));

builder.Services.Configure<ProducerOptions>("TaskProcessing", builder.Configuration.GetSection("Kafka:Producers:TaskProcessing"));
builder.Services.Configure<ConsumerOptions>("FlightCreation", builder.Configuration.GetSection("Kafka:Consumers:FlightCreation"));
builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection("Kafka"));
builder.Services
    .AddMigration()
    .AddInfrastructureDataAccess()
    .AddApplication();

builder.Services.AddHostedService<MigrationService>();

builder.Services.AddGrpc(o => o.Interceptors.Add<ExceptionInterceptor>());
builder.Services.AddFlightServiceGrpcClient();

builder.Services.AddHandlers();
builder.Services.AddKafkaConsumer<FlightCreationKey, FlightCreationValue>("FlightCreation");
builder.Services.AddKafkaProducer<TaskProcessingKey, TaskProcessingValue>("TaskProcessing");
WebApplication app = builder.Build();

app.MapGrpcService<TaskController>();
app.Run();