using Microsoft.AspNetCore.ResponseCompression;
using System.Net.Http.Json;
using TesteMuralisApi.DTOs;
using TesteMuralisApi.Models;

namespace TesteMuralisApi.Services
{
    public class ViaCepService
    {
        private readonly HttpClient _httpClient;

        public ViaCepService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ViaCepResponse?> BuscarEnderecoPorCep(string cep)
        {
            var url = $"https://viacep.com.br/ws/{cep}/json/";
            var response = await _httpClient.GetFromJsonAsync<ViaCepResponse>(url);

            if (response == null || response.Cep == null)
                return null;

            return response;
        }

    }
}
