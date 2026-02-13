using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly ILogger<EnrollmentService> _logger;
    private readonly IEnrollmentRepository _enrollmentRepository;

    public EnrollmentService(ILogger<EnrollmentService> logger, IEnrollmentRepository enrollmentRepository)
    {
        _logger = logger;
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<CustomResponse<IEnumerable<Enrollment>>> GetAllAsync()
    {
        var result = await _enrollmentRepository.GetAllAsync();
        return CustomResponse<IEnumerable<Enrollment>>.SuccessTrade(result);
    }

    public async Task<CustomResponse<Enrollment>> GetByIdAsync(int id)
    {
        var result = await _enrollmentRepository.GetByIdAsync(id);
        return result == null
            ? CustomResponse<Enrollment>.Fail("Enrollment not found")
            : CustomResponse<Enrollment>.SuccessTrade(result);
    }
}
