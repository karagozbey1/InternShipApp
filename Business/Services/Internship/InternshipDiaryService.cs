using Business.Interfaces.Internship;
using Core.Entity;
using DataAccess.Interfaces.Internship;
using DataAccess.Unit.Interface;

namespace Business.Services.Internship
{
    public class InternshipDiaryService : IInternshipDiaryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public InternshipDiaryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<InternshipDiary> CreateDiaryAsync(InternshipDiary diary)
        {
            if (diary == null)
                throw new ArgumentNullException(nameof(diary));

            if (string.IsNullOrWhiteSpace(diary.Title))
                throw new ArgumentException("Defter baþlýðý boþ olamaz.");

            // PostgreSQL için UTC DateTime zorunlu
            var utcNow = DateTime.UtcNow;
            diary.CreatedAt = utcNow;
            diary.UpdatedAt = utcNow;

            // Nullable DateTime alanlarýný UTC'ye zorla
            if (diary.InternshipStartDate.HasValue && diary.InternshipStartDate.Value.Kind == DateTimeKind.Unspecified)
            {
                diary.InternshipStartDate = DateTime.SpecifyKind(diary.InternshipStartDate.Value, DateTimeKind.Utc);
            }

            if (diary.InternshipEndDate.HasValue && diary.InternshipEndDate.Value.Kind == DateTimeKind.Unspecified)
            {
                diary.InternshipEndDate = DateTime.SpecifyKind(diary.InternshipEndDate.Value, DateTimeKind.Utc);
            }

            await _unitOfWork.InternshipDiaries.AddAsync(diary);
            await _unitOfWork.SaveAsync();

            return diary;
        }

        public async Task<List<InternshipDiary>> GetDiariesByStudentIdAsync(int studentId)
        {
            return await _unitOfWork.InternshipDiaries.GetDiariesByStudentIdAsync(studentId);
        }

        public async Task<InternshipDiary?> GetDiaryWithEntriesAsync(int diaryId)
        {
            return await _unitOfWork.InternshipDiaries.GetDiaryWithEntriesAsync(diaryId);
        }

        public async Task<List<InternshipDiary>> GetDiariesByUniversityAsync(int universityId)
        {
            return await _unitOfWork.InternshipDiaries.GetDiariesByUniversityAsync(universityId);
        }

        public async Task<bool> UpdateDiaryAsync(InternshipDiary diary)
        {
            if (diary == null)
                throw new ArgumentNullException(nameof(diary));

            diary.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.InternshipDiaries.Update(diary);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<bool> DeleteDiaryAsync(int diaryId)
        {
            var diary = await _unitOfWork.InternshipDiaries.GetByIdAsync(diaryId);
            if (diary == null) return false;

            _unitOfWork.InternshipDiaries.Remove(diary);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<bool> UpdateDiaryStatusAsync(int diaryId, bool isCompleted)
        {
            return await _unitOfWork.InternshipDiaries.UpdateDiaryStatusAsync(diaryId, isCompleted);
        }

        public async Task<DiaryEntry> AddEntryToDiaryAsync(DiaryEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            if (string.IsNullOrWhiteSpace(entry.Title))
                throw new ArgumentException("Girdi baþlýðý boþ olamaz.");

            if (string.IsNullOrWhiteSpace(entry.Content))
                throw new ArgumentException("Girdi içeriði boþ olamaz.");

            // Verify diary exists and belongs to student
            var diary = await _unitOfWork.InternshipDiaries.GetByIdAsync(entry.DiaryId);
            if (diary == null)
                throw new InvalidOperationException("Belirtilen defter bulunamadý.");

            if (diary.StudentId != entry.StudentId)
                throw new InvalidOperationException("Bu deftere girdi ekleme yetkiniz yok.");

            // PostgreSQL için UTC DateTime zorunlu
            var utcNow = DateTime.UtcNow;
            entry.CreatedAt = utcNow;
            
            // Date alanýný UTC'ye zorla
            if (entry.Date.Kind == DateTimeKind.Unspecified)
            {
                entry.Date = DateTime.SpecifyKind(entry.Date, DateTimeKind.Utc);
            }

            await _unitOfWork.DiaryEntries.AddAsync(entry);
            await _unitOfWork.SaveAsync();

            return entry;
        }

        public async Task<bool> UpdateEntryAsync(DiaryEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            var existingEntry = await _unitOfWork.DiaryEntries.GetByIdAsync(entry.Id);
            if (existingEntry == null) return false;

            // Verify ownership
            if (existingEntry.StudentId != entry.StudentId)
                throw new InvalidOperationException("Bu girdiyi güncelleme yetkiniz yok.");

            // DateTime'ý UTC'ye zorla
            if (entry.Date.Kind == DateTimeKind.Unspecified)
            {
                entry.Date = DateTime.SpecifyKind(entry.Date, DateTimeKind.Utc);
            }

            _unitOfWork.DiaryEntries.Update(entry);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<bool> DeleteEntryAsync(int entryId)
        {
            var entry = await _unitOfWork.DiaryEntries.GetByIdAsync(entryId);
            if (entry == null) return false;

            _unitOfWork.DiaryEntries.Remove(entry);
            await _unitOfWork.SaveAsync();

            return true;
        }
    }
}