using System;
using System.Collections.Generic;

namespace RelationalGit
{
    public class LossSimulation
    {
        public long Id { get; set; }

        public string KnowledgeShareStrategyType { get; set; }

        public int MegaPullRequestSize { get; set; }

        public double FileAbandoningThreshold { get; set; }

        public DateTime StartDateTime { get; set; }

        public string LeaversType { get; set; }

        public DateTime EndDateTime { get; set; }

        public double FilesAtRiksOwnershipThreshold { get;  set; }

        public int FilesAtRiksOwnersThreshold { get;  set; }

        public int LeaversOfPeriodExtendedAbsence { get;  set; }

        public string KnowledgeSaveReviewerReplacementType { get; internal set; }

        public int FirstPeriod { get; internal set; }

        public string SelectedReviewersType { get; internal set; }

        public int? MinimumActualReviewersLength { get; set; }

        public string PullRequestReviewerSelectionStrategy { get; set; }

        public bool? AddOnlyToUnsafePullrequests { get; set; }
        
        public int? NumberOfPeriodsForCalculatingProbabilityOfStay { get; set; }

        public string LgtmTerms { get; set; }

        public IEnumerable<string> MegaDevelopers { get; internal set; }
    }
}
