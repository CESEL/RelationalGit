dotnet-rgit --cmd extract-dev-info
dotnet-rgit --cmd compute-loss --selected-reviewers-type core --simulation-first-period 1 --mega-pr-size 100 --save-strategy nothing
dotnet-rgit --cmd compute-loss --selected-reviewers-type core --simulation-first-period 1 --mega-pr-size 100 --save-strategy reviewers-actual
dotnet-rgit --cmd compute-loss --selected-reviewers-type core --simulation-first-period 1 --mega-pr-size 100 --save-strategy file-level-spreading-knowledge
dotnet-rgit --cmd compute-loss --selected-reviewers-type core --simulation-first-period 5 --mega-pr-size 100 --replacement-strategy all-of-actuals --save-strategy bird
dotnet-rgit --cmd compute-loss --selected-reviewers-type all --simulation-first-period 1 --mega-pr-size 100 --replacement-strategy all-of-actuals --save-strategy file-level-probability-based-spreading-knowledge --pullRequests-reviewer-selection "0:nothing-nothing,1:replace-1,2:replace-1,-:replace-2"
dotnet-rgit --cmd compute-loss --selected-reviewers-type all --simulation-first-period 1 --mega-pr-size 100 --replacement-strategy all-of-actuals --save-strategy file-level-probability-based-spreading-knowledge --pullRequests-reviewer-selection "0:add-1,1:replace-1,2:replace-1,-:replace-1"
dotnet-rgit --cmd compute-loss --selected-reviewers-type all --simulation-first-period 1 --mega-pr-size 100 --replacement-strategy all-of-actuals --save-strategy file-level-probability-based-spreading-knowledge --pullRequests-reviewer-selection "0:add-1,1:add-1,2:replace-1,-:replace-1"
dotnet-rgit --cmd compute-loss --selected-reviewers-type all --simulation-first-period 1 --mega-pr-size 100 --replacement-strategy all-of-actuals --save-strategy folder-level-probability-based-spreading-knowledge --pullRequests-reviewer-selection "0:nothing-nothing,1:replace-1,2:replace-1,-:replace-2"
dotnet-rgit --cmd compute-loss --selected-reviewers-type all --simulation-first-period 1 --mega-pr-size 100 --replacement-strategy all-of-actuals --save-strategy folder-level-probability-based-spreading-knowledge --pullRequests-reviewer-selection "0:add-1,1:replace-1,2:replace-1,-:replace-1"

