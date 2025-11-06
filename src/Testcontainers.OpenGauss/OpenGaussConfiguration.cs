namespace Testcontainers.OpenGauss;

/// <inheritdoc cref="ContainerConfiguration" />
[PublicAPI]
public sealed class OpenGaussConfiguration : ContainerConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OpenGaussConfiguration" /> class.
    /// </summary>
    /// <param name="database">The OpenGauss database.</param>
    /// <param name="username">The OpenGauss username.</param>
    /// <param name="password">The OpenGauss password.</param>
    public OpenGaussConfiguration(
        string database = null,
        string username = null,
        string password = null)
    {
        Database = database;
        Username = username;
        Password = password;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenGaussConfiguration" /> class.
    /// </summary>
    /// <param name="resourceConfiguration">The Docker resource configuration.</param>
    public OpenGaussConfiguration(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
        : base(resourceConfiguration)
    {
        // Passes the configuration upwards to the base implementations to create an updated immutable copy.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenGaussConfiguration" /> class.
    /// </summary>
    /// <param name="resourceConfiguration">The Docker resource configuration.</param>
    public OpenGaussConfiguration(IContainerConfiguration resourceConfiguration)
        : base(resourceConfiguration)
    {
        // Passes the configuration upwards to the base implementations to create an updated immutable copy.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenGaussConfiguration" /> class.
    /// </summary>
    /// <param name="resourceConfiguration">The Docker resource configuration.</param>
    public OpenGaussConfiguration(OpenGaussConfiguration resourceConfiguration)
        : this(new OpenGaussConfiguration(), resourceConfiguration)
    {
        // Passes the configuration upwards to the base implementations to create an updated immutable copy.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenGaussConfiguration" /> class.
    /// </summary>
    /// <param name="oldValue">The old Docker resource configuration.</param>
    /// <param name="newValue">The new Docker resource configuration.</param>
    public OpenGaussConfiguration(OpenGaussConfiguration oldValue, OpenGaussConfiguration newValue)
        : base(oldValue, newValue)
    {
        Database = BuildConfiguration.Combine(oldValue.Database, newValue.Database);
        Username = BuildConfiguration.Combine(oldValue.Username, newValue.Username);
        Password = BuildConfiguration.Combine(oldValue.Password, newValue.Password);
    }

    /// <summary>
    /// Gets the OpenGauss database.
    /// </summary>
    public string Database { get; }

    /// <summary>
    /// Gets the OpenGauss username.
    /// </summary>
    public string Username { get; }

    /// <summary>
    /// Gets the OpenGauss password.
    /// </summary>
    public string Password { get; }
}
