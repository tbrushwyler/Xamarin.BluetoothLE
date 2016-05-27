mono --runtime=v4.0 .nuget/NuGet.exe update -self

mono --runtime=v4.0 .nuget/nuget.exe install FAKE -OutputDirectory packages -ExcludeVersion -Version 3.34.0
mono --runtime=v4.0 .nuget/nuget.exe install OctopusTools -OutputDirectory packages -ExcludeVersion -Version 2.6.3.59
mono --runtime=v4.0 packages/fake/tools/FAKE.exe build.fsx $@