using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Reliability.Models;
using Reliability.Services;

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .SetMinimumLevel(LogLevel.Information)
        .AddSimpleConsole(options =>
        {
            options.SingleLine = true;
            options.TimestampFormat = null;
        });
});

var appointmentService = new AppointmentService(
    loggerFactory.CreateLogger<AppointmentService>(),
    new ScheduleGateway());

Console.WriteLine("Система управления медицинским центром");
Console.WriteLine("Практическая работа №3: надёжная запись пациента на приём");

Console.WriteLine("\n1. Проверка модели с ошибками");
var invalidRequest = new AppointmentRequest
{
    PatientId = 0,
    DoctorId = 0,
    AppointmentAt = null,
    SlotId = 0,
    PatientEmail = "неверная-почта"
};
ValidateAndPrint(invalidRequest);

Console.WriteLine("\n2. Проверка корректной модели");
var firstRequest = new AppointmentRequest
{
    PatientId = 1,
    DoctorId = 10,
    AppointmentAt = new DateTime(2026, 10, 20, 10, 0, 0),
    SlotId = 10,
    PatientEmail = "patient@example.com"
};
Console.WriteLine(ValidateAndPrint(firstRequest)
    ? "Модель корректна."
    : "Модель содержит ошибки.");

Console.WriteLine("\n3. Демонстрационные сценарии записи");
RunScenario("Успешная запись", appointmentService, firstRequest, 101);

var retryRequest = new AppointmentRequest
{
    PatientId = 1,
    DoctorId = 10,
    SlotId = 11,
    AppointmentAt = new DateTime(2026, 10, 20, 10, 30, 0),
    PatientEmail = "patient@example.com"
};
RunScenario("Временный таймаут и повторная попытка", appointmentService, retryRequest, 102);

var unavailableRequest = new AppointmentRequest
{
    PatientId = 1,
    DoctorId = 10,
    SlotId = 12,
    AppointmentAt = new DateTime(2026, 10, 20, 11, 0, 0),
    PatientEmail = "patient@example.com"
};
RunScenario("Недоступен сервис расписания", appointmentService, unavailableRequest, 103);

var duplicateRequest = new AppointmentRequest
{
    PatientId = 2,
    DoctorId = 10,
    SlotId = 10,
    AppointmentAt = new DateTime(2026, 10, 20, 10, 0, 0),
    PatientEmail = "patient2@example.com"
};
RunScenario("Слот уже занят", appointmentService, duplicateRequest, 104);

var unknownPatientRequest = new AppointmentRequest
{
    PatientId = 999,
    DoctorId = 10,
    SlotId = 13,
    AppointmentAt = new DateTime(2026, 10, 20, 11, 30, 0),
    PatientEmail = "unknown@example.com"
};
RunScenario("Пациент не найден", appointmentService, unknownPatientRequest, 105);

Console.WriteLine("\nДемонстрация завершена.");

static bool ValidateAndPrint(AppointmentRequest request)
{
    var context = new ValidationContext(request);
    var results = new List<ValidationResult>();
    var isValid = Validator.TryValidateObject(
        request,
        context,
        results,
        validateAllProperties: true);

    if (!isValid)
    {
        Console.WriteLine("Данные не прошли валидацию:");
        foreach (var error in results)
            Console.WriteLine($"  - {error.ErrorMessage}");
    }

    return isValid;
}

static void RunScenario(
    string title,
    AppointmentService service,
    AppointmentRequest request,
    int appointmentId)
{
    Console.WriteLine($"\nСценарий: {title}");
    if (!ValidateAndPrint(request))
    {
        Console.WriteLine("Операция остановлена до обращения к сервису.");
        return;
    }

    var created = service.TryCreateAppointment(request, appointmentId);
    Console.WriteLine(created
        ? "Результат: запись подтверждена."
        : "Результат: запись не создана, программа продолжает работу.");
}
