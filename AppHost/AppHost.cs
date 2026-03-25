var builder = DistributedApplication.CreateBuilder(args);

// Add PostgreSQL container
var postgres = builder.AddPostgres("postgres")
                      .WithPgAdmin();

// Add database
var triageDb = postgres.AddDatabase("triagedb");

// Add API service with database reference
var apiService = builder.AddProject<Projects.ApiService>("apiservice")
                        .WithReference(triageDb);

// Add worker service
builder.AddProject<Projects.WorkerService>("workerservice");

builder.Build().Run();
