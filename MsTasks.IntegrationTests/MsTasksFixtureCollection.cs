using MsTasks.IntegrationTests.Fixtures;

namespace MsTasks.IntegrationTests;

[CollectionDefinition(nameof(MsTasksFixtureCollection))]
public sealed class MsTasksFixtureCollection : ICollectionFixture<MsTasksFixture>
{
}
