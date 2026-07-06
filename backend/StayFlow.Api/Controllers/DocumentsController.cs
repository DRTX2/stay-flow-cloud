using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Authorization;
using StayFlow.Application.Common.Storage;

namespace StayFlow.Api.Controllers;

/// <summary>
/// Tenant document storage (invoices, contracts, uploads) backed by S3 or local storage. Objects
/// are keyed under the caller's tenant id so one tenant can never read another's documents.
/// </summary>
[Authorize]
public sealed class DocumentsController(IDocumentStorage storage, ITenantProvider tenantProvider) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.BillingRead)]
    public async Task<ActionResult<IEnumerable<DocumentMetadata>>> List(CancellationToken cancellationToken)
    {
        var tenantId = tenantProvider.TenantId;
        if (tenantId is null)
        {
            return BadRequest("Tenant ID is missing.");
        }

        var prefix = $"{tenantId}";
        var documents = await storage.ListAsync(prefix, cancellationToken);
        return Ok(documents);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.BillingManage)]
    [RequestSizeLimit(25 * 1024 * 1024)]
    public async Task<ActionResult> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            return BadRequest("File is empty.");
        }

        var key = $"{TenantPrefix}{Guid.CreateVersion7()}-{Path.GetFileName(file.FileName)}";

        await using var stream = file.OpenReadStream();
        await storage.SaveAsync(key, stream, file.ContentType, cancellationToken);

        return Ok(new { key });
    }

    [HttpGet("url")]
    [Authorize(Policy = Permissions.BillingRead)]
    public async Task<ActionResult> GetUrl([FromQuery] string key, CancellationToken cancellationToken)
    {
        if (!OwnedByTenant(key))
        {
            return Forbid();
        }

        var url = await storage.GetDownloadUrlAsync(key, TimeSpan.FromMinutes(15), cancellationToken);
        return Ok(new { url });
    }

    [HttpGet("content")]
    [Authorize(Policy = Permissions.BillingRead)]
    public async Task<IActionResult> Download([FromQuery] string key, CancellationToken cancellationToken)
    {
        if (!OwnedByTenant(key))
        {
            return Forbid();
        }

        var document = await storage.GetAsync(key, cancellationToken);
        return document is null ? NotFound() : File(document.Content, document.ContentType);
    }

    private string TenantPrefix => $"{tenantProvider.TenantId}/";

    private bool OwnedByTenant(string key) => key.StartsWith(TenantPrefix, StringComparison.Ordinal);
}
