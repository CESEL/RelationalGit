using System.Linq;

namespace RelationalGit
{
    public class SpreadingKnowledgeShareStrategy  : BaseKnowledgeShareStrategy
    {

        public SpreadingKnowledgeShareStrategy(string knowledgeSaveReviewerReplacementType)
            : base(knowledgeSaveReviewerReplacementType)
        {
        }

        protected override DeveloperKnowledge[] SortCandidates(PullRequestContext pullRequestContext, DeveloperKnowledge[] candidates)
        {
            
            if (candidates.Length == 0)
            {
                return pullRequestContext.PRKnowledgeables;
            }
            /*
//var maxTouchedFiles = pullRequestContext.PRKnowledgeables.Max(q=>q.NumberOfTouchedFiles);
var maxTouchedFiles = pullRequestContext.PullRequestFiles.Count();

var touchedFileUpperBound = Math.Ceiling(maxTouchedFiles * 0.5);
var touchedFileLowerBound = Math.Ceiling(maxTouchedFiles * 0.0);

var availableDevs = pullRequestContext.PRKnowledgeables.Where(q=>pullRequestContext.AvailableDevelopers.Any(d=>d.NormalizedName==q.DeveloperName));

var lessKnowledgedDevelopers = availableDevs.Where(q=>q.NumberOfTouchedFiles<= touchedFileUpperBound && q.NumberOfTouchedFiles>= touchedFileLowerBound);

if (lessKnowledgedDevelopers.Count() > 0)
{
   return availableDevs.Where(q => q.NumberOfTouchedFiles > touchedFileUpperBound || q.NumberOfTouchedFiles<touchedFileUpperBound).OrderBy(q=>q.NumberOfTouchedFiles)
       .Concat(lessKnowledgedDevelopers.OrderBy(q => q.NumberOfTouchedFiles).ThenBy(q => q.NumberOfReviews)).ToArray();
}
*/

            return candidates.OrderByDescending(q => q.NumberOfTouchedFiles).ThenByDescending(q => q.NumberOfReviews).ToArray();
        }
    }
}
