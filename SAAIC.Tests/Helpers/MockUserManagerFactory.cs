using Microsoft.AspNetCore.Identity;
using Moq;
using SAAIC.Domain.Entities;
using SAAIC.Infrastructure.Persistence.Context;

namespace SAAIC.Tests.Helpers;

public static class MockUserManagerFactory
{
    public static Mock<UserManager<ApplicationUser>> Create(ApplicationDbContext dbContext)
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);

        userManagerMock.Setup(m => m.Users).Returns(dbContext.Users);
        return userManagerMock;
    }
}
