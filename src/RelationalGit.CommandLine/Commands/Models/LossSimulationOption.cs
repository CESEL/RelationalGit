using System.Collections.Generic;

namespace RelationalGit
{
    public class LossSimulationOption
    {
        public string KnowledgeShareStrategyType { get; internal set; }

        public string KnowledgeSaveReviewerReplacementType { get; internal set; }

        public int MegaPullRequestSize { get; internal set; }

        public string LeaversType { get; internal set; }

        public double FilesAtRiksOwnershipThreshold { get;  set; }

        public int FilesAtRiksOwnersThreshold { get; internal set; }

        public int LeaversOfPeriodExtendedAbsence { get;  set; }

        public int KnowledgeSaveReviewerFirstPeriod { get; set; }

        public string SelectedReviewersType { get; set; }

        public string[] LgtmTerms { get; set; }

        public int? MinimumActualReviewersLength { get; set; }

        public string PullRequestReviewerSelectionStrategy { get; set; }

        public int? NumberOfPeriodsForCalculatingProbabilityOfStay { get; set; }

        public bool? AddOnlyToUnsafePullrequests { get;  set; }

        public IEnumerable<string> MegaDevelopers { get; set; }

        public string RecommenderOption { get;  set; }

        public bool? ChangePast { get;  set; }
    }
}
