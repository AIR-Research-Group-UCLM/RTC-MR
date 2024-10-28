using System;

/// <summary>
/// Mensaje de la rotacion de la cabeza del usuario.
/// </summary>
[Serializable]
public class AxisGizmoMessage
{
    /// <summary>
    /// Componente x del vector de rotacion.
    /// </summary>
    public float x;

    /// <summary>
    /// Componente y del vector de rotacion.
    /// </summary>
    public float y;

    /// <summary>
    /// Componente z del vector de rotacion.
    /// </summary>
    public float z;
}
