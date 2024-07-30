using AutoMapper;
using Data.Entities;
using Data.Models;

namespace Data.Mapping
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<Notification, NotificationModel>().ReverseMap();
            CreateMap<NotificationModel, Notification>().ReverseMap();
            CreateMap<DriverOnline, DriverOnlineModel>().ReverseMap();

            CreateMap<MessageCreateModel, Message>().ReverseMap();
            CreateMap<MessageCallCreateModel, Message>().ReverseMap();
            CreateMap<MessageImgCreateModel, Message>().ReverseMap();
            CreateMap<Message, MessageModel>().ReverseMap();
            CreateMap<MessageModel, Message>().ReverseMap();
            CreateMap<ConversationCreateModel, Conversation>().ReverseMap();
            CreateMap<User, UserModel>().ReverseMap();
            CreateMap<UserFromKafka, User>().ReverseMap();
        }
    }
}

