using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SertecDashboard.Api.Data;
using SertecDashboard.Api.Services;
using System.Text.Json;

namespace SertecDashboard.Api.Controllers;

[ApiController]
[Route("api")]
public class StreamController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly SseService   _sse;

    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNamingPolicy   = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    public StreamController(AppDbContext db, SseService sse)
    {
        _db  = db;
        _sse = sse;
    }

    // GET /api/stream
    [HttpGet("stream")]
    public async Task Stream(CancellationToken ct)
    {
        try
        {
            Response.Headers["Content-Type"]  = "text/event-stream";
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["Connection"]    = "keep-alive";
            Response.Headers["X-Accel-Buffering"] = "no"; 
            await Response.Body.FlushAsync(ct);

            var (clientId, reader) = _sse.AddClient();

            try
            {
                var pending = await _db.PendingItems.ToListAsync(ct);
                var init = new
                {
                    questions      = await _db.Questions.ToListAsync(ct),
                    responses      = await _db.Responses.ToListAsync(ct),
                    machines       = await BuildMachineListAsync(ct),
                    parts          = await _db.Parts.ToListAsync(ct),
                    docs           = await _db.Documents
                                              .Select(d => new { d.Id, d.Name, d.Size, d.UploadedAt })
                                              .ToListAsync(ct),
                    pending        = pending.LastOrDefault(),
                    pendingList    = pending,
                    pendingDoc     = (object?)null,   
                    shifts         = await _db.Shifts.ToListAsync(ct),
                    alerts         = await _db.Alerts.ToListAsync(ct),
                    passwordResets = await _db.PasswordResets.ToListAsync(ct),
                };

                var initJson = JsonSerializer.Serialize(init, _json);
                await WriteLineAsync($"event: init\ndata: {initJson}\n\n", ct);

                await foreach (var msg in reader.ReadAllAsync(ct))
                {
                    await WriteLineAsync(msg, ct);
                }
            }
            finally
            {
                _sse.RemoveClient(clientId);
            }
        }
        catch (OperationCanceledException)
        {
            // Kliens bontotta a kapcsolatot - normális
        }
        catch (Exception ex)
        {
            // Logolj hibát
            await Response.WriteAsync($"event: error\ndata: {ex.Message}\n\n", ct);
        }
    }

    private async Task WriteLineAsync(string text, CancellationToken ct)
    {
        try
        {
            await Response.WriteAsync(text, ct);
            await Response.Body.FlushAsync(ct);
        }
        catch (OperationCanceledException)
        {
            // Ignoreálj, a kliens bontotta
        }
    }

    private async Task<List<object>> BuildMachineListAsync(CancellationToken ct)
    {
        var machines = await _db.Machines.Include(m => m.MachineParts)
            .ThenInclude(mp => mp.Part)
            .ToListAsync(ct);
        return machines.Select(m => (object)new
        {
            id    = m.Id,
            name  = m.Name,
            parts = m.MachineParts.Select(mp => new
            {
                partId = mp.Part.partId,
                name = mp.Part.Name,
                serialNumber = mp.Part.serialNumber
            }).ToList(),
        }).ToList();
    }

    // Valahol ahol a gépek/parts módosulnak:
    [HttpGet]
    public async Task UpdateMachines()
    {
        var machines = await _db.Machines.Include(m => m.MachineParts)
            .ThenInclude(mp => mp.Part)
            .ToListAsync();
        _sse.Broadcast("machines", machines);
    }
}
