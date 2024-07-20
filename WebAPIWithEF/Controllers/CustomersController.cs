using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Web;
using WebAPIWithEF.Models;

namespace WebAPIWithEF.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly StoreContext _context;
        private string serializedObject = "";

        public CustomersController(StoreContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("CustomerList")]
        public async Task<JsonResult> GetCustomerList()
        {
            List<Customer> customerRawList = await _context.Customers.ToListAsync();
            List<Customer> customerList = customerRawList.OrderBy(x => x.CustomerId).ToList();

            if (customerList.Count > 0)
            {
                serializedObject = JsonConvert.SerializeObject(customerList);
                return new JsonResult(serializedObject);
            }
            else
            {
                serializedObject = JsonConvert.SerializeObject(new { ErrorMessage = "No data found." });
                return new JsonResult(serializedObject);
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetCustomer()
        {
            {
                var parsed = HttpUtility.ParseQueryString(HttpContext.Request.QueryString.ToString());
                if (parsed["customerId"] == null)
                {
                    serializedObject = JsonConvert.SerializeObject(new { ErrorMessage = "The key 'CustomerId' does not exist." });
                    return new JsonResult(serializedObject);
                }
                int customerId = Convert.ToInt32(parsed["customerId"]);

                Customer? customer = await _context.Customers.FirstOrDefaultAsync(el => el.CustomerId == customerId);

                if (customer != null)
                {
                    serializedObject = JsonConvert.SerializeObject(customer);
                    return new JsonResult(serializedObject);
                }
                else
                {
                    serializedObject = JsonConvert.SerializeObject(new { ErrorMessage = "No data found." });
                    return new JsonResult(serializedObject);
                }
            }
        }

        [HttpPost]
        public async Task<JsonResult> PostCustomer()
        {
            string? customerStringified = HttpContext.Request.Headers["customer"];
            if (customerStringified == null)
            {
                return new JsonResult("Cannot insert the record: " +
                  "Http Header's object is null.");
            }
            JToken token = JObject.Parse(customerStringified);
            Customer customer = new Customer();
            customer = SetCustomer(customer, token);

            try
            {
                await _context.Customers.AddAsync(customer);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                string? message = (ex.InnerException != null) ? ex.InnerException.Message : ex.Message;
                return new JsonResult("Cannot insert the record with ID=" + customer.CustomerId +
                    ". Error: " + message + ".");
            }

            return new JsonResult("The record with CustomerID=" + customer.CustomerId + " inserted!");
        }

        [HttpPut]
        public async Task<JsonResult> UpdateCustomer()
        {
            string? customerStringified = HttpContext.Request.Headers["customer"];
            if (customerStringified == null)
            {
                return new JsonResult("Cannot update the record: " +
                  "Http Header's object is null.");
            }
            JToken token = JObject.Parse(customerStringified);
            int customerId = Convert.ToInt32(token.SelectToken("customerId"));
            Customer? customer = null;

            try
            {
                customer = await _context.Customers.FirstOrDefaultAsync(el => el.CustomerId == customerId);
                if (customer == null)
                {
                    return new JsonResult("Cannot update the record with ID= " +
                        token.SelectToken("customerId") + ". The specified record does not exist.");
                }
                customer = SetCustomer(customer, token);
                await _context.SaveChangesAsync();
            }

            catch (Exception ex)
            {
                string? message = (ex.InnerException != null) ? ex.InnerException.Message : ex.Message;
                return new JsonResult("Cannot update the record with ID=" + customerId +
                    ". Error: " + message + ".");
            }
            return new JsonResult("The record with CustomerID=" + customerId + " updated!"); ;
        }

        [HttpDelete]
        public async Task<JsonResult> DeleteCustomer()
        {
            var parsed = HttpUtility.ParseQueryString(HttpContext.Request.QueryString.ToString());
            int customerId = Convert.ToInt32(parsed["CustomerId"]);

            try
            {
                int rowsDeleted = await _context.Customers.Where(t => t.CustomerId == customerId).ExecuteDeleteAsync();
                if (rowsDeleted == 0)
                {
                    return new JsonResult("Cannot delete the record with ID= " +
                        customerId + ". The specified record does not exist.");
                }
            }

            catch (Exception ex)
            {
                string? message = (ex.InnerException != null) ? ex.InnerException.Message : ex.Message;
                return new JsonResult("Cannot delete the record with ID=" +
                    customerId +
                    ". Error: " + message + ".");
            }
            return new JsonResult("The record with CustomerID=" + customerId + " deleted!");
        }

        private Customer SetCustomer(Customer customer, JToken token)
        {
            customer.CustomerId = Convert.ToInt32(token.SelectToken("customerId"));
            customer.CustomerName = Convert.ToString(token.SelectToken("customerName"));
            customer.CreatedDate = DateOnly.Parse(Convert.ToString(token.SelectToken("createdDate")));
            customer.CustomerTypeId = Convert.ToString(token.SelectToken("customerTypeId"));
            customer.StateCode = Convert.ToString(token.SelectToken("stateCode"));

            return customer;
        }
    }
}
