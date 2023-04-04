using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace AdapterQueue.Persistence.Configuration;

public class JsonDocumentConverter : ValueConverter<JsonDocument, string>
{
    public JsonDocumentConverter()
        : base(
            v => JsonSerializer.Serialize(v, typeof(JsonDocument), new JsonSerializerOptions() ),
            v => JsonDocument.Parse(v, new JsonDocumentOptions()) )
    {
    }
}
