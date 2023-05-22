namespace VasilyKengele.Services;

public class InternationalDaysService
{
    private const string _daysJsonFile = "unInternationalDays.json";
    private readonly List<InternationalDayEntity> _days;

    public InternationalDaysService()
    {
        if (!System.IO.File.Exists(_daysJsonFile))
        {
            var scrapeJob = new UnScraper();
            scrapeJob.Start();
        }
        var jsonText = System.IO.File.ReadAllText(_daysJsonFile);
        _days = JsonConvert.DeserializeObject<List<InternationalDayEntity>>(jsonText)
            ?? throw new ArgumentNullException(nameof(_days));
    }

    public InternationalDayEntity? GetInternationalDay(DateTime date) => _days.FirstOrDefault
    (
        x => x.Day.Day == date.Day && x.Day.Month == date.Month
    );

    public InternationalDayEntity? GetTodaysInternationalDay() => GetInternationalDay(DateTime.Now);

    private class UnScraper : WebScraper
    {
        public override void Init()
        {
            LoggingLevel = LogLevel.All;
            var identity = new HttpIdentity
            {
                UserAgent = CommonUserAgents.ChromeDesktopUserAgents[0]
            };
            Request("https://www.un.org/en/observances/list-days-weeks", Parse, identity);
        }

        public override void Parse(Response response)
        {
            var days = new List<InternationalDayEntity>();

            foreach (var titleItem in response.Css(".views-field-title"))
            {
                var newDay = new InternationalDayEntity(titleItem.InnerTextClean)
                {
                    Link = titleItem.ChildNodes.FirstOrDefault(cn => cn.NodeName == "SPAN")
                        ?.ChildNodes?.FirstOrDefault(cn => cn.NodeName == "A")
                        ?.Attributes["href"]
                };
                days.Add(newDay);
            }
            var daysIt = days.GetEnumerator();
            foreach (var dateItem in response.Css(".date-display-single"))
            {
                daysIt.MoveNext();
                var day = daysIt.Current;
                day.Day = DateOnly.FromDateTime(DateTime.Parse(dateItem.Attributes["content"]));
            }

            System.IO.File.WriteAllText(_daysJsonFile, JsonConvert.SerializeObject(days));
        }
    }
}
