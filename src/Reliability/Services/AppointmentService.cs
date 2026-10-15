using Microsoft.Extensions.Logging;
using Reliability.Exceptions;
using Reliability.Models;

namespace Reliability.Services;

public sealed class AppointmentService
{
    private readonly ILogger<AppointmentService> _logger;
    private readonly ScheduleGateway _scheduleGateway;
    private readonly HashSet<int> _registeredPatientIds = new() { 1, 2, 3 };

    public AppointmentService(
        ILogger<AppointmentService> logger,
        ScheduleGateway scheduleGateway)
    {
        _logger = logger;
        _scheduleGateway = scheduleGateway;
    }

    public bool TryCreateAppointment(AppointmentRequest request, int appointmentId)
    {
        try
        {
            if (!_registeredPatientIds.Contains(request.PatientId))
                throw new PatientNotFoundException(request.PatientId);

            if (request.AppointmentAt is null)
                throw new ArgumentException("Дата приёма обязательна", nameof(request));

            _logger.LogInformation(
                "Начинаем запись {AppointmentId}: пациент {PatientId}, врач {DoctorId}, слот {SlotId}",
                appointmentId, request.PatientId, request.DoctorId, request.SlotId);

            var reserved = false;
            for (var attempt = 1; attempt <= 2 && !reserved; attempt++)
            {
                try
                {
                    _scheduleGateway.ReserveSlot(request.SlotId, appointmentId);
                    reserved = true;
                }
                catch (TimeoutException ex) when (attempt == 1)
                {
                    _logger.LogWarning(
                        ex,
                        "Временный сбой сервиса расписания, повторяем запись {AppointmentId}, попытка {Attempt}",
                        appointmentId, attempt);
                }
            }

            if (!reserved)
            {
                _logger.LogError(
                    "Слот не был зарезервирован для записи {AppointmentId}",
                    appointmentId);
                return false;
            }

            _logger.LogInformation(
                "Запись {AppointmentId} создана для пациента {PatientId} на слот {SlotId}",
                appointmentId, request.PatientId, request.SlotId);
            return reserved;
        }
        catch (PatientNotFoundException ex)
        {
            _logger.LogWarning(
                ex,
                "Запись {AppointmentId} отклонена: пациент {PatientId} не найден",
                appointmentId, ex.PatientId);
            return false;
        }
        catch (SlotAlreadyBookedException ex)
        {
            _logger.LogWarning(
                ex,
                "Запись {AppointmentId} отклонена: слот {SlotId} уже занят",
                appointmentId, ex.SlotId);
            return false;
        }
        catch (TimeoutException ex)
        {
            _logger.LogWarning(
                ex,
                "Сервис расписания не ответил после повтора для записи {AppointmentId}",
                appointmentId);
            return false;
        }
        catch (ScheduleServiceUnavailableException ex)
        {
            _logger.LogError(
                ex,
                "Невозможно создать запись {AppointmentId}: сервис расписания недоступен",
                appointmentId);
            return false;
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(
                ex,
                "Некорректные данные записи {AppointmentId}",
                appointmentId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Непредвиденная ошибка при создании записи {AppointmentId}",
                appointmentId);
            return false;
        }
        finally
        {
            _logger.LogInformation(
                "Завершена попытка обработки записи {AppointmentId}",
                appointmentId);
        }
    }
}
