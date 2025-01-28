using Microsoft.AspNetCore.Http;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CoursesController : ControllerBase
{
    private readonly ContosoUniversityContext _context;

    public CoursesController(ContosoUniversityContext context)
    {
        _context = context;
    }

    // GET: api/Courses
    [HttpGet(Name = "取得課程Async")]
    [ProducesResponseType<PageCourse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Course>>> GetCourses(
        [Range(1, int.MaxValue, ErrorMessage = "pageIndex 不能小於 1")] 
        int pageIndex = 1, 
        int pageSize = 10)
    {
        var courses = _context.Courses.OrderBy(c => c.CourseId).AsQueryable();

        var totalRecords = await courses.CountAsync();

        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var pagedCourses = await courses
            .Select(c => new CourseRead
            {
                CourseId = c.CourseId,
                Credits = c.Credits,
                Title = c.Title!
            })
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize).ToListAsync();
        
        return Ok(new PageCourse
        {
            TotalPages = totalPages,
            TotalRecords = totalRecords,
            Data = pagedCourses
        });
    }

    // GET: api/Courses/5
    [HttpGet("{id}", Name = "取得指定課程Async")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<ActionResult<CourseRead>> GetCourse(int id)
    {
        var course = await _context.Courses.FindAsync(id);

        if (course == null)
        {
            return NotFound();
        }

        return new CourseRead
        {
            CourseId = course.CourseId,
            Credits = course.Credits,
            Title = course.Title!
        };
    }

    // GET: api/Courses/5/Depart
    [HttpGet("{id}/Depart", Name = "GetCourseWithDepartment")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<ActionResult<Course>> GetCourseWithDepartment(int id)
    {
        var course = await _context.Courses.FindAsync(id);

        if (course == null)
        {
            return NotFound();
        }

        _context.Entry(course)
            .Reference(c => c.Department)
            .Load();

        return Ok(new
        {
            course.CourseId,
            course.Title,
            course.Credits,
            DepartmentName = course.Department.Name
        });
    }

    // PUT: api/Courses/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> PutCourse(int id, CourseUpdate course)
    {
        if (id != course.CourseId)
        {
            return BadRequest();
        }

        // 不好的寫法
        // _context.Attach(course);
        // _context.Entry(course).State = EntityState.Modified;
        // 不好的寫法
        // _context.Update(course);
        // 不好的寫法
        // _context.Entry(course).State = EntityState.Modified;

        var courseToUpdate = await _context.Courses.FindAsync(id);

        if (courseToUpdate == null)
        {
            return NotFound();
        }

        courseToUpdate.Credits = course.Credits;
        courseToUpdate.Title = course.Title;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CourseExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/Courses
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesDefaultResponseType]
    public async Task<ActionResult<Course>> PostCourse(CourseCreate courseToCreate)
    {
        var course = new Course
        {
            Credits = courseToCreate.Credits,
            Title = courseToCreate.Title,
        };

        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetCourse", new { id = course.CourseId }, new Course
        {
            CourseId = course.CourseId,
            Credits = course.Credits,
            Title = course.Title,
            DepartmentId = course.DepartmentId
        });
    }

    // DELETE: api/Courses/5
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null)
        {
            return NotFound();
        }

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("BatchUpdateCredits", Name = "PostBatchUpdateCredits")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> PostBatchUpdateCredits()
    {
        await _context.Courses.ExecuteUpdateAsync(setter => 
            setter.SetProperty(c => c.Credits, c => c.Credits + 1));

        return NoContent();
    }

    private bool CourseExists(int id)
    {
        return _context.Courses.Any(e => e.CourseId == id);
    }
}
