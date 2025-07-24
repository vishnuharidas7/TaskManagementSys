//using LoggingLibrary.Interfaces;
//using Microsoft.EntityFrameworkCore;
//using Moq;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using TaskManagementWebAPI.Application.Interfaces;
//using TaskManagementWebAPI.Application.PasswordService;
//using TaskManagementWebAPI.Application.Services;
//using TaskManagementWebAPI.Domain.Interfaces;
//using TaskManagementWebAPI.Domain.Models;
//using TaskManagementWebAPI.Infrastructure.Persistence;
//using TaskManagementWebAPI.Infrastructure.Repositories;

//namespace TaskManagementWebAPITest.Application.Services
//{
//    public class UserApplicationServiceTest
//    {
//        //private async Task<ApplicationDbContext> applicationDbContext()
//        //{
//        //    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
//        //        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
//        //        .Options;
//        //    var databaseContext=new ApplicationDbContext(options);
//        //    databaseContext.Database.EnsureCreated();
//        //    if(await databaseContext.)

//        //    return new ApplicationDbContext(options);// unique db per test
            
//        //}

//        //private UserApplicationService GetService(ApplicationDbContext context)
//        //{
//        //    // Create mocks
//        //    var randomPswdMock = new Mock<IRandomPasswordGenerator>();
//        //    var userRepoMock = new Mock<IUserRepository>();
//        //    var loggerMock = new Mock<IAppLogger<UserRepository>>();
//        //    var emailContentBuilderMock = new Mock<IUserCreatedEmailContentBuilder>();
//        //    var emailServiceMock = new Mock<IEmailService>();

//        //    return new UserApplicationService(
//        //        context,
//        //        randomPswdMock.Object,
//        //        userRepoMock.Object,
//        //        loggerMock.Object,
//        //        emailContentBuilderMock.Object,
//        //        emailServiceMock.Object
//        //    );
//        //}

//        //[Fact]
//        //public async Task CheckUserExists_ThrowsArgumentException_WhenUsernameIsNullOrEmpty()
//        //{
//        //    // Arrange
//        //    var context = applicationDbContext();
//        //    var service = GetService(context);
//        //    // Act & Assert
//        //    await Assert.ThrowsAsync<ArgumentException>(() => service.CheckUserExists(null));
//        //    await Assert.ThrowsAsync<ArgumentException>(() => service.CheckUserExists(""));
//        //    await Assert.ThrowsAsync<ArgumentException>(() => service.CheckUserExists("  "));

//        //}

//        //[Fact]
//        //public async Task CheckUserExists_ReturnTrue_WhenUserNameExist()
//        //{
//        //    var context = applicationDbContext();
//        //    // Seed user

//        //    context.User.Add(new Users { UserName = "Existinguser" });
//        //    await context.SaveChangesAsync();
//        //    var service = GetService(context);
//        //    // Act
//        //    bool exist = await service.CheckUserExists("Existinguser");
//        //    //
//        //    Assert.True(exist);
//        //}
//        //[Fact]
//        //public async Task CheckUserExists_ReturnsFalse_WhenUserDoesNotExist()
//        //{
//        //    // Arrange
//        //    var context = applicationDbContext();

//        //    var service = GetService(context);
//        //    // Act
//        //    bool exists = await service.CheckUserExists("nonexistentuser");

//        //    // Assert
//        //    Assert.False(exists);
//        //}



//       // private 
//    }
//}
