﻿using APIDesafioIntrabank.Data;
using APIDesafioIntrabank.Dto;
using APIDesafioIntrabank.Model;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIDesafioIntrabank.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClienteEmpresarialController : ControllerBase
    {
        private readonly APIDbContext _context;
        private readonly IMapper _mapper;
        
        public ClienteEmpresarialController(APIDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public IEnumerable<ReadClienteDTO> FindAll([FromQuery] int skip = 0, [FromQuery] int take = 5)
        {
            var listaClientes = _context.ClientesEmpresariais.OrderBy(c => c.RazaoSocial).Skip(skip).Take(take).ToList();
            return _mapper.Map<List<ReadClienteDTO>>(listaClientes);
            
        }

        [HttpGet("{id}")]
        public IActionResult FindById(int id)
        {
            var clienteEmpresarial = _context.ClientesEmpresariais.FirstOrDefault(c => c.Id == id);

            if (clienteEmpresarial == null) return NotFound("Cliente empresarial não encontrado");

            var clienteDTO = _mapper.Map<ReadClienteDTO>(clienteEmpresarial);

            return Ok(clienteDTO);
        }

        [HttpPost]
        public IActionResult Insert([FromBody] CreateClienteDTO createClienteDTO)
        {
            var clienteEmpresarial = _context.ClientesEmpresariais.FirstOrDefault(c => c.Cnpj == createClienteDTO.Cnpj);

            if (clienteEmpresarial != null)
            {
                return BadRequest("Já existe um cliente empresarial cadastrado com esse CNPJ.");
            }

            var endereco = _context.Enderecos.FirstOrDefault(e => e.Id == createClienteDTO.EnderecoId);

            if (endereco == null)
            {
                return BadRequest("Esse endereço não existe");
            }

            if (_context.ClientesEmpresariais.Any(c => c.EnderecoId == createClienteDTO.EnderecoId))
            {
                return BadRequest("Esse endereço ja está cadastrado a outro cliente");
            }

            clienteEmpresarial = _mapper.Map<ClienteEmpresarial>(createClienteDTO);

            _context.ClientesEmpresariais.Add(clienteEmpresarial);
            _context.SaveChanges();

            return CreatedAtAction("FindById", new { id = clienteEmpresarial.Id }, clienteEmpresarial);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] UpdateClienteDTO updateClienteDTO)
        {
            var clienteEmpresarial = _context.ClientesEmpresariais.FirstOrDefault(c => c.Id == id);

            if (clienteEmpresarial == null)
            {
                return NotFound("Esse cliente não está cadastrado na base");
            }

            if (_context.ClientesEmpresariais.Any(c => c.Id != clienteEmpresarial.Id &&  c.Cnpj == updateClienteDTO.Cnpj))
            {
                return BadRequest("Já existe um cliente cadastrado com o mesmo CNPJ.");
            }

            var endereco = _context.Enderecos.FirstOrDefault(e => e.Id == updateClienteDTO.EnderecoId);

            if (endereco == null)
            {
                return BadRequest("Esse endereço não existe");
            }

            if (_context.ClientesEmpresariais.Any(c => c.Id != clienteEmpresarial.Id && c.EnderecoId == updateClienteDTO.EnderecoId))
            {
                return BadRequest("Esse endereço ja está cadastrado a outro cliente");
            }

            _mapper.Map(updateClienteDTO, clienteEmpresarial);
            _context.SaveChanges();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var clienteEmpresarial = _context.ClientesEmpresariais.FirstOrDefault(c => c.Id == id);

            if (clienteEmpresarial == null) return NotFound("Cliente não existe na base de dados");

            var endereco = _context.Enderecos.FirstOrDefault(e => e.Id == clienteEmpresarial.EnderecoId);

            _context.Enderecos.Remove(endereco);
            _context.ClientesEmpresariais.Remove(clienteEmpresarial);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
