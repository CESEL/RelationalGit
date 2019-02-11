dotnet-rgit --cmd compute-loss --selected-reviewers-type core --simulation-first-period 1 --mega-pr-size 100 --save-strategy nothing
dotnet-rgit --cmd compute-loss --selected-reviewers-type core --simulation-first-period 1 --mega-pr-size 100 --save-strategy reviewers-actual
dotnet-rgit --cmd compute-loss --selected-reviewers-type core --simulation-first-period 5 --mega-pr-size 100 --replacement-strategy all-of-actuals --save-strategy reviewers-random
dotnet-rgit --cmd compute-loss --selected-reviewers-type core --simulation-first-period 5 --mega-pr-size 100 --replacement-strategy all-of-actuals --save-strategy reviewers-expertise-commit
dotnet-rgit --cmd compute-loss --selected-reviewers-type core --simulation-first-period 5 --mega-pr-size 100 --replacement-strategy all-of-actuals --save-strategy reviewers-expertise-review
dotnet-rgit --cmd compute-loss --selected-reviewers-type core --simulation-first-period 5 --mega-pr-size 100 --replacement-strategy all-of-actuals --save-strategy most-touched-files
dotnet-rgit --cmd compute-loss --selected-reviewers-type core --simulation-first-period 5 --mega-pr-size 100 --replacement-strategy all-of-actuals --save-strategy least-touched-files
dotnet-rgit --cmd compute-loss --selected-reviewers-type core --simulation-first-period 1 --mega-pr-size 100 --replacement-strategy one-of-actuals --save-strategy least-touched-files
dotnet-rgit --cmd compute-loss --selected-reviewers-type core --simulation-first-period 1 --mega-pr-size 100 --replacement-strategy one-of-actuals --save-strategy reviewers-expertise-review
dotnet-rgit --cmd compute-loss --selected-reviewers-type core --simulation-first-period 1 --mega-pr-size 100 --save-strategy file-level-spreading-knowledge
dotnet-rgit --cmd compute-loss --selected-reviewers-type core --simulation-first-period 1 --mega-pr-size 100 --save-strategy folder-level-spreading-knowledge
dotnet-rgit --cmd compute-loss --selected-reviewers-type core --simulation-first-period 5 --mega-pr-size 100 --replacement-strategy all-of-actuals --save-strategy bird