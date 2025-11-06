namespace Testcontainers.OpenGauss;

/// <inheritdoc cref="DockerContainer" />
[PublicAPI]
public sealed class OpenGaussContainer : DockerContainer, IDatabaseContainer
{
    private const string GaussSetupEnvironment = "export GAUSSHOME=/usr/local/opengauss && export PATH=$GAUSSHOME/bin:$PATH && export LD_LIBRARY_PATH=$GAUSSHOME/lib:$LD_LIBRARY_PATH";

    private readonly OpenGaussConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenGaussContainer" /> class.
    /// </summary>
    /// <param name="configuration">The container configuration.</param>
    public OpenGaussContainer(OpenGaussConfiguration configuration)
        : base(configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Gets the OpenGauss connection string.
    /// </summary>
    /// <returns>The OpenGauss connection string.</returns>
    public string GetConnectionString()
    {
        var properties = new Dictionary<string, string>();
        properties.Add("Host", Hostname);
        properties.Add("Port", GetMappedPublicPort(OpenGaussBuilder.OpenGaussPort).ToString());
        properties.Add("Database", _configuration.Database);
        properties.Add("Username", _configuration.Username);
        properties.Add("Password", _configuration.Password);
        return string.Join(";", properties.Select(property => string.Join("=", property.Key, property.Value)));
    }

    /// <summary>
    /// Executes the SQL script in the OpenGauss container.
    /// </summary>
    /// <param name="scriptContent">The content of the SQL script to execute.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Task that completes when the SQL script has been executed.</returns>
    public async Task<ExecResult> ExecScriptAsync(string scriptContent, CancellationToken ct = default)
    {
        var scriptFilePath = string.Join("/", string.Empty, "tmp", Guid.NewGuid().ToString("D"), Path.GetRandomFileName());

        await CopyAsync(Encoding.Default.GetBytes(scriptContent), scriptFilePath, fileMode: Unix.FileMode644, ct: ct)
            .ConfigureAwait(false);

        var command = $"{GaussSetupEnvironment} && gsql -d {_configuration.Database} -f {scriptFilePath}";
        return await ExecAsync(new[] { "/bin/bash", "-c", command }, ct)
            .ConfigureAwait(false);
    }
}
