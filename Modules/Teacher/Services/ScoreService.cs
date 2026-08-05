using System;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using StudentCourse.Infrastructure;
using StudentCourse.Models;
using StudentCourse.Repositories;

namespace StudentCourse.Services
{
    public sealed class ScoreService
    {
        private readonly TeacherStudentService _teacherStudentService;
        private readonly TeachingClassRepository _teachingClassRepository;
        private readonly ScoreRepository _scoreRepository;

        public ScoreService()
            : this(new TeacherStudentService(), new TeachingClassRepository(), new ScoreRepository())
        {
        }

        public ScoreService(
            TeacherStudentService teacherStudentService,
            TeachingClassRepository teachingClassRepository,
            ScoreRepository scoreRepository)
        {
            _teacherStudentService = teacherStudentService;
            _teachingClassRepository = teachingClassRepository;
            _scoreRepository = scoreRepository;
        }

        public IList<ScoreDto> GetScoreSheet(int classId)
        {
            string teacherNo = _teacherStudentService.RequireClassAccess(classId);
            return _scoreRepository.GetScoreSheet(teacherNo, classId);
        }

        public ScoreDto SaveScore(ScoreSaveRequest request)
        {
            ValidateScoreRequest(request);
            string teacherNo = _teacherStudentService.RequireClassAccess(request.ClassId);
            decimal credit = _teachingClassRepository.GetCourseCredit(teacherNo, request.ClassId);
            ScoreCalculation calculation = Calculate(request.TotalScore, credit);

            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            using (OracleTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    if (!_scoreRepository.StudentSelectedClass(connection, transaction, request.ClassId, request.StudentNo))
                    {
                        throw new InvalidOperationException("该学生未选择此教学班");
                    }

                    ScoreDto? existing = _scoreRepository.GetScore(connection, transaction, request.ClassId, request.StudentNo);
                    if (existing != null && existing.TotalScore.HasValue && string.IsNullOrWhiteSpace(request.UpdateRemark))
                    {
                        throw new InvalidOperationException("修改已有成绩时必须填写修改备注");
                    }

                    ScoreDto saved = _scoreRepository.SaveScore(
                        connection,
                        transaction,
                        request.ClassId,
                        request.StudentNo.Trim(),
                        request.TotalScore,
                        calculation.GradeLevel,
                        calculation.Gpa,
                        calculation.CreditObtained,
                        request.UpdateRemark,
                        existing != null && existing.TotalScore.HasValue);

                    transaction.Commit();
                    return saved;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public object BatchSaveScores(int classId, IList<ScoreSaveRequest> rows)
        {
            if (classId <= 0)
            {
                throw new InvalidOperationException("教学班编号不正确");
            }

            if (rows == null || rows.Count == 0)
            {
                throw new InvalidOperationException("没有需要保存的成绩");
            }

            string teacherNo = _teacherStudentService.RequireClassAccess(classId);
            decimal credit = _teachingClassRepository.GetCourseCredit(teacherNo, classId);

            using (OracleConnection connection = DbConnectionFactory.OpenConnection())
            using (OracleTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    int savedCount = 0;

                    foreach (ScoreSaveRequest row in rows)
                    {
                        row.ClassId = classId;
                        ValidateScoreRequest(row);

                        if (!_scoreRepository.StudentSelectedClass(connection, transaction, row.ClassId, row.StudentNo))
                        {
                            throw new InvalidOperationException("学生 " + row.StudentNo + " 未选择此教学班");
                        }

                        ScoreDto? existing = _scoreRepository.GetScore(connection, transaction, row.ClassId, row.StudentNo);
                        if (existing != null && existing.TotalScore.HasValue && string.IsNullOrWhiteSpace(row.UpdateRemark))
                        {
                            throw new InvalidOperationException("修改学生 " + row.StudentNo + " 的已有成绩时必须填写修改备注");
                        }

                        ScoreCalculation calculation = Calculate(row.TotalScore, credit);
                        _scoreRepository.SaveScore(
                            connection,
                            transaction,
                            row.ClassId,
                            row.StudentNo.Trim(),
                            row.TotalScore,
                            calculation.GradeLevel,
                            calculation.Gpa,
                            calculation.CreditObtained,
                            row.UpdateRemark,
                            existing != null && existing.TotalScore.HasValue);

                        savedCount++;
                    }

                    transaction.Commit();
                    return new { SavedCount = savedCount };
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public ScoreCalculation Calculate(decimal totalScore, decimal credit)
        {
            string gradeLevel = CalculateGradeLevel(totalScore);
            if (gradeLevel == "A")
            {
                return new ScoreCalculation(gradeLevel, 4.0m, credit);
            }

            if (gradeLevel == "B")
            {
                return new ScoreCalculation(gradeLevel, 3.0m, credit);
            }

            if (gradeLevel == "C")
            {
                return new ScoreCalculation(gradeLevel, 2.0m, credit);
            }

            if (gradeLevel == "D")
            {
                return new ScoreCalculation(gradeLevel, 1.0m, credit);
            }

            return new ScoreCalculation(gradeLevel, 0m, 0m);
        }

        public string CalculateGradeLevel(decimal totalScore)
        {
            if (totalScore >= 90m)
            {
                return "A";
            }

            if (totalScore >= 80m)
            {
                return "B";
            }

            if (totalScore >= 70m)
            {
                return "C";
            }

            if (totalScore >= 60m)
            {
                return "D";
            }

            return "F";
        }

        private static void ValidateScoreRequest(ScoreSaveRequest request)
        {
            if (request == null)
            {
                throw new InvalidOperationException("成绩请求不能为空");
            }

            if (request.ClassId <= 0)
            {
                throw new InvalidOperationException("教学班编号不正确");
            }

            if (string.IsNullOrWhiteSpace(request.StudentNo))
            {
                throw new InvalidOperationException("学号不能为空");
            }

            if (request.TotalScore < 0m || request.TotalScore > 100m)
            {
                throw new InvalidOperationException("总评成绩必须在 0 到 100 之间");
            }
        }
    }

    public sealed class ScoreCalculation
    {
        public ScoreCalculation(string gradeLevel, decimal gpa, decimal creditObtained)
        {
            GradeLevel = gradeLevel;
            Gpa = gpa;
            CreditObtained = creditObtained;
        }

        public string GradeLevel { get; private set; }

        public decimal Gpa { get; private set; }

        public decimal CreditObtained { get; private set; }
    }
}
