using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace packt_webapp.Middlewares
{
    public class CustomMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly MyConfiguration _myConfig;

        public CustomMiddleware(RequestDelegate next, IOptions<MyConfiguration> myConfig)
        {
            _next = next;
            _myConfig = myConfig.Value;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            Debug.WriteLine($" ----> Request asked for {httpContext.Request.Path} from {_myConfig.Firstname} {_myConfig.Lastname} ");

            // Call the next middleware delegation in the pipeline
            await _next.Invoke(httpContext);
        }
    }
}
