using FluentAssertions;
using IP.Project.Contracts;
using IP.Project.Features.Samba;
using IP.Project.Entities;
using IP.Project.Tests.Base;

namespace IP.Project.Tests.Features.Samba
{
    public class GetAllSambasTests : BaseTest<SambaAccount>
    {
        [Fact]
        public async Task GetAllSambasHandler_ReturnsListOfSambas()
        {
            // Arrange
            var sambas = new List<SambaAccount>
            {
                new SambaAccount { Id = Guid.Parse("4c727215-0522-4384-8481-4a2d1e094fb7"), Description = "Samba 1", IPv4Address = "192.168.1.1" },
                new SambaAccount { Id = Guid.Parse("4c727215-0522-4384-8481-4a2d1e094fb8"), Description = "Samba 2", IPv4Address = "192.168.1.2" },
                new SambaAccount { Id = Guid.Parse("4c727215-0522-4384-8481-4a2d1e094fb9"), Description = "Samba 3", IPv4Address = "192.168.1.3" }
            };

            var mock = Setup(sambas);
            var sut = new GetAllSambas.Handler(mock);
            var query = new GetAllSambas.Query();

            // Act
            var result = await sut.Handle(query, default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Should().HaveCount(sambas.Count);

            foreach (var samba in sambas)
            {
                result.Value.Should().ContainEquivalentOf(new SambaResponse
                {
                    Id = samba.Id,
                    Description = samba.Description,
                    IPv4Address = samba.IPv4Address
                });
            }
        }

        [Fact]
        public async Task GetAllSambasHandler_ReturnsEmptyListWhenNoSambasExist()
        {
            // Arrange
            var mock = Setup(new List<SambaAccount>());
            var sut = new GetAllSambas.Handler(mock);
            var query = new GetAllSambas.Query();

            // Act
            var result = await sut.Handle(query, default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Should().BeEmpty();
        }
    }
}
