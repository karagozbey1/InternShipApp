using Core.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Interfaces
{
    public interface IDiaryService
    {
        Task<IEnumerable<DiaryEntry>> GetDiaryEntriesByStudentIdAsync(int studentId, int? take = null);
        Task<DiaryEntry?> GetByIdAsync(int id);
        Task AddAsync(DiaryEntry diaryEntry);
        Task UpdateAsync(DiaryEntry diaryEntry);
        Task DeleteAsync(int id);
    }
}
