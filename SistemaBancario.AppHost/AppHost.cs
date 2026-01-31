var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");


var clientes = builder.AddProject<Projects.Clientes>("clientes")
    .WithReference(cache)
    .WaitFor(cache)
    .WithHttpHealthCheck("/health");

var cuentas = builder.AddProject<Projects.Cuentas>("cuentas")
    .WithReference(cache)
    .WaitFor(cache)
    .WithHttpHealthCheck("/health");

var tarjetas = builder.AddProject<Projects.Tarjetas>("tarjetas")
    .WithReference(cache)
    .WaitFor(cache)
    .WithHttpHealthCheck("/health");

var pagos = builder.AddProject<Projects.Pagos>("pagos")
    .WithReference(cache)
    .WaitFor(cache)
    .WithHttpHealthCheck("/health");

var web = builder.AddNpmApp("frontend", "..\\frontend", "dev")
    .WithReference(clientes)
    .WithReference(cuentas)
    .WithReference(tarjetas)
    .WithReference(pagos)
    .WaitFor(clientes)
    .WaitFor(cuentas)
    .WaitFor(tarjetas)
    .WaitFor(pagos);

builder.Build().Run();
