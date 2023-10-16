using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using HandJob.Domain.Base;

namespace HandJob.Domain.Entities;

public class Account : EntityBase, IModifier, IEnabled, ISoftDeleted
{
    [MaxLength(200)]
    public string Username { get; set; } = string.Empty;
    [MaxLength(200)]
    public string Password { get; set; } = string.Empty;

    public bool IsSuper { get; set; } = false;
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public Guid ModifierId { get; set; }
    public User? Modifier { get; set; }
    public DateTime? UpdateTime { get; set; }
    public bool Enabled { get; set; } = true;
    public bool Deleted { get; set; }
}
