var builder = DistributedApplication.CreateBuilder(args);
var pgAdminPassword = builder.AddParameter("pgadmin-password", secret: true);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgAdmin(pgAdmin => pgAdmin
        .WithHostPort(5050)
        .WithEnvironment("PGADMIN_DEFAULT_EMAIL", "admin@example.com")
        .WithEnvironment("PGADMIN_DEFAULT_PASSWORD", pgAdminPassword)
        .WithEnvironment("PGADMIN_CONFIG_SERVER_MODE", "False"));

var database = postgres.AddDatabase("employee-management");

var api = builder.AddProject<Projects.EmployeeManagement_Api>("api")
    .WithReference(database)
    .WaitFor(database)
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.EmployeeManagement_Web>("web")
    .WithReference(api)
    .WaitFor(api)
    .WithExternalHttpEndpoints();

builder.Build().Run();
