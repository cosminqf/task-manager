using TaskManager.Models; 

namespace TaskManager.Services
{
    public interface IAiService
    {
        Task<string> GenerateProjectSummaryAsync(Project project);
    }
}