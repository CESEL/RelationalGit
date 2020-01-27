$corefx_conf = "corefx_conf.json"
$coreclr_conf = "coreclr_conf.json"
$roslyn_conf = "roslyn_conf.json"
$rust_conf = "rust_conf.json"
$kubernetes_conf = "kubernetes_conf.json"



# CoreFX Simulations
dotnet-rgit --cmd simulate-recommender --recommendation-strategy NoReviews --conf-path $corefx_conf
dotnet-rgit --cmd simulate-recommender --recommendation-strategy Reality --conf-path $corefx_conf
dotnet-rgit --cmd simulate-recommender --recommendation-strategy cHRev --conf-path $corefx_conf
dotnet-rgit --cmd simulate-recommender --recommendation-strategy AuthorshipRec --conf-path $corefx_conf
dotnet-rgit --cmd simulate-recommender --recommendation-strategy RecOwnRec  --conf-path $corefx_conf
dotnet-rgit --cmd simulate-recommender --recommendation-strategy RetentionRec  --conf-path $corefx_conf
dotnet-rgit --cmd simulate-recommender --recommendation-strategy LearnRec  --conf-path $corefx_conf
dotnet-rgit --cmd simulate-recommender --recommendation-strategy TurnoverRec --conf-path $corefx_conf
dotnet-rgit --cmd simulate-recommender --recommendation-strategy Sofia  --conf-path $corefx_conf


# CoreCLR Simulations
dotnet-rgit --cmd simulate-recommender --recommendation-strategy NoReviews --conf-path $coreclr_conf
dotnet-rgit --cmd simulate-recommender --recommendation-strategy Reality --conf-path $coreclr_conf
dotnet-rgit --cmd simulate-recommender --recommendation-strategy cHRev --conf-path $coreclr_conf
dotnet-rgit --cmd simulate-recommender --recommendation-strategy AuthorshipRec --conf-path $coreclr_conf
dotnet-rgit --cmd simulate-recommender --recommendation-strategy RecOwnRec  --conf-path $coreclr_conf
dotnet-rgit --cmd simulate-recommender --recommendation-strategy RetentionRec  --conf-path $coreclr_conf
dotnet-rgit --cmd simulate-recommender --recommendation-strategy LearnRec  --conf-path $coreclr_conf
dotnet-rgit --cmd simulate-recommender --recommendation-strategy TurnoverRec --conf-path $coreclr_conf
dotnet-rgit --cmd simulate-recommender --recommendation-strategy Sofia  --conf-path $coreclr_conf


# Roslyn Simulations
dotnet-rgit --cmd simulate-recommender --recommendation-strategy NoReviews --conf-path $roslyn_conf
dotnet-rgit --cmd simulate-recommender --recommendation-strategy Reality --conf-path $roslyn_conf
dotnet-rgit --cmd simulate-recommender --recommendation-strategy cHRev --conf-path $roslyn_conf
dotnet-rgit --cmd simulate-recommender --recommendation-strategy AuthorshipRec --conf-path $roslyn_conf
dotnet-rgit --cmd simulate-recommender --recommendation-strategy RecOwnRec  --conf-path $roslyn_conf
dotnet-rgit --cmd simulate-recommender --recommendation-strategy RetentionRec  --conf-path $roslyn_conf
dotnet-rgit --cmd simulate-recommender --recommendation-strategy LearnRec  --conf-path $roslyn_conf
dotnet-rgit --cmd simulate-recommender --recommendation-strategy TurnoverRec --conf-path $roslyn_conf
dotnet-rgit --cmd simulate-recommender --recommendation-strategy Sofia  --conf-path $roslyn_conf


# Rust Simulations
dotnet-rgit --cmd simulate-recommender --recommendation-strategy NoReviews --conf-path $rust_conf
dotnet-rgit --cmd simulate-recommender --recommendation-strategy Reality --conf-path $rust_conf
dotnet-rgit --cmd simulate-recommender --recommendation-strategy cHRev --conf-path $rust_conf
dotnet-rgit --cmd simulate-recommender --recommendation-strategy AuthorshipRec --conf-path $rust_conf
dotnet-rgit --cmd simulate-recommender --recommendation-strategy RecOwnRec  --conf-path $rust_conf
dotnet-rgit --cmd simulate-recommender --recommendation-strategy RetentionRec  --conf-path $rust_conf
dotnet-rgit --cmd simulate-recommender --recommendation-strategy LearnRec  --conf-path $rust_conf
dotnet-rgit --cmd simulate-recommender --recommendation-strategy TurnoverRec --conf-path $rust_conf
dotnet-rgit --cmd simulate-recommender --recommendation-strategy Sofia  --conf-path $rust_conf


# Kubernetes Simulations
dotnet-rgit --cmd simulate-recommender --recommendation-strategy NoReviews --conf-path $kubernetes_conf
dotnet-rgit --cmd simulate-recommender --recommendation-strategy Reality --conf-path $kubernetes_conf
dotnet-rgit --cmd simulate-recommender --recommendation-strategy cHRev --conf-path $kubernetes_conf
dotnet-rgit --cmd simulate-recommender --recommendation-strategy AuthorshipRec --conf-path $kubernetes_conf
dotnet-rgit --cmd simulate-recommender --recommendation-strategy RecOwnRec  --conf-path $kubernetes_conf
dotnet-rgit --cmd simulate-recommender --recommendation-strategy RetentionRec  --conf-path $kubernetes_conf
dotnet-rgit --cmd simulate-recommender --recommendation-strategy LearnRec  --conf-path $kubernetes_conf
dotnet-rgit --cmd simulate-recommender --recommendation-strategy TurnoverRec --conf-path $kubernetes_conf
dotnet-rgit --cmd simulate-recommender --recommendation-strategy Sofia  --conf-path $kubernetes_conf
