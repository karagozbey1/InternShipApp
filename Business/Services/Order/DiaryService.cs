using Business.Interfaces;
using Core.Entity;
using DataAccess.Unit.Interface;


namespace Business.Services
{
    public class DiaryService : IDiaryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DiaryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<DiaryEntry>> GetDiaryEntriesByStudentIdAsync(int studentId, int? take = null)
        {
            return await _unitOfWork.DiaryRepository.GetDiaryEntriesByStudentIdAsync(studentId, take);
        }

        public async Task<DiaryEntry?> GetByIdAsync(int id)
        {
            return await _unitOfWork.DiaryRepository.GetByIdAsync(id);
        }

        public async Task AddAsync(DiaryEntry diaryEntry)
        {
            await _unitOfWork.DiaryRepository.AddAsync(diaryEntry);
            await _unitOfWork.DiaryRepository.SaveChangesAsync();
        }

        public async Task UpdateAsync(DiaryEntry diaryEntry)
        {
            _unitOfWork.DiaryRepository.Update(diaryEntry);
            await _unitOfWork.DiaryRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entry = await _unitOfWork.DiaryRepository.GetByIdAsync(id);
            if (entry != null)
            {
                _unitOfWork.DiaryRepository.Remove(entry);
                await _unitOfWork.DiaryRepository.SaveChangesAsync();
            }
        }
    }
}
