using System;

namespace RelationalGit.Data
{
    public interface IEvent
    {
        DateTime? OccurrenceDateTime { get; }

        string EventId { get; }
    }
}
