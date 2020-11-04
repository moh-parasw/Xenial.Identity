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
var selfContained = true;
var packageAsSingleFile = false;


Target("restore:npm", () => RunAsync("cmd.exe", $"/C npm ci"));
Target("restore:dotnet", () => RunAsync("dotnet", $"restore {sln}"));
Target("restore", DependsOn("restore:npm", "restore:dotnet"));

Target("build:npm", () => RunAsync("cmd.exe", $"/C npm run build"));
Target("build:dotnet", DependsOn("restore:dotnet"), () => RunAsync("dotnet", $"build {sln} --no-restore"));
Target("build", DependsOn("restore", "build:npm", "build:dotnet"));


var connectionString = Environment.GetEnvironmentVariable("XENIAL_DEFAULTCONNECTIONSTRING");
Target("publish", DependsOn("build"), () => RunAsync("dotnet", $"msbuild {web} /t:Restore;Build /p:Configuration={configuration} /p:RuntimeIdentifier=win-x64 /p:SelfContained={selfContained} /p:PackageAsSingleFile={packageAsSingleFile} /p:DeployOnBuild=true /p:WebPublishMethod=package /p:PublishProfile=Package /v:minimal /p:DesktopBuildPackageLocation={artifact} /p:DeployIisAppPath={iisPackageName} /p:DefaultConnectionString=\"{connectionString}\""));
Target("deploy", DependsOn("publish"), () => RunAsync("cmd.exe", $"/C {projectName}.deploy.cmd /Y /M:{Environment.GetEnvironmentVariable("WEBDEPLOY_IP")} /U:{Environment.GetEnvironmentVariable("WEBDEPLOY_USER")} /P:{Environment.GetEnvironmentVariable("WEBDEPLOY_PASS")} -allowUntrusted -enableRule:AppOffline", workingDirectory: artifactsLocation));
Target("default", DependsOn("publish"));

await RunTargetsAndExitAsync(args);