using System;
using System.Collections.Generic;
using StudentCourse.Models;
using StudentCourse.Repositories;

namespace StudentCourse.Services
{
    public sealed class CourseApplicationService
    {
        private readonly CourseApplicationRepository _courseApplicationRepository;

        public CourseApplicationService()
            : this(new CourseApplicationRepository())
        {
        }

        public CourseApplicationService(CourseApplicationRepository courseApplicationRepository)
        {
            _courseApplicationRepository = courseApplicationRepository;
        }

        public CourseApplicationDto Submit(CourseApplicationInput input)
        {
            UserSession session = TeacherService.RequireTeacherSession();
            Validate(input);
            return _courseApplicationRepository.Insert(session.TeacherNo, input);
        }

        public IList<CourseApplicationDto> GetMine()
        {
            UserSession session = TeacherService.RequireTeacherSession();
            return _courseApplicationRepository.GetByTeacher(session.TeacherNo);
        }

        private static void Validate(CourseApplicationInput input)
        {
            if (input == null)
            {
                throw new InvalidOperationException("开课申请不能为空");
            }

            if (string.IsNullOrWhiteSpace(input.CourseName))
            {
                throw new InvalidOperationException("课程名称不能为空");
            }

            if (input.Credit <= 0m)
            {
                throw new InvalidOperationException("学分必须大于 0");
            }

            if (input.TotalHours <= 0)
            {
                throw new InvalidOperationException("学时必须大于 0");
            }
        }
    }
}
