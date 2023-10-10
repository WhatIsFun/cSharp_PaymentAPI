using cSharp_PaymentAPI.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.OAuth;
using cSharp_PaymentAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using Serilog;
using Newtonsoft.Json.Linq;

namespace cSharp_PaymentAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        HttpClient BankClient;
        public static string Token;

        public PaymentController()
        {
            BankClient = new HttpClient();
            BankClient.BaseAddress = new Uri("https://localhost:70/");// check
        }

        [HttpPost("ProcessPayment")]
        public async Task<IActionResult> ProcessPayment(string email, string password)
        {
            var loginRequest = new { Email = email, Password = password };

            try
            {
                // Log received credentials
                Log.Information($"Received login request - Email: {email}, Password: {password}");

                HttpResponseMessage loginResponse = await BankClient.PostAsJsonAsync("api/Login", loginRequest);

                if (!loginResponse.IsSuccessStatusCode)
                {
                    // Handle authentication failure
                    Log.Warning($"Login failed - Status code: {loginResponse.StatusCode}");
                    return Unauthorized("Invalid credentials");
                }

                var tokenResponse = await loginResponse.Content.ReadAsStringAsync();
                Token = tokenResponse; // Store the token as a JSON string

                // Use the retrieved token to make a payment
                BankClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", Token);

                HttpResponseMessage paymentResponse = await BankClient.GetAsync("api/Payment");

                if (paymentResponse.IsSuccessStatusCode)
                {
                    return Ok("Payment successful");
                }
                else
                {
                    return BadRequest($"Failed to make the payment. Status code: {paymentResponse.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"An error occurred: {ex.Message}");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
