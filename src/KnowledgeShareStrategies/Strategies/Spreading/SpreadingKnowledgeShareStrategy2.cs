using System;
using System.Collections.Generic;
using System.Linq;

namespace RelationalGit
{
    public class SpreadingKnowledgeShareStrategy2 : KnowledgeShareStrategy
    {
        internal override string[] RecommendReviewers(PullRequestContext pullRequestContext)
        {
            if (pullRequestContext.ActualReviewers.Length <= 1)
                return pullRequestContext.ActualReviewers;

            // we do not take devs who are going to leave the team in this period.
            var availableDevs = pullRequestContext.AvailablePRKnowledgeables()
                .Where(q => pullRequestContext.ActualReviewers.All(r => r != q.DeveloperName))
                .Where(q=> pullRequestContext.Developers[q.DeveloperName].TotalCommits + pullRequestContext.Developers[q.DeveloperName].TotalReviews > 50)
                .ToArray();

            if (availableDevs.Length == 0)
                return pullRequestContext.ActualReviewers;

            var actualReviewers = pullRequestContext.ActualReviewers
                .OrderBy(q => pullRequestContext.Developers[q].TotalCommits + pullRequestContext.Developers[q].TotalReviews)
                .ToArray();
            
            var expetisedReviewer = actualReviewers[actualReviewers.Length-1];
            actualReviewers = actualReviewers.Take(actualReviewers.Length-1).ToArray();

            var simulationResults = new List<PullRequestKnowledgeDistributionFactors>();
            var simulator = new PullRequestReviewSimulator(pullRequestContext);
            actualReviewers = (string[])actualReviewers.Clone();

            simulationResults.Add(simulator.Simulate((string[])actualReviewers.Clone(), null).PullRequestKnowledgeDistributionFactors);

            for (int i = 0; i < actualReviewers.Length; i++)
            {
                var actualReviewer = actualReviewers[i];

                foreach (var candidateReviewer in availableDevs)
                {
                    actualReviewers[i] = candidateReviewer.DeveloperName;
                    simulationResults.Add(simulator.Simulate((string[])actualReviewers.Clone(), candidateReviewer).PullRequestKnowledgeDistributionFactors);
                }

                actualReviewers[i] = actualReviewer;
            }

            simulationResults.Sort();

            var recommendedSet = simulationResults[simulationResults.Count-1];

            return recommendedSet.Reviewers.Concat(new string[] { expetisedReviewer }).ToArray();
        }

        private class PullRequestReviewSimulator
        {
            private PullRequestKnowledgeDistribution _knowledge = new PullRequestKnowledgeDistribution(null,null,null);
            private string[] _pullRequestFiles;
            private PullRequestContext _pullRequestContext;

            public PullRequestReviewSimulator(PullRequestContext pullRequestContext)
            {
                _pullRequestContext = pullRequestContext;
                InitKnowledgeDistribution(pullRequestContext);
            }

            public PullRequestKnowledgeDistribution Simulate(string[] reviewers, DeveloperKnowledge candidateReviewer)
            {
                var simulatedKnowledge = new PullRequestKnowledgeDistribution(reviewers, candidateReviewer,_pullRequestContext);

                foreach (var pullRequestFile in _pullRequestFiles)
                {
                    simulatedKnowledge.Add(pullRequestFile,_knowledge[pullRequestFile],reviewers);
                }

                return simulatedKnowledge;
            }

            private void InitKnowledgeDistribution(PullRequestContext pullRequestContext)
            {
                _pullRequestFiles = pullRequestContext
                    .PullRequestFiles
                    .Select(q => pullRequestContext.CanononicalPathMapper.GetValueOrDefault(q.FileName)).Where(q => q != null).ToArray();

                foreach (var pullRequestFile in _pullRequestFiles)
                {
                    var availableCommitters = pullRequestContext.KnowledgeMap.CommitBasedKnowledgeMap[pullRequestFile]
                        ?.Select(q=>q.Key).Where(q=> pullRequestContext.AvailableDevelopers.Any(d=>d.NormalizedName==q));
                    
                    var availableReviewers = pullRequestContext.KnowledgeMap.ReviewBasedKnowledgeMap[pullRequestFile]
                         ?.Select(q => q.Key).Where(q => pullRequestContext.AvailableDevelopers.Any(d => d.NormalizedName == q));

                    _knowledge.Add(pullRequestFile, (availableCommitters?? new string[0]).Union(availableReviewers??new string[0]).ToArray());
                }

            }
        }

