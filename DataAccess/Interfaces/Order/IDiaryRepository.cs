using Core.Entity;
using DataAccess.GenericRepository.İnternface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IDiaryRepository : IRepository<DiaryEntry>
    {
        Task<IEnumerable<DiaryEntry>> GetDiaryEntriesByStudentIdAsync(int studentId, int? take = null);
    }
}
