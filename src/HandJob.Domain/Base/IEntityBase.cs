using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HandJob.Domain.Base;

public interface IEntity
{
    public Guid Id { get; set; }
}

public abstract class EntityBase : IEntity
{
    public Guid Id { get; set; } = Guid.Empty;
}

public interface IEnabled
{
    public bool Enabled { get; set; }
}

public interface IDeleted
{
    public bool Deleted { get; set; }
}

public interface ISoftDeleted
{
    public bool Deleted { get; set; }
}

public interface ICreator
{
    public Guid CreatorId { get; set; }
    public DateTime CreateTime { get; set; }
}

public interface IModifier
{
    public Guid ModifierId { get; set; }
    public DateTime? UpdateTime { get; set; }
}

public interface IReadOnly
{

}
