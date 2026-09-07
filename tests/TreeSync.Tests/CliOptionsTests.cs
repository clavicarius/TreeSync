using clausTrarius.TreeSync.Cli;
using clausTrarius.TreeSync.Core.Logging;
using Xunit;

namespace clausTrarius.TreeSync.Tests;

public sealed class CliOptionsTests
{
    [Fact]
    public void Parse_UsesDocumentedDefaults()
    {
        using TemporaryDirectory directory = new();
        string source = directory.Combine("source");
        string target = directory.Combine("target");

        CliOptions options = CliOptions.Parse(
            new[] { "--source", source, "--target", target },
            currentDirectory: directory.Path);

        Assert.Equal(System.IO.Path.GetFullPath(System.IO.Path.Combine(source, "config.json")), options.ConfigPath);
        Assert.Equal(System.IO.Path.GetFullPath(System.IO.Path.Combine(source, ".treesyncignore")), options.IgnorePath);
        Assert.Equal(System.IO.Path.GetFullPath(System.IO.Path.Combine(directory.Path, "treesync.log")), options.LogFilePath);
        Assert.False(options.DryRun);
    }

    [Fact]
    public void Parse_AcceptsLogLevelOverrideAndDryRun()
    {
        using TemporaryDirectory directory = new();

        CliOptions options = CliOptions.Parse(
            new[]
            {
                "--source", directory.Combine("source"),
                "--target", directory.Combine("target"),
                "--log-level", "error",
                "--dry-run"
            },
            currentDirectory: directory.Path);

        Assert.Equal(TreeSyncLogLevel.Error, options.LogLevelOverride);
        Assert.True(options.DryRun);
    }

    [Fact]
    public void Parse_AcceptsHelpWithoutRequiredParameters()
    {
        CliOptions options = CliOptions.Parse(new[] { "--help" });
        string copyrightNotice = CliOptions.GetCopyrightNotice();

        Assert.True(options.HelpRequested);
        Assert.False(options.VersionRequested);
        Assert.Contains("Usage:", CliOptions.GetHelpText());
        Assert.Contains("--version", CliOptions.GetHelpText());
        Assert.Contains(copyrightNotice, CliOptions.GetHelpText());
    }

    [Fact]
    public void Parse_AcceptsVersionWithoutRequiredParameters()
    {
        CliOptions options = CliOptions.Parse(new[] { "--version" });
        string copyrightNotice = CliOptions.GetCopyrightNotice();

        Assert.False(options.HelpRequested);
        Assert.True(options.VersionRequested);
        Assert.StartsWith("TreeSync ", CliOptions.GetVersionText());
        Assert.Contains(copyrightNotice, CliOptions.GetVersionText());
    }

    [Fact]
    public void GetCopyrightNotice_UsesCurrentYearRange()
    {
        int currentYear = System.DateTime.UtcNow.Year;
        string expected = currentYear <= 2024
            ? "© 2024 clavicarius. Licensed under MIT."
            : $"© 2024-{currentYear} clavicarius. Licensed under MIT.";

        Assert.Equal(expected, CliOptions.GetCopyrightNotice());
    }

    [Fact]
    public void Parse_RejectsMissingRequiredParameters()
    {
        Assert.Throws<CliOptionsException>(() => CliOptions.Parse(Array.Empty<string>()));
    }
}
