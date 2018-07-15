﻿using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RelationalGit.Mapping
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
                .ForMember(d => d.ClosedAtDateTime, opt => opt.MapFrom(s => s.ClosedAt.Value.DateTime))
                .ForMember(d => d.MergedAtDateTime, opt => opt.MapFrom(s => s.MergedAt.HasValue? s.MergedAt.Value.DateTime:default(DateTime?)));

                cfg.CreateMap<Octokit.Issue, Issue>()
                .ForMember(d => d.ClosedAtDateTime, opt => opt.MapFrom(s => s.ClosedAt.HasValue ? s.ClosedAt.Value.DateTime : default(DateTime?)))
                .ForMember(d=>d.State,opt=>opt.MapFrom(s=>s.State.ToString()))
                .ForMember(d => d.PullRequestNumber, 
                opt => 
                opt.MapFrom(s => s.PullRequest!=null? int.Parse(new Regex(".*\\/(?<pull_request_number>\\d+)")
                                .Match(s.PullRequest.Url)
                                .Groups["pull_request_number"]
                                .Value):0 ))
                .ForMember(d => d.Label, opt => opt.MapFrom(s => string.Join(",",s.Labels.SelectMany(l=>l.Name))));

                cfg.CreateMap<Octokit.PullRequestReview, PullRequestReviewer>()
                .ForMember(d => d.State, opt => opt.MapFrom(s => s.State.ToString()));

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
