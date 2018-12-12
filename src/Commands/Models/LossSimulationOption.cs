namespace RelationalGit
{
    public class LossSimulationOption
    {
        public string KnowledgeShareStrategyType { get; internal set; }

        public string KnowledgeSaveReviewerReplacementType { get; internal set; }

        public int MegaPullRequestSize { get; internal set; }

        public string LeaversType { get; internal set; }

        public double FilesAtRiksOwnershipThreshold { get; internal set; }

        public int FilesAtRiksOwnersThreshold { get; internal set; }

        public int LeaversOfPeriodExtendedAbsence { get; internal set; }

        public int KnowledgeSaveReviewerFirstPeriod { get; set; }

        public string SelectedReviewersType { get; set; }
    }
}

