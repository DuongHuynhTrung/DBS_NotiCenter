using Data.Enum;

namespace Data.Entities
{
    public class Conversation : BaseEntity
    {
        public Guid MessageFrom { get; set; }
        public Guid MessageTo { get; set; }
        public ConversationType Type { get; set; }
    }
}
