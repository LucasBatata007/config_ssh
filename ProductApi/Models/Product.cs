namespace ProductApi.Models
{
    // Este é o "record" (modelo) que representa nosso produto no banco
    public class Product
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
    }

    // Este é o DTO (Data Transfer Object) que usamos para criar um novo produto
    // Note que ele não tem o 'Id', pois o banco vai gerar isso.
    public class CreateProductDto
    {
        public string Nome { get; set; } = string.Empty;
    }
}