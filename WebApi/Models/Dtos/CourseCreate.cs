namespace WebApi.Models.Dtos;

public class CourseCreate
{
    public int CourseId { get; set; }

    public required string Title { get; set; }

    public int Credits { get; set; }
}
