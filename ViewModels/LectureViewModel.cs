using LMS.Data.Entities;

namespace LMS.ViewModels;

public class ResourceViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public class LectureViewModel
{
    // Lecture hiện tại
    public Lecture Lecture { get; set; } = null!;

    // Lesson chứa lecture hiện tại
    public Lesson Lesson { get; set; } = null!;

    // Thông tin về Classroom
    public ClassRoom ClassRoom { get; set; } = null!;

    // Danh sách tất cả các lesson trong khóa học
    public List<Lesson> Lessons { get; set; } = new List<Lesson>();

    // Danh sách ID của các lecture đã hoàn thành
    public List<int> CompletedLectures { get; set; } = new List<int>();

    // Lecture trước và lecture sau (để điều hướng)
    public Lecture? PreviousLecture { get; set; }
    public Lecture? NextLecture { get; set; }

    // Tài nguyên bổ sung cho lecture
    public List<ResourceViewModel> Resources { get; set; } = new List<ResourceViewModel>();

    // Tiến độ hoàn thành khóa học (phần trăm)
    public int CourseProgress { get; set; }
}