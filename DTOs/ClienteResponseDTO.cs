namespace TesteMuralisApi.DTOs
{
    public class ClienteResponseDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string DataCadastro { get; set; }
        public List<ContatoDTO> Contatos { get; set; }
        public List<EnderecoResponseDTO> Enderecos { get; set; }
    }
}

