using Xunit;

namespace ClipChopper.WinUIApp.UiTests.Infrastructure;

public sealed class AdminUiTheoryAttribute : TheoryAttribute
{
    public AdminUiTheoryAttribute()
    {
        Skip = UiEnvironment.AdminSkipReason;
    }
}
