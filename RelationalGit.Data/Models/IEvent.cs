using System;
using System.Collections.Generic;
using System.Text;

namespace RelationalGit.Data
{
    public interface IEvent
    {
        DateTime? OccurrenceDateTime { get; }

        string EventId { get; }
    }
}
