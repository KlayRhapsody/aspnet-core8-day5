﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable enable

namespace WebApi.Models
{
    public partial class GetMyDepartCoursesResult
    {
        public int CourseId { get; set; }
        [StringLength(50)]
        public string? Title { get; set; }
        public int Credits { get; set; }
        [StringLength(50)]
        public string? DepartmentName { get; set; }
    }
}
