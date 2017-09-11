using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using packt_webapp.Dtos;
using packt_webapp.Entities;
using packt_webapp.Repositories;

namespace packt_webapp.Controllers
{
    [Route("api/[controller]")]
    public class CustomersController: Controller
    {
        // 2xx = Positive
        // 4xx = Negative (Consumer did something wrong)
        // 5xx = Negative (Server did something wrong)
        // Request using POSTMAN

        private readonly ICustomerRepository _customerRepository;
        public CustomersController(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        [HttpGet]
        public IActionResult GetAllCustomers()
        {
            var allCustomers = _customerRepository.GetAll().ToList();
            var allCustomersDto = allCustomers.Select(Mapper.Map<CustomerDto>);

            return Ok(allCustomersDto);
        }

        [HttpGet]
        [Route("{id}", Name = "GetSingleCustomer")]
        public IActionResult GetSingleCustomer(Guid id)
        {
            var customer = _customerRepository.GetSingle(id);

            if (customer == null)
            {
                return NotFound();
            }

            return Ok(Mapper.Map<CustomerDto>(customer));
        }

        [HttpPost]
        public IActionResult AddCustomer([FromBody] CustomerCreateDto customerCreateDto)
        {
            var toAdd = Mapper.Map<Customer>(customerCreateDto);

            _customerRepository.Add(toAdd);

            var result = _customerRepository.Save();

            if (!result)
            {
                return new StatusCodeResult(500);
            }

            //return Ok(Mapper.Map<CustomerDto>(toAdd));
            return CreatedAtRoute("GetSingleCustomer", new { id = toAdd.Id }, Mapper.Map<CustomerDto>(toAdd));
        }

        [HttpPut]
        [Route("{id}")]
        public IActionResult UpdateCustomer(Guid id, [FromBody] CustomerUpdateDto updateDto)
        {
            var existingCustomer = _customerRepository.GetSingle(id);

            if (existingCustomer == null)
            {
                return NotFound();
            }

            Mapper.Map(updateDto, existingCustomer);

            _customerRepository.Update(existingCustomer);

            var result = _customerRepository.Save();

            if (!result)
            {
                return new StatusCodeResult(500);
            }

            return Ok(Mapper.Map<CustomerDto>(existingCustomer));
        }

        // PATCH Operations
        // Add -        [{"op": "add", "path":"/customers", "value": {"lastname": "Doe"}}]
        // Remove -     [{"op": "remove", "path":"/lastname"}]
        // Replace -    [{"op": "replace", "path":"/customers/1/lastname", "value": "Foo"}]
        // Copy -       [{"op": "copy", "from":"/customers/0", "path": "/best_customer"}]
        // Move -       [{"op": "move", "from":"/customers", "path": "/persons"}]
        // Test -       [{"op": "test", "path":"/best_customer/lastname", "value": "Doe"}]
        [HttpPatch]
        [Route("{id}")]
        public IActionResult PartiallyUpdate(Guid id, [FromBody] JsonPatchDocument<CustomerUpdateDto> customerPatch)
        {
            if (customerPatch == null)
            {
                return NotFound();
            }

            var existingCustomer = _customerRepository.GetSingle(id);

            if (existingCustomer == null)
            {
                return NotFound();
            }

            var customerToPatch = Mapper.Map<CustomerUpdateDto>(existingCustomer);
            customerPatch.ApplyTo(customerToPatch);

            Mapper.Map(customerToPatch, existingCustomer);

            _customerRepository.Update(existingCustomer);

            var result = _customerRepository.Save();

            if (!result)
            {
                return new StatusCodeResult(500);
            }
            return Ok(Mapper.Map<CustomerDto>(existingCustomer));
        }

        [HttpDelete]
        [Route("{id}")]
        public IActionResult Remove(Guid id)
        {
            var exisitingCustomer = _customerRepository.GetSingle(id);

            if (exisitingCustomer == null)
            {
                return NotFound();
            }

            _customerRepository.Delete(id);

            var result = _customerRepository.Save();

            if (!result)
            {
                return new StatusCodeResult(500);
            }

            return NoContent();
        }
    }
}
