using TestManagementApplication.Models.Entities;
using TestManagementApplication.Repositories.Generic;

namespace TestManagementApplication.Repositories.Interfaces
{
    public interface IVideoRecordingRepository : IRepository<VideoRecording>
    {
        Task<IEnumerable<VideoRecording>> GetBySessionIdAsync(Guid sessionId);
    }
}
