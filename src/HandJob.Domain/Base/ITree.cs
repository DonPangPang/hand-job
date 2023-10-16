namespace HandJob.Domain.Base;

/// <summary>
/// 树形数据
/// </summary>
public interface ITree<T>
{
    /// <summary>
    /// Id
    /// </summary>
    /// <value></value>
    public Guid Id { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public Guid? ParentId { get; set; }
    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public ICollection<T>? Children { get; set; }

    /// <summary>
    /// 树状结构
    /// </summary>
    /// <value></value>
    public string? TreeIds { get; set; }

    /// <summary>
    /// 层级
    /// </summary>
    /// <value></value>
    public int Level { get; set; }
}