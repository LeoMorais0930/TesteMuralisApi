using Microsoft.AspNetCore.Mvc;
using TesteMuralisApi.DTOs;
using TesteMuralisApi.Services;

[ApiController]
[Route("api/[controller]")]
public class ClienteController : ControllerBase
{
    private readonly ClienteService _clienteService;

    public ClienteController(ClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    [HttpPost]
    public async Task<IActionResult> CriarCliente([FromBody] ClienteDTO clienteDto)
    {
        var cliente = await _clienteService.CriarClienteAsync(clienteDto);
        return CreatedAtAction(nameof(ObterCliente), new { id = cliente.Id }, cliente);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObterCliente(int id)
    {
        var cliente = await _clienteService.ObterClienteAsync(id);
        return cliente == null ? NotFound($"Cliente com ID ({id}) não encontrado") : Ok(cliente);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> ExcluirCliente(int id)
    {
        var sucesso = await _clienteService.ExcluirClienteAsync(id);
        return sucesso ? NoContent() : NotFound($"Cliente com ID ({id}) não encontrado");
    }

    [HttpGet]
    public async Task<IActionResult> ListarClientes()
    {
        var clientes = await _clienteService.ListarClientesAsync();
        return Ok(clientes);
    }

    [HttpGet("pesquisa")]
    public async Task<IActionResult> PesquisarClientes([FromQuery] string? nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return BadRequest("O parâmetro 'nome' não pode ser vazio.");

        var clientes = await _clienteService.PesquisarClientesPorNomeAsync(nome);
        return Ok(clientes);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> AtualizarCliente(int id, [FromBody] ClienteDTO clienteDto)
    {
        var cliente = await _clienteService.AtualizarClienteAsync(id, clienteDto);
        return cliente == null ? NotFound() : Ok(cliente);
    }
}
