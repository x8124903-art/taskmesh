using MsNotifications.Infrastructure.Repositories;
using MsNotifications.IntegrationTests.Fixtures;

namespace MsNotifications.IntegrationTests.Infrastructure.Repositories;

[Collection("Database collection")]
public sealed class ProcessedEventSqlRepositoryShould
{
    private readonly DatabaseFixture _fixture;
    private readonly IProcessedEventRepository _repository;

    public ProcessedEventSqlRepositoryShould(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new ProcessedEventSqlRepository(_fixture.Context);
        _fixture.Cleanup();
    }

    [Fact]
    public async Task IsEventProcessedAsync_ReturnsFalse_WhenEventNotProcessed()
    {
        // Arrange
        const string GIVEN_EVENT_ID = "event-not-exists";

        // Act
        var result = await _repository.IsEventProcessedAsync(GIVEN_EVENT_ID);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task MarkEventAsProcessedAsync_InsertsEvent()
    {
        // Arrange
        const string GIVEN_EVENT_ID = "event-123";
        const string GIVEN_EVENT_TYPE = "TaskAssignedEvent";

        // Act
        await _repository.MarkEventAsProcessedAsync(GIVEN_EVENT_ID, GIVEN_EVENT_TYPE);

        // Assert
        var isProcessed = await _repository.IsEventProcessedAsync(GIVEN_EVENT_ID);
        isProcessed.Should().BeTrue();
    }

    [Fact]
    public async Task IsEventProcessedAsync_ReturnsTrue_WhenEventProcessed()
    {
        // Arrange
        const string GIVEN_EVENT_ID = "event-456";
        await _repository.MarkEventAsProcessedAsync(GIVEN_EVENT_ID, "TaskStatusChangedEvent");

        // Act
        var result = await _repository.IsEventProcessedAsync(GIVEN_EVENT_ID);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task MarkEventAsProcessedAsync_Idempotent_DoesNotFailOnDuplicate()
    {
        // Arrange
        const string GIVEN_EVENT_ID = "event-789";

        // Act
        await _repository.MarkEventAsProcessedAsync(GIVEN_EVENT_ID, "SomeEvent");

        // Act
        var act = async () => await _repository.MarkEventAsProcessedAsync(GIVEN_EVENT_ID, "SomeEvent");

        // Assert
        await act.Should().ThrowAsync<Exception>();

        var isProcessed = await _repository.IsEventProcessedAsync(GIVEN_EVENT_ID);
        isProcessed.Should().BeTrue();
    }
}
