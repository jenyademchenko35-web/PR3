using System.ComponentModel.DataAnnotations;

namespace Reliability.Models;

/// <summary>
/// Данные, которые нужны для записи пациента к врачу.
/// </summary>
public class AppointmentRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Нужно указать положительный идентификатор пациента")]
    public int PatientId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Нужно указать положительный идентификатор врача")]
    public int DoctorId { get; set; }

    [Required(ErrorMessage = "Нужно указать дату и время приёма")]
    public DateTime? AppointmentAt { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Нужно указать положительный идентификатор слота")]
    public int SlotId { get; set; }

    [Required(ErrorMessage = "Нужно указать электронную почту пациента")]
    [EmailAddress(ErrorMessage = "Некорректный формат электронной почты")]
    [StringLength(100, ErrorMessage = "Электронная почта не должна быть длиннее 100 символов")]
    public string PatientEmail { get; set; } = string.Empty;
}
