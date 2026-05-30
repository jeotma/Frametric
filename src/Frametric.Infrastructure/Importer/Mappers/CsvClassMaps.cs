using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Frametric.Infrastructure.Importer.CsvModels;

namespace Frametric.Infrastructure.Importer.Mappers;

// Custom converter for the weird year formats in some lists (e.g., "2022.0" instead of "2022")
public class YearNullableIntConverter : Int32Converter
{
    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        if (decimal.TryParse(text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var decimalYear))
        {
            return (int)decimalYear; // "2022.0" -> 2022
        }
        return base.ConvertFromString(text, row, memberMapData);
    }
}

public class DiaryCsvRecordMap : ClassMap<DiaryCsvRecord>
{
    public DiaryCsvRecordMap()
    {
        AutoMap(System.Globalization.CultureInfo.InvariantCulture);
        Map(m => m.Year).TypeConverter<YearNullableIntConverter>();
    }
}

public class RatingCsvRecordMap : ClassMap<RatingCsvRecord>
{
    public RatingCsvRecordMap()
    {
        AutoMap(System.Globalization.CultureInfo.InvariantCulture);
        Map(m => m.Year).TypeConverter<YearNullableIntConverter>();
    }
}

public class WatchlistCsvRecordMap : ClassMap<WatchlistCsvRecord>
{
    public WatchlistCsvRecordMap()
    {
        AutoMap(System.Globalization.CultureInfo.InvariantCulture);
        Map(m => m.Year).TypeConverter<YearNullableIntConverter>();
    }
}

public class LikeCsvRecordMap : ClassMap<LikeCsvRecord>
{
    public LikeCsvRecordMap()
    {
        AutoMap(System.Globalization.CultureInfo.InvariantCulture);
        Map(m => m.Year).TypeConverter<YearNullableIntConverter>();
    }
}
