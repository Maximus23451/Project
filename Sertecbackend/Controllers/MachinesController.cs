using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SertecDashboard.Api.Data;
using SertecDashboard.Api.Models;
using SertecDashboard.Api.Services;

namespace SertecDashboard.Api.Controllers;


[ApiController]
[Route("api/machines")]
public class MachinesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly SseService   _sse;

    public MachinesController(AppDbContext db, SseService sse)
    {
        _db  = db;
        _sse = sse;
    }

    // GET /api/machines
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var machines = await _db.Machines
                .Include(m => m.MachineParts)
                .ThenInclude(mp => mp.Part)
                .ToListAsync();

            return Ok(machines.Select(ToDto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Gépek lekérése sikertelen", details = ex.Message });
        }
    }


    // POST /api/machines
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMachineRequest body)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(body.Name))
                return BadRequest(new { error = "Név megadása szükséges" });

            var machine = new Machine
            {
                Name = body.Name.Trim()

            };
            _db.Machines.Add(machine);
            await _db.SaveChangesAsync();

            await BroadcastAsync();
            return Ok(ToDto(machine));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Gép létrehozása sikertelen", details = ex.Message });
        }
    }

    // POST /api/machines/{id}/parts
    [HttpPost("{id}/parts")]
    public async Task<IActionResult> AddPart(int id, [FromBody] AddPartRequest body)
    {

        try
        {
            var machine = _db.Machines
            .Where(x => x.Id == id)
            .FirstOrDefault();


            if (machine == null) return NotFound(new { error = "Gép nem található" });

            var partName = body.Part?.Trim();

            var part = _db.Parts
                .Where(x => x.serialNumber == body.Part)
                .FirstOrDefault();

            var machineparts = _db.MachineParts
                .Where(x => x.MachineId == id && x.PartId == part.partId)
                .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(partName) && machineparts == null)
            {
                _db.MachineParts.Add(new MachinePart { MachineId = id, PartId = part.partId });
                await _db.SaveChangesAsync();
                await BroadcastAsync();
            }

            return Ok(ToDto(machine));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Alkatrész hozzáadása sikertelen", details = ex.Message });
        }

    }

    // DELETE /api/machines/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {

            var machineparts = _db.MachineParts
                .Where(x => x.MachineId == id)
                .ToList();

            foreach (var item in machineparts)
            {
                _db.MachineParts.Remove(item);
            }
            _db.SaveChanges();

            var machine = await _db.Machines.FindAsync(id);
            if (machine != null)
            {
                _db.Machines.Remove(machine);
                await _db.SaveChangesAsync();
            }

            await BroadcastAsync();
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Gép törlése sikertelen", details = ex.Message });
        }

    }

    [HttpDelete("{id}/parts/{serialNumber}")]
    public async Task<IActionResult> RemovePart(int id, string serialNumber)
    {
        try
        {

            var machineparts = _db.MachineParts
                .Include(mp => mp.Part)
                .Where(x => x. MachineId == id)
                .ToList();

            foreach (var item in machineparts)
            {
                if(item.Part.serialNumber == serialNumber) _db.MachineParts.Remove(item);
            }
            _db.SaveChanges();

            await BroadcastAsync();
            return Ok(new { message = "Alkatrész sikeresen törölve" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Alkatrész törlése sikertelen", details = ex.Message });
        }

    }


    private static object ToDto(Machine m) => new
    {
        id    = m.Id,
        name  = m.Name,
        parts = m.MachineParts.Select(mp =>  new
        {
            mp.Part.Name,
            mp.Part.serialNumber,
        }).ToList(),
    };

    private async Task BroadcastAsync()
    {
        var machines = await _db.Machines.Include(m => m.MachineParts)
            .ThenInclude(mp => mp.Part)
            .ToListAsync();
        _sse.Broadcast("machines", machines.Select(ToDto));
    }
}

public record CreateMachineRequest(string? Name);
public record AddPartRequest(string? Part);
