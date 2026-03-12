using System.ComponentModel.DataAnnotations;

namespace Users.Domain.Entity
{
    public class AuditLog : EntityBase
    {
        [Required]
        public string TableName { get; set; } = string.Empty;

        [Required]
        public string Action { get; set; } = string.Empty; // Added/Modified/Deleted

        public string? KeyValues { get; set; }

        public string? OldValues { get; set; }

        public string? NewValues { get; set; }

        public string? User { get; set; }

        public string? CorrelationId { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
