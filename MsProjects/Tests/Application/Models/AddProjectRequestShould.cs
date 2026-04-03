namespace MsProjects.Tests.Application.Models;

using MsProjects.Application.Models;

public sealed class AddProjectRequestShould
{
    [Fact]
    public void CreateWithValidParameters()
    {
        var request = new AddProjectRequest("Test Project", "Test Description");
        
        request.Name.Should().Be("Test Project");
        request.Description.Should().Be("Test Description");
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("Name", "")]
    [InlineData("", "Description")]
    public void CreateWithEmptyValues(string name, string description)
    {
        var request = new AddProjectRequest(name, description);
        
        request.Name.Should().Be(name);
        request.Description.Should().Be(description);
    }

    [Fact]
    public void SupportRecordEquality()
    {
        var request1 = new AddProjectRequest("Project", "Description");
        var request2 = new AddProjectRequest("Project", "Description");
        var request3 = new AddProjectRequest("Different", "Description");
        
        request1.Should().Be(request2);
        request1.Should().NotBe(request3);
    }
}
