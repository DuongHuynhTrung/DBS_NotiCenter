using Data.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities
{
    public class Message : BaseEntity
    {
        public Guid MessageFrom { get; set; }
        public Guid MessageTo { get; set; }
        public string Content { get; set; }
        public MessageType Type { get; set; }
    }
}
