using optimizerDuck.Common.Helpers;
using optimizerDuck.Domain.Abstractions;

namespace optimizerDuck.Test.Common.Helpers;

/// <summary>
///     Pins that discovery finds the app's own types even though the app assembly is named
///     "Dadeum" while its namespace stays "optimizerDuck".
/// </summary>
public class ReflectionHelperTests
{
    private static readonly System.Reflection.Assembly AppAssembly =
        typeof(ReflectionHelper).Assembly;

    [Fact]
    public void AppAssembly_IsNamedDadeum()
    {
        Assert.Equal("Dadeum", AppAssembly.GetName().Name);
    }

    [Fact]
    public void FindImplementations_IncludesRevertStepsFromTheAppAssembly()
    {
        var steps = ReflectionHelper.FindImplementationsInLoadedAssemblies<IRevertStep>();

        Assert.Contains(steps, type => type.Assembly == AppAssembly);
    }

    [Fact]
    public void FindImplementations_IncludesOptimizationCategoriesFromTheAppAssembly()
    {
        var categories =
            ReflectionHelper.FindImplementationsInLoadedAssemblies<IOptimizationCategory>();

        Assert.Contains(categories, type => type.Assembly == AppAssembly);
    }
}
