using HandJob.Domain.Base;
using HandJob.Domain.Enums;

namespace HandJob.Domain.Parameters;

public class AccountParameters : IParameterBase, IPaging, ISorting
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public string? OrderBy { get; set; }
}

public class UserParameters : IParameterBase, IPaging, ISorting
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public string? OrderBy { get; set; }
}

public class RecordParameters : IParameterBase, IPaging, ISorting
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public string? OrderBy { get; set; }

    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}

public class RecordDetailParameters : IParameterBase, IPaging, ISorting
{
    public Guid? RecordId { get; set; }

    public JobType? Type { get; set; }
    public int? Seconds { get; set; }

    public int Page { get; set; }
    public int PageSize { get; set; }
    public string? OrderBy { get; set; }
}