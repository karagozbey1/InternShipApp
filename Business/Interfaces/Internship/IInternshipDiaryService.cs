using Core.Entity;

namespace Business.Interfaces.Internship
{
    public interface IInternshipDiaryService
    {
        Task<InternshipDiary> CreateDiaryAsync(InternshipDiary diary);
        Task<List<InternshipDiary>> GetDiariesByStudentIdAsync(int studentId);
        Task<InternshipDiary?> GetDiaryWithEntriesAsync(int diaryId);
        Task<List<InternshipDiary>> GetDiariesByUniversityAsync(int universityId);
        Task<bool> UpdateDiaryAsync(InternshipDiary diary);
        Task<bool> DeleteDiaryAsync(int diaryId);
        Task<bool> UpdateDiaryStatusAsync(int diaryId, bool isCompleted);
        Task<DiaryEntry> AddEntryToDiaryAsync(DiaryEntry entry);
        Task<bool> UpdateEntryAsync(DiaryEntry entry);
        Task<bool> DeleteEntryAsync(int entryId);
    }
}