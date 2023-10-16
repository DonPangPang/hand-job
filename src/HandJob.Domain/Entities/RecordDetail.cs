using HandJob.Domain.Base;
using HandJob.Domain.Enums;

namespace HandJob.Domain.Entities;

public class RecordDetail : EntityBase, ISoftDeleted, ICreator, IReadOnly
{
    public JobType Type { get; set; } = JobType.None;
    /// <summary>
    /// seconds
    /// </summary>
    public decimal Seconds { get; set; } = 0;

    public bool Deleted { get; set; }
    public Guid CreatorId { get; set; }
    public User? Creator { get; set; }
    public DateTime CreateTime { get; set; }

    public Guid RecordId { get; set; }
    public Record? Record { get; set; }
}