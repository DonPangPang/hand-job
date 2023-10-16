namespace HandJob.Domain.Base;

public interface IPaging
{
    public int Page { get; set; }
    public int PageSize { get; set; }
}