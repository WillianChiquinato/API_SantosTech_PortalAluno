using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace API_PortalSantosTech.Repository;

public class ClassRepository : IClassRepository
{
    private readonly AppDbContext _efDbContext;

    public ClassRepository(AppDbContext efDbContext)
    {
        _efDbContext = efDbContext;
    }

    public async Task<List<Class>> GetAllAsync()
    {
        return await _efDbContext.Classes.AsNoTracking().ToListAsync();
    }

    public Task<Class?> GetByCourseAndModuleIdAsync(int courseId, int moduleId)
    {
        return _efDbContext.Classes.AsNoTracking().FirstOrDefaultAsync(x => x.CourseId == courseId && x.CurrentModuleId == moduleId);
    }

    public async Task<Class?> GetByIdAsync(int id)
    {
        return await _efDbContext.Classes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<ClassRoom>> GetClassRoomsByClassIdAsync(int classId)
    {
        return await _efDbContext.ClassRooms.AsNoTracking().Where(cr => cr.ClassId == classId && cr.IsAuthorized).ToListAsync();
    }

    public async Task<Module?> GetCurrentModuleByClassIdAsync(int classId)
    {
        var classEntity = await _efDbContext.Classes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == classId);
        if (classEntity == null)
        {
            return null;
        }

        return await _efDbContext.Modules.AsNoTracking().FirstOrDefaultAsync(x => x.Id == classEntity.CurrentModuleId);
    }

    public async Task<Module?> GetModuleByPhaseIdAsync(int phaseId)
    {
        var phaseEntity = await _efDbContext.Phases.AsNoTracking().FirstOrDefaultAsync(x => x.Id == phaseId);
        if (phaseEntity == null)
        {
            return null;
        }

        return await _efDbContext.Modules.AsNoTracking().FirstOrDefaultAsync(x => x.Id == phaseEntity.ModuleId);  
    }

    public async Task<IEnumerable<IslandPhaseDTO>> GetPhasesByCurrentModuleAsync(int moduleId)
    {
        var phases = await _efDbContext.Phases
            .AsNoTracking()
            .Where(i => i.ModuleId == moduleId)
            .OrderBy(index => index.IndexOrder)
            .ToListAsync();

        var islands = phases.Select(i =>
        {
            string title = string.Empty;
            string helper = string.Empty;
            if (!string.IsNullOrEmpty(i.Name) && i.Name.Contains(":"))
            {
                var parts = i.Name.Split(':');
                title = parts.Length > 0 ? parts[0].Trim() : string.Empty;
                helper = parts.Length > 1 ? parts[1].Trim() : string.Empty;
            }
            else
            {
                title = i.Name ?? string.Empty;
                helper = i.Name ?? string.Empty;
            }

            return new IslandPhaseDTO
            {
                Id = i.Id,
                Order = i.WeekNumber,
                Title = title,
                Helper = helper
            };
        }).ToList();

        return islands;
    }
}
