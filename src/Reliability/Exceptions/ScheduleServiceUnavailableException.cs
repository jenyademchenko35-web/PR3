namespace Reliability.Exceptions;

public sealed class ScheduleServiceUnavailableException : Exception
{
    public ScheduleServiceUnavailableException()
        : base("Сервис расписания временно недоступен")
    {
    }
}
