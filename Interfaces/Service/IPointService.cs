using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Interfaces;

public interface IPointService
{
    Task<CustomResponse<IEnumerable<Point>>> GetAllAsync();
    Task<CustomResponse<Point>> GetByIdAsync(int id);
}
