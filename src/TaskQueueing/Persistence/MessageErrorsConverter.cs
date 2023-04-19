using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

using TaskQueueing.ObjectModel.Models;

namespace AdapterQueue.Persistence.Configuration;

public class MessageErrorsConverter : ValueConverter<IEnumerable<MessageError>, string>
{
    public MessageErrorsConverter()
        : base(
            v => JsonSerializer.Serialize(v, typeof(IEnumerable<MessageError>), new JsonSerializerOptions() ),
            v => JsonDocument.Parse(v, new JsonDocumentOptions()).Deserialize<IEnumerable<MessageError>>(new JsonSerializerOptions()) ?? Enumerable.Empty<MessageError>() )
    {
    }
}
