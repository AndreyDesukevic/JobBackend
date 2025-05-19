using JobMonitor.Domain.Interfaces.Entities;
using JobMonitor.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobMonitor.Infrastructure.Repositories;

internal class UserRepositry : IUserRepository
{
    public Task AddAsync(IUser user)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IUser> GetByHhIdAsync(string hhId)
    {
        throw new NotImplementedException();
    }

    public Task<IUser> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(IUser user)
    {
        throw new NotImplementedException();
    }
}
