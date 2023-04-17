using System.Text.Json;

namespace TaskQueueing.ObjectModel.Models;

public record class Response : AbstractMessage
{
    // RequestId is essential to the identity of the response and means we can
    // lazy load the full Request object only when we need its content.
    public string RequestId { get; set; } = "";
    public string ResponseId { get; set; } = "";

    public Guid RequestRefId { get; set; }
    [System.ComponentModel.DataAnnotations.Schema.ForeignKey("RequestRefId")]
    public Request Request { get; set; } = null!;
}
