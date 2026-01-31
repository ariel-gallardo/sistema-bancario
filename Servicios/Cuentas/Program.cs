using EventFlow;
using EventFlow.Configuration;
using EventFlow.Extensions;
using EventFlow.Sql;
using HotChocolate.AspNetCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("SqlServer") ?? "Server=(localdb)\\MSSQLLocalDB;Database=Cuentas;Trusted_Connection=True;");

builder.Services
    .AddGraphQLServer()
    .AddQueryType(d => d.Name("Query"))
    .AddTypeExtension(typeof(QueryExtensions));

builder.Services.AddEventFlow(options => options
    .UseMicrosoftSqlServer(MsSqlConfiguration.New
        .ConnectionString(builder.Configuration.GetConnectionString("SqlServer") ?? "Server=(localdb)\\MSSQLLocalDB;Database=Cuentas;Trusted_Connection=True;"))
    .AddDefaults());

var app = builder.Build();

app.MapGraphQL("/graphql");
app.MapHealthChecks("/health");
app.MapGet("/", () => "Cuentas service");

app.Run();

public static class QueryExtensions
{
    public static string HelloCuentas() => "ok";
}