        private class PullRequestKnowledgeDistribution
        {
            private Dictionary<string, string[]> _knowledge = new Dictionary<string, string[]>();
            private PullRequestKnowledgeDistributionFactors _factors;
            public PullRequestKnowledgeDistributionFactors PullRequestKnowledgeDistributionFactors => _factors;
            public DeveloperKnowledge CandidateReviewerKnowledge { get; internal set; }
            public string[] this[string pullRequestFileName] => _knowledge[pullRequestFileName];

            public PullRequestKnowledgeDistribution(string[] reviewers, DeveloperKnowledge candidateReviewerKnowledge, PullRequestContext pullRequestContext)
            {
                _factors = new PullRequestKnowledgeDistributionFactors(reviewers,candidateReviewerKnowledge,pullRequestContext);
            }

            public void Add(string pullRequestFile, string[] knowledgeables)
            {
                _knowledge[pullRequestFile] = knowledgeables;
            }

            private void UpdateDistributionFacors(string pullRequestFile, string[] existingKnowledgeables, string[] reviewers)
            {
                var knowledgeables = _knowledge[pullRequestFile];

                if (knowledgeables.Length <= 3)
                {
                    _factors.FilesAtRisk++;
                }

                _factors.TotalKnowledgeables += knowledgeables.Length;
                _factors.AddedKnowledge += reviewers.Where(r => existingKnowledgeables.All(e => e != r)).Count();
            }

            internal void Add(string pullRequestFile, string[] existingKnowledgeables, string[] reviewers)
            {
                if (_knowledge.ContainsKey(pullRequestFile))
                {
                    // it happens, we need to find a way to address this.
                    //throw new Exception("You are allowed to add a file just once.");
                }

                _knowledge[pullRequestFile] = existingKnowledgeables.Union(reviewers).ToArray();
                UpdateDistributionFacors(pullRequestFile,existingKnowledgeables,reviewers);
            }
        }

        private class PullRequestKnowledgeDistributionFactors : IComparable<PullRequestKnowledgeDistributionFactors>
        {
            
            public PullRequestKnowledgeDistributionFactors(string[] reviewers, DeveloperKnowledge candidateReviewerKnowledge, PullRequestContext pullRequestContext)
            {
                CandidateReviewerKnowledge = candidateReviewerKnowledge;
                Reviewers = reviewers;
                PullRequestContext = pullRequestContext;
            }

            public int FilesAtRisk { get; set; }

            public int AddedKnowledge { get; set; }

            public int TotalKnowledgeables { get; set; }

            public DeveloperKnowledge CandidateReviewerKnowledge;

            public string[] Reviewers { get; private set; }

            private PullRequestContext PullRequestContext { get; }

            public int CompareTo(PullRequestKnowledgeDistributionFactors other)
            {
                if (AddedKnowledge > other.AddedKnowledge)
                    return 1;
                if (AddedKnowledge < other.AddedKnowledge)
                    return -1;
                   
                if (FilesAtRisk > other.FilesAtRisk)
                    return 1;
                if (FilesAtRisk < other.FilesAtRisk)
                    return -1;
                 
                if(CandidateReviewerKnowledge==null || other.CandidateReviewerKnowledge == null)
                {
                    var actual = CandidateReviewerKnowledge == null ? this : other;
                    var simulated = CandidateReviewerKnowledge == null ? other : this;

                    foreach (var actualReviewer in actual.Reviewers)
                    {
                        if (!simulated.Reviewers.Any(q => q == actualReviewer))
                        {
                            var developerKnowledge = PullRequestContext.AvailablePRKnowledgeables().SingleOrDefault(q => q.DeveloperName == actualReviewer);

                            if (developerKnowledge != null && developerKnowledge.NumberOfTouchedFiles >= simulated.CandidateReviewerKnowledge.NumberOfTouchedFiles)
                                return (actual==this)?1:-1;
                        }
                    }
                }
                else
                {
                    if (CandidateReviewerKnowledge.NumberOfTouchedFiles > other.CandidateReviewerKnowledge.NumberOfTouchedFiles)
                        return 1;
                    if (CandidateReviewerKnowledge.NumberOfTouchedFiles < other.CandidateReviewerKnowledge.NumberOfTouchedFiles)
                        return -1;
                }

                if (TotalKnowledgeables > other.TotalKnowledgeables)
                    return 1;
                if (TotalKnowledgeables < other.TotalKnowledgeables)
                    return -1;

                if (Reviewers.All(q => PullRequestContext.ActualReviewers.Any(q1 => q1 == q)))
                    return 1;
                if (other.Reviewers.All(q => PullRequestContext.ActualReviewers.Any(q1 => q1 == q)))
                    return -1;

                return 0;
            }
        }

    }
  
}
