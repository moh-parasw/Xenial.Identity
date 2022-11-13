using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using static Bullseye.Targets;
using static SimpleExec.Command;

var sln = "Xenial.Identity.sln";
var projectName = "Xenial.Identity";
var web = $"src/{projectName}/{projectName}.csproj";
var artifactsLocation = Path.GetFullPath($"./artifacts");
var artifact = Path.GetFullPath($"{artifactsLocation}/{projectName}.zip");
var configuration = "Release";
var iisPackageName = Environment.GetEnvironmentVariable("WEBDEPLOY_SITENAME") ?? "identity.xenial.io";
var selfContained = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBDEPLOY_SELFHOST")) ? false : true;
var skipExtraFilesOnServer = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBDEPLOY_REMOVEFILESONSERVER")) ? false : true;
var nugetApiKey = Environment.GetEnvironmentVariable("NUGET_API_KEY");

var packageAsSingleFile = false;
var nugetClientProject = "./src/Xenial.Identity.Client/Xenial.Identity.Client.csproj";

var version = new Lazy<Task<string>>(async () => (await ReadToolAsync(() => ReadTrimmedAsync("dotnet", "minver -v e"))).Trim());
var branch = new Lazy<Task<string>>(async () => await ReadTrimmedAsync("git", "rev-parse --abbrev-ref HEAD"));
var lastUpdate = new Lazy<Task<string>>(async () => $"{UnixTimeStampToDateTime(await ReadTrimmedAsync("git", "log -1 --format=%ct")):yyyy-MM-dd}");
var hash = new Lazy<Task<string>>(async () => await ReadTrimmedAsync("git", "rev-parse HEAD"));

Func<Task<string>> assemblyProperties = async () => $"/property:LastUpdate={await lastUpdate.Value} /property:GitBranch={await branch.Value} /property:GitHash={await hash.Value}";

Target("restore:yarn", () => RunAsync("yarn", $"install"));
Target("restore:dotnet", () => RunAsync("dotnet", $"restore {sln}"));
Target("restore", DependsOn("restore:yarn", "restore:dotnet"));

Target("build:yarn", () => RunAsync("yarn", $"build"));
Target("build:dotnet", DependsOn("restore:dotnet"), async () => await RunAsync("dotnet", $"build {sln} --no-restore {await assemblyProperties()} -c {configuration} "));
Target("build", DependsOn("restore", "build:yarn", "build:dotnet"));

Target("test", DependsOn("build", "test:storage", "test:identity", "test:dotnet"));
Target("test:storage", () => RunAsync("dotnet", $"run --project src/Xenial.AspNetIdentity.Xpo.Tests/Xenial.AspNetIdentity.Xpo.Tests.csproj -c {configuration} "));
Target("test:identity", () => RunAsync("dotnet", $"run --project src/Xenial.Identity.Xpo.Storage.Tests/Xenial.Identity.Xpo.Storage.Tests.csproj -c {configuration} "));
Target("test:dotnet", () => RunAsync("dotnet", $"test {sln} --no-build --no-restore --logger:\"console;verbosity=normal\" -c {configuration} -- xunit.parallelizeAssembly=true"));

var connectionString = Environment.GetEnvironmentVariable("XENIAL_DEFAULTCONNECTIONSTRING");

Target("publish", DependsOn("publish:dotnet", "publish:nuget"));
Target("publish:dotnet", DependsOn("test"), async () => await RunAsync("dotnet", $"msbuild {web} /t:Restore;Build /p:Configuration={configuration} /p:RuntimeIdentifier=win-x64 /p:SelfContained={selfContained} /p:PackageAsSingleFile={packageAsSingleFile} /p:DeployOnBuild=true /p:WebPublishMethod=package /p:PublishProfile=Package /v:minimal /p:DesktopBuildPackageLocation={artifact} /p:DeployIisAppPath={iisPackageName} /p:DefaultConnectionString=\"{connectionString}\" /p:SkipExtraFilesOnServer={skipExtraFilesOnServer} {await assemblyProperties()}"));
Target("publish:nuget", DependsOn("test"), () => RunAsync("dotnet", $"pack {nugetClientProject} --no-build --no-restore -c {configuration}"));

Target("deploy", DependsOn("publish"), () => RunAsync("cmd.exe", $"/C {projectName}.deploy.cmd /Y /M:{Environment.GetEnvironmentVariable("WEBDEPLOY_IP")} /U:{Environment.GetEnvironmentVariable("WEBDEPLOY_USER")} /P:{Environment.GetEnvironmentVariable("WEBDEPLOY_PASS")} -allowUntrusted -enableRule:AppOffline {(skipExtraFilesOnServer ? "-enableRule:DoNotDeleteRule" : "")}", workingDirectory: artifactsLocation));

Target("deploy:nuget", DependsOn("publish:dotnet"), async () =>
{
    var files = Directory.EnumerateFiles("artifacts/nuget", "*.nupkg")
        .Concat(Directory.EnumerateFiles("artifacts/nuget", "*.snupkg"));

    foreach (var file in files)
    {
        await RunAsync("dotnet", $"nuget push {file} --skip-duplicate -s https://api.nuget.org/v3/index.json -k {nugetApiKey}",
            noEcho: true
        );
    }
});

Target("default", DependsOn("publish"));

await RunTargetsAndExitAsync(args);

static async Task<string> ReadTrimmedAsync(string command, string arguments)
{
    var (output, _) = await ReadAsync(command, arguments);
    return output.Trim();
}

static async Task<string> ReadToolAsync(Func<Task<string>> action)
{
    try
    {
        return await action();
    }
    catch (SimpleExec.ExitCodeReadException)
    {
        Console.WriteLine("Tool seams missing. Try to restore");
        await RunAsync("dotnet", "tool restore");
        return await action();
    }
}

static DateTime UnixTimeStampToDateTime(string unixTimeStamp)
{
    var time = double.Parse(unixTimeStamp.Trim());
    var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
    dtDateTime = dtDateTime.AddSeconds(time).ToLocalTime();
    return dtDateTime;
}
