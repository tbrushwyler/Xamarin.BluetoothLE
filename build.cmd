@echo off
IF NOT [%1]==[help] (
	cls
	".nuget\NuGet.exe" "Install" "FAKE" "-OutputDirectory" "packages" "-ExcludeVersion" "-version" "4.21.4"
	".nuget\NuGet.exe" "Install" "OctopusTools" "-OutputDirectory" "packages" "-ExcludeVersion" "-version" "3.3.8"
)

SET TARGET="CI"

IF NOT [%1]==[] (set TARGET="%1")
IF NOT [%2]==[] (set BUILDNUMBER="%2")

"packages\FAKE\tools\Fake.exe" "build.fsx" "target=%TARGET%" "buildnumber=%BUILDNUMBER%"