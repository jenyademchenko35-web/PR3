namespace Reliability.Exceptions;

public sealed class SlotAlreadyBookedException : Exception
{
    public int SlotId { get; }

    public SlotAlreadyBookedException(int slotId)
        : base($"Слот {slotId} уже занят другой записью")
    {
        SlotId = slotId;
    }
}
