using Microsoft.AspNetCore.Mvc;
using TesteMuralisApi.DTOs;
using TesteMuralisApi.Models;
using TesteMuralisApi.Services;
using TesteMuralisApi.Data;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace TesteMuralisApi.Controllers
{
    /// <summary>
    /// Controller responsável por gerenciar clientes.
    /// </summary>
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
        
        /// <summary>
        /// Teste
        /// </summary>
        /// <param name="clienteDto">cliente</param>
        /// <returns>action result</returns>
        [HttpPost] // Cadastramento
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

        [HttpGet("{id}")] // Consulta por id
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

        [HttpDelete("{id}")] // Exclusão por id
        public async Task<IActionResult> ExcluirCliente(int id)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Endereco)
                .Include(c => c.Contatos)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null) return NotFound();

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet] // Listar todos os Clientes
        public async Task<IActionResult> ListarClientes()
        {
            var clientes = await _context.Clientes
                .Include(c => c.Endereco)
                .Include(c => c.Contatos)
                .ToListAsync();

            var clientesDto = _mapper.Map<List<ClienteDTO>>(clientes);
            return Ok(clientesDto);
        }

        [HttpGet("pesquisa")] //Pesquisa por nome
        public async Task<IActionResult> PesquisarClientes([FromQuery] string nome)
        {
            var clientes = await _context.Clientes
                .Include(c => c.Endereco)
                .Include(c => c.Contatos)
                .Where(c => EF.Functions.Like(c.Nome, $"%{nome}%"))
                .ToListAsync();

            var clientesDto = _mapper.Map<List<ClienteDTO>>(clientes);
            return Ok(clientesDto);
        }

        [HttpPut("{id}")] // Alteração de dados do cliente
        public async Task<IActionResult> AtualizarCliente(int id, [FromBody] ClienteDTO clienteDto)
        {
            var clienteExistente = await _context.Clientes
                .Include(c => c.Endereco)
                .Include(c => c.Contatos)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (clienteExistente == null) return NotFound();

            // Atualizar propriedades principais
            clienteExistente.Nome = clienteDto.Nome;

            // Atualizar endereço com ViaCEP novamente
            var enderecoViaCep = await _viaCep.BuscarEnderecoPorCep(clienteDto.Endereco.Cep);
            clienteExistente.Endereco.Cep = clienteDto.Endereco.Cep;
            clienteExistente.Endereco.Numero = clienteDto.Endereco.Numero;
            clienteExistente.Endereco.Complemento = clienteDto.Endereco.Complemento;
            clienteExistente.Endereco.Logradouro = enderecoViaCep.Logradouro;
            clienteExistente.Endereco.Cidade = enderecoViaCep.Localidade;

            // Atualizar contatos — neste exemplo, vamos limpar e adicionar novos
            clienteExistente.Contatos.Clear();
            clienteExistente.Contatos = _mapper.Map<List<Contato>>(clienteDto.Contatos);

            await _context.SaveChangesAsync();

            var clienteAtualizado = _mapper.Map<ClienteDTO>(clienteExistente);
            return Ok(clienteAtualizado);
        }
    }
}
