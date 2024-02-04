using System.Net;
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.MapGet("/", () => Results.Json(new {count = 10}));
app.Run();
