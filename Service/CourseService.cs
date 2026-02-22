using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Services;

public class CourseService : ICourseService
{
    private readonly ILogger<CourseService> _logger;
    private readonly ICourseRepository _courseRepository;

    public CourseService(ILogger<CourseService> logger, ICourseRepository courseRepository)
    {
        _logger = logger;
        _courseRepository = courseRepository;
    }

    public async Task<CustomResponse<IEnumerable<Course>>> GetAllAsync()
    {
        try
        {
            var result = await _courseRepository.GetAllAsync();
            return result == null ? CustomResponse<IEnumerable<Course>>.Fail("Nenhum curso encontrado") : CustomResponse<IEnumerable<Course>>.SuccessTrade(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar cursos");
            return CustomResponse<IEnumerable<Course>>.Fail("Erro ao buscar cursos");
        }
    }

    public async Task<CustomResponse<Course>> GetByIdAsync(int id)
    {
        try
        {
            var result = await _courseRepository.GetByIdAsync(id);
            return result == null
                ? CustomResponse<Course>.Fail("Curso não encontrado")
                : CustomResponse<Course>.SuccessTrade(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao buscar curso com ID {id}");
            return CustomResponse<Course>.Fail("Erro ao buscar curso");
        }
    }

    public async Task<CustomResponse<IEnumerable<Course>>> GetCoursesAvailablesAsync()
    {
        try
        {
            var result = await _courseRepository.GetCoursesAvailablesAsync();
            return result == null ? CustomResponse<IEnumerable<Course>>.Fail("Nenhum curso disponível encontrado") : CustomResponse<IEnumerable<Course>>.SuccessTrade(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar cursos disponíveis");
            return CustomResponse<IEnumerable<Course>>.Fail("Erro ao buscar cursos disponíveis");
        }
    }

    public async Task<CustomResponse<IEnumerable<Course>>> GetFullCoursesPaidAsync()
    {
        try
        {
            var result = await _courseRepository.GetFullCoursesPaidAsync();
            return result == null ? CustomResponse<IEnumerable<Course>>.Fail("Nenhum curso pago completo encontrado") : CustomResponse<IEnumerable<Course>>.SuccessTrade(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar cursos pagos completos");
            return CustomResponse<IEnumerable<Course>>.Fail("Erro ao buscar cursos pagos completos");
        }
    }

    public async Task<CustomResponse<IEnumerable<ProgressPaidCourses>>> GetProgressUserPaidCoursesAsync(int userId)
    {
        try
        {
            var result = await _courseRepository.GetProgressUserPaidCoursesAsync(userId);
            return result == null ? CustomResponse<IEnumerable<ProgressPaidCourses>>.Fail("Nenhum progresso encontrado") : CustomResponse<IEnumerable<ProgressPaidCourses>>.SuccessTrade(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao buscar progresso dos cursos pagos do usuário com ID {userId}");
            return CustomResponse<IEnumerable<ProgressPaidCourses>>.Fail("Erro ao buscar progresso dos cursos pagos do usuário");
        }
    }
}
