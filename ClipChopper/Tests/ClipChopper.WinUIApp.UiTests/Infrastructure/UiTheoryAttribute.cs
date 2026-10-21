using Xunit;

namespace ClipChopper.WinUIApp.UiTests.Infrastructure;

public sealed class UiTheoryAttribute : TheoryAttribute
{
    public UiTheoryAttribute()
    {
        Skip = UiEnvironment.SkipReason;
    }
}
