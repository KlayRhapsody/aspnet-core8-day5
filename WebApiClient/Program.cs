

using WebApiClient;

var http = new HttpClient();

var client = new WebApi(http);

// // var course = await client.取得指定課程Async(1);

// // Console.WriteLine(course.Title);

var courses = await client.GetCoursesV2Async(1, 10, "2.0", "2.0");

Console.WriteLine($"TotalPages: {courses.TotalPages}, TotalRecords: {courses.TotalRecords}");
foreach (var c in courses.Data)
{
    Console.WriteLine($"CourseId: {c.CourseId}, Title: {c.Title}, Credits: {c.Credits}");
}

