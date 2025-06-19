using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TesteMuralisApi.Data;
using TesteMuralisApi.DTOs;
using TesteMuralisApi.Models;

namespace TesteMuralisApi.Services;

public class ClienteService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ViaCepService _viaCep;

    public ClienteService(AppDbContext context, IMapper mapper, ViaCepService viaCep)
    {
        _context = context;
        _mapper = mapper;
        _viaCep = viaCep;
    }

    public async Task<ClienteResponseDTO> CriarClienteAsync(ClienteDTO clienteDto)
    {
        var cliente = await MapearCliente(clienteDto);
        cliente.DataCadastro = DateTime.UtcNow.ToString();

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        return _mapper.Map<ClienteResponseDTO>(cliente);
    }

    public async Task<ClienteResponseDTO?> ObterClienteAsync(int id)
    {
        var cliente = await BuscarClientePorId(id);
        return cliente == null ? null : _mapper.Map<ClienteResponseDTO>(cliente);
    }

    public async Task<bool> ExcluirClienteAsync(int id)
    {
        var cliente = await BuscarClientePorId(id);
        if (cliente == null) return false;

        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<ClienteResponseDTO>> ListarClientesAsync()
    {
        var clientes = await _context.Clientes
            .Include(c => c.Enderecos)
            .Include(c => c.Contatos)
            .ToListAsync();

        return _mapper.Map<List<ClienteResponseDTO>>(clientes);
    }

    public async Task<List<ClienteResponseDTO>> PesquisarClientesPorNomeAsync(string nome)
    {
        var clientes = await _context.Clientes
            .Include(c => c.Enderecos)
            .Include(c => c.Contatos)
            .Where(c => c.Nome.ToLower().Contains(nome.ToLower()))
            .ToListAsync();

        return _mapper.Map<List<ClienteResponseDTO>>(clientes);
    }

    public async Task<ClienteResponseDTO?> AtualizarClienteAsync(int id, ClienteDTO clienteDto)
    {
        var cliente = await BuscarClientePorId(id);
        if (cliente == null) return null;

        cliente.Nome = clienteDto.Nome;
        cliente.Enderecos = await MapearEndereco(clienteDto.Enderecos);
        cliente.Contatos = _mapper.Map<List<Contato>>(clienteDto.Contatos);

        await _context.SaveChangesAsync();

        return _mapper.Map<ClienteResponseDTO>(cliente);
    }

    private async Task<Cliente> MapearCliente(ClienteDTO dto)
    {
        var cliente = _mapper.Map<Cliente>(dto);
        cliente.Enderecos = await MapearEndereco(dto.Enderecos);
        return cliente;
    }

    private async Task<List<Endereco>> MapearEndereco(List<EnderecoDTO> enderecosDto)
    {
        var listaEnderecos = new List<Endereco>();

        foreach (var dto in enderecosDto)
        {
            var viaCep = await _viaCep.BuscarEnderecoPorCep(dto.Cep);
            if (viaCep == null)
                throw new Exception($"Erro ao buscar endereço para o CEP na API ViaCep: {dto.Cep}");

            listaEnderecos.Add(new Endereco
            {
                Cep = dto.Cep,
                Numero = dto.Numero,
                Complemento = dto.Complemento,
                Logradouro = viaCep.Logradouro,
                Cidade = viaCep.Localidade
            });
        }

        return listaEnderecos;
    }

    private Task<Cliente?> BuscarClientePorId(int id) =>
        _context.Clientes
            .Include(c => c.Enderecos)
            .Include(c => c.Contatos)
            .FirstOrDefaultAsync(c => c.Id == id);
}
