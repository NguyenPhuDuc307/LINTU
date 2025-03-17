// using FluentAssertions;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Logging;
// using Moq;
// using LMS.Controllers;
// using Xunit;
// using LMS.Services;
// using LMS.Data.Entities;
// using LMS.ViewModels;
// using System.Security.Claims;
// using Microsoft.AspNetCore.Http;

// namespace LMS.Tests.Controllers
// {
//     public class PostsControllerTests
//     {
//         // private readonly Mock<IPostService> _postServiceMock = new Mock<IPostService>();
//         private readonly Mock<ILogger<PostsController>> _loggerMock = new Mock<ILogger<PostsController>>();
//         // private readonly PostsController _controller = new PostsController(_postServiceMock.Object);
//         private readonly Mock<IPostService> _postServiceMock;
//         private readonly PostsController _controller;
//         private readonly Guid _testClassRoomId;
//         public PostsControllerTests()
//         {
//             _postServiceMock = new Mock<IPostService>();
//             _controller = new PostsController(_postServiceMock.Object);
//             _testClassRoomId = Guid.NewGuid(); 
//             SetUpTestData(); 
//         }
//         private void SetUpTestData()
//         {
//             var testClassRoom = new ClassRoom
//             {
//                 Id = _testClassRoomId,
//                 Name = "Test ClassRoom"
//             };
//             _postServiceMock!.Setup(s => s.GetPostByIdAsync(It.IsAny<int>())).ReturnsAsync((Post?)null);
//         }
//         [Fact]
//         public async Task Index_ShouldReturnViewWithPosts()
//         {
//             // Arrange
//             var mockPosts = new List<Post>
//             {
//                 new Post { Id = 1, Title = "Post 1", Message = "Content 1" },
//                 new Post { Id = 2, Title = "Post 2", Message = "Content 2" }
//             };
//             _postServiceMock.Setup(s => s.GetAllPostsAsync()).ReturnsAsync(mockPosts);

//             // Act
//             var result = await _controller.Index();

//             // Assert
//             var viewResult = result.Should().BeOfType<ViewResult>().Subject;
//             viewResult.Model.Should().BeAssignableTo<IEnumerable<Post>>().Which.Should().HaveCount(2);
//         }

//         [Fact]
//         public async Task Create_ShouldReturnBadRequest_WhenInvalidModel()
//         {
//             // Arrange
//             _controller.ModelState.AddModelError("Title", "Required");

//             // Act
//             var result = await _controller.Create(new PostCreateRequest());

//             // Assert
//             result.Should().BeOfType<ViewResult>();
//         }
//         [Fact]
//         public async Task Create_ShouldReturnRedirectToDetails_WhenSuccess()
//         {
//             // Arrange
//             var request = new PostCreateRequest
//             { 
//                 ClassRoomId = _testClassRoomId.ToString(),
//                 Title = "New Post",
//                 Message = "Content" 
//             };
//             var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
//             {
//                 new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
//             }, "mock"));

//             _controller.ControllerContext = new ControllerContext
//             {
//                 HttpContext = new DefaultHttpContext { User = user }
//             };
//             _postServiceMock.Setup(s => s.CreatePostAsync(It.IsAny<PostCreateRequest>(), It.IsAny<string>())).ReturnsAsync(true);

//             // Act
//             var result = await _controller.Create(request);

//             // Assert
//             result.Should().BeOfType<RedirectToActionResult>()
//                 .Which.ActionName.Should().Be("Details");
//         }

//         [Fact]
//         public async Task Edit_ShouldReturnNotFound_WhenPostDoesNotExist()
//         {
//             // Arrange
//             _postServiceMock.Setup(s => s.GetPostByIdAsync(It.IsAny<int>())).ReturnsAsync((Post?)null);

//             // Act
//             var result = await _controller.Edit(999);

//             // Assert
//             result.Should().BeOfType<NotFoundResult>();
//         }

//         [Fact]
//         public async Task Edit_ShouldReturnView_WhenPostExists()
//         {
//             // Arrange
//             var post = new Post { Id = 1, Title = "Test Post", Message = "Content" };
//             _postServiceMock.Setup(s => s.GetPostByIdAsync(1)).ReturnsAsync(post);

//             // Act
//             var result = await _controller.Edit(1);

//             // Assert
//             var viewResult = result.Should().BeOfType<ViewResult>().Subject;
//             viewResult.Model.Should().BeAssignableTo<Post>().Which.Id.Should().Be(1);
//         }

//         [Fact]
//         public async Task Delete_ShouldReturnNotFound_WhenPostDoesNotExist()
//         {
//             // Arrange
//             _postServiceMock.Setup(s => s.GetPostByIdAsync(It.IsAny<int>())).ReturnsAsync((Post?)null);

//             // Act
//             var result = await _controller.Delete(999);

//             // Assert
//             result.Should().BeOfType<NotFoundResult>();
//         }

//         [Fact]
//         public async Task Delete_ShouldReturnView_WhenPostExists()
//         {
//             // Arrange
//             var post = new Post { Id = 1, Title = "Test Post", Message = "Content" }; 
//             _postServiceMock.Setup(s => s.GetPostByIdAsync(1)).ReturnsAsync(post);

//             // Act
//             var result = await _controller.Delete(1);

//             // Assert
//             var viewResult = result.Should().BeOfType<ViewResult>().Subject;
//             viewResult.Model.Should().BeAssignableTo<Post>().Which.Id.Should().Be(1);
//         }

//         [Fact]
//         public async Task DeleteConfirmed_ShouldReturnJsonSuccess_WhenPostExists()
//         {
//             // Arrange
//             _postServiceMock.Setup(s => s.DeletePostAsync(It.IsAny<int>())).ReturnsAsync(true);

//             // Act
//             var result = await _controller.DeleteConfirmed(1);

//             // Assert
//             var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
//             jsonResult.Value.Should().NotBeNull("DeleteConfirmed should return valid JSON");

//             dynamic dynamicResponse = jsonResult.Value;
//             ((bool)dynamicResponse.success).Should().BeTrue();
//             ((string)dynamicResponse.message).Should().Be("Xóa bài viết thành công!");
//         }

//         [Fact]
//         public async Task DeleteConfirmed_ShouldReturnJsonFailure_WhenPostDoesNotExist()
//         {
//             // Arrange
//             _postServiceMock.Setup(s => s.DeletePostAsync(It.IsAny<int>())).ReturnsAsync(false);

//             // Act
//             var result = await _controller.DeleteConfirmed(999);

//             // Assert
//             var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
//             jsonResult.Value.Should().NotBeNull("DeleteConfirmed should return valid JSON even when post does not exist");

//             dynamic dynamicResponse = jsonResult.Value;
//             ((bool)dynamicResponse.success).Should().BeFalse();
//             ((string)dynamicResponse.message).Should().Be("Bài viết không tồn tại.");
//         }
//     }
// }