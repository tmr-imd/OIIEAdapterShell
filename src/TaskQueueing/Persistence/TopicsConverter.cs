using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace AdapterQueue.Persistence.Configuration;

public class TopicsConverter : ValueConverter<IEnumerable<string>, string>
{
    public TopicsConverter()
        : base(
            // May want to use the enescaped encoder since we are handling the JSON at the database translation level of EF, clients never see the raw JSON
            v => JsonSerializer.Serialize(v, typeof(IEnumerable<string>), new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping } ),
            // v => JsonSerializer.Serialize(v, typeof(IEnumerable<string>), new JsonSerializerOptions() ),
            v => JsonDocument.Parse(v, new JsonDocumentOptions()).Deserialize<IEnumerable<string>>(new JsonSerializerOptions()) ?? Array.Empty<string>() )
    {
    }
}
