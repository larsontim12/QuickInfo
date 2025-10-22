using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Wisej.Core;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    WebRootPath = "./"
});

// Add services for controllers
builder.Services.AddControllers();

var app = builder.Build();

// Add Wisej.
app.UseWisej();

// Add routing for controllers
app.UseRouting();

// Map controller endpoints
app.MapControllers();

// Add FileServer middleware to serve content files excluding .json files.
app.UseWhen(
    context => !context.Request.Path.Value.EndsWith(".json", StringComparison.InvariantCulture),
    app => app.UseFileServer());

app.Run();
