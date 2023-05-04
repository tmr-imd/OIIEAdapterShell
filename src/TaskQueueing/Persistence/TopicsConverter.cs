using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace AdapterQueue.Persistence.Configuration;

public class TopicsConverter : ValueConverter<IEnumerable<string>, string>
{
    public TopicsConverter()
        : base(
            v => JsonSerializer.Serialize(v, typeof(IEnumerable<string>), new JsonSerializerOptions() ),
            v => JsonDocument.Parse(v, new JsonDocumentOptions()).Deserialize<IEnumerable<string>>(new JsonSerializerOptions()) ?? Array.Empty<string>() )
    {
    }
}
