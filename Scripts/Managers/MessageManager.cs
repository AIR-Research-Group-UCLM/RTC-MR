using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// El evento <see cref="ClickedVideoMessageReceivedEvent" /> se invoca
/// cuando llega un mensaje de tipo <see cref="ClickedVideoMessage" />.
/// </summary>
[Serializable]
public class ClickedVideoMessageReceivedEvent : UnityEvent<ClickedVideoMessage>
{
}

;

/// <summary>
/// El evento <see cref="ShapeModifiersMessageReceivedEvent" /> se invoca
/// cuando llega un mensaje de tipo <see cref="ShapeModifiersMessage" />.
/// </summary>
[Serializable]
public class ShapeModifiersMessageReceivedEvent : UnityEvent<ShapeModifiersMessage>
{
}

;

/// <summary>
/// El evento <see cref="AxisGizmoMessageReceivedEvent" /> se invoca
/// cuando llega un mensaje de tipo <see cref="AxisGizmoMessage" />.
/// </summary>
[Serializable]
public class AxisGizmoMessageReceivedEvent : UnityEvent<AxisGizmoMessage>
{
}

;

/// <summary>
/// Facilita el envio y la recepcion de mensajes a traves de WebRTC.
/// </summary>
public class MessageManager : MonoBehaviour
{
    /// <summary>
    /// El evento <see cref="ClickedVideoMessageReceivedEvent" /> se invoca
    /// cuando llega un mensaje de tipo <see cref="ClickedVideoMessage" />.
    /// </summary>
    public ClickedVideoMessageReceivedEvent clickedVideoMessageReceived = new ClickedVideoMessageReceivedEvent();

    /// <summary>
    /// El evento <see cref="ShapeModifiersMessageReceivedEvent" /> se invoca
    /// cuando llega un mensaje de tipo <see cref="ShapeModifiersMessage" />.
    /// </summary>
    public ShapeModifiersMessageReceivedEvent shapeModifiersMessageReceived = new ShapeModifiersMessageReceivedEvent();

    /// <summary>
    /// El evento <see cref="AxisGizmoMessageReceivedEvent" /> se invoca
    /// cuando llega un mensaje de tipo <see cref="AxisGizmoMessage" />.
    /// </summary>
    public AxisGizmoMessageReceivedEvent axisGizmoMessageReceived = new AxisGizmoMessageReceivedEvent();

    /// <summary>
    /// Nombre del canal de envio y recepcion de mensajes <see cref="ClickedVideoMessage" />
    /// </summary>
    private const string ClickedVideoMessageChannel = "clickedVideo";

    /// <summary>
    /// Identificador del canal de envio y recepcion de mensajes <see cref="ClickedVideoMessage" />
    /// </summary>
    private Microsoft.MixedReality.WebRTC.DataChannel clickedVideoMessageChannelObject;

    /// <summary>
    /// Nombre del canal de envio y recepcion de mensajes <see cref="ShapeModifiersMessage" />
    /// </summary>
    private const string ShapeModifiersMessageChannel = "shapeModifiers";

    /// <summary>
    /// Identificador del canal de envio y recepcion de mensajes <see cref="ShapeModifiersMessage" />
    /// </summary>
    private Microsoft.MixedReality.WebRTC.DataChannel shapeModifiersMessageChannelObject;

    /// <summary>
    /// Nombre del canal de envio y recepcion de mensajes <see cref="AxisGizmoMessage" />
    /// </summary>
    private const string AxisGizmoMessageChannel = "axisGizmo";

    /// <summary>
    /// Identificador del canal de envio y recepcion de mensajes <see cref="AxisGizmoMessage" />
    /// </summary>
    private Microsoft.MixedReality.WebRTC.DataChannel axisGizmoMessageChannelObject;

    /// <summary>
    /// Canales que se abriran al iniciar una llamada.
    /// </summary>
    private string[] preallocatedDataChannels = { ClickedVideoMessageChannel, ShapeModifiersMessageChannel };

    /// <summary>
    /// Canales no fiables que se abriran al iniciar una llamada.
    /// </summary>
    private string[] preallocatedDataChannelsUnreliable = { AxisGizmoMessageChannel };

    /// <summary>
    /// Instancia estatica de este manager.
    /// </summary>
    private static MessageManager _instance;

    /// <summary>
    /// Retorna la instancia estatica de este manager.
    /// </summary>
    public static MessageManager Instance
    {
        get { return _instance; }
    }

