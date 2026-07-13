using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrbanSync.Domain.DTOs;

namespace UrbanSync.Business.Services
{
    public interface IRolService
    {
        Task<IEnumerable<RolDto>> GetAllAsync();
        Task<RolDto?> GetByIdAsync(int id);
        Task<RolDto> CreateAsync(RolCreateDto dto);
    }
}
