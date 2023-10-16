using System.ComponentModel.DataAnnotations;
using HandJob.Domain.Base;

namespace HandJob.Domain.Entities;

public class User : EntityBase, ISoftDeleted
{
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    public DateTime Born { get; set; }
    [MaxLength(500)]
    public string? Description { get; set; }

    public bool Deleted { get; set; }

    public Guid AccountId { get; set; }
    public Account? Account { get; set; }
}
