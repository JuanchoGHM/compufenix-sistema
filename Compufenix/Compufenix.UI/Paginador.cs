namespace Compufenix.UI;

public class Paginador<T>
{
    private List<T> _elementos = new();

    public int TamanoPagina { get; }
    public int PaginaActual { get; private set; } = 1;
    public int TotalPaginas => Math.Max(1, (int)Math.Ceiling(_elementos.Count / (double)TamanoPagina));
    public string TextoPagina => $"Página {PaginaActual} de {TotalPaginas} ({_elementos.Count} en total)";

    public Paginador(int tamanoPagina = 15)
    {
        TamanoPagina = tamanoPagina;
    }

    // Carga una lista nueva y vuelve a la página 1
    public void Cargar(List<T> elementos)
    {
        _elementos = elementos;
        PaginaActual = 1;
    }

    public List<T> ObtenerPaginaActual()
    {
        return _elementos.Skip((PaginaActual - 1) * TamanoPagina).Take(TamanoPagina).ToList();
    }

    public bool Anterior()
    {
        if (PaginaActual <= 1) return false;
        PaginaActual--;
        return true;
    }

    public bool Siguiente()
    {
        if (PaginaActual >= TotalPaginas) return false;
        PaginaActual++;
        return true;
    }
}