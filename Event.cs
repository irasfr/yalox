
namespace Event;
public class EventsGrader : BackgroundService
{
    private const int _intervalMinutes = 5; //константа, определяющая интервал в минутах между выполнениями задачи.

    private readonly ILoggerManager _logger; //экземпляр для логирования.
    private readonly DataContext _dataContext; //контекст данных для работы с базой данных.
    private readonly IServiceScope _scope; //область видимости.

    private readonly IEventScheduler _eventScheduler;

    private readonly IDbService _dbService;
    
    public EventsGrader(IDbService dbService, ILoggerManager logger, IServiceProvider serviceProvider, IEventScheduler eventScheduler)
    {
        _logger = logger;
        _scope = serviceProvider.CreateScope();
        _dataContext = _scope.ServiceProvider.GetRequiredService<DataContext>();
        _eventScheduler = eventScheduler;
        _dbService = dbService;
    }

    protected override async Task ExecuteAsync(CancellationToken token) //запрошена ли отмена через CancellationToken
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                await Call(token);
            }
            catch (Exception e)
            {
                _logger.LogError($"EventGraded exception {e.Message} {e}");
            }
            finally
            {
                await Task.Delay(_intervalMinutes * 60 * 1000, token);
            }
        }
    }

    private async Task Call(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        _logger.LogInfo($"EventsGrader Starts {DateTime.Now}");

        var evnt = _dataContext.Events.FirstOrDefault(e => !e.IsGraded && e.ExpiresAt < DateTime.Now);

        if (evnt == null) { _logger.LogInfo($"EventsGrader: event is null"); return; }

        _logger.LogInfo($"Choose event with id {evnt.Id} {evnt.Name}");

        if (evnt.Type == (int)EventType.OnWater)
        {
            await OnWaterEventController.MakeGiftForAll(evnt.Id, _dataContext);

            evnt.IsGraded = true;

            await _dataContext.SaveChangesAsync();
        }
        else if (evnt.Type == (int)EventType.OnGround)
        {
            await OnGroundEventController.MakeGiftForAll(evnt.Id, _dataContext);

            evnt.IsGraded = true;

            await _dataContext.SaveChangesAsync();
        }

        _logger.LogInfo($"EventsGrader Ends");

        bool eventExists = await _dbService.EventExistsAsync("New Event");

        if (eventExists)
        {
            _logger.LogInfo($"Event scheduled successfully");
        }
        else
        {
            _logger.LogInfo($"No events to schedule");
        }
    }

    public override void Dispose()
    {
        _scope.Dispose();
        base.Dispose();
    }
}