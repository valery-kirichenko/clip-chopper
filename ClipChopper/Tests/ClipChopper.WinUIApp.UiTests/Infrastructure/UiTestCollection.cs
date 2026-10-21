using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace ClipChopper.WinUIApp.UiTests.Infrastructure;

[CollectionDefinition("UI")]
public sealed class UiTestCollection : ICollectionFixture<TestClipFixture>
{
}
