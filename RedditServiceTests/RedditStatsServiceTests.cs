using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RedditDemoAPI;
using RedditDemoAPI.Core;

namespace RedditServiceTests
{
    public class RedditStatsServiceTests
    {
        private readonly ILogger<RedditStatsService> _logger = Substitute.For<ILogger<RedditStatsService>>();

        [Fact]
        public void GetTopUserStatsByPercentage_WhenPercentageIsLowerThanThreshold_ThrowException()
        {
            // Arrange
            var redditStatsService = new RedditStatsService(_logger);

            // Act & Assert
            Action act = () => redditStatsService.GetTopUserStatsByPercentage(0);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GetTopUserStatsByPercentage_WhenPercentageIsHigherThanThreshold_ThrowException()
        {
            // Arrange
            var redditStatsService = new RedditStatsService(_logger);

            // Act & Assert
            Action act = () => redditStatsService.GetTopUserStatsByPercentage(101);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GetTopPostStatsByPercentage_WhenPercentageIsLowerThanThreshold_ThrowException()
        {
            // Arrange
            var redditStatsService = new RedditStatsService(_logger);

            // Act & Assert
            Action act = () => redditStatsService.GetTopPostStatsByPercentage(0);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GetTopPostStatsByPercentage_WhenPercentageIsHigherThanThreshold_ThrowException()
        {
            // Arrange
            var redditStatsService = new RedditStatsService(_logger);

            // Act & Assert
            Action act = () => redditStatsService.GetTopPostStatsByPercentage(101);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ReportUsersStats_WhenOneUserPostAdded_ReturnsAccuratePostCount()
        {
            // Arrange
            var redditStatsService = new RedditStatsService(_logger);
            var userStats = new UserPostReport { Author = "TestUser", Title = "TestTitle", NbPosts = 1, AddedTimestamp = DateTime.UtcNow };

            // Act
            redditStatsService.ReportUsersStats(userStats);

            var userStatsList = redditStatsService.GetTopUserStatsByPercentage(100);

            // Assert
            userStatsList.Count(s => s.Author == "TestUser").Should().Be(1);
            userStatsList.First(s => s.Author == "TestUser").NbPosts.Should().Be(1);
        }


        [Fact]
        public void ReportUsersStats_WhenMultipleUserPostAdded_ReturnsAccuratePostCount()
        {
            // Arrange
            var redditStatsService = new RedditStatsService(_logger);

            // Act
            Enumerable.Range(1, 100).Select(r => new UserPostReport { Author = "TestUser", Title = Guid.NewGuid().ToString(), NbPosts = 1, AddedTimestamp = DateTime.UtcNow }).ToList()
                .ForEach(redditStatsService.ReportUsersStats);
            Enumerable.Range(1, 20).Select(r => new UserPostReport { Author = "TestUser", Title = Guid.NewGuid().ToString(), NbPosts = -1, AddedTimestamp = DateTime.UtcNow }).ToList()
                .ForEach(redditStatsService.ReportUsersStats);

            var userStatsList = redditStatsService.GetTopUserStatsByPercentage(100);

            // Assert
            userStatsList.Count(s => s.Author == "TestUser").Should().Be(1);
            userStatsList.First(s => s.Author == "TestUser").NbPosts.Should().Be(80);
        }

        [Fact]
        public void ReportUsersStats_WhenMultipleUserPostAddedInParallel_ReturnsAccuratePostCount()
        {
            // Arrange
            var redditStatsService = new RedditStatsService(_logger);

            // Act
            Enumerable.Range(1, 20).Select(r => new UserPostReport { Author = "TestUser", Title = Guid.NewGuid().ToString(), NbPosts = 1, AddedTimestamp = DateTime.UtcNow })
            .Union(Enumerable.Range(1, 10).Select(r => new UserPostReport { Author = "TestUser", Title = Guid.NewGuid().ToString(), NbPosts = -1, AddedTimestamp = DateTime.UtcNow }))
            .Union(Enumerable.Range(1, 20).Select(r => new UserPostReport { Author = "TestUser", Title = Guid.NewGuid().ToString(), NbPosts = 1, AddedTimestamp = DateTime.UtcNow }))
            .AsParallel()
            .ForAll(redditStatsService.ReportUsersStats);

            var userStatsList = redditStatsService.GetTopUserStatsByPercentage(100);

            // Assert
            userStatsList.Count(s => s.Author == "TestUser").Should().Be(1);
            userStatsList.First(s => s.Author == "TestUser").NbPosts.Should().Be(30);
        }

        [Fact]
        public void ReportPostsStats_WhenOneUserPostAdded_ReturnsAccuratePostCount()
        {
            // Arrange
            var redditStatsService = new RedditStatsService(_logger);
            var postStats = new PostStats { PostId = "TestId", PostTitle = "Test Title", UpVotes = 88, DownVotes = 0, AddedTimestamp = DateTime.UtcNow };

            // Act
            redditStatsService.ReportPostsStats(postStats);

            var userStatsList = redditStatsService.GetTopPostStatsByPercentage(100);

            // Assert
            userStatsList.Count(s => s.PostId == "TestId").Should().Be(1);
            userStatsList.First(s => s.PostId == "TestId").Score.Should().Be(88);
        }

        [Fact]
        public void ReportPostsStats_WhenMultipleUserPostAdded_ReturnsAccuratePostCount()
        {
            // Arrange
            var redditStatsService = new RedditStatsService(_logger);

            // Act
            Enumerable.Range(1, 100).Select(r => new PostStats { PostId = "TestId", PostTitle = "Test Title", UpVotes = 2, AddedTimestamp = DateTime.UtcNow }).ToList()
                .ForEach(redditStatsService.ReportPostsStats);
            Enumerable.Range(1, 20).Select(r => new PostStats { PostId = "TestId", PostTitle = "Test Title", DownVotes = 1, AddedTimestamp = DateTime.UtcNow }).ToList()
                .ForEach(redditStatsService.ReportPostsStats);

            var userStatsList = redditStatsService.GetTopPostStatsByPercentage(100);

            // Assert
            userStatsList.Count(s => s.PostId == "TestId").Should().Be(1);
            userStatsList.First(s => s.PostId == "TestId").Score.Should().Be(180);
        }

        [Fact]
        public void ReportPostsStats_WhenMultipleUserPostAddedInParallel_ReturnsAccuratePostCount()
        {
            // Arrange
            var redditStatsService = new RedditStatsService(_logger);

            // Act
            Enumerable.Range(1, 50).Select(r => new PostStats { PostId = "TestId", PostTitle = "Test Title",UpVotes = 2, AddedTimestamp = DateTime.UtcNow })
            .Union(Enumerable.Range(1, 20).Select(r => new PostStats { PostId = "TestId", PostTitle = "Test Title", DownVotes = 1, AddedTimestamp = DateTime.UtcNow }))
            .Union(Enumerable.Range(1, 50).Select(r => new PostStats { PostId = "TestId", PostTitle = "Test Title",UpVotes = 2, AddedTimestamp = DateTime.UtcNow }))
            .AsParallel()
            .ForAll(redditStatsService.ReportPostsStats);

            var userStatsList = redditStatsService.GetTopPostStatsByPercentage(100);

            // Assert
            userStatsList.Count(s => s.PostId == "TestId").Should().Be(1);
            userStatsList.First(s => s.PostId == "TestId").Score.Should().Be(180);
        }

        [Fact]
        public void ReportUsersStats_WhenPostIsRemoved_ThenIsNoLongerRetrieved()
        {
            // Arrange
            var redditStatsService = new RedditStatsService(_logger);
            var postStats = new PostStats { PostId = "TestId", PostTitle = "Test Title",UpVotes = 88, AddedTimestamp = DateTime.UtcNow };

            // Act
            redditStatsService.ReportPostsStats(postStats);
            redditStatsService.RemovePost(postStats);

            var userStatsList = redditStatsService.GetTopPostStatsByPercentage(100);

            // Assert
            userStatsList.Count(s => s.PostId == "TestId").Should().Be(0);
        }

    }
}