namespace Xenial.Identity.Client.Tests;

public sealed class DockerRunningFactAttribute : FactAttribute
{
    public DockerRunningFactAttribute()
    {
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NCRUNCH")))
        {
            Skip = "Don't run docker facts in NCrunch.";
            return;
        }

        if (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS())
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS")))
            {
                var isDockerRunning = false;
                SimpleExec.Command.Run("docker", "info", handleExitCode: exitCode =>
                {
                    isDockerRunning = exitCode == 0;
                    return true;
                });
                if (!isDockerRunning)
                {
                    Skip = "Docker is not running. Start Docker to run";
                }
            }
            else
            {
                Skip = "Docker is not supported on Github Actions for windows or macos.";
            }
        }

    }
}
