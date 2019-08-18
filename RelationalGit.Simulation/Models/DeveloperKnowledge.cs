using System.Collections.Generic;

namespace RelationalGit.Simulation
{
    public class DeveloperKnowledge
    {
        private HashSet<string> _committedFile = new HashSet<string>();

        private HashSet<string> _reviewedFile = new HashSet<string>();

        private HashSet<string> _touchedFiles = new HashSet<string>();

        public int NumberOfTouchedFiles => _touchedFiles.Count;

        public int NumberOfReviewedFiles => _reviewedFile.Count;

        public int NumberOfCommittedFiles => _committedFile.Count;

        public int NumberOfCommits { get; set; }

        public int NumberOfReviews { get; set; }

        public int NumberOfContributions => NumberOfCommits + NumberOfReviews;

        public string DeveloperName { get; set; }

        public int NumberOfAuthoredLines { get; set; }

        /// <summary>
        /// Use this field when you want assign score to developers
        /// </summary>
        public double Score { get; set; }

        public bool IsFolderLevel { get; set; }

        public IEnumerable<string> GetTouchedFiles()
        {
            return _touchedFiles;
        }

        public void AddCommittedFile(string fileName)
        {
            _committedFile.Add(fileName);
            _touchedFiles.Add(fileName);
        }

        public void AddReviewedFile(string fileName)
        {
            _reviewedFile.Add(fileName);
            _touchedFiles.Add(fileName);
        }
    }
}
