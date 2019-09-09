using AutoMapper;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using RelationalGit.Data;

namespace RelationalGit.Gathering
{
    public class ConfigMapping
    {
        public static void Config()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Octokit.EventInfo, IssueEvent>()
                .ForMember(d => d.Event, opt => opt.MapFrom(s => s.Event.ToString()));

                cfg.CreateMap<Octokit.PullRequest, PullRequest>()
                .ForMember(d => d.ClosedAtDateTime, opt => opt.MapFrom(s => s.ClosedAt.Value.ToUniversalTime().UtcDateTime))
                .ForMember(d => d.MergedAtDateTime, opt => opt.MapFrom(s => s.MergedAt.HasValue ? s.MergedAt.Value.ToUniversalTime().UtcDateTime : default(DateTime?)));

                cfg.CreateMap<Octokit.IssueComment, IssueComment>()
                .ForMember(d => d.CreatedAtDateTime, opt => opt.MapFrom(s => s.CreatedAt.ToUniversalTime().UtcDateTime))
                .ForMember(d => d.IssueNumber,
                opt =>
                opt.MapFrom(s => int.Parse(new Regex(@".*/(?<issue_number>\d+)#").Match(s.HtmlUrl).Groups["issue_number"].Value)));
               //.ForMember(d => d.AuthorAssociation, opt => opt.MapFrom(s => s.AuthorAssociation.ToString())); 

                cfg.CreateMap<Octokit.Issue, Issue>()
                .ForMember(d => d.ClosedAtDateTime, opt => opt.MapFrom(s => s.ClosedAt.HasValue ? s.ClosedAt.Value.UtcDateTime : default(DateTime?)))
                .ForMember(d => d.State, opt => opt.MapFrom(s => s.State.ToString()))
                .ForMember(d => d.PullRequestNumber, 
                opt => 
                opt.MapFrom(s => s.PullRequest != null ? int.Parse(new Regex(".*\\/(?<pull_request_number>\\d+)")
                                .Match(s.PullRequest.Url)
                                .Groups["pull_request_number"]
                                .Value) : 0))
                .ForMember(d => d.Label, opt => opt.MapFrom(s => string.Join(",", s.Labels.SelectMany(l => l.Name))));

                cfg.CreateMap<Octokit.PullRequestReview, PullRequestReviewer>()
                .ForMember(d => d.State, opt => opt.MapFrom(s => s.State.ToString()));
                //.ForMember(d => d.AuthorAssociation, opt => opt.MapFrom(s => s.AuthorAssociation.ToString()));

                cfg.CreateMap<Octokit.PullRequestReviewComment, PullRequestReviewerComment>()
                .ForMember(d => d.PullRequestNumber, opt =>
                opt.MapFrom(s => s.PullRequestUrl != null ? int.Parse(new Regex(".*\\/(?<pull_request_number>\\d+)")
                                .Match(s.PullRequestUrl)
                                .Groups["pull_request_number"]
                                .Value) : 0));

                cfg.CreateMap<Octokit.PullRequestFile, PullRequestFile>();

                cfg.CreateMap<LibGit2Sharp.Commit, Commit>()
                .ForMember(d => d.GitCommit, opts => opts.MapFrom(s => s))
                .ForMember(d => d.AuthorDateTime, opts => opts.MapFrom(s => s.Author.When.UtcDateTime))
                .ForMember(d => d.CommitterDateTime, opts => opts.MapFrom(s => s.Committer.When.UtcDateTime))
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
