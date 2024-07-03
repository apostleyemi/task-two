using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebApiTask.Models;

namespace WebApiTask.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class HelloController : ControllerBase
	{
		private readonly HttpClient _httpClient;
		private GeolocationResponse _globalGeoData;

		public HelloController(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		[HttpGet]
		public async Task<IActionResult> Get([FromQuery] string visitor_name)
		{
			if (string.IsNullOrEmpty(visitor_name))
			{
				return BadRequest("Visitor name is required.");
			}
			

			var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

			if (string.IsNullOrEmpty(ipAddress))
			{
				return StatusCode(500, "Unable to determine IP address.");
			}
			// Call method to get location and temperature
			var locationInfo = await GetLocationAndTemperature(ipAddress);

			if (locationInfo == null)
			{
				return StatusCode(500, "Unable to get location or temperature information.");
			}

			var response = new
			{
				client_ip = ipAddress,
				Location = locationInfo.City,
				greeting = $"Hello, {visitor_name}! the temparature is{locationInfo.Temperature} degrees Celcius in {locationInfo.City} ",
				
				
			};

			//return Ok($"Hello, {visitor_name}!,IP : {ipAddress}");
			return Ok(response);
			
		}

		private async Task<LocationInfo> GetLocationAndTemperature(string ipAddress)
		{
			// Get location information
			var location = await GetLocationFromIP(ipAddress);
			if (location == null)
			{
				return null;
			}

			// Get temperature information
			var temperature = await GetTemperatureFromLocation();
			if (temperature == null)
			{
				return null;
			}

			return new LocationInfo
			{
				City = location.City,
				Temperature = temperature.Value
			};
		}

		private async Task<Location> GetLocationFromIP(string ipAddress)
		{
			string apiKey= "e00f0d9921144eb6ab39449278e125c5";
			string ipInfo = ipAddress;
			try
			{
				var response = await _httpClient.GetStringAsync($"https://api.ipgeolocation.io/ipgeo?apiKey={apiKey}&ip={ipInfo}");
              
				
				
				Console.WriteLine(	response);

                var geolocationResponse = JsonSerializer.Deserialize<GeolocationResponse>(response);
				
				var city = geolocationResponse?.city;

				_globalGeoData = geolocationResponse;

                Console.WriteLine("City infomation "+geolocationResponse.city);

                return new Location { City = city };
			}
			catch (Exception ex)
			{

                Console.WriteLine(	ex);
            }

			return null;
		}



		private async Task<double?> GetTemperatureFromLocation()
		{
			try
			{
				var response = await _httpClient.GetStringAsync($"https://api.openweathermap.org/data/2.5/weather?lat={_globalGeoData.latitude}&lon={_globalGeoData.longitude}&appid=939726dc6ce85888f6a1122243602125&units=metric");

				var tempDataResponse = JsonSerializer.Deserialize<WeatherResonse>(response);

                Console.WriteLine("Temprature "+tempDataResponse.main.temp);
                return tempDataResponse.main.temp;
			}
			catch (Exception ex)
			{

                Console.WriteLine(	ex);
            }
			return null;
		}

		private class Location
		{
			public string City { get; set; }
		}

		private class LocationInfo
		{
			public string City { get; set; }
			public double Temperature { get; set; }
		}
	}
}
