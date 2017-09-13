using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using packt_webapp.Dtos;
using packt_webapp.Entities;
using packt_webapp.QueryParameters;
using packt_webapp.Repositories;

namespace packt_webapp.Controllers
{
    //[ApiVersion("2.0", Deprecated = true)]
    //[Route("api/v{version:apiVersion}/customers")]
    //[Route("api/customers")]
    [Route("api/[controller]")]
    //[Authorize(Policy = "resourcesAdmin")]
    public class Customers2Controller : Controller
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<CustomersController> _logger;

        public Customers2Controller(ICustomerRepository customerRepository, ILogger<CustomersController> logger)
        {
            _customerRepository = customerRepository;
            _logger = logger;
            _logger.LogInformation("customersController started");
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<Customer>), 200)]
        public IActionResult GetAllCustomers(CustomerQueryParameters customerQueryParameters)
        {
            //throw new Exception("test");
            _logger.LogInformation(" -------> GetAllCustomers() ");

            var allCustomers = _customerRepository.GetAll(customerQueryParameters).ToList();

            var allCustomersDto = allCustomers.Select(Mapper.Map<CustomerDto>);

            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(new { totalCount = _customerRepository.Count() }));

            return Ok(allCustomersDto);
        }
    }
}
