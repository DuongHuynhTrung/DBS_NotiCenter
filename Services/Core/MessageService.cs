using AutoMapper;
using AutoMapper.QueryableExtensions;
using Background;
using Confluent.Kafka;
using Data.Common.PaginationModel;
using Data.DataAccess;
using Data.Entities;
using Data.Enum;
using Data.Models;
using Data.Utils.Paging;
using MongoDB.Driver;
using Services.kafka;
using Services.SignalR;
using Services.Utils;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Services.Core
{
    public interface IMessageService
    {
        Task<ResultModel> NewMessage(MessageCreateModel model, Guid messageFrom);
        Task<ResultModel> NewMessageCall(MessageCallCreateModel model);
        Task<ResultModel> NewImgMessage(MessageImgCreateModel model, Guid messageFrom);
        Task<ResultModel> GetMessage(PagingParam<SortMessageCriteria> paginationModel, Guid messageTo, Guid messageFrom);
        Task<ResultModel> CallNotification(CallMessageModel model, Guid messageFrom);
        Task<ResultModel> InConversation(Guid messageFrom, Guid messageTo);
        Task<ResultModel> OutConversation(Guid messageFrom, Guid messageTo);
    }

    public class MessageService : IMessageService
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly INotificationHub _notificationHub;
        private readonly IFireBaseNotificationService _fireBaseNotificationService;
        private readonly IBackgroundTaskQueue _backgroundQueue;

        public MessageService(AppDbContext dbContext, IMapper mapper, INotificationHub notificationHub, IFireBaseNotificationService fireBaseNotificationService, IBackgroundTaskQueue backgroundTaskQueue)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _notificationHub = notificationHub;
            _fireBaseNotificationService = fireBaseNotificationService;
            _backgroundQueue = backgroundTaskQueue;
        }

        public async Task<ResultModel> GetMessage(PagingParam<SortMessageCriteria> paginationModel, Guid messageTo, Guid messageFrom)
        {
            var result = new ResultModel();
            result.Succeed = true;
            try
            {
                var data = _dbContext.Messages.Find(_ => _.MessageFrom == messageFrom && _.MessageTo == messageTo
                 || _.MessageFrom == messageTo && _.MessageTo == messageFrom)
                    .SortByDescending(_ => _.DateCreated);
                var paging = new PagingModel(paginationModel.PageIndex, paginationModel.PageSize, data.CountDocuments());
                var sortedData = data.GetWithSorting(paginationModel.SortKey.ToString(), paginationModel.SortOrder);
                var pagedData = sortedData.GetWithPaging(paginationModel.PageIndex, paginationModel.PageSize);
                var viewModels = pagedData.ToEnumerable().AsQueryable().ProjectTo<MessageModel>(_mapper.ConfigurationProvider);

                paging.Data = viewModels;
                result.Data = paging;
                result.Succeed = true;

            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
            return result;
        }

        public async Task<ResultModel> NewMessage(MessageCreateModel model, Guid messageFrom)
        {
            var result = new ResultModel();
            result.Succeed = true;
            try
            {
                var newMessage = _mapper.Map<Message>(model);
                newMessage.MessageFrom = messageFrom;

                await _dbContext.Messages.InsertOneAsync(newMessage);
                result.Data = newMessage.Id;
                result.Succeed = true;

                await _notificationHub.NewMessage(newMessage, messageFrom.ToString(), model.MessageTo.ToString());

                _backgroundQueue.QueueBackgroundWorkItem(async token =>
                {
                    await CheckAndSendNotification(newMessage);
                });

            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
            return result;
        }

        public async Task<ResultModel> NewMessageCall(MessageCallCreateModel model)
        {
            var result = new ResultModel();
            result.Succeed = true;
            try
            {
                var newMessage = _mapper.Map<Message>(model);

                await _dbContext.Messages.InsertOneAsync(newMessage);
                result.Data = newMessage.Id;
                result.Succeed = true;

                await _notificationHub.NewMessage(newMessage, newMessage.MessageFrom.ToString(), newMessage.MessageTo.ToString());

            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
            return result;
        }

        public async Task<ResultModel> NewImgMessage(MessageImgCreateModel model, Guid messageFrom)
        {
            var result = new ResultModel();
            result.Succeed = true;
            try
            {
                var newMessage = _mapper.Map<Message>(model);
                newMessage.MessageFrom = messageFrom;
                newMessage.Type = MessageType.Image;
                newMessage.Content = "...";
                await _dbContext.Messages.InsertOneAsync(newMessage);

                string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "message", newMessage.Id.ToString());
                newMessage.Content = await MyFunction.UploadImageAsync(model.File, dirPath);
                _dbContext.Messages.ReplaceOne(_ => _.Id == newMessage.Id, newMessage);
                result.Data = newMessage.Content;
                result.Succeed = true;
                await _notificationHub.NewMessage(newMessage, messageFrom.ToString(), model.MessageTo.ToString());

                _backgroundQueue.QueueBackgroundWorkItem(async token =>
                {
                    await CheckAndSendNotification(newMessage);
                });
            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
            return result;
        }

        public async Task<ResultModel> CallNotification(CallMessageModel model, Guid messageFrom)
        {
            var result = new ResultModel();
            result.Succeed = true;
            try
            {
                var incomingMessage = new IncomingMessageModel { MessageFrom = messageFrom };

                switch (model.CallType)
                {
                    case Data.Enum.CallType.Voice:
                        incomingMessage.CallType = Data.Enum.CallType.Voice;
                        break;
                    case Data.Enum.CallType.Cancel:
                        incomingMessage.CallType = Data.Enum.CallType.Cancel;
                        break;
                    default:
                        break;
                }
                var notification = new Notification
                {
                    Data = Newtonsoft.Json.JsonConvert.SerializeObject(incomingMessage),
                    UserId = model.MessageTo,
                    TypeModel = "VoiceCall"
                };
                SendNotifyFcm(model.MessageTo, notification);
                result.Succeed = true;
                result.Data = model;
            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
            return result;
        }

        public async Task<ResultModel> InConversation(Guid messageFrom, Guid messageTo)
        {
            var result = new ResultModel();
            result.Succeed = true;
            try
            {
                var conversation = _dbContext.Conversations.Find(_ => _.MessageFrom == messageFrom && _.MessageTo == messageTo).FirstOrDefault();
                if (conversation == null)
                {
                    var conversationCreateModel = new ConversationCreateModel { MessageFrom = messageFrom, MessageTo = messageTo, Type = ConversationType.In };
                    conversation = _mapper.Map<Conversation>(conversationCreateModel);
                    await _dbContext.Conversations.InsertOneAsync(conversation);
                }
                conversation.Type = ConversationType.In;
                conversation.DateUpdated = DateTime.Now;
                _dbContext.Conversations.ReplaceOne(_ => _.Id == conversation.Id, conversation);

                result.Data = conversation.Id;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
            return result;
        }

        public async Task<ResultModel> OutConversation(Guid messageFrom, Guid messageTo)
        {
            var result = new ResultModel();
            result.Succeed = true;
            try
            {
                var conversation = _dbContext.Conversations.Find(_ => _.MessageFrom == messageFrom && _.MessageTo == messageTo).FirstOrDefault();
                if (conversation == null)
                {
                    var conversationCreateModel = new ConversationCreateModel { MessageFrom = messageFrom, MessageTo = messageTo, Type = ConversationType.Out };
                    conversation = _mapper.Map<Conversation>(conversationCreateModel);
                    await _dbContext.Conversations.InsertOneAsync(conversation);
                }
                conversation.Type = ConversationType.Out;
                conversation.DateUpdated = DateTime.Now;
                _dbContext.Conversations.ReplaceOne(_ => _.Id == conversation.Id, conversation);

                result.Data = conversation.Id;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                var message = e.Message + "\n" + (e.InnerException != null ? e.InnerException.Message : "") + "\n ***Trace*** \n" + e.StackTrace;
            }
            return result;
        }

        private async Task CheckAndSendNotification(Message newMessage)
        {
            var conversation = _dbContext.Conversations.Find(_ => _.MessageFrom == newMessage.MessageTo && _.MessageTo == newMessage.MessageFrom).FirstOrDefault();

            if (conversation == null)
            {
                var conversationCreateModel = new ConversationCreateModel { MessageFrom = newMessage.MessageTo, MessageTo = newMessage.MessageFrom, Type = ConversationType.Out };
                conversation = _mapper.Map<Conversation>(conversationCreateModel);
                await _dbContext.Conversations.InsertOneAsync(conversation);
            }

            if (conversation.Type == ConversationType.Out && newMessage.Type == MessageType.Message)
            {
                var userReceive = _dbContext.Users.Find(_ => _.Id == conversation.MessageFrom && !_.IsDeleted).FirstOrDefault();
                var userSent = _dbContext.Users.Find(_ => _.Id == conversation.MessageTo && !_.IsDeleted).FirstOrDefault();
                var data = new Notification
                {
                    Title = userSent.Email,
                    Body = newMessage.Content,
                    Data = Newtonsoft.Json.JsonConvert.SerializeObject(newMessage),
                    UserId = newMessage.MessageTo,
                    TypeModel = "Message"
                };
                var result = new NotificationFcmModel
                {
                    Title = data.Title,
                    Body = data.Body,
                    Data = data,
                    RegistrationIds = userReceive.FcmTokens,
                    UserId = userReceive.Id,
                };
                await _fireBaseNotificationService.SendNotification(result);
            }
            else if (conversation.Type == ConversationType.Out && newMessage.Type == MessageType.Image)
            {
                var userReceive = _dbContext.Users.Find(_ => _.Id == conversation.MessageFrom && !_.IsDeleted).FirstOrDefault();
                var userSent = _dbContext.Users.Find(_ => _.Id == conversation.MessageTo && !_.IsDeleted).FirstOrDefault();
                var data = new Notification
                {
                    Title = userSent.Email,
                    Body = "Đã gửi một ảnh cho bạn",
                    Data = Newtonsoft.Json.JsonConvert.SerializeObject(newMessage),
                    UserId = newMessage.MessageTo,
                    TypeModel = "Message"
                };
                var result = new NotificationFcmModel
                {
                    Title = data.Title,
                    Body = data.Body,
                    Data = data,
                    RegistrationIds = userReceive.FcmTokens,
                    UserId = userReceive.Id,
                };
                await _fireBaseNotificationService.SendNotification(result);
            }
        }
        private async void SendNotifyFcm(Guid userReceiveId, Notification data)
        {
            try
            {
                var userReceive = _dbContext.Users.Find(_ => _.Id == userReceiveId && !_.IsDeleted).FirstOrDefault();
                if (userReceive != null)
                {
                    userReceive.CurrenNoticeCount++;
                    if (userReceive != null && userReceive.FcmTokens.Count > 0)
                    {
                        var result = new NotificationFcmModel
                        {
                            Title = null,
                            Body = null,
                            Data = data,
                            RegistrationIds = userReceive.FcmTokens,
                            UserId = userReceiveId,
                        };
                        await _fireBaseNotificationService.SendNotification(result);
                    }
                }

            }
            catch (Exception ex)
            {

            }

        }
    }
}
