namespace VasilyKengele.Entities;

public record InternationalDayEntity(string Name)
{
    public DateOnly Day { get; set; }
    public string? Link { get; set; }
}
