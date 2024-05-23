public interface IEventScheduler
{
    Task TrySchedule();
}

public class EventScheduler : IEventScheduler
{
    private readonly IDbService _dbService;

    public EventScheduler(IDbService dbService)
    {
        _dbService = dbService;
    }

    public async Task TrySchedule()
    {
        var currentEvents = await _dbService.GetEventsAsync(DateTime.Now);

        foreach (var ev in currentEvents)
        {
            if (ev.ExpiresAt <= DateTime.Now && !ev.IsGraded)
            {
                ev.IsGraded = true;
                await _dbService.UpdateEventAsync(ev);
            }
        }

        if (DateTime.Now.DayOfWeek == DayOfWeek.Monday && DateTime.Now.TimeOfDay == new TimeSpan(0, 0, 0))
        {
            var newEvent = new Event
            {
                Name = "New Event",
                Type = 1,
                StartsAt = DateTime.Now.AddHours(24),
                ExpiresAt = DateTime.Now.AddHours(24 * 8)
            };

            await _dbService.CreateEventAsync(newEvent);
        }
    }
}