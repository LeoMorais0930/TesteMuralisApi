namespace TesteMuralisApi.DTOs
{
    public class ClienteDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public DateTime DataCadastro { get; set; }
        public List<ContatoDTO> Contatos { get; set; }
        public EnderecoDTO Endereco { get; set; }
    }
}
