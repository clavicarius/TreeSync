using clausTrarius.TreeSync.Cli;
using Xunit;

namespace clausTrarius.TreeSync.Tests;

public sealed class ProgramRunnerTests
{
    [Fact]
    public void Run_WritesStartupAndShutdownEntriesToLogFile()
    {
        using TemporaryDirectory root = new();
        string source = root.Combine("source files");
        string target = root.Combine("target files");
        Directory.CreateDirectory(source);
        Directory.CreateDirectory(target);
        File.WriteAllText(
            System.IO.Path.Combine(source, "config.json"),
            """
            {
              "include_extensions": [".php"],
              "logging": {
                "level": "info"
              }
            }
            """);

        int exitCode = ProgramRunner.Run(
            new[] { "--source", source, "--target", target, "--dry-run" },
            currentDirectory: root.Path,
            applicationName: "TreeSync",
            applicationVersion: "9.8.7");

        string logContent = File.ReadAllText(root.Combine("treesync.log"));

        Assert.Equal(0, exitCode);
        Assert.Contains("START TreeSync 9.8.7 args:", logContent, StringComparison.Ordinal);
        Assert.Contains("--source", logContent, StringComparison.Ordinal);
        Assert.Contains("source files", logContent, StringComparison.Ordinal);
        Assert.Contains("target files", logContent, StringComparison.Ordinal);
        Assert.Contains("--dry-run", logContent, StringComparison.Ordinal);
        Assert.Contains("END TreeSync 9.8.7 exit code 0", logContent, StringComparison.Ordinal);
    }

    [Fact]
    public void Run_WritesShutdownEntryForValidationFailures()
    {
        using TemporaryDirectory root = new();
        string source = root.Combine("source");
        Directory.CreateDirectory(source);
        File.WriteAllText(
            System.IO.Path.Combine(source, "config.json"),
            """
            {
              "include_extensions": [".php"],
              "logging": {
                "level": "info"
              }
            }
            """);

        int exitCode = ProgramRunner.Run(
            new[] { "--source", source, "--target", source },
            currentDirectory: root.Path,
            applicationName: "TreeSync",
            applicationVersion: "9.8.7");

        string logContent = File.ReadAllText(root.Combine("treesync.log"));

        Assert.Equal(2, exitCode);
        Assert.Contains("ERROR Source and target must not point to the same directory.", logContent, StringComparison.Ordinal);
        Assert.Contains("END TreeSync 9.8.7 exit code 2", logContent, StringComparison.Ordinal);
    }
}
