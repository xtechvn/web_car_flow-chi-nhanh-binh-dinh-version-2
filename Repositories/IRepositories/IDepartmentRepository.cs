using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IDepartmentRepository
    {
        Task<long> Create(Department model);
        Task<long> Update(Department model);
        Task<IEnumerable<Department>> GetAll(string name);
        Task<Department> GetById(int id);
        Task<long> Delete(int id);
    }
}
