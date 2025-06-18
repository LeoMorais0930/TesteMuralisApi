using Microsoft.AspNetCore.Mvc;
using TesteMuralisApi.DTOs;
using TesteMuralisApi.Models;
using TesteMuralisApi.Services;
using TesteMuralisApi.Data;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace TesteMuralisApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClienteController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ViaCepService _viaCep;

        public ClienteController(AppDbContext context, IMapper mapper, ViaCepService viaCep)
        {
            _context = context;
            _mapper = mapper;
            _viaCep = viaCep;
        }

        [HttpPost]
        public async Task<IActionResult> CriarCliente([FromBody] ClienteDTO clienteDto)
        {
            var enderecoViaCep = await _viaCep.BuscarEnderecoPorCep(clienteDto.Endereco.Cep);
            Endereco endereco = new Endereco()
            {
                Cep = clienteDto.Endereco.Cep,
                Logradouro = enderecoViaCep.Logradouro,
                Cidade = enderecoViaCep.Localidade,
                Numero = clienteDto.Endereco.Numero,
                Complemento = clienteDto.Endereco.Complemento
            };

            var cliente = _mapper.Map<Cliente>(clienteDto);
            cliente.Endereco = endereco;

            cliente.DataCadastro = DateTime.UtcNow;

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            var clienteResposta = _mapper.Map<ClienteDTO>(cliente);
            return CreatedAtAction(nameof(ObterCliente), new { id = cliente.Id }, clienteResposta);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObterCliente(int id)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Endereco)
                .Include(c => c.Contatos)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null) return NotFound();

            var clienteDTO = _mapper.Map<ClienteDTO>(cliente);
            return Ok(clienteDTO);
        }
    }
}
