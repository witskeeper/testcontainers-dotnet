namespace Testcontainers.OpenGauss;

/// <inheritdoc cref="ContainerBuilder{TBuilderEntity, TContainerEntity, TConfigurationEntity}" />
[PublicAPI]
public sealed class OpenGaussBuilder : ContainerBuilder<OpenGaussBuilder, OpenGaussContainer, OpenGaussConfiguration>
{
    public const string OpenGaussImage = "opengauss/opengauss:5.0.0";

    public const ushort OpenGaussPort = 5432;

    public const string DefaultDatabase = "postgres";

    public const string DefaultUsername = "gaussdb";

    public const string DefaultPassword = "openGauss@123";

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenGaussBuilder" /> class.
    /// </summary>
    public OpenGaussBuilder()
        : this(new OpenGaussConfiguration())
    {
        DockerResourceConfiguration = Init().DockerResourceConfiguration;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenGaussBuilder" /> class.
    /// </summary>
    /// <param name="resourceConfiguration">The Docker resource configuration.</param>
    private OpenGaussBuilder(OpenGaussConfiguration resourceConfiguration)
        : base(resourceConfiguration)
    {
        DockerResourceConfiguration = resourceConfiguration;
    }

    /// <inheritdoc />
    protected override OpenGaussConfiguration DockerResourceConfiguration { get; }

    /// <summary>
    /// Sets the OpenGauss database.
    /// </summary>
    /// <param name="database">The OpenGauss database.</param>
    /// <returns>A configured instance of <see cref="OpenGaussBuilder" />.</returns>
    public OpenGaussBuilder WithDatabase(string database)
    {
        return Merge(DockerResourceConfiguration, new OpenGaussConfiguration(database: database))
            .WithEnvironment("GS_DB", database);
    }

    /// <summary>
    /// Sets the OpenGauss username.
    /// </summary>
    /// <param name="username">The OpenGauss username.</param>
    /// <returns>A configured instance of <see cref="OpenGaussBuilder" />.</returns>
    public OpenGaussBuilder WithUsername(string username)
    {
        return Merge(DockerResourceConfiguration, new OpenGaussConfiguration(username: username))
            .WithEnvironment("GS_USERNAME", username);
    }

    /// <summary>
    /// Sets the OpenGauss password.
    /// </summary>
    /// <param name="password">The OpenGauss password.</param>
    /// <returns>A configured instance of <see cref="OpenGaussBuilder" />.</returns>
    public OpenGaussBuilder WithPassword(string password)
    {
        return Merge(DockerResourceConfiguration, new OpenGaussConfiguration(password: password))
            .WithEnvironment("GS_PASSWORD", password);
    }

    /// <inheritdoc />
    public override OpenGaussContainer Build()
    {
        Validate();

        // By default, the base builder waits until the container is running. However, for OpenGauss, a more advanced waiting strategy is necessary that requires access to the configured database and username.
        // If the user does not provide a custom waiting strategy, append the default OpenGauss waiting strategy.
        var openGaussBuilder = DockerResourceConfiguration.WaitStrategies.Count() > 1 ? this : WithWaitStrategy(Wait.ForUnixContainer().AddCustomWaitStrategy(new WaitUntil(DockerResourceConfiguration)));
        return new OpenGaussContainer(openGaussBuilder.DockerResourceConfiguration);
    }

    /// <inheritdoc />
    protected override OpenGaussBuilder Init()
    {
        return base.Init()
            .WithImage(OpenGaussImage)
            .WithPortBinding(OpenGaussPort, true)
            .WithDatabase(DefaultDatabase)
            .WithUsername(DefaultUsername)
            .WithPassword(DefaultPassword)
            .WithPrivileged(true);
    }

    /// <inheritdoc />
    protected override void Validate()
    {
        base.Validate();

        _ = Guard.Argument(DockerResourceConfiguration.Password, nameof(DockerResourceConfiguration.Password))
            .NotNull()
            .NotEmpty();
    }

    /// <inheritdoc />
    protected override OpenGaussBuilder Clone(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
    {
        return Merge(DockerResourceConfiguration, new OpenGaussConfiguration(resourceConfiguration));
    }

    /// <inheritdoc />
    protected override OpenGaussBuilder Clone(IContainerConfiguration resourceConfiguration)
    {
        return Merge(DockerResourceConfiguration, new OpenGaussConfiguration(resourceConfiguration));
    }

    /// <inheritdoc />
    protected override OpenGaussBuilder Merge(OpenGaussConfiguration oldValue, OpenGaussConfiguration newValue)
    {
        return new OpenGaussBuilder(new OpenGaussConfiguration(oldValue, newValue));
    }

    /// <inheritdoc cref="IWaitUntil" />
    private sealed class WaitUntil : IWaitUntil
    {
        private readonly IList<string> _command;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaitUntil" /> class.
        /// </summary>
        /// <param name="configuration">The container configuration.</param>
        public WaitUntil(OpenGaussConfiguration configuration)
        {
            // Use gs_ctl to check if the database is ready
            _command = new List<string> { "gs_ctl", "status", "-D", "/var/lib/opengauss/data" };
        }

        /// <summary>
        /// Checks whether the database is ready and accepts connections or not.
        /// </summary>
        /// <remarks>
        /// The wait strategy uses <c>gs_ctl status</c> to check the status of OpenGauss.
        /// </remarks>
        /// <param name="container">The starting container instance.</param>
        /// <returns>Task that completes and returns true when the database is ready and accepts connections, otherwise false.</returns>
        public async Task<bool> UntilAsync(IContainer container)
        {
            var execResult = await container.ExecAsync(_command)
                .ConfigureAwait(false);

            return execResult.Stdout.Contains("server is running");
        }
    }
}
