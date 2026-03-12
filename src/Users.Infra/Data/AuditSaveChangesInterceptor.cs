using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Users.Domain.Entity;

namespace Users.Infra.Data
{
    public class AuditSaveChangesInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuditSaveChangesInterceptor> _logger;

        public AuditSaveChangesInterceptor(IHttpContextAccessor httpContextAccessor, ILogger<AuditSaveChangesInterceptor> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            TryAudit(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            TryAudit(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void TryAudit(DbContext? context)
        {
            if (context == null) return;

            var entries = context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
                .ToList();

            if (!entries.Any()) return;

            var httpContext = _httpContextAccessor.HttpContext;
            var user = httpContext?.User?.Identity?.Name ?? "-";
            const string headerKey = "X-Correlation-Id";
            var correlationId = httpContext?.Items[headerKey] as string ?? httpContext?.Request.Headers[headerKey].FirstOrDefault();

            var auditEntries = new List<AuditLog>();

            foreach (var entry in entries)
            {
                var audit = new AuditLog
                {
                    TableName = entry.Entity.GetType().Name,
                    Action = entry.State.ToString(),
                    User = user,
                    CorrelationId = correlationId,
                    Timestamp = DateTime.UtcNow
                };

                var keyValues = new Dictionary<string, object?>();
                foreach (var prop in entry.Properties)
                {
                    if (prop.Metadata.IsPrimaryKey())
                        keyValues[prop.Metadata.Name] = prop.CurrentValue;
                }

                audit.KeyValues = JsonSerializer.Serialize(keyValues);

                if (entry.State == EntityState.Added)
                {
                    var newValues = entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
                    audit.NewValues = JsonSerializer.Serialize(newValues);
                }
                else if (entry.State == EntityState.Modified)
                {
                    var oldValues = new Dictionary<string, object?>();
                    var newValues = new Dictionary<string, object?>();
                    foreach (var prop in entry.Properties)
                    {
                        oldValues[prop.Metadata.Name] = prop.OriginalValue;
                        newValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                    audit.OldValues = JsonSerializer.Serialize(oldValues);
                    audit.NewValues = JsonSerializer.Serialize(newValues);
                }
                else if (entry.State == EntityState.Deleted)
                {
                    var oldValues = entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);
                    audit.OldValues = JsonSerializer.Serialize(oldValues);
                }

                auditEntries.Add(audit);
            }

            try
            {
                // add audit entries to the same context
                var auditSet = context.Set<AuditLog>();
                auditSet.AddRange(auditEntries);
                // do not save here; they will be saved as part of the same SaveChanges call
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add audit entries");
            }
        }
    }
}
