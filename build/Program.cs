﻿using System;
using System.IO;
using System.Threading.Tasks;

using static Bullseye.Targets;
using static SimpleExec.Command;

var sln = "Xenial.Identity.sln";
var projectName = "Xenial.Identity";
var web = $"src/{projectName}/{projectName}.csproj";
var iisPackageName = Environment.GetEnvironmentVariable("WEBDEPLOY_SITENAME") ?? "identity.xenial.io";
var artifactsLocation = Path.GetFullPath($"./artifacts");
var artifact = Path.GetFullPath($"{artifactsLocation}/{projectName}.zip");
var configuration = "Release";
var selfContained = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBDEPLOY_SELFHOST")) ? false : true;
var skipExtraFilesOnServer = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBDEPLOY_REMOVEFILESONSERVER")) ? false : true;
var packageAsSingleFile = false;

var version = new Lazy<Task<string>>(async () => (await ReadToolAsync(() => ReadTrimmedAsync("dotnet", "minver -v e"))).Trim());
var branch = new Lazy<Task<string>>(async () => await ReadTrimmedAsync("git", "rev-parse --abbrev-ref HEAD"));
var lastUpdate = new Lazy<Task<string>>(async () => $"{UnixTimeStampToDateTime(await ReadTrimmedAsync("git", "log -1 --format=%ct")):yyyy-MM-dd}");
var hash = new Lazy<Task<string>>(async () => await ReadTrimmedAsync("git", "rev-parse HEAD"));

Func<Task<string>> assemblyProperties = async () => $"/property:LastUpdate={await lastUpdate.Value} /property:GitBranch={await branch.Value} /property:GitHash={await hash.Value}";

Target("restore:yarn", () => RunAsync("yarn", $"install"));
Target("restore:dotnet", () => RunAsync("dotnet", $"restore {sln}"));
Target("restore", DependsOn("restore:yarn", "restore:dotnet"));

Target("build:yarn", () => RunAsync("yarn", $"build"));
Target("build:dotnet", DependsOn("restore:dotnet"), async () => await RunAsync("dotnet", $"build {sln} --no-restore {await assemblyProperties()}"));
Target("build", DependsOn("restore", "build:yarn", "build:dotnet"));


var connectionString = Environment.GetEnvironmentVariable("XENIAL_DEFAULTCONNECTIONSTRING");
Target("publish", DependsOn("build"), async () => await RunAsync("dotnet", $"msbuild {web} /t:Restore;Build /p:Configuration={configuration} /p:RuntimeIdentifier=win-x64 /p:SelfContained={selfContained} /p:PackageAsSingleFile={packageAsSingleFile} /p:DeployOnBuild=true /p:WebPublishMethod=package /p:PublishProfile=Package /v:minimal /p:DesktopBuildPackageLocation={artifact} /p:DeployIisAppPath={iisPackageName} /p:DefaultConnectionString=\"{connectionString}\" /p:SkipExtraFilesOnServer={skipExtraFilesOnServer} {await assemblyProperties()}"));
Target("deploy", DependsOn("publish"), () => RunAsync("cmd.exe", $"/C {projectName}.deploy.cmd /Y /M:{Environment.GetEnvironmentVariable("WEBDEPLOY_IP")} /U:{Environment.GetEnvironmentVariable("WEBDEPLOY_USER")} /P:{Environment.GetEnvironmentVariable("WEBDEPLOY_PASS")} -allowUntrusted -enableRule:AppOffline {(skipExtraFilesOnServer ? "-enableRule:DoNotDeleteRule" : "")}", workingDirectory: artifactsLocation));
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
