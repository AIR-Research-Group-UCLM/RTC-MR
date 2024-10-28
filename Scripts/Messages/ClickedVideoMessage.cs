using System;

/// <summary>
/// Mensaje que se lanza cuando se interactua con el video
/// proveniente de HoloLens.
/// </summary>
[Serializable]
public class ClickedVideoMessage
{
    /// <summary>
    /// ID del v�deo sobre el que se ha hecho click.
    /// </summary>
    public string videoId;

    /// <summary>
    /// ID de la forma que se quiere poner
    ///Arrow = 0, Box = 1, Cross = 2, Line = 3
    /// </summary>
    public int shapeId;

    /// <summary>
    /// Coordenada 'x' en el intervalo [0, 1]
    /// </summary>
    public float x;

    /// <summary>
    /// Coordenada 'y' en el intervalo [0, 1]
    /// </summary>
    public float y;

    /// <summary>
    /// Distancia por defecto a la que colocar el marcador
    /// si el raycasting no surte efecto
    /// </summary>
    public float noHitObjectDistance;

    /// <summary>
    /// Acci�n del mouse
    ///  Down = 0, Drag = 1, Up = 2
    /// </summary>
    public int mouseActionId;
}