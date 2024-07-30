using System;
using Data.Enum;

namespace Data.Models
{
    public class CallTokenModel
    {
        public string ChannelName { get; set; }
    }

    public class CallMessageModel
    {
        public Guid MessageTo { get; set; }
        public CallType CallType { get; set; }
    }

    public class IncomingMessageModel
    {
        public Guid MessageFrom { get; set; }
        public CallType CallType { get; set; }
    }
}

