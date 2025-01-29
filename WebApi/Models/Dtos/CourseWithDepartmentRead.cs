namespace WebApi.Models.Dtos;

public class CourseWithDepartmentRead
{
    public int CourseId { get; set; }

    public required string Title { get; set; }

    public int Credits { get; set; }

    public required string DepartmentName { get; set; }
}
