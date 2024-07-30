using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Enum;

public enum MessageType
{
    Message,
    Image,
    CallSuccess,
    CallMissed,
}

public enum CallType
{
    Voice,
    Cancel
}

public enum ConversationType
{
    In,
    Out,
}

public enum SortMessageCriteria
{
    DateCreated
}

public enum SortNotiCriteria
{
    DateCreated
}
