using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HW03.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HW03.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Controller : ControllerBase
    { 
        private HttpClient _client = new HttpClient();

        public Controller()
        {
        }

        [HttpGet]
        public async Task<ReturnObject> Get(string queryAirportTemp, string queryStockPrice, string queryEval) {

            if (!string.IsNullOrEmpty(queryAirportTemp)) {
                return await GetTemperature(queryAirportTemp);
            }

            if (!string.IsNullOrEmpty(queryStockPrice)) {
                return await GetStockPrice(queryStockPrice);
            }

            if (!string.IsNullOrEmpty(queryEval)) {
                return EvalQuery(queryEval);
            }

            return new ReturnObject() {
                Result = "Undefined"
            };
        }

        private ReturnObject EvalQuery(string queryEval) {
            var dt = new DataTable();
            var result = dt.Compute(queryEval, "");

            return new ReturnObject() {
                Result = result.ToString()
            };
        }

        private async Task<ReturnObject> GetStockPrice(string queryStockPrice) {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://apidojo-yahoo-finance-v1.p.rapidapi.com/stock/v2/get-summary?symbol={queryStockPrice}"),
                Headers =
                {
                    { "x-rapidapi-key", "3c71051d4bmsh9eb794ce625481ap145fa7jsn1792a48d5cab" },
                    { "x-rapidapi-host", "apidojo-yahoo-finance-v1.p.rapidapi.com" },
                },
            };

            string price = null;

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var parsed = JObject.Parse(await response.Content.ReadAsStringAsync());
                price = parsed["price"]["regularMarketOpen"]["raw"].ToString();
            }

            return new ReturnObject() {
                Result = price
            };
        }

        private async Task<ReturnObject> GetTemperature(string queryAirportTemp) {
            var request = new HttpRequestMessage() {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"http://dataservice.accuweather.com/locations/v1/poi/search?apikey=W3rpvGUABiLS5jAGfXCsZCJ6zC7Eyqc4&q={queryAirportTemp}&type=38")
            };

            string locationId = null;

            using (var response = await _client.SendAsync(request)) {
                response.EnsureSuccessStatusCode();
                var parsed = JArray.Parse(await response.Content.ReadAsStringAsync());
                var location = parsed[0]["Key"];
                locationId = location.ToString();
            }

            request = new HttpRequestMessage() {
                Method = HttpMethod.Get, 
                RequestUri = new Uri($"http://dataservice.accuweather.com/currentconditions/v1/{locationId}?apikey=W3rpvGUABiLS5jAGfXCsZCJ6zC7Eyqc4")

            };

            string temperature = null;

            using (var response = await _client.SendAsync(request)) {
                response.EnsureSuccessStatusCode();
                var parsed = JArray.Parse(await response.Content.ReadAsStringAsync());
                temperature = parsed[0]["Temperature"]["Metric"]["Value"].ToString();
            }
            return new ReturnObject() {
                Result = temperature
            };
        }
    }
}
