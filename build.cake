//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var outputDirectory = $"build/{configuration}";
var publishDirectory = "build/Publish";

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .WithCriteria(c => HasArgument("rebuild"))
    .Does(() =>
{
    CleanDirectory(publishDirectory);
});

Task("Build")
    .IsDependentOn("Clean")
    .Does(() =>
{
    var settings = new DotNetPublishSettings
    {
        Configuration = configuration,
        OutputDirectory = outputDirectory
    };
    
    DotNetPublish("./src/Shell/Shell.csproj", settings);

});

Task("Zip")
    .IsDependentOn("Build")
    .Does(() =>
{
    Zip(outputDirectory, System.IO.Path.Combine(outputDirectory,"..",configuration+".zip"));    
});

Task("Test")
    .IsDependentOn("Zip")
    .Does(() =>
{
    DotNetTest("./Slam.sln", new DotNetTestSettings
    {
        Configuration = configuration,
        NoBuild = true,
    });
});

Task("Default")
    .IsDependentOn("Test");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);