using CsvHelper;
using System.Globalization;

namespace AdapterServer.Data;

public class StructureAssetService
{
    public static StructureAsset[] GetStructures(StructureAssetsFilter filter)
    {
        using var reader = new StreamReader("./Data/Structure Assets.csv");
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        var records = csv.GetRecords<StructureAsset>();

        if (filter.FilterCode != "")
            records = records.Where(x => x.Code.ToLower().Contains(filter.FilterCode.ToLower()));

        if (filter.FilterType != "")
            records = records.Where(x => x.Type.ToLower().Contains(filter.FilterType.ToLower()));

        if (filter.FilterLocation != "")
            records = records.Where(x => x.Location.ToLower().Contains(filter.FilterLocation.ToLower()));

        if (filter.FilterOwner != "")
            records = records.Where(x => x.Owner.ToLower().Contains(filter.FilterOwner.ToLower()));

        if (filter.FilterCondition != "")
            records = records.Where(x => x.Condition.ToLower().Contains(filter.FilterCondition.ToLower()));

        if (filter.FilterInspector != "")
            records = records.Where(x => x.Inspector.ToLower().Contains(filter.FilterInspector.ToLower()));

        return records.ToArray();
    }
}