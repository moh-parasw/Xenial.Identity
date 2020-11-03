using System;

using static SimpleExec.Command;
using static Bullseye.Targets;
using System.IO;

var sln = "Xenial.Identity.sln";
var projectName = "Xenial.Identity";
var web = $"src/{projectName}/{projectName}.csproj";
var iisPackageName = "identity.xenial.io";
var artifactsLocation = Path.GetFullPath($"./artifacts");
var artifact = Path.GetFullPath($"{artifactsLocation}/{projectName}.zip");
var configuration = "Release";
var selfContained = false;
var packageAsSingleFile = false;

Target("restore", () => RunAsync("dotnet", $"restore {sln}"));
Target("build", DependsOn("restore"), () => RunAsync("dotnet", $"build {sln} --no-restore"));

Target("publish", DependsOn("build"), () => RunAsync("dotnet", $"msbuild {web} /t:Restore;Build /p:Configuration={configuration} /p:RuntimeIdentifier=win-x64 /p:SelfContained={selfContained} /p:PackageAsSingleFile={packageAsSingleFile} /p:DeployOnBuild=true /p:WebPublishMethod=package /p:PublishProfile=Package /v:minimal /p:DesktopBuildPackageLocation={artifact} /p:DeployIisAppPath={iisPackageName}"));

Target("deploy", DependsOn("publish"), () => RunAsync("cmd.exe", $"/C {projectName}.deploy.cmd /T /M:{Environment.GetEnvironmentVariable("WEBDEPLOY_IP")} /U:{Environment.GetEnvironmentVariable("WEBDEPLOY_USER")} /P:{Environment.GetEnvironmentVariable("WEBDEPLOY_PASS")} -allowUntrusted", workingDirectory: artifactsLocation));

Target("default", DependsOn("publish"));

await RunTargetsAndExitAsync(args);