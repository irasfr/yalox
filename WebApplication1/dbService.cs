using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public interface IDbService
{
    Task<List<Event>> GetEventsAsync(DateTime currentDate);
    Task UpdateEventAsync(Event updatedEvent);
    Task CreateEventAsync(Event newEvent);
}

public class DbService : IDbService
{
    private readonly AppDbContext _dbContext;

    public DbService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Event>> GetEventsAsync(DateTime currentDate)
    {
        return await _dbContext.Events
            .Where(e => e.StartsAt <= currentDate && e.ExpiresAt >= currentDate)
            .ToListAsync();
    }

    public async Task UpdateEventAsync(Event updatedEvent)
    {
        _dbContext.Events.Update(updatedEvent);
        await _dbContext.SaveChangesAsync();
    }

    public async Task CreateEventAsync(Event newEvent)
    {
        await _dbContext.Events.AddAsync(newEvent);
        await _dbContext.SaveChangesAsync();
    }
}
