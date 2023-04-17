using System.Text.Json;

namespace TaskQueueing.ObjectModel.Models;

public record class Request : AbstractMessage
{
    public string RequestId { get; set; } = "";
    public string Topic { get; set; } = "";
    public ICollection<Response> Responses { get; } = new List<Response>();

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public JsonDocument? ResponseContent
    {
        get
        {
            return Responses.LastOrDefault()?.Content;
        }
    }

}
