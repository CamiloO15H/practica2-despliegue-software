using System.Collections.Concurrent;
using Practica2.Api.Models;

namespace Practica2.Api.Data;

/// <summary>
/// Repositorio en memoria. Los datos se reinician cada vez que el contenedor/pod arranca,
/// lo cual es suficiente para esta práctica (el foco es el despliegue, no la persistencia).
/// </summary>
public class ProductoRepository
{
    private readonly ConcurrentDictionary<int, Producto> _productos = new();
    private int _ultimoId;

    public ProductoRepository()
    {
        Agregar(new ProductoRequest { Nombre = "Teclado mecánico", Categoria = "Periféricos", Precio = 250000, Stock = 15 });
        Agregar(new ProductoRequest { Nombre = "Mouse inalámbrico", Categoria = "Periféricos", Precio = 85000, Stock = 40 });
        Agregar(new ProductoRequest { Nombre = "Monitor 24 pulgadas", Categoria = "Pantallas", Precio = 690000, Stock = 8 });
    }

    public IEnumerable<Producto> ObtenerTodos() => _productos.Values.OrderBy(p => p.Id);

    public Producto? ObtenerPorId(int id) => _productos.TryGetValue(id, out var p) ? p : null;

    public Producto Agregar(ProductoRequest datos)
    {
        var producto = new Producto
        {
            Id = Interlocked.Increment(ref _ultimoId),
            Nombre = datos.Nombre,
            Categoria = datos.Categoria,
            Precio = datos.Precio,
            Stock = datos.Stock
        };
        _productos[producto.Id] = producto;
        return producto;
    }

    public Producto? Actualizar(int id, ProductoRequest datos)
    {
        if (!_productos.TryGetValue(id, out var producto)) return null;
        producto.Nombre = datos.Nombre;
        producto.Categoria = datos.Categoria;
        producto.Precio = datos.Precio;
        producto.Stock = datos.Stock;
        return producto;
    }

    public bool Eliminar(int id) => _productos.TryRemove(id, out _);
}
