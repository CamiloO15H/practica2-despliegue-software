using Microsoft.AspNetCore.Mvc;
using Practica2.Api.Data;
using Practica2.Api.Models;

namespace Practica2.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductosController : ControllerBase
{
    private readonly ProductoRepository _repo;

    public ProductosController(ProductoRepository repo) => _repo = repo;

    /// <summary>Lista todos los productos.</summary>
    [HttpGet]
    public ActionResult<IEnumerable<Producto>> Listar() => Ok(_repo.ObtenerTodos());

    /// <summary>Obtiene un producto por su id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<Producto> Obtener(int id)
    {
        var producto = _repo.ObtenerPorId(id);
        return producto is null ? NotFound(new { mensaje = $"No existe el producto {id}." }) : Ok(producto);
    }

    /// <summary>Crea un producto.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<Producto> Crear([FromBody] ProductoRequest datos)
    {
        var producto = _repo.Agregar(datos);
        return CreatedAtAction(nameof(Obtener), new { id = producto.Id }, producto);
    }

    /// <summary>Actualiza un producto existente.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<Producto> Actualizar(int id, [FromBody] ProductoRequest datos)
    {
        var producto = _repo.Actualizar(id, datos);
        return producto is null ? NotFound(new { mensaje = $"No existe el producto {id}." }) : Ok(producto);
    }

    /// <summary>Elimina un producto.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Eliminar(int id) =>
        _repo.Eliminar(id) ? NoContent() : NotFound(new { mensaje = $"No existe el producto {id}." });
}
