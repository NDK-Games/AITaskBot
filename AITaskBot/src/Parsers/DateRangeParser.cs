using System.Globalization;

public class DateRangeParser
{
    public bool TryParse(string input, out DateTime dateFrom, out DateTime dateTo)
    {
        dateFrom = DateTime.MinValue;
        dateTo = DateTime.MinValue;

        var parts = input.Split('-', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2) return false;

        if (!DateTime.TryParseExact(parts[0].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateFrom))
            return false;

        if (!DateTime.TryParseExact(parts[1].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTo))
            return false;

        if (dateTo < dateFrom)
            (dateFrom, dateTo) = (dateTo, dateFrom);

        return true;
    }
}