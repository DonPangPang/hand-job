using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using HandJob.Domain.Entities;
using HandJob.Domain.Enums;

namespace HandJob.Domain.ViewModels;

public interface IViewModel
{
    public Guid Id { get; set; }
}

[AutoMap(typeof(Account), ReverseMap = true)]
public record VAccount : IViewModel
{
    public Guid Id { get; set; }
    [MaxLength(200)]
    public string Username { get; set; } = string.Empty;
    [MaxLength(200)]
    public string Password { get; set; } = string.Empty;

    public Guid UserId { get; set; }
    public VUser? User { get; set; }

    public Guid ModifierId { get; set; }
    public VUser? Modifier { get; set; }
    public DateTime? UpdateTime { get; set; }
    public bool Enabled { get; set; } = true;
    public bool Deleted { get; set; }
}

[AutoMap(typeof(Record), ReverseMap = true)]
public record VRecord : IViewModel
{
    public Guid Id { get; set; }

    public bool Deleted { get; set; }
    public Guid CreatorId { get; set; }
    public VUser? Creator { get; set; }
    public DateTime CreateTime { get; set; }


    public ICollection<VRecordDetail> RecordDetails { get; set; } = new List<VRecordDetail>();
}

[AutoMap(typeof(Record), ReverseMap = true)]
public record VRecordList : IViewModel
{
    public Guid Id { get; set; }

    public bool Deleted { get; set; }
    public Guid CreatorId { get; set; }
    public VUser? Creator { get; set; }
    public DateTime CreateTime { get; set; }
}

[AutoMap(typeof(RecordDetail), ReverseMap = true)]
public record VRecordDetail : IViewModel
{
    public Guid Id { get; set; }

    public JobType Type { get; set; } = JobType.None;
    /// <summary>
    /// seconds
    /// </summary>
    public decimal Seconds { get; set; } = 0;

    public bool Deleted { get; set; }
    public Guid CreatorId { get; set; }
    public VUser? Creator { get; set; }
    public DateTime CreateTime { get; set; }

    public Guid RecordId { get; set; }
    public VRecord? Record { get; set; }
}
[AutoMap(typeof(User), ReverseMap = true)]
public record VUser : IViewModel
{
    public Guid Id { get; set; }

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    public DateTime Born { get; set; }
    [MaxLength(500)]
    public string? Description { get; set; }

    public bool Deleted { get; set; }

    public Guid AccountId { get; set; }
    public VAccount? Account { get; set; }
}

public record VLogin
{
    [Required]
    public string Username { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
}
[AutoMap(typeof(Account), ReverseMap = true)]
public record VRegisterAccount
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public VRegisterUser? User { get; set; }
}
[AutoMap(typeof(User), ReverseMap = true)]
public record VRegisterUser
{
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    public DateTime Born { get; set; }
    [MaxLength(500)]
    public string? Description { get; set; }
}

public record VChangePassword
{
    [Required]
    public string NewPassword { get; set; } = string.Empty;
    [Required]
    public string OldPassword { get; set; } = string.Empty;
}