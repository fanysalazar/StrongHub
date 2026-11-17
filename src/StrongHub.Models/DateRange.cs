namespace StrongHub.Models;

/// <summary>A closed range of dates (start and end included).</summary>
public sealed class DateRange
{
    public DateRange(DateOnly start, DateOnly end)
    {
        if (start > end)
        {
            throw new ValidationException("The start date cannot be after the end date.");
        }

        Start = start;
        End = end;
    }

    public DateOnly Start { get; }
    public DateOnly End { get; }

    public bool Contains(DateOnly date) => date >= Start && date <= End;
}
