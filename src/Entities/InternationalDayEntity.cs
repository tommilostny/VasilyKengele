namespace VasilyKengele.Entities;

public record InternationalDayEntity(string Name)
{
    public DateOnly Date { get; set; }
    public string? Link { get; set; }
}
