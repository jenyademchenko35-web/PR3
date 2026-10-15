using Reliability.Exceptions;

namespace Reliability.Services;

/// <summary>
/// Имитация внешнего сервиса расписания для демонстрации сбоев.
/// Проверка и занятие слота выполняются одним методом.
/// </summary>
public sealed class ScheduleGateway
{
    private readonly HashSet<int> _availableSlotIds = new() { 10, 11, 12, 13 };
    private readonly HashSet<int> _retryOnceAppointmentIds = new() { 102 };
    private readonly HashSet<int> _unavailableAppointmentIds = new() { 103 };

    public void ReserveSlot(int slotId, int appointmentId)
    {
        if (_retryOnceAppointmentIds.Remove(appointmentId))
            throw new TimeoutException("Сервис расписания не ответил вовремя");

        if (_unavailableAppointmentIds.Contains(appointmentId))
            throw new ScheduleServiceUnavailableException();

        if (!_availableSlotIds.Remove(slotId))
            throw new SlotAlreadyBookedException(slotId);
    }
}
