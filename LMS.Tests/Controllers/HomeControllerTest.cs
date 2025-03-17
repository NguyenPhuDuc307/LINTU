// using Moq;
// using Xunit;
// using Microsoft.AspNetCore.Mvc;
// using LMS.Controllers;
// using LMS.Data;
// using LMS.Data.Entities;  // Thêm dòng này để sử dụng các lớp từ LMS.Data.Entities
// using LMS.Models;
// using Microsoft.EntityFrameworkCore;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;

// namespace LMS.Tests
// {
//     public class HomeControllerTests
//     {
//         private readonly Mock<ILogger<HomeController>> _mockLogger;
//         private readonly ApplicationDbContext _mockContext; // Xóa Mock cho ApplicationDbContext
//         private readonly HomeController _controller;

//         public HomeControllerTests()
//         {
//             // Setup mock logger
//             _mockLogger = new Mock<ILogger<HomeController>>();

//             // Setup in-memory database context (đảm bảo có sử dụng đúng namespace)
//             var options = new DbContextOptionsBuilder<ApplicationDbContext>()
//                 .UseInMemoryDatabase("TestDatabase")
//                 .Options;

//             _mockContext = new ApplicationDbContext(options); // Sử dụng ApplicationDbContext thực sự

//             // Khởi tạo controller với các mock
//             _controller = new HomeController(_mockLogger.Object, _mockContext);
//         }

//         [Fact]
//         public async Task Index_ReturnsViewResult_WithListOfClassRooms()
//         {
//             // Arrange
//             var classRooms = new List<ClassRoom>
//             {
//                 new ClassRoom { Id = Guid.NewGuid(), Name = "Math 101", Price = 100, Students = 10 },
//                 new ClassRoom { Id = Guid.NewGuid(), Name = "Science 101", Price = 150, Students = 20 }
//             };
            
//             var classDetails = new List<ClassDetail>
//             {
//                 new ClassDetail { ClassRoomId = 1 },
//                 new ClassDetail { ClassRoomId = 2 },
//                 new ClassDetail { ClassRoomId = 2 }
//             };

//             // Thêm dữ liệu vào cơ sở dữ liệu InMemory
//             _mockContext.ClassRooms.AddRange(classRooms);
//             _mockContext.ClassDetails.AddRange(classDetails);
//             await _mockContext.SaveChangesAsync();

//             // Act
//             var result = await _controller.Index(null, null, null);

//             // Assert
//             var viewResult = Assert.IsType<ViewResult>(result);
//             var model = Assert.IsAssignableFrom<List<ClassRoom>>(viewResult.ViewData.Model);
//             Assert.Equal(2, model.Count); // Kiểm tra số lớp học trả về là 2
//         }

//         [Fact]
//         public async Task JoinClass_RedirectsToClassRoomIntroduction_WhenClassRoomExists()
//         {
//             // Arrange
//             var classRoom = new ClassRoom { Id = 1, Code = "ABC123" };
//             _mockContext.ClassRooms.Add(classRoom);
//             await _mockContext.SaveChangesAsync();

//             // Act
//             var result = await _controller.JoinClass("ABC123");

//             // Assert
//             var redirectResult = Assert.IsType<RedirectToActionResult>(result);
//             Assert.Equal("Introduction", redirectResult.ActionName);
//             Assert.Equal("ClassRooms", redirectResult.ControllerName);
//             Assert.Equal(1, redirectResult.RouteValues["id"]);
//         }

//         [Fact]
//         public async Task JoinClass_RedirectsToIndex_WhenClassRoomDoesNotExist()
//         {
//             // Act
//             var result = await _controller.JoinClass("NonExistentCode");

//             // Assert
//             var redirectResult = Assert.IsType<RedirectToActionResult>(result);
//             Assert.Equal("Index", redirectResult.ActionName);
//             Assert.Equal("Home", redirectResult.ControllerName);
//         }
//     }
// }
