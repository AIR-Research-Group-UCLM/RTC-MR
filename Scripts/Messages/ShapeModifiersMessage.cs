using System;

/// <summary>
/// Mensaje que se envia al utilizar las herramientas
/// de modificacion de marcadores.
/// </summary>
[Serializable]
public class ShapeModifiersMessage
{
    /// <summary>
    /// Herramienta seleccionada
    /// Scale = 0, Position = 2, SelectNext = 3,
    /// SelectPrevious = 4, Reset = 5, Delete = 6
    /// </summary>
    public int selectedTool;

    // Las siguientes variables solo son útiles si la herramienta
    // seleccionada es Scale o Position

    /// <summary>
    /// Modificador
    /// More = 0, Less = 1
    /// </summary>
    public int selectedModifier;

    /// <summary>
    /// Aplicar transformaciones en el eje X
    /// </summary>
    public bool selectedXAxis;

    /// <summary>
    /// Aplicar transformaciones en el eje Y
    /// </summary>
    public bool selectedYAxis;

    /// <summary>
    /// Aplicar transformaciones en el eje Z
    /// </summary>
    public bool selectedZAxis;
}