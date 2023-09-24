using Microsoft.JSInterop;

namespace Notifications.UI;

// This class provides an example of how JavaScript functionality can be wrapped
// in a .NET class for easy consumption. The associated JavaScript module is
// loaded on demand when first needed.
//
// This class can be registered as scoped DI service and then injected into Blazor
// components for use.

public class NotificationsJs : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> moduleTask;

    public NotificationsJs(IJSRuntime jsRuntime)
    {
        moduleTask = new (() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/Notifications.UI/notifications.js").AsTask());
    }

    public async ValueTask<bool> UpdateCounter(int count, bool replayAnimation)
    {
        // Best to always return some value from JS used through interop to avoid task cancelled errors.
        var module = await moduleTask.Value;
        return await module.InvokeAsync<bool>("updateNotificationCounter", count, replayAnimation);
    }

    public async ValueTask DisposeAsync()
    {
        if (moduleTask.IsValueCreated)
        {
            var module = await moduleTask.Value;
            await module.DisposeAsync();
        }
    }
}
