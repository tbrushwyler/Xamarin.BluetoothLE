// include Fake lib
#r "packages/FAKE/tools/FakeLib.dll"
open Fake
open System
open Fake.AssemblyInfoFile
open Fake.OctoTools
open Fake.XamarinHelper
open Fake.NuGetHelper

// project name and description
let authors = ["byBrick";"Taylor Brushwyler"]
let projectName = "XamarinBluetoothLE"
let projectDescription = "Xamarin bluetooth library"
let releaseNotesFile = "ReleaseNotes.md"

// directories & properties
let testDir  = @".\build\test-results"
let buildDir = "./build/files/"
let nugetDir = "./build/nuget/"
let packagingDir = "./build/package/"

// solution info
let nuspecFile = "./nuget.nuspec"
let solutionFile = "./Xamarin.BluetoothLE.sln"

let nugetServer = "http://nuget.bybrick.eu/"
let nugetApiKey = "qwerty1234!"

let Exec command args =
    let result = Shell.Exec(command, args)
    if result <> 0 then failwithf "%s exited with error %d" command result

let releaseNotes = 
    ReadFile releaseNotesFile
    |> ReleaseNotesHelper.parseReleaseNotes


Target "Clean" (fun _ ->
    CleanDirs [buildDir]
)

Target "RestoreNuget" (fun _ ->
     
     "./BluetoothLE.Core/packages.config"
     |> RestorePackage (fun p ->
         { p with 
            Retries = 2 
            OutputPath = "packages"
         })

     "./BluetoothLE.iOS/packages.config"
     |> RestorePackage (fun p ->
         { p with 
            Retries = 2 
            OutputPath = "packages"
         })

     "./BluetoothLE.Droid/packages.config"
     |> RestorePackage (fun p ->
         { p with 
            Retries = 2 
            OutputPath = "packages"
         })
)

Target "AssemblyInfo" (fun _ ->
    CreateCSharpAssemblyInfo "./SolutionInfo.cs"
      [ Attribute.Product projectName
        Attribute.Version releaseNotes.AssemblyVersion
        Attribute.FileVersion releaseNotes.AssemblyVersion ]
)

Target "BuildDebug" (fun () ->
    !! solutionFile
    |> MSBuildDebug "" "Build"
    |> ignore
)

Target "CompileRelease" (fun () ->
    !! solutionFile
    |> MSBuildRelease "" "Rebuild"
    |> ignore
)

Target "BuildRelease" (fun () ->
    !! solutionFile
    |> MSBuildRelease buildDir "Rebuild"
    |> ignore
)

Target "CreateNugetPackage" (fun _ ->
    let nugetVersion = releaseNotes.AssemblyVersion + "." + getBuildParamOrDefault "buildnumber" "0" 
    CreateDir nugetDir
    CleanDirs [packagingDir]

    let nugetRootDir = packagingDir @@ "nuget"
    let androidMonoDir = nugetRootDir @@ "lib/MonoAndroid10"
    let iosMonoDir = nugetRootDir @@ "lib/Xamarin.iOS10"
    let portableMonoDir = nugetRootDir @@ "lib\portable-net45+wp8+wpa81+win8+MonoAndroid10+MonoTouch10+Xamarin.iOS10"
    CreateDir nugetRootDir
    CreateDir androidMonoDir
    CreateDir iosMonoDir
    CreateDir portableMonoDir
    //Copy files from build to package folder
    CopyFiles androidMonoDir [buildDir @@ "BluetoothLE.Core.dll";buildDir @@ "BluetoothLE.Core.xml";buildDir @@ "BluetoothLE.Droid.dll";buildDir @@ "BluetoothLE.Droid.xml"] |> tracefn "%A"
    CopyFiles iosMonoDir  [buildDir @@ "BluetoothLE.Core.dll";buildDir @@ "BluetoothLE.Core.xml";buildDir @@ "BluetoothLE.iOS.dll";buildDir @@ "BluetoothLE.iOS.xml"] |> tracefn "%A"
    CopyFiles portableMonoDir [buildDir @@ "BluetoothLE.Core.dll";buildDir @@ "BluetoothLE.Core.xml"] |> tracefn "%A"
   
    NuGet (fun p -> 
        {p with
            Authors = authors
            Project = projectName
            Description = projectDescription                               
            OutputPath = nugetDir
            WorkingDir = nugetRootDir
            Version = nugetVersion
            ReleaseNotes = toLines releaseNotes.Notes
            AccessKey = nugetApiKey
            Publish = true
            PublishUrl = nugetServer
            Files = [("**/*.*", None, None)]
        }) nuspecFile 

    
)

// --------------------------------------------------------------------------------------
// Run the unit tests using test runner & kill test runner when complete

Target "RunTests" (fun _ ->
    let nunitVersion = GetPackageVersion "packages" "NUnit.Runners"
    let nunitPath = sprintf "packages/NUnit.Runners.%s/tools" nunitVersion
    ActivateFinalTarget "CloseTestRunner"

    !! ("**/bin/Debug/*.Test.dll")
    |> NUnit (fun p ->
        { p with
            Framework = "v4.0.30319"
            ToolPath = nunitPath
            DisableShadowCopy = true
            TimeOut = TimeSpan.FromMinutes 20.
            OutputFile = "TestResults.xml" })
)

FinalTarget "CloseTestRunner" (fun _ ->  
    ProcessHelper.killProcess "nunit-agent.exe"
)


Target "Deploy" DoNothing
Target "CI" DoNothing
Target "Release" DoNothing

// Dependencie
"Clean"
 // ==> "RestoreNuget"
  ==> "AssemblyInfo"
  ==> "BuildRelease"
  ==> "CreateNugetPackage"
  ==> "Deploy"


//"RestoreNuget"
//  ==> 
"BuildDebug"
//  ==> "RunTests"
  ==> "CI"

"Release"
 // ==> "RestoreNuget"
  ==> "CompileRelease"
  
RunTargetOrDefault "CI"
