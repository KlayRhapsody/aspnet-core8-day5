﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using WebApi.Models;

namespace WebApi.Models
{
    public partial interface IContosoUniversityContextProcedures
    {
        Task<List<GetMyDepartCoursesResult>> GetMyDepartCoursesAsync(string query, OutputParameter<int> returnValue = null, CancellationToken cancellationToken = default);
    }
}
