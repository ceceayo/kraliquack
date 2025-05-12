var builder = DistributedApplication.CreateBuilder(args);

//var cache = builder.AddGarnet("cache").WithOtlpExporter()
//    .WithDataVolume(isReadOnly: false);

var cache = builder.AddRedis("cache").WithRedisInsight();

var queue = builder.AddRabbitMQ("queue").WithManagementPlugin().WithOtlpExporter().WithExternalHttpEndpoints();

var postgres = builder.AddPostgres("postgres").WithDataVolume(isReadOnly: false);
var postgresdb = postgres.AddDatabase("postgresdb");

var migrationWorker = builder.AddProject<Projects.UGS_MigrationWorker>("migrationworker")
    .WithReference(postgresdb).WaitFor(postgresdb);

var worker = builder.AddProject<Projects.UGS_Worker>("worker").WaitForCompletion(migrationWorker)
    .WithReference(cache).WaitFor(cache)
    .WithReference(postgresdb).WaitFor(postgresdb)
    .WithReference(queue).WaitFor(queue);

var apiService = builder.AddProject<Projects.UGS_ApiService>("apiservice")
    .WithExternalHttpEndpoints()
    .WaitForCompletion(migrationWorker).WaitFor(worker)
    .WithReference(cache).WaitFor(cache)
    .WithReference(postgresdb).WaitFor(postgresdb)
    .WithReference(queue).WaitFor(queue);

var adminPanel = builder.AddProject<Projects.UGS_AdminPanel>("adminpanel")
    .WithExternalHttpEndpoints()
    .WaitForCompletion(migrationWorker)
    .WithReference(cache).WaitFor(cache)
    .WithReference(postgresdb).WaitFor(postgresdb)
    .WithReference(queue).WaitFor(queue);



builder.Build().Run();
