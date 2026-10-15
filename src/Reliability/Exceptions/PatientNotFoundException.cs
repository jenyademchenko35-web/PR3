namespace Reliability.Exceptions;

public sealed class PatientNotFoundException : Exception
{
    public int PatientId { get; }

    public PatientNotFoundException(int patientId)
        : base($"Пациент {patientId} не найден в системе")
    {
        PatientId = patientId;
    }
}
