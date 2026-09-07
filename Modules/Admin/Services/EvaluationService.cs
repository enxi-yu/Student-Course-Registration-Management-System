using StudentCourse.Models;
using StudentCourse.Repositories;
namespace StudentCourse.Services
{
    public sealed class EvaluationService
    {
        private readonly EvaluationRepository _repository;
        public EvaluationService(EvaluationRepository repository) => _repository = repository;
        public IList<AdminEvaluationDto> Search(string? keyword, string? semester)
        { 
            AdminAuthService.RequireAdminSession();
            return _repository.Search(keyword, semester); 
        }
    }
}
