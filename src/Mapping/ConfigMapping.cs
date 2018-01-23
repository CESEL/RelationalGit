using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelationalGit.Mapping
{
    public class ConfigMapping
    {
        public static void Config()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Octokit.PullRequest, PullRequest>()
                .ForMember(d => d.ClosedAtDateTime, opt => opt.MapFrom(s => s.ClosedAt.Value.DateTime))
                .ForMember(d => d.MergedAtDateTime, opt => opt.MapFrom(s => s.MergedAt.HasValue? s.MergedAt.Value.DateTime:default(DateTime?)));

                cfg.CreateMap<Octokit.PullRequestReview, PullRequestReviewer>()
                .ForMember(d => d.State, opt => opt.MapFrom(s => s.State.ToString()));

                cfg.CreateMap<Octokit.PullRequestReviewComment, PullRequestReviewerComment>();
                cfg.CreateMap<Octokit.PullRequestFile, PullRequestFile>();

                cfg.CreateMap<LibGit2Sharp.Commit, Commit>()
                .ForMember(d => d.GitCommit, opts => opts.MapFrom(s => s))
                .ForMember(d => d.AuthorDateTime, opts => opts.MapFrom(s => s.Author.When.DateTime))
                .ForMember(d => d.CommitterDateTime, opts => opts.MapFrom(s => s.Committer.When.DateTime))
                .ForMember(d => d.CommitRelationship, opts => opts.MapFrom(s => s.Parents.Select(p => new CommitRelationship()
                {
                    Child = s.Sha,
                    Parent = p.Sha
                }).ToArray()))
                .ForMember(d => d.IsMergeCommit, opts => opts.MapFrom(s => s.Parents.Count() > 1)); // test this

            });
        }
    }
}
