using JobBackend.Domain.Interfaces.Repositories;
using JobBackend.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobBackend.Infrastructure.Database.Repositories;

public class JobTrackerRepository : IJobTrackerRepository
{
    private readonly ApplicationDbContext _context;

    public JobTrackerRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SaveJobAsync(string searchId, string jobId)
    {
        var existing = await _context.ActiveJobs.FindAsync(searchId);
        if (existing != null)
        {
            existing.JobId = jobId;
            _context.ActiveJobs.Update(existing);
        }
        else
        {
            var activeJob = new ActiveJobEntity
            {
                SearchId = searchId,
                JobId = jobId
            };
            await _context.ActiveJobs.AddAsync(activeJob);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<string?> GetJobIdAsync(string searchId)
    {
        var job = await _context.ActiveJobs.FirstOrDefaultAsync(x => x.SearchId == searchId);
        return job?.JobId;
    }

    public async Task DeleteJobAsync(string searchId)
    {
        var job = await _context.ActiveJobs.FirstOrDefaultAsync(x => x.SearchId == searchId);
        if (job != null)
        {
            _context.ActiveJobs.Remove(job);
            await _context.SaveChangesAsync();
        }
    }
    public async Task<bool> IsJobCancelledAsync(string searchId)
    {
        var job = await _context.ActiveJobs.FirstOrDefaultAsync(x => x.SearchId == searchId);
        return job != null;
    }
}