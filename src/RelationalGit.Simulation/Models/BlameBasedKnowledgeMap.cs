using System.Collections.Generic;
using RelationalGit.Data;

namespace RelationalGit.Simulation
{
    public class BlameBasedKnowledgeMap
    {
        private readonly Dictionary<long, BlameSnapshot> map = new Dictionary<long, BlameSnapshot>();

        public void Add(Period period, string filePath, string developerName, CommitBlobBlame commitBlobBlame)
        {
            var periodId = period.Id;

            if (!map.ContainsKey(periodId))
            {
                map[periodId] = new BlameSnapshot();
            }

            map[periodId].Add(period, filePath, developerName, commitBlobBlame);
        }

        public void ComputeOwnershipPercentage()
        {
            foreach (var periodId in map.Keys)
            {
                map[periodId].ComputeOwnershipPercentage();
            }
        }

        public BlameSnapshot GetSnapshopOfPeriod(long periodId)
        {
            return map.GetValueOrDefault(periodId);
        }
    }
}
