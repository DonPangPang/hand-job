using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HandJob.Domain.Base;
using HandJob.Domain.Responses;
using HandJob.Domain.ViewModels;
using HandJob.WebApi.Extensions;
using HandJob.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Soda.AutoMapper;

namespace HandJob.WebApi.Controllers;

[ApiController]
[Route("[Controller]")]
[Authorize]
public class ApiControllerBase : ControllerBase
{
    [NonAction]
    public IActionResult Success(string message, object? data = null)
    {
        return Ok(new ResponseBox
        {
            Code = 200,
            Data = data,
            Message = message
        });
    }

    [NonAction]
    public IActionResult Success(object? data = null)
    {
        return Ok(new ResponseBox
        {
            Code = 200,
            Data = data,
            Message = "获取成功"
        });
    }

    [NonAction]
    public IActionResult Fail(string message, object? data = null)
    {
        return Ok(new ResponseBox
        {
            Code = 500,
            Data = data,
            Message = message
        });
    }

    [NonAction]
    public IActionResult Fail(object? data = null)
    {
        return Ok(new ResponseBox
        {
            Code = 500,
            Data = data,
            Message = "获取失败"
        });
    }
}

public abstract class ApiControllerBase<TEntity, TViewModel> : ApiControllerBase
    where TEntity : class, IEntity
    where TViewModel : class, IViewModel
{
    private readonly IUnitOfWork _unitOfWork;

    public ApiControllerBase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        var res = await _unitOfWork.Query<TEntity>().Map<TEntity, TViewModel>().FirstOrDefaultAsync(x => x.Id == id);

        return Success(res);
    }

    [HttpPost]
    public async Task<IActionResult> AddAsync(TViewModel model)
    {
        if (model.Id != Guid.Empty) return Fail("该数据已存在, 请勿重复添加");

        var entity = model.MapTo<TEntity>();

        _unitOfWork.Db.Add(entity);

        if (await _unitOfWork.CommitAsync()) return Success("保存成功");

        return Fail("保存失败");
    }

    [HttpPut]
    public async Task<IActionResult> UpdateAsync(TViewModel model)
    {
        var old = await _unitOfWork.Query<TEntity>().FirstOrDefaultAsync(x => x.Id == model.Id);
        if (old is null)
        {
            var entity = model.MapTo<TEntity>();

            _unitOfWork.Db.Add(entity);
        }
        else
        {
            var entity = model.MapTo<TEntity>();
            entity.Map(model);

            _unitOfWork.Db.Update(entity);
        }

        if (await _unitOfWork.CommitAsync()) return Success("更新成功");

        return Fail("更新失败");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var entity = await _unitOfWork.Query<TEntity>().FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null) return Fail("数据不存在");

        _unitOfWork.Db.Remove(entity);

        if (await _unitOfWork.CommitAsync()) return Success("删除成功");

        return Fail("删除失败");
    }
}
