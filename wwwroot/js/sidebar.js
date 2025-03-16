// function joinClass() {
//   var classCode = document.getElementById("classCodeInput").value.trim();
//   if (classCode === "") {
//     alert("Vui lòng nhập mã lớp!");
//     return;
//   }

//   $.ajax({
//     url: '@Url.Action("JoinFreeClass", "ClassRooms")',
//     type: "POST",
//     data: { id: classCode },
//     success: function (response) {
//       if (response.success) {
//         alert("Tham gia lớp thành công!");
//         // location.reload();
//         window.location.href =
//           '@Url.Action("Introduction", "ClassRooms")' + "?id=" + classCode;
//       } else {
//         alert("Không tìm thấy lớp với mã này!");
//       }
//     },
//     error: function () {
//       alert("Có lỗi xảy ra khi tham gia lớp.");
//     },
//   });
// }
