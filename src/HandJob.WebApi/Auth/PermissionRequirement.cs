using System.Security.Claims;
using HandJob.Domain.Entities;
using HandJob.WebApi.Options;
using HandJob.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HandJob.WebApi.Auth;

public class Session
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Guid UserId => Identity.UserId ?? Guid.Empty;

    public Session(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public CustomIdentity Identity => _httpContextAccessor?.HttpContext?.User?.Identity as CustomIdentity ?? new CustomIdentity();

    public User? User { get; set; }
}

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Name { get; set; } = null!;
    public string Secret { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;

    public int AccessExpiration { get; set; }
    public int RefreshExpiration { get; set; }
}

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUnitOfWork _unitOfWork;

    private readonly PermissionRequirement _tokenParameter;

    private Session _session;

    public PermissionHandler(
        IConfiguration config,
        IHttpContextAccessor httpContextAccessor,
        IUnitOfWork unitOfWork,
        Session session,
        IOptions<AppSettings> options)
    {
        _httpContextAccessor = httpContextAccessor;
        _unitOfWork = unitOfWork;
        _tokenParameter = options.Value.TokenParameter;
        _session = session;
    }

    /// <summary>
    /// </summary>
    /// <param name="context">     </param>
    /// <param name="requirement"> </param>
    /// <returns> </returns>
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        // 校验 颁发和接收对象
        if (!context.User.HasClaim(c => c.Type == ClaimTypes.DateOfBirth &&
                                        c.Issuer == _tokenParameter.Issuer))
        {
            await Task.CompletedTask;
        }

        var dateOfBirth = Convert.ToDateTime(context.User.FindFirst(c => c.Type == ClaimTypes.DateOfBirth &&
                                                                         c.Issuer == _tokenParameter.Issuer)
            ?.Value);

        var accessExpiration = dateOfBirth.AddMinutes(_tokenParameter.AccessExpiration);
        var nowExpiration = DateTime.Now;
        if (accessExpiration < nowExpiration)
        {
            context.Fail();
            await Task.CompletedTask;
            return;
        }

        var id = Guid.Parse(context.User.Claims.FirstOrDefault(x => x.Type.Equals(ClaimTypes.Sid))!.Value);

        var user = await _unitOfWork.Query<User>().Where(x => x.Id == id).FirstOrDefaultAsync();

        if (user is not null)
        {
            _session.User = user;
        }
        else
        {
            context.Fail();
            await Task.CompletedTask;
            return;
        }
        context.Succeed(requirement);
        await Task.CompletedTask;
    }
}

public class TokenParameter : IAuthorizationRequirement
{
    public string Name { get; set; } = null!;
    public string Secret { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;

    public int AccessExpiration { get; set; }
    public int RefreshExpiration { get; set; }
}

public class CustomIdentity : ClaimsIdentity
{
    private string? GetClaimValue(string claimType, string? defaultValue = null)
    {
        var val = Claims?.FirstOrDefault(e => e.Type == claimType)?.Value;
        return string.IsNullOrWhiteSpace(val) ? defaultValue : val;
    }

    private void SetClaimValue(string claimType, string? value, string? defaultValue = null)
    {
        var claim = Claims?.FirstOrDefault(e => e.Type == claimType);
        if (claim != null)
        {
            RemoveClaim(claim);
        }
        AddClaim(new Claim(claimType, value ?? defaultValue ?? string.Empty));
    }

    public Guid? UserId
    {
        get
        {
            var userId = GetClaimValue("UserId");
            Guid.TryParse(userId, out var id);
            return id;
        }
        set => SetClaimValue("UserId", value.ToString());
    }
}