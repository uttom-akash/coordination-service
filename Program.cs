using System;
using System.Threading.Tasks;
using LeaderElection.Contracts;
using LeaderElection.Database;
using LeaderElection.Zookeeeper;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton<IZooKeeperClient, ZooKeeperClient>();
builder.Services.AddSingleton<IZooKeeperReadClient, ZooKeeperReadClient>();
builder.Services.AddSingleton<IKvService, KvService>();
builder.Services.AddSingleton(NodeInfo.Create(builder.Configuration["urls"]));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();

var app = builder.Build();

await app.Services.GetService<IZooKeeperClient>().ParticipateElection();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(x=>x.AllowAnyOrigin());

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
