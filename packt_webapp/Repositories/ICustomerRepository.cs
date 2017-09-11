using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using packt_webapp.Entities;

namespace packt_webapp.Repositories
{
    public interface ICustomerRepository
    {
        void Add(Customer item);
        void Delete(Guid id);
        IQueryable<Customer> GetAll();
        Customer GetSingle(Guid id);
        bool Save();
        void Update(Customer item);

    }
}
