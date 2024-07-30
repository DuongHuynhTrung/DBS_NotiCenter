using Data.Entities;
using Data.Enum;
using Microsoft.AspNetCore.Http;


namespace Data.Models
{
    public class MessageCreateModel
    {
        public Guid MessageTo { get; set; }
        public string Content { get; set; }
        public MessageType Type { get; set; }
    }

    public class MessageCallCreateModel
    {
        public Guid MessageFrom { get; set; }
        public Guid MessageTo { get; set; }
        public string Content { get; set; }
        public MessageType Type { get; set; }
    }

    public class MessageModel
    {
        public Guid Id { get; set; }
        public Guid MessageFrom { get; set; }
        public Guid MessageTo { get; set; }
        public string Content { get; set; }
        public MessageType Type { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
    }

    public class MessageImgCreateModel
    {
        public Guid MessageTo { get; set; }
        public IFormFile File { get; set; }
    }

    public class ConversationCreateModel
    {
        public Guid MessageFrom { get; set; }
        public Guid MessageTo { get; set; }
        public ConversationType Type { get; set; }
    }

    public class ConversationInOutModel
    {
        public Guid MessageTo { get; set; }
    }
}

