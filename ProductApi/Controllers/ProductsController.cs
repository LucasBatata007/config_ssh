using Microsoft.AspNetCore.Mvc;
using System.Data;
using Dapper;
using ProductApi.Models; // Importa nossos modelos

namespace ProductApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IDbConnection _db;

        // O IDbConnection (SqlConnetion) é injetado aqui pelo 'builder.Services.AddScoped'
        public ProductsController(IDbConnection db)
        {
            _db = db;
        }

        /// <summary>
        /// Busca todos os produtos do banco.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var sql = "SELECT * FROM Products";
            var products = await _db.QueryAsync<Product>(sql);
            return Ok(products);
        }

        /// <summary>
        /// Busca um produto específico pelo ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var sql = "SELECT * FROM Products WHERE Id = @Id";
            var product = await _db.QuerySingleOrDefaultAsync<Product>(sql, new { Id = id });

            if (product == null)
            {
                return NotFound($"Produto com ID {id} não encontrado.");
            }

            return Ok(product);
        }

        /// <summary>
        /// Cria um novo produto.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto productDto)
        {
            if (productDto == null || string.IsNullOrWhiteSpace(productDto.Nome))
            {
                return BadRequest("O nome do produto é obrigatório.");
            }

            var sql = "INSERT INTO Products (Nome) VALUES (@Nome); SELECT CAST(SCOPE_IDENTITY() as int)";
            
            // O Dapper executa o INSERT e retorna o novo ID gerado pelo "SCOPE_IDENTITY()"
            var newId = await _db.QuerySingleAsync<int>(sql, new { Nome = productDto.Nome });

            var newProduct = new Product
            {
                Id = newId,
                Nome = productDto.Nome
            };

            // Retorna um 201 Created (padrão de API REST)
            return CreatedAtAction(nameof(GetProductById), new { id = newId }, newProduct);
        }
    }
}