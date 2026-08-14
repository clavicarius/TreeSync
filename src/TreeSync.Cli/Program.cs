using System.Reflection;
using clausTrarius.TreeSync.Cli;
using clausTrarius.TreeSync.Core.Configuration;
using clausTrarius.TreeSync.Core.Ignore;
using clausTrarius.TreeSync.Core.Logging;
using clausTrarius.TreeSync.Core.Safety;
using clausTrarius.TreeSync.Core.Sync;

return ProgramRunner.Run(args);

public static class ProgramRunner
{
    public static int Run(
        string[] args,
        string? currentDirectory = null,
        string? applicationName = null,
        string? applicationVersion = null)
    {
        currentDirectory ??= Directory.GetCurrentDirectory();
        applicationName ??= GetApplicationName();
        applicationVersion ??= GetApplicationVersion();

        try
        {
            CliOptions options = CliOptions.Parse(args, currentDirectory);
            if (options.HelpRequested)
            {
                Console.Out.WriteLine(CliOptions.GetHelpText());
                return 0;
            }

            TreeSyncConfig config = TreeSyncConfig.Load(options.ConfigPath);
            TreeSyncLogLevel logLevel = options.LogLevelOverride ?? config.LogLevel;

            using TreeSyncLogger logger = new(options.LogFilePath, logLevel);
            return RunSync(args, options, config, logger, applicationName, applicationVersion);
        }
        catch (CliOptionsException ex)
        {
            LogStartupError(ex.Message);
            return 1;
        }
        catch (ConfigurationException ex)
        {
            LogStartupError(ex.Message);
            return 1;
        }
        catch (ArgumentException ex)
        {
            LogStartupError(ex.Message);
            return 1;
        }
        catch (DirectoryNotFoundException ex)
        {
            LogStartupError(ex.Message);
            return 1;
        }
        catch (Exception ex)
        {
            LogStartupError(ex.ToString());
            return 3;
        }
    }

    private static int RunSync(
        string[] args,
        CliOptions options,
        TreeSyncConfig config,
        TreeSyncLogger logger,
        string applicationName,
        string applicationVersion)
    {
        logger.LogAction("START", $"{applicationName} {applicationVersion} args: {FormatCommandLineArguments(args)}");

        int exitCode = 0;

        try
        {
            PathSafetyValidator.ValidatePaths(options.SourcePath, options.TargetPath);

            IgnoreRuleSet ignoreRules = IgnoreRuleSet.Load(options.IgnorePath);
            SyncEngine syncEngine = new(
                options.SourcePath,
                options.TargetPath,
                config,
                ignoreRules,
                logger,
                options.DryRun);

            syncEngine.Run();
        }
        catch (SafetyValidationException ex)
        {
            logger.LogError(ex.Message);
            exitCode = 2;
        }
        catch (ArgumentException ex)
        {
            logger.LogError(ex.Message);
            exitCode = 1;
        }
        catch (DirectoryNotFoundException ex)
        {
            logger.LogError(ex.Message);
            exitCode = 1;
        }
        catch (Exception ex)
        {
            logger.LogError(ex.ToString());
            exitCode = 3;
        }
        finally
        {
            logger.LogAction("END", $"{applicationName} {applicationVersion} exit code {exitCode}");
        }

        return exitCode;
    }

    private static void LogStartupError(string message)
    {
        using TreeSyncLogger logger = new("treesync.log", TreeSyncLogLevel.Error);
        logger.LogError(message);
    }

    private static string GetApplicationName()
    {
        return typeof(ProgramRunner).Assembly.GetName().Name ?? "TreeSync";
    }

    private static string GetApplicationVersion()
    {
        Assembly assembly = typeof(ProgramRunner).Assembly;
        return assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? assembly.GetName().Version?.ToString()
            ?? "unknown";
    }

    private static string FormatCommandLineArguments(string[] args)
    {
        return args.Length == 0
            ? "(none)"
            : string.Join(" ", args.Select(FormatCommandLineArgument));
    }

    private static string FormatCommandLineArgument(string argument)
    {
        if (argument.Length == 0)
        {
            return "\"\"";
        }

        if (!argument.Any(char.IsWhiteSpace) && !argument.Contains('"'))
        {
            return argument;
        }

        return $"\"{argument.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";
    }
}
