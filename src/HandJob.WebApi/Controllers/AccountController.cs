using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HandJob.Domain.Entities;
using HandJob.Domain.Parameters;
using HandJob.Domain.Responses;
using HandJob.Domain.ViewModels;
using HandJob.WebApi.Auth;
using HandJob.WebApi.Extensions;
using HandJob.WebApi.Options;
using HandJob.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Soda.AutoMapper;

namespace HandJob.WebApi.Controllers;

public class AccountController : ApiControllerBase<Account, VAccount>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly Session _session;
    private PermissionRequirement _tokenParameter;

    public AccountController(IUnitOfWork unitOfWork
        , Session session
        , IOptions<AppSettings> options) : base(unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _session = session;

        _tokenParameter = options.Value.TokenParameter;
    }

    [HttpGet]
    public async Task<IActionResult> GetList(AccountParameters parameters)
    {
        var res = await _unitOfWork.Query<Account>().Map<Account, VAccount>().QueryAsync(parameters);

        return Success(res);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] VLogin request)
    {
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            return Fail("Invalid Request");

        var account = await _unitOfWork.Query<Account>().Where(x => x.Username.Equals(request.Username)).FirstOrDefaultAsync();

        if (account is null)
        {
            return Fail("账号不存在");
        }

        if (account.Password != request.Password)
        {
            return Fail("密码错误");
        }

        _session.User = account.User;

        //生成Token和RefreshToken
        var token = GenUserToken(account.Id, account.Username, account.User?.Id.ToString() ?? "");
        var refreshToken = "123456";

        //await _redisHelper.SetStringAsync(user.Id.ToString(), token);

        //LoginUserInfo.Set(user);

        return Success(new ResponseToken { Token = token, RefreshToken = refreshToken, User = account.User?.MapTo<VUser>() });
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] VRegisterAccount dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        var account = dto.MapTo<Account>();

        _unitOfWork.Db.Add(account);

        var res = await _unitOfWork.CommitAsync();

        if (res) Success("注册成功");

        return Fail("注册失败");
    }

    /// <summary>
    /// 生成Token
    /// </summary>
    /// <param name="id">       </param>
    /// <param name="username"> </param>
    /// <param name="role">     </param>
    /// <returns> </returns>
    private string GenUserToken(Guid id, string username, string role)
    {
        var claims = new[]
        {
                new Claim(ClaimTypes.Sid, id.ToString()),
                new Claim(ClaimTypes.DateOfBirth,DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role),
            };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenParameter.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var jwtToken = new JwtSecurityToken(_tokenParameter.Issuer,
                                            _tokenParameter.Audience,
                                            claims,
                                            expires: DateTime.UtcNow.AddMinutes(_tokenParameter.AccessExpiration),
                                            signingCredentials: credentials);

        var token = new JwtSecurityTokenHandler().WriteToken(jwtToken);

        return token;
    }
}

public class RecordController : ApiControllerBase<Record, VRecord>
{
    private readonly IUnitOfWork _unitOfWork;

    public RecordController(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetList(RecordParameters parameters)
    {
        var res = await _unitOfWork.Query<Record>()
            .WhereIf(parameters.StartTime.HasValue, x => x.CreateTime > parameters.StartTime)
            .WhereIf(parameters.EndTime.HasValue, x => x.CreateTime > parameters.EndTime)
            .Map<Record, VRecordList>().QueryAsync(parameters);

        return Success(res);
    }
}

public class RecordDetailController : ApiControllerBase<RecordDetail, VRecordDetail>
{
    private readonly IUnitOfWork _unitOfWork;

    public RecordDetailController(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetList(RecordDetailParameters parameters)
    {
        var res = await _unitOfWork.Query<RecordDetail>()
            .WhereIf(parameters.RecordId.HasValue, x => x.RecordId == parameters.RecordId)
            .WhereIf(parameters.Type.HasValue, x => x.Type == parameters.Type)
            .WhereIf(parameters.Seconds.HasValue, x => x.Seconds >= parameters.Seconds)
            .Map<RecordDetail, VRecordDetail>().QueryAsync(parameters);

        return Success(res);
    }
}

public class UserController : ApiControllerBase<User, VUser>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly Session _session;

    public UserController(IUnitOfWork unitOfWork, Session session) : base(unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _session = session;
    }

    [HttpGet]
    public async Task<IActionResult> GetList(UserParameters parameters)
    {
        var res = await _unitOfWork.Query<User>().Map<User, VUser>().QueryAsync(parameters);

        return Success(res);
    }

    [HttpGet("current")]
    public IActionResult CurrentUser()
    {
        return Success(_session.User?.MapTo<VUser>());
    }
}
