namespace serg046.dev.Client
{
	public class ContributionsCollection
	{
		public PullRequestContribution? PullRequestContributions { get; set; }

		public class PullRequestContribution
		{
			public Node[]? Nodes { get; set; }

			public class Node
			{
				public PullRequestModel? PullRequest { get; set; }

				public class PullRequestModel
				{
					public string? Title { get; set; }
					public Uri? Url { get; set; }
					public DateTime? CreatedAt { get; set; }
					public RepositoryModel? Repository { get; set; }

					public class RepositoryModel
					{
						public int? StargazerCount { get; set; }
					}
				}
			}
		}
	}
}
