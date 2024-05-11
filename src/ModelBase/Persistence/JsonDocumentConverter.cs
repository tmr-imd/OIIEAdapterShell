using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace ModelBase.Persistence.Configuration;

public class JsonDocumentConverter : ValueConverter<JsonDocument, string>
{
    public JsonDocumentConverter()
        : base(
            // May want to use the enescaped encoder since we are handling the JSON at the database translation level of EF, clients never see the raw JSON
            v => JsonSerializer.Serialize(v, typeof(JsonDocument), new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping } ),
            // v => JsonSerializer.Serialize(v, typeof(JsonDocument), new JsonSerializerOptions() ),
            v => JsonDocument.Parse(v, new JsonDocumentOptions()) )
    {
    }
}
