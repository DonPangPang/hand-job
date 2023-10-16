using HandJob.Domain.Base;

namespace HandJob.Domain.Entities;

public class Record : EntityBase, ISoftDeleted, ICreator, IReadOnly
{
    public bool Deleted { get; set; }
    public Guid CreatorId { get; set; }
    public User? Creator { get; set; }
    public DateTime CreateTime { get; set; }


    public ICollection<RecordDetail> RecordDetails { get; set; } = new List<RecordDetail>();
}
