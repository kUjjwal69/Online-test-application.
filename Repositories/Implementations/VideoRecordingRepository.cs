using Microsoft.EntityFrameworkCore;
using TestManagementApplication.Data;
using TestManagementApplication.Models.Entities;
using TestManagementApplication.Repositories.Generic;
using TestManagementApplication.Repositories.Interfaces;

namespace TestManagementApplication.Repositories.Implementations
{
    public class VideoRecordingRepository : Repository<VideoRecording>, IVideoRecordingRepository
    {
        public VideoRecordingRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<VideoRecording?> GetByIdAsync(Guid id)
            => await _context.VideoRecordings.FindAsync(id);

        public async Task<IEnumerable<VideoRecording>> GetBySessionIdAsync(Guid sessionId)
            => await _context.VideoRecordings
                .Where(v => v.SessionId == sessionId)
                .OrderBy(v => v.StartTime)
                .ToListAsync();

        public async Task<VideoRecording> CreateAsync(VideoRecording recording)
        {
            _context.VideoRecordings.Add(recording);
            await _context.SaveChangesAsync();
            return recording;
        }

        public async Task UpdateAsync(VideoRecording recording)
        {
            _context.VideoRecordings.Update(recording);
            await _context.SaveChangesAsync();
        }
    }
}
