using FluentAssertions;
using IP.Project.Contracts;
using IP.Project.Database;
using IP.Project.Features.Samba;
using IP.Project.Shared;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

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
                new SambaAccount { Id = Guid.NewGuid(), Description = "Samba 1", IPv4Address = "192.168.1.1" },
                new SambaAccount { Id = Guid.NewGuid(), Description = "Samba 2", IPv4Address = "192.168.1.2" },
                new SambaAccount { Id = Guid.NewGuid(), Description = "Samba 3", IPv4Address = "192.168.1.3" }
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
