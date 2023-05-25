using ContosoUniversity.Data;
using ContosoUniversity.Extensions;
using ContosoUniversity.Infrastructure;
using ContosoUniversity.Infrastructure.TagHelpers;
using ContosoUniversity.Infrastructure.TagHelpers.SelectOptionsProviders;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.AddSerilog();

// Add services to the container.
builder.Services.AddRazorPages(options => options.Conventions.ConfigureFilter(new DbContextTransactionPageFilter()));
builder.Services.AddDbContext<SchoolContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SchoolContext") ?? throw new InvalidOperationException("Connection string 'SchoolContext' not found.")));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddMediatR(options => options.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddScoped<ISelectOptionsProvider, DepartmentSelectOptionsProvider>();
builder.Services.AddScoped<ISelectOptionsProvider, InstructorSelectOptionsProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}

await app.Services.InitializeDatabaseAsync();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();

// Make the implicit Program.cs class public, so integration tests can reference the correct assembly for host building
public partial class Program
{
}