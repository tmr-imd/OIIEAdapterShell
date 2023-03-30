using Isbm2Client.Interface;

namespace TaskQueueing;

public class ConsumerJob
{
    private readonly IConsumerRequest consumer;

    public ConsumerJob( IConsumerRequest consumer )
    {
        this.consumer = consumer;
    }

    public async Task<string> PostRequest<T>( string sessionId, T value, string topic ) where T : notnull
    {
        var request = await consumer.PostRequest( sessionId, value, topic );

        return request.Id;
    }
}
