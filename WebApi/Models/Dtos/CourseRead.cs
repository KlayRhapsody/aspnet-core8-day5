namespace WebApi.Models.Dtos;

public class CourseRead
{
    public int CourseId { get; set; }

    public required string Title { get; set; }

    public int Credits { get; set; }
}
