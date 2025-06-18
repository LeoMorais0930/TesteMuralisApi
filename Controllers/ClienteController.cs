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
        cliente.DataCadastro = DateTime.UtcNow;

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObterCliente), new { id = cliente.Id }, _mapper.Map<ClienteDTO>(cliente));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObterCliente(int id)
    {
        var cliente = await BuscarClienteCompleto(id);
        return cliente == null ? NotFound() : Ok(_mapper.Map<ClienteDTO>(cliente));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> ExcluirCliente(int id)
    {
        var cliente = await BuscarClienteCompleto(id);
        if (cliente == null) return NotFound();

        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> ListarClientes()
    {
        var clientes = await _context.Clientes
            .Include(c => c.Endereco)
            .Include(c => c.Contatos)
            .ToListAsync();

        return Ok(_mapper.Map<List<ClienteDTO>>(clientes));
    }

    [HttpGet("pesquisa")]
    public async Task<IActionResult> PesquisarClientes([FromQuery] string nome)
    {
        var clientes = await _context.Clientes
            .Include(c => c.Endereco)
            .Include(c => c.Contatos)
            .Where(c => EF.Functions.Like(c.Nome, $"%{nome}%"))
            .ToListAsync();

        return Ok(_mapper.Map<List<ClienteDTO>>(clientes));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> AtualizarCliente(int id, [FromBody] ClienteDTO clienteDto)
    {
        var cliente = await BuscarClienteCompleto(id);
        if (cliente == null) return NotFound();

        cliente.Nome = clienteDto.Nome;
        cliente.Endereco = await MapearEndereco(clienteDto.Endereco);
        cliente.Contatos = _mapper.Map<List<Contato>>(clienteDto.Contatos);

        await _context.SaveChangesAsync();

        return Ok(_mapper.Map<ClienteDTO>(cliente));
    }

    private async Task<Cliente> MapearCliente(ClienteDTO dto)
    {
        var cliente = _mapper.Map<Cliente>(dto);
        cliente.Endereco = await MapearEndereco(dto.Endereco);
        return cliente;
    }

    private async Task<Endereco> MapearEndereco(EnderecoDTO dto)
    {
        var viaCep = await _viaCep.BuscarEnderecoPorCep(dto.Cep);
        return new Endereco
        {
            Cep = dto.Cep,
            Numero = dto.Numero,
            Complemento = dto.Complemento,
            Logradouro = viaCep.Logradouro,
            Cidade = viaCep.Localidade
        };
    }

    private Task<Cliente?> BuscarClienteCompleto(int id) =>
        _context.Clientes
            .Include(c => c.Endereco)
            .Include(c => c.Contatos)
            .FirstOrDefaultAsync(c => c.Id == id);
}
