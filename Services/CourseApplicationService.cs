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
            Normalize(input);
            Validate(input);

            if (_courseApplicationRepository.HasActiveCourseName(session.TeacherNo, input.CourseName))
            {
                throw new InvalidOperationException("你已经申请过同名课程，不能重复申请。");
            }

            return _courseApplicationRepository.Insert(session.TeacherNo, input);
        }

        public IList<CourseApplicationDto> GetMine()
        {
            UserSession session = TeacherService.RequireTeacherSession();
            return _courseApplicationRepository.GetByTeacher(session.TeacherNo);
        }

        private static void Normalize(CourseApplicationInput input)
        {
            if (input == null)
            {
                return;
            }

            input.CourseName = input.CourseName?.Trim();
            input.CourseType = input.CourseType?.Trim();
            input.Textbook = input.Textbook?.Trim();
            input.Department = FirstNotBlank(input.Department, input.TargetMajor);
            input.CourseSummary = FirstNotBlank(input.CourseSummary, input.Description);
        }

        private static string? FirstNotBlank(params string?[] values)
        {
            foreach (string? value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }

            return null;
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

            if (input.CourseName.Trim().Length > 100)
            {
                throw new InvalidOperationException("课程名称不能超过 100 个字符");
            }

            if (input.Credit <= 0m)
            {
                throw new InvalidOperationException("学分必须大于 0");
            }

            if (input.TotalHours <= 0)
            {
                throw new InvalidOperationException("总学时必须大于 0");
            }

            if (string.IsNullOrWhiteSpace(input.CourseType))
            {
                throw new InvalidOperationException("课程类型不能为空");
            }

            if (input.CourseType.Trim().Length > 20)
            {
                throw new InvalidOperationException("课程类型不能超过 20 个字符");
            }

            if (string.IsNullOrWhiteSpace(input.Department))
            {
                throw new InvalidOperationException("面向学院不能为空");
            }

            if (input.Department.Trim().Length > 20)
            {
                throw new InvalidOperationException("面向学院不能超过 20 个字符");
            }

            if (!string.IsNullOrWhiteSpace(input.Textbook) && input.Textbook.Trim().Length > 200)
            {
                throw new InvalidOperationException("参考教材不能超过 200 个字符");
            }
        }
    }
}