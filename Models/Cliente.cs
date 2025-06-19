using TesteMuralisApi.Models;

namespace TesteMuralisApi.Models
{
    public class Cliente
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string DataCadastro { get; set; }
        public List<Contato> Contatos { get; set; }
        public List<Endereco> Enderecos { get; set; }
    }
}
