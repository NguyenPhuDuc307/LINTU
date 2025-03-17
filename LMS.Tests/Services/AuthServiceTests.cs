// using LMS.Areas.Identity.Pages.Account;
// using LMS.Data.Entities;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Identity;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Mvc.RazorPages;
// using Microsoft.Extensions.Logging;
// using Moq;
// using System.Collections.Generic;
// using System.Linq;
// using System.Security.Claims;
// using System.Threading.Tasks;
// using Xunit;


// public class LoginModelTests
// {
//     private readonly Mock<SignInManager<User>> _signInManagerMock;
//     private readonly Mock<ILogger<LoginModel>> _loggerMock;
//     private readonly LoginModel _loginModel;

//     public LoginModelTests()
//     {
//         var userStoreMock = new Mock<IUserStore<User>>();
//         var userManagerMock = new Mock<UserManager<User>>(
//             userStoreMock.Object, default!, default!, default!, default!, default!, default!, default!, default!
//         );

//         _signInManagerMock = new Mock<SignInManager<User>>(
//             userManagerMock.Object,
//             new Mock<IHttpContextAccessor>().Object,
//             new Mock<IUserClaimsPrincipalFactory<User>>().Object,
//             default!, default!, default!, default!
//         );

//         _loggerMock = new Mock<ILogger<LoginModel>>();

//         _loginModel = new LoginModel(_signInManagerMock.Object, _loggerMock.Object)
//         {
//             Input = new LoginModel.InputModel
//             {
//                 Email = "test@example.com",
//                 Password = "Password123",
//                 RememberMe = false
//             }
//         };
//     }

//     [Fact]
//     public async Task OnPostAsync_Login_Successful_RedirectsToReturnUrl()
//     {
//         // Arrange
//         _signInManagerMock
//             .Setup(s => s.PasswordSignInAsync("test@example.com", "Password123", false, false))
//             .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

//         // Act
//         var result = await _loginModel.OnPostAsync("~/home");

//         // Assert
//         var redirectResult = Assert.IsType<LocalRedirectResult>(result);
//         Assert.Equal("~/home", redirectResult.Url);
//     }

//     [Fact]
//     public async Task OnPostAsync_Login_Fails_ReturnsPage()
//     {
//         // Arrange
//         _signInManagerMock
//             .Setup(s => s.PasswordSignInAsync("test@example.com", "WrongPassword", false, false))
//             .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

//         _loginModel.Input.Password = "WrongPassword";

//         // Act
//         var result = await _loginModel.OnPostAsync("~/home");

//         // Assert
//         Assert.IsType<PageResult>(result);
//         Assert.True(_loginModel.ModelState.ContainsKey(string.Empty));
//     }

//     [Fact]
//     public async Task OnPostAsync_LockedOut_ReturnsLockoutPage()
//     {
//         // Arrange
//         _signInManagerMock
//             .Setup(s => s.PasswordSignInAsync("test@example.com", "Password123", false, false))
//             .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.LockedOut);

//         // Act
//         var result = await _loginModel.OnPostAsync("~/home");

//         // Assert
//         var redirectResult = Assert.IsType<RedirectToPageResult>(result);
//         Assert.Equal("./Lockout", redirectResult.PageName);
//     }

//     [Fact]
//     public async Task OnPostAsync_RequiresTwoFactor_RedirectsTo2faPage()
//     {
//         // Arrange
//         _signInManagerMock
//             .Setup(s => s.PasswordSignInAsync("test@example.com", "Password123", false, false))
//             .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.TwoFactorRequired);

//         // Act
//         var result = await _loginModel.OnPostAsync("~/home");

//         // Assert
//         var redirectResult = Assert.IsType<RedirectToPageResult>(result);
//         Assert.Equal("./LoginWith2fa", redirectResult!.PageName);
//         Assert.Equal("~/home", redirectResult!.RouteValues!["ReturnUrl"]);
//         Assert.Equal(false, redirectResult!.RouteValues!["RememberMe"]);
//     }
// }
