using AutoMapper;
using TesteMuralisApi.DTOs;
using TesteMuralisApi.Models;

namespace TesteMuralisApi.Mapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<ClienteDTO, Cliente>().ReverseMap();
            CreateMap<ContatoDTO, Contato>().ReverseMap();
            CreateMap<EnderecoDTO, Endereco>().ReverseMap();
        }
    }
}
