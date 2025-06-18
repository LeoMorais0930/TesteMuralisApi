using TesteMuralisApi.Models;

namespace TesteMuralisApi.Models
{
    public class Cliente
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public DateTime DataCadastro { get; set; }
        public List<Contato> Contatos { get; set; }
        public Endereco Endereco { get; set; }
    }
}
