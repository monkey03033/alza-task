using Alza.Delivery.ApplicationLayer.Abstractions;
using Alza.Delivery.ApplicationLayer.Services;
using Alza.Delivery.InfrastructureLayer.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddTransient<IOrderService, OrderService>();

var app = builder.Build();


app.MapControllers();

app.Run();