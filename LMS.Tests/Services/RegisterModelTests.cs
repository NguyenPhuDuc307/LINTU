// using LMS.Areas.Identity.Pages.Account;
// using LMS.Data.Entities;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Identity;
// using Microsoft.AspNetCore.Identity.UI.Services;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Mvc.RazorPages;
// using Microsoft.Extensions.Logging;
// using Moq;
// using System.Collections.Generic;
// using System.Threading;
// using System.Threading.Tasks;
// using Xunit;

// public class RegisterModelTests
// {
//     private readonly Mock<UserManager<User>> _userManagerMock;
//     private readonly Mock<SignInManager<User>> _signInManagerMock;
//     private readonly Mock<ILogger<RegisterModel>> _loggerMock;
//     private readonly Mock<IEmailSender> _emailSenderMock;
//     private readonly Mock<IUserEmailStore<User>> _userStoreMock;
//     private readonly RegisterModel _registerModel;

//     public RegisterModelTests()
//     {
//         _userStoreMock = new Mock<IUserEmailStore<User>>();

//         _userManagerMock = new Mock<UserManager<User>>(
//             _userStoreMock.As<IUserStore<User>>().Object, 
//             null, null, null, null, null, null, null, null
//         );

//         _signInManagerMock = new Mock<SignInManager<User>>(
//             _userManagerMock.Object,
//             new Mock<IHttpContextAccessor>().Object,
//             new Mock<IUserClaimsPrincipalFactory<User>>().Object,
//             null, null, null, null
//         );

//         _loggerMock = new Mock<ILogger<RegisterModel>>();
//         _emailSenderMock = new Mock<IEmailSender>();

//         // Mock các phương thức để tránh lỗi null
//         _userManagerMock
//             .Setup(u => u.SupportsUserEmail)
//             .Returns(true);

//         _userStoreMock
//             .Setup(x => x.SetEmailAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
//             .Returns(Task.CompletedTask);

//         _userStoreMock
//             .Setup(x => x.SetUserNameAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
//             .Returns(Task.CompletedTask);

//         _userManagerMock
//             .Setup(u => u.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()))
//             .ReturnsAsync("test-token");

//         _registerModel = new RegisterModel(
//             _userManagerMock.Object,
//             _userStoreMock.Object,
//             _signInManagerMock.Object,
//             _loggerMock.Object,
//             _emailSenderMock.Object
//         )
//         {
//             Input = new RegisterModel.InputModel
//             {
//                 Email = "test@example.com",
//                 Password = "Password123",
//                 ConfirmPassword = "Password123"
//             }
//         };
//     }

//     [Fact]
//     public async Task OnPostAsync_Register_Successful_RedirectsToReturnUrl()
//     {
//         // Arrange
//         _userManagerMock
//             .Setup(u => u.CreateAsync(It.IsAny<User>(), "Password123"))
//             .ReturnsAsync(IdentityResult.Success);

//         _signInManagerMock
//             .Setup(s => s.SignInAsync(It.IsAny<User>(), false, null))
//             .Returns(Task.CompletedTask);

//         // Act
//         var result = await _registerModel.OnPostAsync("~/home");

//         // Assert
//         var redirectResult = Assert.IsType<LocalRedirectResult>(result);
//         Assert.Equal("~/home", redirectResult.Url);
//     }

//     [Fact]
//     public async Task OnPostAsync_Register_Fails_ReturnsPageWithErrors()
//     {
//         // Arrange
//         _userManagerMock
//             .Setup(u => u.CreateAsync(It.IsAny<User>(), "Password123"))
//             .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Email is already taken" }));

//         // Act
//         var result = await _registerModel.OnPostAsync("~/home");

//         // Assert
//         Assert.IsType<PageResult>(result);
//         Assert.Contains(_registerModel.ModelState.Values, v => v.Errors.Count > 0);
//     }

//     [Fact]
//     public async Task OnPostAsync_ModelStateInvalid_ReturnsPage()
//     {
//         // Arrange
//         _registerModel.ModelState.AddModelError("Email", "Email is required");

//         // Act
//         var result = await _registerModel.OnPostAsync("~/home");

//         // Assert
//         Assert.IsType<PageResult>(result);
//         Assert.True(_registerModel.ModelState.ContainsKey("Email"));
//     }
// }
