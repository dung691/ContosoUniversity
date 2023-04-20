using ContosoUniversity.Data;
using ContosoUniversity.IntegrationTests.Helpers;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ContosoUniversity.IntegrationTests;

[CollectionDefinition(nameof(SliceFixture))]
public class SliceFixtureCollection : ICollectionFixture<SliceFixture> { }

public class SliceFixture : IAsyncLifetime
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly WebApplicationFactory<Program> _factory;

    public SliceFixture()
    {
        _factory = new ContosoTestApplicationFactory();
        _scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
    }

    class ContosoTestApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddAntiforgery(t =>
                {
                    t.Cookie.Name = AntiForgeryTokenExtractor.AntiForgeryCookieName;
                    t.FormFieldName = AntiForgeryTokenExtractor.AntiForgeryFieldName;
                });
            });

            builder.ConfigureAppConfiguration(configurationBuilder =>
            {
                var integrationConfig = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .AddEnvironmentVariables()
                    .Build();

                configurationBuilder.AddConfiguration(integrationConfig);
            });
        }
    }

    public WebApplicationFactory<Program> Factory => _factory;

    public async Task ExecuteScopeAsync(Func<IServiceProvider, Task> action)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SchoolContext>();

        try
        {
            await dbContext.BeginTransactionAsync();

            await action(scope.ServiceProvider);

            await dbContext.CommitTransactionAsync();
        }
        catch (Exception)
        {
            dbContext.RollbackTransaction();
            throw;
        }
    }

    public async Task<T> ExecuteScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SchoolContext>();

        try
        {
            await dbContext.BeginTransactionAsync();

            var result = await action(scope.ServiceProvider);

            await dbContext.CommitTransactionAsync();

            return result;
        }
        catch (Exception)
        {
            dbContext.RollbackTransaction();
            throw;
        }
    }

    public Task ExecuteDbContextAsync(Func<SchoolContext, Task> action)
        => ExecuteScopeAsync(sp => action(sp.GetRequiredService<SchoolContext>()));

    public Task ExecuteDbContextAsync(Func<SchoolContext, ValueTask> action)
        => ExecuteScopeAsync(sp => action(sp.GetRequiredService<SchoolContext>()).AsTask());

    public Task ExecuteDbContextAsync(Func<SchoolContext, IMediator, Task> action)
        => ExecuteScopeAsync(sp => action(sp.GetRequiredService<SchoolContext>(), sp.GetRequiredService<IMediator>()));

    public Task<T> ExecuteDbContextAsync<T>(Func<SchoolContext, Task<T>> action)
        => ExecuteScopeAsync(sp => action(sp.GetRequiredService<SchoolContext>()));

    public Task<T> ExecuteDbContextAsync<T>(Func<SchoolContext, ValueTask<T>> action)
        => ExecuteScopeAsync(sp => action(sp.GetRequiredService<SchoolContext>()).AsTask());

    public Task<T> ExecuteDbContextAsync<T>(Func<SchoolContext, IMediator, Task<T>> action)
        => ExecuteScopeAsync(sp => action(sp.GetRequiredService<SchoolContext>(), sp.GetRequiredService<IMediator>()));

    public Task InsertAsync<T>(params T[] entities) where T : class
    {
        return ExecuteDbContextAsync(db =>
        {
            foreach (var entity in entities)
            {
                db.Set<T>().Add(entity);
            }
            return db.SaveChangesAsync();
        });
    }

    public Task InsertAsync<TEntity>(TEntity entity) where TEntity : class
    {
        return ExecuteDbContextAsync(db =>
        {
            db.Set<TEntity>().Add(entity);

            return db.SaveChangesAsync();
        });
    }

    public Task InsertAsync<TEntity, TEntity2>(TEntity entity, TEntity2 entity2)
        where TEntity : class
        where TEntity2 : class
    {
        return ExecuteDbContextAsync(db =>
        {
            db.Set<TEntity>().Add(entity);
            db.Set<TEntity2>().Add(entity2);

            return db.SaveChangesAsync();
        });
    }

    public Task InsertAsync<TEntity, TEntity2, TEntity3>(TEntity entity, TEntity2 entity2, TEntity3 entity3)
        where TEntity : class
        where TEntity2 : class
        where TEntity3 : class
    {
        return ExecuteDbContextAsync(db =>
        {
            db.Set<TEntity>().Add(entity);
            db.Set<TEntity2>().Add(entity2);
            db.Set<TEntity3>().Add(entity3);

            return db.SaveChangesAsync();
        });
    }

    public Task InsertAsync<TEntity, TEntity2, TEntity3, TEntity4>(TEntity entity, TEntity2 entity2, TEntity3 entity3, TEntity4 entity4)
        where TEntity : class
        where TEntity2 : class
        where TEntity3 : class
        where TEntity4 : class
    {
        return ExecuteDbContextAsync(db =>
        {
            db.Set<TEntity>().Add(entity);
            db.Set<TEntity2>().Add(entity2);
            db.Set<TEntity3>().Add(entity3);
            db.Set<TEntity4>().Add(entity4);

            return db.SaveChangesAsync();
        });
    }

    public Task<T?> FindAsync<T>(int id)
        where T : class
    {
        return ExecuteDbContextAsync(db => db.Set<T>().FindAsync(id).AsTask());
    }

    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        return ExecuteScopeAsync(sp =>
        {
            var mediator = sp.GetRequiredService<IMediator>();

            return mediator.Send(request);
        });
    }

    public Task SendAsync(IRequest request)
    {
        return ExecuteScopeAsync(sp =>
        {
            var mediator = sp.GetRequiredService<IMediator>();

            return mediator.Send(request);
        });
    }

    private int _courseNumber = 1;

    public int NextCourseNumber() => Interlocked.Increment(ref _courseNumber);

    public async Task InitializeAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SchoolContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SchoolContext>();
        await dbContext.Database.EnsureDeletedAsync();
        await _factory.DisposeAsync().AsTask();
    }
}