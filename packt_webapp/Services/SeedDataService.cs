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

            var customer3 = new Customer()
            {
                Firstname = "Micheal",
                Lastname = "Jackson",
                Age = 29,
                Id = Guid.NewGuid()
            };

            _context.Add(customer3);

            var customer4 = new Customer()
            {
                Firstname = "Jackie",
                Lastname = "Chan",
                Age = 44,
                Id = Guid.NewGuid()
            };

            _context.Add(customer4);

            var customer5 = new Customer()
            {
                Firstname = "John",
                Lastname = "Doe",
                Age = 25,
                Id = Guid.NewGuid()
            };

            _context.Add(customer5);

            await _context.SaveChangesAsync();

        }
    }
}
