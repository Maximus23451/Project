using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Channels;

namespace SertecDashboard.Api.Services;

/// <summary>
/// Singleton service that manages all active Server-Sent Event (SSE) connections.
///
/// Architecture:
///   • Each connected client gets its own unbounded Channel&lt;string&gt;.
///     The channel is thread-safe — the background service can write to it
///     without holding any locks, and the HTTP response writer reads from it
///     on the request's own thread.
///   • BroadcastAsync() writes the formatted SSE message to every active channel.
///   • Disconnected clients have their channels removed lazily when writing fails.
///
/// Why channels instead of direct HttpResponse.Write()?
///   Direct writes from a BackgroundService would require synchronizing with the
///   ASP.NET request pipeline, which is not thread-safe.  Channels decouple the
///   producer (background service) from the consumer (HTTP streaming loop).
/// </summary>
public class SseService
{
    // clientId → write-side of that client's message channel
    private readonly ConcurrentDictionary<string, Channel<string>> _clients = new();

    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNamingPolicy        = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition      = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    // ── Client lifecycle ──────────────────────────────────────────────────

    /// <summary>
    /// Registers a new SSE client and returns its message channel.
    /// Call this when the HTTP request for /api/stream is established.
    /// </summary>
    public (string clientId, ChannelReader<string> reader) AddClient()
    {
        var id      = NewId();
        var channel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
        {
            SingleReader = true,   // one reader per client (the HTTP streaming loop)
            SingleWriter = false,  // multiple writers (broadcast from background service etc.)
        });
        _clients[id] = channel;
        return (id, channel.Reader);
    }

    /// <summary>Removes a client when its HTTP connection closes.</summary>
    public void RemoveClient(string clientId)
    {
        if (_clients.TryRemove(clientId, out var ch))
            ch.Writer.TryComplete();   // signal the reader loop to stop
    }

    // ── Broadcasting ──────────────────────────────────────────────────────

    /// <summary>
    /// Broadcasts an SSE event to every connected client.
    /// The message is formatted as:  "event: {name}\ndata: {json}\n\n"
    /// </summary>
    public void Broadcast(string eventName, object data)
    {
        var json = JsonSerializer.Serialize(data, _json);
        var msg  = $"event: {eventName}\ndata: {json}\n\n";

        foreach (var (id, ch) in _clients)
        {
            // TryWrite never blocks — if the channel is full (shouldn't happen with
            // UnboundedChannel) or completed (client disconnected), we skip it.
            if (!ch.Writer.TryWrite(msg))
                _clients.TryRemove(id, out _);
        }
    }

    /// <summary>Sends a keep-alive ping comment to every client (no event name).</summary>
    public void Ping()
    {
        foreach (var (id, ch) in _clients)
        {
            if (!ch.Writer.TryWrite(": ping\n\n"))
                _clients.TryRemove(id, out _);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────
    private static string NewId() =>
        Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(8))
               .ToLowerInvariant();
}
