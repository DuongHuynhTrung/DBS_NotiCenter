using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Core;
using Services.ClaimExtensions;
using Data.Common.PaginationModel;
using Data.Enum;

namespace NotificationCenter;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult> Get([FromQuery] PagingParam<SortMessageCriteria> paginationModel)
    {
        var rs = await _notificationService.Get(paginationModel, Guid.Parse(User.GetId()));
        if (rs.Succeed) return Ok(rs.Data);
        return BadRequest(rs.ErrorMessage);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> Get(Guid id)
    {
        var rs = await _notificationService.GetById(id);
        if (rs.Succeed) return Ok(rs.Data);
        return BadRequest(rs.ErrorMessage);
    }

    [HttpPut("SeenNotify/{id}")]
    public async Task<IActionResult> SeenNotify(Guid id)
    {
        var rs = await _notificationService.SeenNotify(id, Guid.Parse(User.GetId()));
        if (rs.Succeed) return Ok(rs.Data);
        return BadRequest(rs.ErrorMessage);
    }

    [HttpPut("SeenAllNotify")]
    public async Task<IActionResult> SeenAllNotify()
    {
        var rs = await _notificationService.SeenAllNotify(Guid.Parse(User.GetId()));
        if (rs.Succeed) return Ok(rs.Data);
        return BadRequest(rs.ErrorMessage);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNotify(Guid id)
    {
        var rs = await _notificationService.DeleteNotify(id, Guid.Parse(User.GetId()));
        if (rs.Succeed) return Ok(rs.Data);
        return BadRequest(rs.ErrorMessage);
    }
}

