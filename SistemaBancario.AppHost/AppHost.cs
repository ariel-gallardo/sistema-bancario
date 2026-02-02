var builder = DistributedApplication.CreateBuilder(args);

var sqlServer = builder.AddSqlServer("sqlserver");

var clientesDb = sqlServer.AddDatabase("clientesdb");
var cuentasDb = sqlServer.AddDatabase("cuentasdb");
var tarjetasDb = sqlServer.AddDatabase("tarjetasdb");
var pagosDb = sqlServer.AddDatabase("pagosdb");

var clientes = builder.AddProject<Projects.Clientes>("clientes")
    .WithReference(clientesDb)
    .WithEndpoint("http", endpoint => endpoint.Port = 65534)
    .WithHttpHealthCheck("/health")
    .WaitFor(clientesDb);

var cuentas = builder.AddProject<Projects.Cuentas>("cuentas")
    .WithReference(cuentasDb)
    .WithEndpoint("http", endpoint => endpoint.Port = 65532)
    .WithHttpHealthCheck("/health")
    .WaitFor(cuentasDb);

var tarjetas = builder.AddProject<Projects.Tarjetas>("tarjetas")
    .WithReference(tarjetasDb)
    .WithEndpoint("http", endpoint => endpoint.Port = 65515)
    .WithHttpHealthCheck("/health")
    .WaitFor(tarjetasDb);

var pagos = builder.AddProject<Projects.Pagos>("pagos")
    .WithReference(pagosDb)
    .WithEndpoint("http", endpoint => endpoint.Port = 65530)
    .WithHttpHealthCheck("/health")
    .WaitFor(pagosDb);

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