    /// <summary>
    /// Awake se ejecuta justo cuando se instancia el objeto.
    /// </summary>
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
        }
        else
        {
            _instance = this;
        }
    }

    /// <summary>
    /// Start se ejecuta justo antes del primer Update.
    /// </summary>
    private void Start()
    {
        ConnectionManager.Instance.PreallocatedDataChannels = preallocatedDataChannels;
        ConnectionManager.Instance.PreallocatedDataChannelsUnreliable = preallocatedDataChannelsUnreliable;
        ConnectionManager.Instance.DataChannelAdded.AddListener(OnDataChannelAdded);
    }

    /// <summary>
    /// Envia un mensaje.
    /// </summary>
    /// <typeparam name="T">.</typeparam>
    /// <param name="msg">El mensaje <see cref="T"/> a enviar.</param>
    /// <param name="channelLabel">Identificador del canal.</param>
    /// <returns>True si se ha podido enviar, false si no.</returns>
    public bool SendMessage<T>(T msg, string channelLabel)
    {
        return ConnectionManager.Instance.SendMessage<T>(msg, channelLabel);
    }

    /// <summary>
    /// Envia un mensaje.
    /// </summary>
    /// <typeparam name="T">.</typeparam>
    /// <param name="msg">The msg<see cref="T"/>.</param>
    /// <param name="channel">El canal <see cref="Microsoft.MixedReality.WebRTC.DataChannel"/> por el que se envia el mensaje.</param>
    /// <returns>True si se ha podido enviar, false si no.</returns>
    public bool SendMessage<T>(T msg, Microsoft.MixedReality.WebRTC.DataChannel channel)
    {
        return ConnectionManager.Instance.SendMessage<T>(msg, channel);
    }

    /// <summary>
    /// Envia un mensaje.
    /// </summary>
    /// <param name="msg">El mensaje de tipo <see cref="ClickedVideoMessage"/> a enviar.</param>
    /// <returns>True si se ha podido enviar, false si no.</returns>
    public bool SendClickedVideoMessage(ClickedVideoMessage msg)
    {
        return ConnectionManager.Instance.SendMessage(msg, clickedVideoMessageChannelObject);
    }

    /// <summary>
    /// Envia un mensaje.
    /// </summary>
    /// <param name="msg">El mensaje de tipo <see cref="ShapeModifiersMessage"/> a enviar.</param>
    /// <returns>True si se ha podido enviar, false si no.</returns>
    public bool SendShapeModifiersMessage(ShapeModifiersMessage msg)
    {
        return ConnectionManager.Instance.SendMessage(msg, shapeModifiersMessageChannelObject);
    }

    /// <summary>
    /// Envia un mensaje.
    /// </summary>
    /// <param name="msg">El mensaje de tipo <see cref="AxisGizmoMessage"/> a enviar.</param>
    /// <returns>True si se ha podido enviar, false si no.</returns>
    public bool SendAxisGizmoMessage(AxisGizmoMessage msg)
    {
        return ConnectionManager.Instance.SendMessage(msg, axisGizmoMessageChannelObject);
    }

    /// <summary>
    /// Delegado que se lanza cuando se crea un canal de datos.
    /// </summary>
    /// <param name="channel">El canal <see cref="Microsoft.MixedReality.WebRTC.DataChannel"/> creado.</param>
    private void OnDataChannelAdded(Microsoft.MixedReality.WebRTC.DataChannel channel)
    {
        if (channel != null && channel.Label != null)
        {
            Debug.Log("Added data channel of type: " + channel.Label);
            switch (channel.Label)
            {
                case ClickedVideoMessageChannel:
                    channel.MessageReceived += delegate (byte[] msg) { OnGenericMessageReceived<ClickedVideoMessage>(msg, clickedVideoMessageReceived); };
                    clickedVideoMessageChannelObject = channel;
                    break;

                case ShapeModifiersMessageChannel:
                    channel.MessageReceived += delegate (byte[] msg) { OnGenericMessageReceived<ShapeModifiersMessage>(msg, shapeModifiersMessageReceived); };
                    shapeModifiersMessageChannelObject = channel;
                    break;

                case AxisGizmoMessageChannel:
                    channel.MessageReceived += delegate (byte[] msg) { OnGenericMessageReceived<AxisGizmoMessage>(msg, axisGizmoMessageReceived); };
                    axisGizmoMessageChannelObject = channel;
                    break;
            }
        }
    }

    /// <summary>
    /// Recibe cualquier tipo de mensaje.
    /// </summary>
    /// <typeparam name="T">.</typeparam>
    /// <param name="msg">El mensaje en formato <see cref="byte[]"/>.</param>
    /// <param name="eventToInvoke">El evento <see cref="UnityEvent{T}"/> a invocar.</param>
    private void OnGenericMessageReceived<T>(byte[] msg, UnityEvent<T> eventToInvoke)
    {
        string text = System.Text.Encoding.UTF8.GetString(msg);
        T parsedMessage = JsonUtility.FromJson<T>(text);
        if (parsedMessage != null)
        {
            MainThreadManager.manager.AddJob(() =>
            { eventToInvoke.Invoke(parsedMessage); });
        }
    }
}
