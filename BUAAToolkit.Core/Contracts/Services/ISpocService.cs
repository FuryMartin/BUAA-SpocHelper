﻿using BUAAToolkit.Core.Models;

namespace BUAAToolkit.Core.Contracts.Services;
public interface ISpocService
{
    Task<IEnumerable<Course>> GetCourseListAsync();
}
