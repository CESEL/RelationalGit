namespace RelationalGit
{
    using System.Collections.Generic;
    using System.Linq;

    public class BirdKnowledgeShareStrategy : BaseKnowledgeShareStrategy
    {
        public BirdKnowledgeShareStrategy(string knowledgeSaveReviewerReplacementType)
            : base(knowledgeSaveReviewerReplacementType)
        {
        }

        protected override DeveloperKnowledge[] SortCandidates(PullRequestContext pullRequestContext, DeveloperKnowledge[] candidates)
        {
            foreach (var candidate in candidates)
            {
                foreach (var pullRequestFile in pullRequestContext.PullRequestFiles)
                {
                    var canonicalPath = pullRequestContext.CanononicalPathMapper.GetValueOrDefault(pullRequestFile.FileName);
                    if (canonicalPath == null)
                    {
                        continue;
                    }

                    var fileExpertise = pullRequestContext.KnowledgeMap.PullRequestEffortKnowledgeMap.GetFileExpertise(canonicalPath);

                    if (fileExpertise.TotalComments == 0)
                    {
                        continue;
                    }

                    var reviewerExpertise = pullRequestContext.KnowledgeMap.PullRequestEffortKnowledgeMap.GetReviewerExpertise(canonicalPath,candidate.DeveloperName);

                    if (reviewerExpertise == (0, 0, null))
                    {
                        continue;
                    }

                    var scoreTotalComments = reviewerExpertise.TotalComments / (double)fileExpertise.TotalComments;
                    var scoreTotalWorkDays = reviewerExpertise.TotalWorkDays / (double)fileExpertise.TotalWorkDays;
                    var scoreRecency = (fileExpertise.RecentWorkDay == reviewerExpertise.RecentWorkDay)
                        ? 1
                        : 1 / (fileExpertise.RecentWorkDay - reviewerExpertise.RecentWorkDay).Value.TotalDays;

                    var score = scoreTotalComments + scoreTotalWorkDays + scoreRecency;
                    candidate.Score += score;
                }
            }


            return candidates.OrderBy(q => q.Score).ToArray();
        }
    }
}