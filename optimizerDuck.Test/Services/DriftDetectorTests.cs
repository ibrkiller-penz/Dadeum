using Microsoft.Win32;
using optimizerDuck.Domain.Execution;
using optimizerDuck.Services.Optimization;

namespace optimizerDuck.Test.Services;

/// <summary>
///     Pins how an applied item is compared with the live system: a value still as written is in
///     place, a value set back is drifted, and anything not checkable is unknown, never guessed.
///     Uses a throwaway key under HKCU only.
/// </summary>
public sealed class DriftDetectorTests : IDisposable
{
    private readonly string _nativePath = $@"Software\TestOptimizerDuckDrift_{Guid.NewGuid():N}";
    private string KeyPath => $@"HKCU\{_nativePath}";

    public void Dispose()
    {
        Registry.CurrentUser.DeleteSubKeyTree(_nativePath, throwOnMissingSubKey: false);
    }

    private void SetValue(string name, object value, RegistryValueKind kind)
    {
        using var key = Registry.CurrentUser.CreateSubKey(_nativePath, writable: true);
        key.SetValue(name, value, kind);
    }

    private static ChangeRecord Record(params ChangeRecordStep[] steps) =>
        new()
        {
            Id = Guid.NewGuid(),
            OptimizationKey = "Test",
            Operation = ChangeRecordOperation.Apply,
            Steps = steps,
        };

    private ChangeRecordStep Write(
        string name,
        string newValue,
        ChangeKind kind = ChangeKind.Change,
        bool ok = true
    ) =>
        new()
        {
            Operation = "registry.write",
            Target = KeyPath,
            ValueName = name,
            NewValue = newValue,
            Kind = kind,
            Ok = ok,
        };

    [Fact]
    public async Task NoRecord_IsUnknown()
    {
        var result = await DriftDetector.CheckAsync(null);

        Assert.Equal(DriftStatus.Unknown, result.Status);
    }

    [Fact]
    public async Task ValueStillAsWritten_IsInPlace()
    {
        SetValue("Enabled", 0, RegistryValueKind.DWord);

        var result = await DriftDetector.CheckAsync(Record(Write("Enabled", "0")));

        Assert.Equal(DriftStatus.InPlace, result.Status);
        Assert.Equal(1, result.Checked);
    }

    [Fact]
    public async Task ValueSetBack_IsDrifted_AndNamesTheSetting()
    {
        SetValue("Enabled", 1, RegistryValueKind.DWord);

        var result = await DriftDetector.CheckAsync(Record(Write("Enabled", "0")));

        Assert.Equal(DriftStatus.Drifted, result.Status);
        Assert.Equal($@"{KeyPath}\Enabled", Assert.Single(result.Changed));
    }

    [Fact]
    public async Task SkippedStepThatWasAlreadyRight_IsCheckedToo()
    {
        SetValue("Mode", "High", RegistryValueKind.String);

        var result = await DriftDetector.CheckAsync(Record(Write("Mode", "High", ChangeKind.Skip)));

        Assert.Equal(DriftStatus.InPlace, result.Status);
    }

    [Fact]
    public async Task FailedStep_IsNotChecked()
    {
        SetValue("Enabled", 1, RegistryValueKind.DWord);

        var result = await DriftDetector.CheckAsync(Record(Write("Enabled", "0", ok: false)));

        Assert.Equal(DriftStatus.Unknown, result.Status);
    }

    [Fact]
    public async Task RemovedValueThatCameBack_IsDrifted()
    {
        SetValue("Gone", 1, RegistryValueKind.DWord);
        var step = new ChangeRecordStep
        {
            Operation = "registry.delete",
            Target = KeyPath,
            ValueName = "Gone",
            Kind = ChangeKind.Change,
        };

        var result = await DriftDetector.CheckAsync(Record(step));

        Assert.Equal(DriftStatus.Drifted, result.Status);
    }

    [Fact]
    public async Task RevertRecord_IsUnknown()
    {
        SetValue("Enabled", 1, RegistryValueKind.DWord);
        var record = Record(Write("Enabled", "0")) with
        {
            Operation = ChangeRecordOperation.Revert,
        };

        var result = await DriftDetector.CheckAsync(record);

        Assert.Equal(DriftStatus.Unknown, result.Status);
    }

    [Fact]
    public async Task MissingService_IsNotChecked()
    {
        var step = new ChangeRecordStep
        {
            Operation = "service.startup",
            Target = $"OptimizerDuckDriftMissing_{Guid.NewGuid():N}",
            NewValue = "Manual",
            Kind = ChangeKind.Change,
        };

        var result = await DriftDetector.CheckAsync(Record(step));

        Assert.Equal(DriftStatus.Unknown, result.Status);
    }
}
