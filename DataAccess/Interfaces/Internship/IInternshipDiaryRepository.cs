using Core.Entity;
using DataAccess.GenericRepository.Ýnternface;

namespace DataAccess.Interfaces.Internship
{
    public interface IInternshipDiaryRepository : IRepository<InternshipDiary>
    {
        Task<List<InternshipDiary>> GetDiariesByStudentIdAsync(int studentId);
        Task<InternshipDiary?> GetDiaryWithEntriesAsync(int diaryId);
        Task<List<InternshipDiary>> GetDiariesByUniversityAsync(int universityId);
        Task<bool> UpdateDiaryStatusAsync(int diaryId, bool isCompleted);
    }
}