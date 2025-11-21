var builder = DistributedApplication.CreateBuilder(args);

var rabbitmq = builder.AddRabbitMQ("messaging");

builder.AddProject<Projects.MassTransit_Api>("api")
    .WithReference(rabbitmq);

builder.Build().Run();