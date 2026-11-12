using System.Globalization;
using System.Reflection;
using optimizerDuck.Common.Helpers;
using optimizerDuck.Domain.Abstractions;
using optimizerDuck.Domain.Optimizations.Models;
using optimizerDuck.Domain.UI;
using optimizerDuck.Resources.Languages;

namespace optimizerDuck.Test.Services;

/// <summary>
///     Pins Dadeum's catalog rules: every Moderate or Risky item says what the user gives up, and
///     every source link is an absolute https URL.
/// </summary>
public class OptimizationEvidenceTests
{
    private static IEnumerable<BaseOptimization> Catalog()
    {
        foreach (
            var category in ReflectionHelper.FindImplementationsInLoadedAssemblies<IOptimizationCategory>()
        )
        {
            // The Debug category (Debug builds only) carries hard-coded English test items.
            if (category.Name == "Debug")
                continue;

            foreach (
                var nested in category
                    .GetNestedTypes(BindingFlags.Public)
                    .Where(t =>
                        typeof(IOptimization).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass
                    )
            )
            {
                var optimization = (BaseOptimization)Activator.CreateInstance(nested)!;
                optimization.OwnerType = category;
                yield return optimization;
            }
        }
    }

    [Fact]
    public void EveryModerateOrRiskyItem_HasAConsequence()
    {
        var missing = Catalog()
            .Where(o => o.Risk is OptimizationRisk.Moderate or OptimizationRisk.Risky)
            .Select(o => $"Optimizer.{o.OwnerKey}.{o.OptimizationKey}.Consequence")
            .Where(key =>
                string.IsNullOrWhiteSpace(
                    Translations.ResourceManager.GetString(key, CultureInfo.InvariantCulture)
                )
            )
            .ToList();

        Assert.True(missing.Count == 0, "Missing consequences:\n" + string.Join("\n", missing));
    }

    [Fact]
    public void EveryEvidenceUrl_IsAnAbsoluteHttpsLink()
    {
        var invalid = Catalog()
            .Where(o => o.EvidenceUrl is not null)
            .Where(o =>
                !Uri.TryCreate(o.EvidenceUrl, UriKind.Absolute, out var uri)
                || uri.Scheme != Uri.UriSchemeHttps
            )
            .Select(o => $"{o.OptimizationKey}: {o.EvidenceUrl}")
            .ToList();

        Assert.True(invalid.Count == 0, "Invalid evidence links:\n" + string.Join("\n", invalid));
    }

    [Fact]
    public void Consequence_IsNullWhenTheKeyIsAbsent()
    {
        var safeWithoutText = Catalog().First(o => o.OptimizationKey == "DisableClickToDo");

        Assert.Null(safeWithoutText.Consequence);
    }

    [Fact]
    public void Catalog_HasEvidenceForTheDocumentedItems()
    {
        var withEvidence = Catalog().Count(o => o.EvidenceUrl is not null);

        Assert.True(withEvidence >= 20, $"Only {withEvidence} items link a source.");
    }
}
