namespace Testcontainers.OpenGauss;

public abstract class OpenGaussContainerTest(OpenGaussContainerTest.OpenGaussDefaultFixture fixture)
{
    // # --8<-- [start:UseOpenGaussContainer]
    [Fact]
    [Trait(nameof(DockerCli.DockerPlatform), nameof(DockerCli.DockerPlatform.Linux))]
    public void ConnectionStateReturnsOpen()
    {
        // Given
        using DbConnection connection = fixture.CreateConnection();

        // When
        connection.Open();

        // Then
        Assert.Equal(ConnectionState.Open, connection.State);
    }

    [Fact]
    [Trait(nameof(DockerCli.DockerPlatform), nameof(DockerCli.DockerPlatform.Linux))]
    public async Task ExecScriptReturnsSuccessful()
    {
        // Given
        const string scriptContent = "SELECT 1;";

        // When
        var execResult = await fixture.Container.ExecScriptAsync(scriptContent, TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        // Then
        Assert.True(0L.Equals(execResult.ExitCode), execResult.Stderr);
        Assert.Empty(execResult.Stderr);
    }
    // # --8<-- [end:UseOpenGaussContainer]

    public sealed class ReuseContainerTest : IClassFixture<OpenGaussDefaultFixture>, IDisposable
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource(TimeSpan.FromMinutes(1));

        private readonly OpenGaussDefaultFixture _fixture;

        public ReuseContainerTest(OpenGaussDefaultFixture fixture)
        {
            _fixture = fixture;
        }

        public void Dispose()
        {
            _cts.Dispose();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public async Task StopsAndStartsContainerSuccessful(int _)
        {
            await _fixture.Container.StopAsync(_cts.Token)
                .ConfigureAwait(true);

            await _fixture.Container.StartAsync(_cts.Token)
                .ConfigureAwait(true);

            Assert.False(_cts.IsCancellationRequested);
        }
    }

    public class OpenGaussDefaultFixture(IMessageSink messageSink)
        : DbContainerFixture<OpenGaussBuilder, OpenGaussContainer>(messageSink)
    {
        public override DbProviderFactory DbProviderFactory
            => NpgsqlFactory.Instance;
    }

    [UsedImplicitly]
    public class OpenGaussWaitForDatabaseFixture(IMessageSink messageSink)
        : OpenGaussDefaultFixture(messageSink)
    {
        protected override OpenGaussBuilder Configure(OpenGaussBuilder builder)
            => builder.WithWaitStrategy(Wait.ForUnixContainer().UntilDatabaseIsAvailable(DbProviderFactory));
    }

    [UsedImplicitly]
    public sealed class OpenGaussDefaultConfiguration(OpenGaussDefaultFixture fixture)
        : OpenGaussContainerTest(fixture), IClassFixture<OpenGaussDefaultFixture>;

    [UsedImplicitly]
    public sealed class OpenGaussWaitForDatabaseConfiguration(OpenGaussWaitForDatabaseFixture fixture)
        : OpenGaussContainerTest(fixture), IClassFixture<OpenGaussWaitForDatabaseFixture>;
}
