using FsCheck;
using FsCheck.Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using MongoDB.Driver;
using Xunit;

public class AdventurousServiceTests
{
    private readonly Mock<IMongoDatabase> _database;
    private readonly Mock<IMongoCollection<Adventurous>> _collection;
    private readonly Mock<IEventBus> _eventBus;
    private readonly Mock<ILogger<AdventurousService>> _logger;
    private readonly AdventurousService _sut;

    public AdventurousServiceTests()
    {
        _database = new Mock<IMongoDatabase>();
        _collection = new Mock<IMongoCollection<Adventurous>>();
        _eventBus = new Mock<IEventBus>();
        _logger = new Mock<ILogger<AdventurousService>>();

        _database
            .Setup(d => d.GetCollection<Adventurous>("adventurous", null))
            .Returns(_collection.Object);

        _sut = new AdventurousService(_database.Object, _eventBus.Object, _logger.Object);
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private void SetupFind(List<Adventurous> items)
    {
        var cursor = new Mock<IAsyncCursor<Adventurous>>();
        cursor.SetupSequence(c => c.MoveNextAsync(default))
              .ReturnsAsync(true)
              .ReturnsAsync(false);
        cursor.Setup(c => c.Current).Returns(items);

        _collection
            .Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<Adventurous>>(),
                It.IsAny<FindOptions<Adventurous, Adventurous>>(),
                default))
            .ReturnsAsync(cursor.Object);
    }

    private void SetupDeleteResult(long deletedCount)
    {
        var result = new Mock<DeleteResult>();
        result.Setup(r => r.DeletedCount).Returns(deletedCount);
        _collection
            .Setup(c => c.DeleteOneAsync(
                It.IsAny<FilterDefinition<Adventurous>>(),
                default))
            .ReturnsAsync(result.Object);
    }

    private void SetupUpdateResult(long matchedCount)
    {
        var result = new Mock<UpdateResult>();
        result.Setup(r => r.MatchedCount).Returns(matchedCount);
        _collection
            .Setup(c => c.UpdateOneAsync(
                It.IsAny<FilterDefinition<Adventurous>>(),
                It.IsAny<UpdateDefinition<Adventurous>>(),
                It.IsAny<UpdateOptions>(),
                default))
            .ReturnsAsync(result.Object);
    }

    private static Adventurous MakeAdventurer(Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Name = "Test",
        Class = "Warrior",
        Level = 1,
        Money = 0,
        CreatedAt = DateTime.UtcNow
    };

    // ── Property 1: GetAdventurous returns full list ──────────────────────────

    [Fact]
    public async Task GetAdventurous_ReturnsFullList()
    {
        var items = new List<Adventurous> { MakeAdventurer(), MakeAdventurer() };
        SetupFind(items);

        var result = await _sut.GetAdventurous();

        Assert.Equal(items.Count, result.Count);
        Assert.Equal(items, result);
    }

    // ── Property 2: GetAdventurous(id) returns correct adventurer ────────────

    [Fact]
    public async Task GetAdventurous_ExistingId_ReturnsAdventurer()
    {
        var adventurer = MakeAdventurer();
        SetupFind(new List<Adventurous> { adventurer });

        var result = await _sut.GetAdventurous(adventurer.Id);

        Assert.NotNull(result);
        Assert.Equal(adventurer.Id, result.Id);
    }

    // ── Edge case: non-existing id returns null ───────────────────────────────

    [Fact]
    public async Task GetAdventurous_NonExistingId_ReturnsNull()
    {
        SetupFind(new List<Adventurous>());

        var result = await _sut.GetAdventurous(Guid.NewGuid());

        Assert.Null(result);
    }

    // ── Property 3: CreateAdventurous initialises model invariants ────────────

    [Property]
    public bool CreateAdventurous_ValidRequest_ReturnsAdventurerWithCorrectInvariants(
        NonEmptyString name, NonEmptyString cls, PositiveInt level)
    {
        _collection
            .Setup(c => c.InsertOneAsync(
                It.IsAny<Adventurous>(),
                It.IsAny<InsertOneOptions>(),
                default))
            .Returns(Task.CompletedTask);

        var request = new CreateAdventurousRequest
        {
            Name = name.Get,
            Class = cls.Get,
            Level = level.Get
        };

        var result = _sut.CreateAdventurous(request).GetAwaiter().GetResult();

        return result.Id != Guid.Empty && result.Money == 0f;
    }

    // ── Property 4: CreateAdventurous publishes AdventurerCreatedEvent ────────

    [Property]
    public bool CreateAdventurous_ValidRequest_PublishesAdventurerCreatedEvent(
        NonEmptyString name, NonEmptyString cls, PositiveInt level)
    {
        var publishedEvents = new List<AdventurerCreatedEvent>();

        _collection
            .Setup(c => c.InsertOneAsync(
                It.IsAny<Adventurous>(),
                It.IsAny<InsertOneOptions>(),
                default))
            .Returns(Task.CompletedTask);

        _eventBus
            .Setup(e => e.Publish(It.IsAny<AdventurerCreatedEvent>(), It.IsAny<string>()))
            .Callback<AdventurerCreatedEvent, string>((evt, _) => publishedEvents.Add(evt));

        var request = new CreateAdventurousRequest
        {
            Name = name.Get,
            Class = cls.Get,
            Level = level.Get
        };

        var result = _sut.CreateAdventurous(request).GetAwaiter().GetResult();

        return publishedEvents.Count == 1
            && publishedEvents[0].AdventurerId == result.Id
            && publishedEvents[0].Name == result.Name
            && publishedEvents[0].Class == result.Class
            && publishedEvents[0].Level == result.Level;
    }

    // ── Edge case: IEventBus throws — exception must not propagate ────────────

    [Fact]
    public async Task CreateAdventurous_EventBusThrows_DoesNotPropagateException()
    {
        _collection
            .Setup(c => c.InsertOneAsync(
                It.IsAny<Adventurous>(),
                It.IsAny<InsertOneOptions>(),
                default))
            .Returns(Task.CompletedTask);

        _eventBus
            .Setup(e => e.Publish(It.IsAny<AdventurerCreatedEvent>(), It.IsAny<string>()))
            .Throws(new Exception("bus failure"));

        var request = new CreateAdventurousRequest { Name = "Hero", Class = "Mage", Level = 5 };

        var result = await _sut.CreateAdventurous(request);

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    // ── Property 5: DeleteAdventurous returns true for existing id ────────────

    [Fact]
    public async Task DeleteAdventurous_ExistingId_ReturnsTrue()
    {
        SetupDeleteResult(1);

        var result = await _sut.DeleteAdventurous(Guid.NewGuid());

        Assert.True(result);
    }

    // ── Edge case: non-existing id returns false ──────────────────────────────

    [Fact]
    public async Task DeleteAdventurous_NonExistingId_ReturnsFalse()
    {
        SetupDeleteResult(0);

        var result = await _sut.DeleteAdventurous(Guid.NewGuid());

        Assert.False(result);
    }

    // ── Property 6: UpdateAdventurous (PUT) returns true for existing id ──────

    [Fact]
    public async Task UpdateAdventurous_ExistingId_ReturnsTrue()
    {
        SetupUpdateResult(1);

        var request = new UpdateAdventurousRequest { Name = "Hero", Class = "Warrior", Level = 10 };
        var result = await _sut.UpdateAdventurous(Guid.NewGuid(), request);

        Assert.True(result);
    }

    // ── Edge case: non-existing id returns false ──────────────────────────────

    [Fact]
    public async Task UpdateAdventurous_NonExistingId_ReturnsFalse()
    {
        SetupUpdateResult(0);

        var request = new UpdateAdventurousRequest { Name = "Hero", Class = "Warrior", Level = 10 };
        var result = await _sut.UpdateAdventurous(Guid.NewGuid(), request);

        Assert.False(result);
    }

    // ── Property 7: PATCH with all-null fields returns NoFields (no DB call) ──

    [Property]
    public bool UpdateAdventurous_AllPatchFieldsNull_ReturnsNoFieldsWithoutDbCall(Guid id)
    {
        var request = new PatchAdventurousRequest(); // all fields null

        var result = _sut.UpdateAdventurous(id, request).GetAwaiter().GetResult();

        _collection.Verify(
            c => c.UpdateOneAsync(
                It.IsAny<FilterDefinition<Adventurous>>(),
                It.IsAny<UpdateDefinition<Adventurous>>(),
                It.IsAny<UpdateOptions>(),
                default),
            Times.Never);

        return result == RestResult.NoFields;
    }

    // ── Edge case: PATCH with field + non-existing id returns NotFound ─────────

    [Fact]
    public async Task UpdateAdventurous_PatchWithFieldNonExistingId_ReturnsNotFound()
    {
        SetupUpdateResult(0);

        var request = new PatchAdventurousRequest { Name = "NewName" };
        var result = await _sut.UpdateAdventurous(Guid.NewGuid(), request);

        Assert.Equal(RestResult.NotFound, result);
    }

    // ── Property 8: PATCH with field + existing id returns Updated ────────────

    [Fact]
    public async Task UpdateAdventurous_PatchWithFieldExistingId_ReturnsUpdated()
    {
        SetupUpdateResult(1);

        var request = new PatchAdventurousRequest { Name = "NewName" };
        var result = await _sut.UpdateAdventurous(Guid.NewGuid(), request);

        Assert.Equal(RestResult.Updated, result);
    }
}
