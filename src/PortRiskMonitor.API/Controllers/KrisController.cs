// ============================================================
// KrisController.cs — REST API for KRI CRUD operations
//
// Presentation Layer: This controller is THIN.
// It does no business logic — it delegates everything to IKriService.
// Its only responsibilities are:
//   1. Parse and validate HTTP requests
//   2. Call the appropriate service method
//   3. Return the correct HTTP status code and response body
//
// All audit logging is handled automatically by BusinessLogicAuditFilter.
// No [AuditLog] attributes needed here.
//
// Routes:
//   GET    /api/kris              → GetAllKris
//   GET    /api/kris/status       → GetDashboardStatus (dashboard polling)
//   GET    /api/kris/{id}         → GetKriById
//   GET    /api/kris/{id}/detail  → GetKriDetail (chart + alert history)
//   POST   /api/kris              → CreateKri
//   PUT    /api/kris/{id}         → UpdateKri (optimistic locking)
//   DELETE /api/kris/{id}         → DeleteKri
//   POST   /api/kris/{id}/override → OverrideKriValue (demo scenario control)
// ============================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortRiskMonitor.Application.DTOs;
using PortRiskMonitor.Application.DTOs.Read;
using PortRiskMonitor.Application.DTOs.Write;
using PortRiskMonitor.Application.DTOs.Shared;
using PortRiskMonitor.Application.DTOs.Enums;
using PortRiskMonitor.Application.Interfaces;

namespace PortRiskMonitor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class KrisController : ControllerBase
{
    private readonly IKriService _kriService;
    private readonly ILogger<KrisController> _logger;

    public KrisController(IKriService kriService, ILogger<KrisController> logger)
    {
        _kriService = kriService;
        _logger     = logger;
    }

    // ── GET /api/kris ─────────────────────────────────────────────────────────
    // Returns all KRI definitions for the KRI Manager list view
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<KriDefinitionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllKris()
    {
        var kris = await _kriService.GetAllKrisAsync();
        return Ok(kris);
    }

    // ── GET /api/kris/status ──────────────────────────────────────────────────
    // Dashboard polling endpoint — called every 30s by React dashboard
    // Returns current value, risk level, and composite score for all KRIs
    // This must be fast — it's the hot path
    [HttpGet("status")]
    [ProducesResponseType(typeof(DashboardStatusDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardStatus()
    {
        var status = await _kriService.GetDashboardStatusAsync();
        return Ok(status);
    }

    // ── GET /api/kris/{id} ────────────────────────────────────────────────────
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(KriDefinitionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetKriById([FromRoute] Guid id)
    {
        var kris = await _kriService.GetAllKrisAsync();
        var kri  = kris.FirstOrDefault(k => k.Id == id);

        if (kri == null) return NotFound(new { message = $"KRI with id {id} not found" });

        return Ok(kri);
    }

    // ── GET /api/kris/{id}/detail ─────────────────────────────────────────────
    // Returns full detail for a single KRI — used by the trend chart view
    [HttpGet("{id:guid}/detail")]
    [ProducesResponseType(typeof(KriDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetKriDetail([FromRoute] Guid id)
    {
        var detail = await _kriService.GetKriDetailAsync(id);

        if (detail == null) return NotFound(new { message = $"KRI with id {id} not found" });

        return Ok(detail);
    }

    // ── POST /api/kris ────────────────────────────────────────────────────────
    [HttpPost]
    [ProducesResponseType(typeof(KriDefinitionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateKri([FromBody] CreateKriDto dto)
    {
        // FluentValidation runs automatically via the global filter — no manual validation needed here
        // TODO: Catch domain validation exceptions (e.g., weight sum > 1.0) and return 400

        var created = await _kriService.CreateKriAsync(dto);

        return CreatedAtAction(
            nameof(GetKriById),
            new { id = created.Id },
            created);
    }

    // ── PUT /api/kris/{id} ────────────────────────────────────────────────────
    // NFR: Optimistic Locking
    // The request body MUST include RowVersion (base64 string from the GET response).
    // If the KRI was modified by another user since it was loaded, returns 409 Conflict.
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(KriDefinitionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)] // optimistic lock conflict
    public async Task<IActionResult> UpdateKri([FromRoute] Guid id, [FromBody] UpdateKriDto dto)
    {
        try
        {
            var updated = await _kriService.UpdateKriAsync(id, dto);
            return Ok(updated);
        }
        catch (DbUpdateConcurrencyException)
        {
            // NFR: Optimistic Locking — another user modified this KRI since the client loaded it.
            // Return 409 Conflict with the CURRENT server values so the React frontend
            // can show the ConcurrencyConflictModal with a side-by-side diff.
            _logger.LogWarning("Optimistic concurrency conflict on KRI {id}", id);

            // TODO: Fetch the current server values and include them in the response
            // var currentServerValues = await _kriService.GetKriDetailAsync(id);

            return Conflict(new
            {
                message = "This KRI was modified by another user. Please review the changes and try again.",
                // TODO: Include current server values here so React can show the diff
                // currentValues = currentServerValues
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"KRI with id {id} not found" });
        }
    }

    // ── DELETE /api/kris/{id} ─────────────────────────────────────────────────
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteKri([FromRoute] Guid id)
    {
        try
        {
            await _kriService.DeleteKriAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"KRI with id {id} not found" });
        }
    }

    // ── POST /api/kris/{id}/override ──────────────────────────────────────────
    // Demo scenario control — manually force a specific value for a KRI
    // Used by the Demo Control Panel in the React frontend
    [HttpPost("{id:guid}/override")]
    [ProducesResponseType(typeof(KriReadingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> OverrideKriValue(
        [FromRoute] Guid id,
        [FromBody] OverrideKriValueDto dto)
    {
        try
        {
            var reading = await _kriService.OverrideKriValueAsync(id, dto.Value);
            return Ok(reading);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"KRI with id {id} not found" });
        }
    }
}
