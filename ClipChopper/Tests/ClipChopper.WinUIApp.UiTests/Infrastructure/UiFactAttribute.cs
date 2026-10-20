using Xunit;

namespace ClipChopper.WinUIApp.UiTests.Infrastructure;

public sealed class UiFactAttribute : FactAttribute
{
    public UiFactAttribute()
    {
        Skip = UiEnvironment.SkipReason;
    }
}

public sealed class UiTheoryAttribute : TheoryAttribute
{
    public UiTheoryAttribute()
    {
        Skip = UiEnvironment.SkipReason;
    }
}
