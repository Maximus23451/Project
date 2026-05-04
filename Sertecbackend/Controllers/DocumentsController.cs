using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SertecDashboard.Api.Data;
using SertecDashboard.Api.Models;
using SertecDashboard.Api.Services;

namespace SertecDashboard.Api.Controllers;

[ApiController]
[Route("api")]
public class DocumentsController : ControllerBase
{
    private readonly AppDbContext      _db;
    private readonly SseService        _sse;
    private readonly IWebHostEnvironment _env;

    private static object? _pendingDoc = null;

    public DocumentsController(AppDbContext db, SseService sse, IWebHostEnvironment env)
    {
        _db  = db;
        _sse = sse;
        _env = env;
    }


    // GET /api/docs
    [HttpGet("docs")]
    public async Task<IActionResult> GetDocs()
    {
        try
        {
            return Ok(await _db.Documents
                    .Select(d => new { d.Id, d.Name, d.Size, d.UploadedAt })
                    .ToListAsync());
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Dokumentumok lekérése sikertelen", details = ex.Message });
        }

    }


    [HttpPost("docs")]
    public async Task<IActionResult> Upload([FromBody] UploadDocRequest body)
    {

        try
        {
            if (string.IsNullOrWhiteSpace(body.Name) || string.IsNullOrWhiteSpace(body.Data))
                return BadRequest(new { error = "Név és adat megadása kötelező" });

            var id = NewId();
            string dataToStore;

            if (body.Data.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                body.Data.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                dataToStore = body.Data;
            }
            else
            {
                try
                {
                    var base64 = body.Data.Contains(',')
                        ? body.Data[(body.Data.IndexOf(',') + 1)..]
                        : body.Data;
                    var bytes = Convert.FromBase64String(base64);
                    var ext = Path.GetExtension(body.Name).ToLowerInvariant();
                    if (string.IsNullOrEmpty(ext)) ext = ".pdf";
                    var fileName = id + ext;
                    var docsDir = Path.Combine(_env.WebRootPath, "docs");
                    Directory.CreateDirectory(docsDir);
                    await System.IO.File.WriteAllBytesAsync(Path.Combine(docsDir, fileName), bytes);
                    dataToStore = $"/docs/{fileName}";
                }
                catch (FormatException)
                {
                    return BadRequest(new { error = "Érvénytelen base64 adat" });
                }
            }

            var doc = new Document
            {
                Id = id,
                Name = body.Name,
                Size = body.Size ?? string.Empty,
                Data = dataToStore,
                UploadedAt = DateTime.Now.ToString("yyyy.MM.dd. HH:mm:ss"),
            };
            _db.Documents.Add(doc);
            await _db.SaveChangesAsync();

            var list = await _db.Documents
                .Select(d => new { d.Id, d.Name, d.Size, d.UploadedAt })
                .ToListAsync();
            _sse.Broadcast("docs", list);

            return Ok(new { doc.Id, doc.Name, doc.Size, doc.UploadedAt });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Dokumentum feltöltése sikertelen", details = ex.Message });
        }

    }

    // GET /api/docs/{id}
    [HttpGet("docs/{id}")]
    public async Task<IActionResult> GetDoc(string id)
    {
        try
        {
            var doc = await _db.Documents.FindAsync(id);
            if (doc == null) return NotFound(new { error = "Not found" });
            return Ok(new { doc.Id, doc.Name, data = doc.Data });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Dokumentum lekérése sikertelen", details = ex.Message });
        }

    }

    // DELETE /api/docs
    [HttpDelete("docs")]
    public async Task<IActionResult> DeleteAllDocs()
    {
        try
        {
            var all = await _db.Documents.ToListAsync();
            foreach (var doc in all)
            {
                if (doc.Data.StartsWith("/docs/", StringComparison.OrdinalIgnoreCase))
                {
                    var filePath = Path.Combine(_env.WebRootPath, doc.Data.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                }
            }
            _db.Documents.RemoveRange(all);
            await _db.SaveChangesAsync();
            _sse.Broadcast("docs", new List<object>());
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Dokumentumok törlése sikertelen", details = ex.Message });
        }


    }

    // DELETE /api/docs/{id}
    [HttpDelete("docs/{id}")]
    public async Task<IActionResult> DeleteDoc(string id)
    {
        try
        {
            var doc = await _db.Documents.FindAsync(id);
            if (doc != null)
            {
                if (doc.Data.StartsWith("/docs/", StringComparison.OrdinalIgnoreCase))
                {
                    var filePath = Path.Combine(_env.WebRootPath, doc.Data.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                }
                _db.Documents.Remove(doc);
                await _db.SaveChangesAsync();
            }

            var list = await _db.Documents
                .Select(d => new { d.Id, d.Name, d.Size, d.UploadedAt })
                .ToListAsync();
            _sse.Broadcast("docs", list);
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Dokumentum törlése sikertelen", details = ex.Message });


        }
    }

    // GET /api/pending-doc
    [HttpGet("pending-doc")]
    public IActionResult GetPendingDoc()
    {

        try
        {
            return Ok(_pendingDoc);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Dokumentumok lekérése sikertelen", details = ex.Message });

        }
    }


    // POST /api/pending-doc
    [HttpPost("pending-doc")]
    public async Task<IActionResult> SetPendingDoc([FromBody] SetPendingDocRequest body)
    {

        try
        {
            var doc = await _db.Documents.FindAsync(body.DocId);
            if (doc == null) return NotFound(new { error = "Dokumentum nem található" });

            _pendingDoc = new
            {
                id = NewId(),
                docId = doc.Id,
                name = doc.Name,
                embedUrl = doc.Data,
                sentAt = DateTime.Now.ToString("yyyy.MM.dd. HH:mm:ss"),
            };
            _sse.Broadcast("pending-doc", _pendingDoc);
            return Ok(_pendingDoc);


        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Dokumentum lekérése sikertelen", details = ex.Message });

        }
    }

    // DELETE /api/pending-doc
    [HttpDelete("pending-doc")]
    public IActionResult ClearPendingDoc()
    {
        try
        {
            _pendingDoc = null;
            _sse.Broadcast("pending-doc", null!);
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Dokumentum törlése sikertelen", details = ex.Message });
        }
    }

    private static string NewId() =>
        Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(8))
               .ToLowerInvariant();
}

public record UploadDocRequest(string? Name, string? Size, string? Data);
public record SetPendingDocRequest(string? DocId);
