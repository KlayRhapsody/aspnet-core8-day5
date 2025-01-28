namespace WebApi.Models.Dtos;

public class PageCourse
{
    public int TotalPages { get; set; }
    public int TotalRecords { get; set; }
    public IEnumerable<CourseRead>? Data { get; set; }
}