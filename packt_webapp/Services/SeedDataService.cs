using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using packt_webapp.Entities;

namespace packt_webapp.Services
{
    public class SeedDataService : ISeedDataService
    {
        private readonly PacktDbContext _context;
        public SeedDataService(PacktDbContext context)
        {
            _context = context;
        }

        public async Task EnsureSeedData()
        {
            _context.Database.EnsureCreated();

            _context.Customers.RemoveRange(_context.Customers);
            _context.SaveChanges();

            var customer = new Customer()
            {
                Firstname = "Chuck",
                Lastname = "Norris",
                Age = 30,
                Id = Guid.NewGuid()
            };

            _context.Add(customer);

            var customer2 = new Customer()
            {
                Firstname = "Fabrian",
                Lastname = "Gosedrink",
                Age = 34,
                Id = Guid.NewGuid()
            };

            _context.Add(customer2);

            await _context.SaveChangesAsync();

        }
    }
}
