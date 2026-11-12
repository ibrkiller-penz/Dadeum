using Microsoft.Extensions.Logging;
using optimizerDuck.Domain.Execution;
using optimizerDuck.Domain.Optimizations.Models.Services;
using optimizerDuck.Services.System.Primitives;

namespace optimizerDuck.Services.Optimization;

/// <summary>Whether the settings an applied item wrote are still in place.</summary>
public enum DriftStatus
{
    /// <summary>Nothing could be checked: no apply record, or no step of a checkable kind.</summary>
    Unknown,

    /// <summary>Every checked setting still holds the value the apply left.</summary>
    InPlace,

    /// <summary>At least one checked setting no longer holds the value the apply left.</summary>
    Drifted,
}

/// <summary>The outcome of a drift check.</summary>
/// <param name="Status">The overall status.</param>
/// <param name="Changed">The settings that no longer hold the applied value, as shown to the user.</param>
/// <param name="Checked">How many settings were read.</param>
public sealed record DriftResult(DriftStatus Status, IReadOnlyList<string> Changed, int Checked)
{
    public static readonly DriftResult Unknown = new(DriftStatus.Unknown, [], 0);
}

/// <summary>
///     Dadeum: compares what an apply left (its change record) with what the system holds now,
///     so an item that Windows Update or another program quietly set back is shown as changed
///     instead of applied. It only reads. Registry values and service startup types are checked;
///     other step kinds are skipped, never guessed.
/// </summary>
public static class DriftDetector
{
    /// <summary>Checks one item's last apply record against the live system.</summary>
    /// <param name="record">The item's change record, or <see langword="null" /> when there is none.</param>
    /// <param name="logger">An optional logger.</param>
    public static async Task<DriftResult> CheckAsync(ChangeRecord? record, ILogger? logger = null)
    {
        if (record is null || record.Operation != ChangeRecordOperation.Apply)
            return DriftResult.Unknown;

        var changed = new List<string>();
        var checkedCount = 0;

        foreach (var step in record.Steps)
        {
            // Only steps that left the system in a known state: written now, or already right.
            // A step with a reason wrote nothing (for example a missing service).
            if (!step.Ok || step.Reason is not null)
                continue;
            if (step.Kind is not (ChangeKind.Change or ChangeKind.Skip))
                continue;
            if (string.IsNullOrEmpty(step.Target))
                continue;

            switch (step.Operation)
            {
                case "registry.write":
                {
                    if (!TryRead(step, logger, out var current))
                        continue;
                    checkedCount++;
                    if (RegistryService.DescribeValue(current) != step.NewValue)
                        changed.Add(Describe(step));
                    break;
                }
                case "registry.delete":
                {
                    if (!TryRead(step, logger, out var current))
                        continue;
                    checkedCount++;
                    if (current is not null)
                        changed.Add(Describe(step));
                    break;
                }
                case "service.startup":
                {
                    if (!Enum.TryParse<ServiceStartupType>(step.NewValue, out var expected))
                        continue;
                    var (actual, notFound) = await ServiceProcessService
                        .GetStartupTypeAsync(step.Target, logger)
                        .ConfigureAwait(false);
                    if (notFound || actual is null)
                        continue;
                    checkedCount++;
                    if (actual != expected)
                        changed.Add(step.Target);
                    break;
                }
            }
        }

        if (checkedCount == 0)
            return DriftResult.Unknown;

        return new DriftResult(
            changed.Count > 0 ? DriftStatus.Drifted : DriftStatus.InPlace,
            changed,
            checkedCount
        );
    }

    private static bool TryRead(ChangeRecordStep step, ILogger? logger, out object? current) =>
        RegistryService.TryReadValue(
            new RegistryItem(step.Target!, step.ValueName),
            out current,
            logger
        );

    private static string Describe(ChangeRecordStep step) =>
        string.IsNullOrEmpty(step.ValueName) ? step.Target! : $@"{step.Target}\{step.ValueName}";
}
