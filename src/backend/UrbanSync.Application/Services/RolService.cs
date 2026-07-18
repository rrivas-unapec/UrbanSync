using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrbanSync.Application.Common.Interfaces.Persistence;
using UrbanSync.Domain.DTOs;
using UrbanSync.Domain.Entities;

namespace UrbanSync.Application.Services
{
    public class RolService : IRolService
    {
        private readonly IRolRepository _rolRepository;

        public RolService(IRolRepository rolRepository)
        {
            _rolRepository = rolRepository;
        }

        public async Task<IEnumerable<RolDto>> GetAllAsync()
        {
            var roles = await _rolRepository.GetAllAsync();
            return roles.Select(r => new RolDto { Id = r.Id, Nombre = r.Nombre, Descripcion = r.Descripcion });
        }

        public async Task<RolDto?> GetByIdAsync(int id)
        {
            var rol = await _rolRepository.GetByIdAsync(id);
            return rol is null ? null : new RolDto { Id = rol.Id, Nombre = rol.Nombre, Descripcion = rol.Descripcion };
        }

        public async Task<RolDto> CreateAsync(RolCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                throw new ArgumentException("El nombre del rol es obligatorio.");

            var entity = new Rol { Nombre = dto.Nombre, Descripcion = dto.Descripcion };
            var newId = await _rolRepository.CreateAsync(entity);
            return new RolDto { Id = newId, Nombre = dto.Nombre, Descripcion = dto.Descripcion };
        }
    }
}
