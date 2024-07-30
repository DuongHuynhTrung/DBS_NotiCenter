using Data.Common.PaginationModel;
using Data.Enum;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.ClaimExtensions;
using Services.Core;
using Services.kafka;

namespace NotificationCenter.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class MessageController : ControllerBase
{
    private readonly IMessageService _messageService;
    private readonly IAgoraService _agoraService;

    public MessageController(IMessageService messageService, IAgoraService agoraService)
    {
        _messageService = messageService;
        _agoraService = agoraService;
    }

    [HttpPost]
    public async Task<ActionResult> NewMessage([FromBody] MessageCreateModel model)
    {
        var rs = await _messageService.NewMessage(model, Guid.Parse(User.GetId()));
        if (rs.Succeed) return Ok(rs.Data);
        return BadRequest(rs.ErrorMessage);
    }

    [HttpPost("NewMessageCall")]
    public async Task<ActionResult> NewMessageCall([FromBody] MessageCallCreateModel model)
    {
        var rs = await _messageService.NewMessageCall(model);
        if (rs.Succeed) return Ok(rs.Data);
        return BadRequest(rs.ErrorMessage);
    }

    [HttpPost("SendImage")]
    public async Task<ActionResult> NewImgMessage([FromForm] MessageImgCreateModel model)
    {
        var rs = await _messageService.NewImgMessage(model, Guid.Parse(User.GetId()));
        if (rs.Succeed) return Ok(rs.Data);
        return BadRequest(rs.ErrorMessage);
    }

    [HttpGet("{messageTo}")]
    public async Task<ActionResult> GetMessage([FromQuery] PagingParam<SortMessageCriteria> paginationModel, Guid messageTo)
    {
        var rs = await _messageService.GetMessage(paginationModel, messageTo, Guid.Parse(User.GetId()));
        if (rs.Succeed) return Ok(rs.Data);
        return BadRequest(rs.ErrorMessage);
    }

    [HttpGet("RtcToken")]
    public async Task<ActionResult> GetRtcToken([FromQuery] CallTokenModel model)
    {
        var rs = await _agoraService.GetRtcToken(model);
        if (rs.Succeed) return Ok(rs.Data);
        return BadRequest(rs.ErrorMessage);
    }

    [HttpPost("CallNotification")]
    public async Task<ActionResult> CallNotification([FromBody] CallMessageModel model)
    {
        var rs = await _messageService.CallNotification(model, Guid.Parse(User.GetId()));
        if (rs.Succeed) return Ok(rs.Data);
        return BadRequest(rs.ErrorMessage);
    }

    [HttpPost("InConversation")]
    public async Task<ActionResult> InConversation([FromBody] ConversationInOutModel model)
    {
        var rs = await _messageService.InConversation(Guid.Parse(User.GetId()), model.MessageTo);
        if (rs.Succeed) return Ok(rs.Data);
        return BadRequest(rs.ErrorMessage);
    }

    [HttpPost("OutConversation")]
    public async Task<ActionResult> OutConversation([FromBody] ConversationInOutModel model)
    {
        var rs = await _messageService.OutConversation(Guid.Parse(User.GetId()), model.MessageTo);
        if (rs.Succeed) return Ok(rs.Data);
        return BadRequest(rs.ErrorMessage);
    }

}
