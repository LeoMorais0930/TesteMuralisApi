using Microsoft.AspNetCore.Mvc;
using TesteMuralisApi.DTOs;
using TesteMuralisApi.Models;
using TesteMuralisApi.Services;
using TesteMuralisApi.Data;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace TesteMuralisApi.Controllers;

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
        var cliente = await MapearCliente(clienteDto);
        cliente.DataCadastro = DateTime.UtcNow.ToString();

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObterCliente), new { id = cliente.Id }, _mapper.Map<ClienteResponseDTO>(cliente));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObterCliente(int id)
    {
        var cliente = await BuscarClientePorId(id);
        return cliente == null ? NotFound($"Cliente com ID ({id}) não encontrado") : Ok(_mapper.Map<ClienteResponseDTO>(cliente));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> ExcluirCliente(int id)
    {
        var cliente = await BuscarClientePorId(id);
        if (cliente == null) return NotFound($"Cliente com ID ({id}) não encontrado");

        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> ListarClientes()
    {
        var clientes = await _context.Clientes
            .Include(c => c.Enderecos)
            .Include(c => c.Contatos)
            .ToListAsync();

        return Ok(_mapper.Map<List<ClienteResponseDTO>>(clientes));
    }

    [HttpGet("pesquisa")]
    public async Task<IActionResult> PesquisarClientes([FromQuery] string? nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            return BadRequest("O parâmetro 'nome' não pode ser vazio.");
        }

        var clientes = await _context.Clientes
            .Include(c => c.Enderecos)
            .Include(c => c.Contatos)
            .Where(c => c.Nome.ToLower().Contains(nome.ToLower()))
            .ToListAsync();

        return Ok(_mapper.Map<List<ClienteResponseDTO>>(clientes));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> AtualizarCliente(int id, [FromBody] ClienteDTO clienteDto)
    {
        var cliente = await BuscarClientePorId(id);
        if (cliente == null) return NotFound();

        cliente.Nome = clienteDto.Nome;
        cliente.Enderecos = await MapearEndereco(clienteDto.Enderecos);
        cliente.Contatos = _mapper.Map<List<Contato>>(clienteDto.Contatos);

        await _context.SaveChangesAsync();

        return Ok(_mapper.Map<ClienteResponseDTO>(cliente));
    }

    private async Task<Cliente> MapearCliente(ClienteDTO dto)
    {
        var cliente = _mapper.Map<Cliente>(dto);
        cliente.Enderecos = await MapearEndereco(dto.Enderecos);
        return cliente;
    }

    private async Task<List<Endereco>> MapearEndereco(List<EnderecoDTO> enderecosDto)
    {
        List<Endereco> listaEnderecosRetorno = new List<Endereco>();

        foreach (EnderecoDTO dto in enderecosDto)
        {
            var viaCep = await _viaCep.BuscarEnderecoPorCep(dto.Cep);

            if (viaCep == null)
            {
                throw new Exception($"Erro ao buscar endereço para o CEP na API ViaCep: {dto.Cep}");
            }

            listaEnderecosRetorno.Add(new Endereco
            {
                Cep = dto.Cep,
                Numero = dto.Numero,
                Complemento = dto.Complemento,
                Logradouro = viaCep.Logradouro,
                Cidade = viaCep.Localidade
            });
        }
        return listaEnderecosRetorno;
    }

    private Task<Cliente?> BuscarClientePorId(int id) =>
        _context.Clientes
            .Include(c => c.Enderecos)
            .Include(c => c.Contatos)
            .FirstOrDefaultAsync(c => c.Id == id);
}