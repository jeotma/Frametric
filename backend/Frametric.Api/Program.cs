// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using Frametric.Api.Extensions;
using Frametric.Application;
using Frametric.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure Web/API specific services using Extensions
builder.Services.AddApiAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddFrontendCors();
builder.Services.AddPresentationOpenApi();

// Clean Architecture Dependency Injection
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UsePresentationOpenApi(app.Environment);
}

app.UseCors("FrontendPolicy");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
