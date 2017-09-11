using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace packt_webapp.Services
{
    public interface ISeedDataService
    {
        Task EnsureSeedData();
    }
}
