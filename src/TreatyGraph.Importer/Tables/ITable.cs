using System.Data;
using TreatyGraph.Importer.Etl;

namespace TreatyGraph.Importer.Tables;

/// <summary>One SQL table: dbo.{Name}, created by sql/Tables/{Name}.sql.</summary>
public interface ITable
{
    string Name { get; }

    DataTable BuildData(ImportData data);
}
