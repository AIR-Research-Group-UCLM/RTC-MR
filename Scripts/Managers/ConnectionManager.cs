using Microsoft.MixedReality.WebRTC.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static Microsoft.MixedReality.WebRTC.DataChannel;

/// <summary>
/// Evento que se lanza cuando se ha iniciado de forma exitosa una llamada.
/// </summary>
[Serializable]
public class CallStartedEvent : UnityEvent
{
}

;

/// <summary>
/// Evento que se lanza cuando se ha terminado una llamada.
/// </summary>
[Serializable]
public class CallEndedEvent : UnityEvent
{
}

;

/// <summary>
/// Evento que se lanza cuando una llamada falla al intentar iniciarla.
/// </summary>
[Serializable]
public class CallFailedEvent : UnityEvent
{
}

;

/// <summary>
/// Evento que se lanza cuando se pulsa el boton de iniciar llamada y no hay llamada en proceso.
/// </summary>
[Serializable]
public class CallButtonClickedEvent : UnityEvent
{
}

;

/// <summary>
/// Evento que indica que se ha creado un nuevo canal de datos.
/// </summary>
[Serializable]
public class DataChannelAddedEvent : UnityEvent<Microsoft.MixedReality.WebRTC.DataChannel>
{
}

;

/// <summary>
/// La clase singleton <c>ConnectionManager</c> encapsula y simplifica el funcionamiento de MixedReality WebRTC para
/// permitir su utilizacion de forma sencilla en cualquier aplicacion.
/// Es necesario que en la escena donde se use este script haya tambien presente un objeto del tipo <c>PeerConnection</c>.
/// </summary>
public class ConnectionManager : MonoBehaviour
{
    /// <summary>
    /// Evento que se lanza cuando se ha iniciado de forma exitosa una llamada.
    /// </summary>
    [Header("Call events")]
    [SerializeField]
    private CallStartedEvent callStarted = new CallStartedEvent();

    /// <summary>
    /// Retorna el evento CallStarted
    /// </summary>
    public CallStartedEvent CallStarted { get => callStarted; }

    /// <summary>
    /// Evento que se lanza cuando se ha terminado una llamada.
    /// </summary>
    [SerializeField]
    private CallEndedEvent callEnded = new CallEndedEvent();

    /// <summary>
    /// Retorna el evento CallEnded
    /// </summary>
    public CallEndedEvent CallEnded { get => callEnded; }

    /// <summary>
    /// Evento que se lanza cuando ha fallado una llamada.
    /// </summary>
    [SerializeField]
    private CallFailedEvent callFailed = new CallFailedEvent();

    /// <summary>
    /// Retorna el evento CallFailedEvent
    /// </summary>
    public CallFailedEvent CallFailed { get => callFailed; }

    /// <summary>
    /// Evento que se lanza cuando se pulsa el boton de iniciar llamada.
    /// </summary>
    [SerializeField]
    private CallButtonClickedEvent callButtonClicked = new CallButtonClickedEvent();

    /// <summary>
    /// Retorna el evento CallButtonClickedEvent
    /// </summary>
    public CallButtonClickedEvent CallButtonClicked { get => callButtonClicked; }

    /// <summary>
    /// Evento que se lanza cuando se crea un canal de datos.
    /// </summary>
    [Header("Data channel events")]
    [SerializeField]
    private DataChannelAddedEvent dataChannelAdded = new DataChannelAddedEvent();

    /// <summary>
    /// Retorna el evento DataChannelAddedEvent.
    /// </summary>
    public DataChannelAddedEvent DataChannelAdded { get => dataChannelAdded; }

    /// <summary>
    /// Tiempo maximo en segundos desde que se inicia una llamada hasta que se corta si no se obtiene respuesta desde el peer..
    /// </summary>
    [Header("Call settings")]
    [SerializeField]
    [Range(2, 150)]
    [Tooltip("Maximum amount of time (in seconds) in which the call will be ended if there is no response from the peer")]
    private float callTimeoutSeconds = 5;

    /// <summary>
    /// Referencia a la corrutina que finaliza la llamada si no se obtine respuesta de parte del peer.
    /// </summary>
    private Coroutine callTimeoutCoroutine;

    /// <summary>
    /// Ignora errores no criticos al iniciar una llamada.
    /// </summary>
    [SerializeField]
    [Tooltip("Ignore non critical errors thrown when starting a call")]
    private bool ignoreErrorsStartingConnection = true;

    /// <summary>
    /// Canales de datos que se crean al iniciar una llamada.
    /// </summary>
    [SerializeField]
    [Tooltip("Default data channels which will be created when the call is started")]
    private string[] preallocatedDataChannels = { };

    /// <summary>
    /// Canales de datos no fiables que se crearan al iniciar una llamada.
    /// </summary>
    [SerializeField]
    [Tooltip("Default data channels which will be created when the call is started")]
    private string[] preallocatedDataChannelsUnreliable = { };

    /// <summary>
    /// Webcam desde la que se capturan imagenes.
    /// </summary>
    [SerializeField]
    private WebcamSource webcamSource;

    /// <summary>
    /// Aunque esta clase de utilidades soporta la creacion de canales de datos en cualquier momento de la llamada,
    /// esta propiedad permite que una serie de canales por defecto sean iniciados al empezar la llamada.
    /// Es buena practica que los canales que se vayan a usar si o si se pongan en esta propiedad para que se creen
    /// al momento de iniciar la llamada.
    /// </summary>
    public string[] PreallocatedDataChannels { get => preallocatedDataChannels; set => preallocatedDataChannels = value; }

    /// <summary>
    /// Aunque esta clase de utilidades soporta la creacion de canales de datos en cualquier momento de la llamada,
    /// esta propiedad permite que una serie de canales por defecto sean iniciados al empezar la llamada.
    /// Es buena practica que los canales que se vayan a usar si o si se pongan en esta propiedad para que se creen
    /// al momento de iniciar la llamada.
    /// </summary>
    public string[] PreallocatedDataChannelsUnreliable { get => preallocatedDataChannelsUnreliable; set => preallocatedDataChannelsUnreliable = value; }

    /// <summary>
    /// Referencia al script <c>PeerConnection</c> de la escena.
    /// </summary>
    private PeerConnection connection;

    /// <summary>
    /// Instancia activa de <c>ConnectionManager</c>.
    /// </summary>
    private static ConnectionManager _instance;

    /// <summary>
    /// Gets the Instance
    /// Instancia activa de <c>ConnectionManager</c>.
    /// </summary>
    public static ConnectionManager Instance
    {
        get { return _instance; }
    }

    /// <summary>
    /// Lista de tuplas que guarda los mensajes que todavia no se han podido enviar debido a que el canal de datos al que pertenecen no esta abierto..
    /// </summary>
    private List<Tuple<Microsoft.MixedReality.WebRTC.DataChannel, byte[]>> pendingMessages = new List<Tuple<Microsoft.MixedReality.WebRTC.DataChannel, byte[]>>();

    /// <summary>
    /// Retorna la variable que indica si nos encontramos dentro de una llamada.
    /// </summary>
    public bool OnCall { get => onCall; set => onCall = value; }
    public float CallTimeoutSeconds { get => callTimeoutSeconds; set => callTimeoutSeconds = value; }
    public bool IgnoreErrorsStartingConnection { get => ignoreErrorsStartingConnection; set => ignoreErrorsStartingConnection = value; }

    /// <summary>
    /// Variable que indica si nos encontramos dentro de una llamada.
    /// </summary>
    private bool onCall = false;

    /// <summary>
    /// Indica si se deben ignorar las pulsaciones al boton de llamar.
    /// </summary>
    private bool ignoreCallButtonPresses = false;

    /// <summary>
    /// Inicia o termina la llamada, segun corresponda.
    /// </summary>
    public void StartCall()
    {
        // Ignoramos el boton si estamos en medio de un proceso de conexion o desconexion.
        if (ignoreCallButtonPresses)
        {
            return;
        }

        ignoreCallButtonPresses = true;

        if (!onCall)
        {
            MainThreadManager.manager.AddJob(() =>
            {
                Debug.Log("Call Button Clicked!");
                callButtonClicked.Invoke();
            });
        }
        try
        {
            if (!onCall)
            {
                Debug.Log("Starting call...");

                // Es importante abrir al menos un canal de datos antes de realizar la llamada,
                // de no abrirlo no podemos crear canales dinamicamente.
                Microsoft.MixedReality.WebRTC.DataChannel defaultChannel = null;

                bool channelAllocationSucceded = true;

                // Creamos los canales de datos por defecto.
                if ((PreallocatedDataChannels != null && PreallocatedDataChannels.Length > 0) || (PreallocatedDataChannelsUnreliable != null && PreallocatedDataChannelsUnreliable.Length > 0))
                {
                    if (preallocatedDataChannels != null)
                    {
                        foreach (string channelLabel in PreallocatedDataChannels)
                        {
                            try
                            {
                                defaultChannel = GetOrCreateDataChannel(channelLabel);

                                if (defaultChannel == null)
                                {
                                    Debug.LogError("Failed to open default data channel. Ending call...");
                                    channelAllocationSucceded = false;
                                    break;
                                }
                            }
                            catch
                            {
                                Debug.LogError("Failed to open default data channel. Ending call...");
                                channelAllocationSucceded = false;
                                break;
                            }
                        }
                    }

                    if (PreallocatedDataChannelsUnreliable != null)
                    {
                        foreach (string channelLabel in PreallocatedDataChannelsUnreliable)
                        {
                            try
                            {
                                defaultChannel = GetOrCreateDataChannelUnreliable(channelLabel);

                                if (defaultChannel == null)
                                {
                                    Debug.LogError("Failed to open default data channel. Ending call...");
                                    channelAllocationSucceded = false;
                                    break;
                                }
                            }
                            catch
                            {
                                Debug.LogError("Failed to open default data channel. Ending call...");
                                channelAllocationSucceded = false;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    try
                    {
                        defaultChannel = GetOrCreateDataChannel("defaultChannel");
                        if (defaultChannel == null)
                        {
                            Debug.LogError("Failed to open default data channel. Ending call...");
                            channelAllocationSucceded = false;
                        }
                    }
                    catch
                    {
                        Debug.LogError("Failed to open default data channel. Ending call...");
                        channelAllocationSucceded = false;
                    }
                }

                // Si no se han podido crear todos los canales de datos acabamos la llamada.
                if (!channelAllocationSucceded)
                {
                    ignoreCallButtonPresses = false;
                    pendingMessages = new List<Tuple<Microsoft.MixedReality.WebRTC.DataChannel, byte[]>>();
                    CloseAllDataChannels();
                    MainThreadManager.manager.AddJob(() =>
                    {
                        Debug.LogError("Call Failed!");
                        callFailed.Invoke();
                    });
                    return;
                }

                // Iniciamos la llamada
                try
                {
                    if (ignoreErrorsStartingConnection)
                    {
                        connection.StartConnectionIgnoreError();
                    }
                    else
                    {
                        connection.StartConnection();
                    }

                    // Lanzamos una coroutina que para la llamada si pasa demasiado tiempo
                    // y no obtenemos respuesta del peer
                    callTimeoutCoroutine = StartCoroutine(CallTimeoutCoroutine());
                }
                catch
                {
                    MainThreadManager.manager.AddJob(() =>
                    {
                        Debug.LogError("Call Failed!");
                        callFailed.Invoke();
                    });
                    ignoreCallButtonPresses = false;
                }
            }
            else
            {
                EndCall(false);
            }
        }
        catch
        {
            ignoreCallButtonPresses = false;
        }
    }

    /// <summary>
    /// Devuelve un canal de datos a partir de su label (o lo crea si no existe).
    /// </summary>
    /// <param name="channelLabel">El nombre del canal de datos.</param>
    /// <returns>El canal <see cref="Microsoft.MixedReality.WebRTC.DataChannel"/>.</returns>
    public Microsoft.MixedReality.WebRTC.DataChannel GetOrCreateDataChannel(string channelLabel)
    {
        return GetOrCreateDataChannelCore(channelLabel, true, true);
    }

    /// <summary>
    /// Devuelve un canal de datos no fiable a partir de su label (o lo crea si no existe).
    /// </summary>
    /// <param name="channelLabel">El nombre del canal de datos.</param>
    /// <returns>El canal <see cref="Microsoft.MixedReality.WebRTC.DataChannel"/>.</returns>
    public Microsoft.MixedReality.WebRTC.DataChannel GetOrCreateDataChannelUnreliable(string channelLabel)
    {
        return GetOrCreateDataChannelCore(channelLabel, false, false);
    }

    /// <summary>
    /// Devuelve un canal de datos a partir de su label (o lo crea si no existe).
    /// </summary>
    /// <param name="channelLabel">El nombre del canal de datos.</param>
    /// <param name="ordered">¿Los mensajes deben llegar ordenados?</param>
    /// <param name="reliable">¿Es un canal fiable?</param>
    /// <returns>El canal <see cref="Microsoft.MixedReality.WebRTC.DataChannel"/>.</returns>
    private Microsoft.MixedReality.WebRTC.DataChannel GetOrCreateDataChannelCore(string channelLabel, bool ordered, bool reliable)
    {
        if (connection == null || connection.Peer == null)
        {
            Debug.LogError("Peer is not present.");
            return null;
        }

        // Buscamos el canal de datos
        Microsoft.MixedReality.WebRTC.DataChannel channel = connection.Peer.DataChannels.Find((e) => e.Label == channelLabel);
        if (channel != null)
        {
            return channel;
        }

        // Si no se encuentra intentamos abrirlo
        Debug.Log("Channel not found. Trying to open it...");
        channel = connection.Peer.AddDataChannelAsync(channelLabel, ordered, reliable, default).Result;
        channel.StateChanged += OnDataChannelStateChanged;
        return channel;
    }

    /// <summary>
    /// Envia un mensaje por el canal de datos correspondiente.
    /// </summary>
    /// <typeparam name="T">Tipo del mensaje a enviar.</typeparam>
    /// <param name="msg">Objeto que se va a enviar.</param>
    /// <param name="channelLabel">Nombre del canal de datos.</param>
    /// <returns>True si se ha enviado correctamente, false si no.</returns>
    public bool SendMessage<T>(T msg, string channelLabel)
    {
        Microsoft.MixedReality.WebRTC.DataChannel channel = connection.Peer.DataChannels.Find((e) => e.Label == channelLabel);

        if (channel == null)
        {
            Debug.Log("Data channel not found");
            return false;
        }

        return SendMessage<T>(msg, channel);
    }

    /// <summary>
    /// Envia un mensaje por el canal de datos correspondiente.
    /// </summary>
    /// <typeparam name="T">Tipo del mensaje a enviar.</typeparam>
    /// <param name="msg">Objeto que se va a enviar.</param>
    /// <param name="channel">Referencia del canal de datos.</param>
    /// <returns>True si se ha enviado correctamente, false si no.</returns>
    public bool SendMessage<T>(T msg, Microsoft.MixedReality.WebRTC.DataChannel channel)
    {
        if (channel == null || msg == null)
        {
            Debug.LogError("Neither the message nor the channel should be null");
            return false;
        }

        if (!(connection != null && connection.Peer != null))
        {
            Debug.LogError("Peer not found");
            return false;
        }

        // Convertimos el mensaje a JSON
        string jsonString = JsonUtility.ToJson(msg);

        // Canal cerrado o cerrandose
        if (channel.State == ChannelState.Closed || channel.State == ChannelState.Closing)
        {
            Debug.LogError("Channel is closed or closing");
            return false;
        }

        if (channel.State == ChannelState.Connecting)
        {
            // Ponemos el mensaje en la lista de mensajes pendientes, ya que hasta que no se habra el canal sera imposible enviarlos
            pendingMessages.Add(Tuple.Create(channel, System.Text.Encoding.UTF8.GetBytes(jsonString)));
            Debug.Log("Channel " + channel.Label + " is connecting. Message will be sent when it's open.");
            return true;
        }

        Debug.Log("Sending message...");
        try
        {
            channel.SendMessage(System.Text.Encoding.UTF8.GetBytes(jsonString));
        }
        catch (Exception ex)
        {
            Debug.LogError("Unable to send the message...");
            Debug.LogError($"{ex.Message}");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Evento que se lanza cuando la conexion esta lista para ser utilizada.
    /// </summary>
    private void OnPeerConnectionInitialized()
    {
        Debug.Log("Peer connection initialized!");
        connection.Peer.IceStateChanged += IceStateChangedDelegate;
        connection.Peer.IceGatheringStateChanged += IceGatheringStateChangedDelegate;
        connection.Peer.DataChannelAdded += DataChannelAddedDelegate;
    }

    /// <summary>
    /// Evento que se lanza cuando la conexion ya no puede ser utilizada.
    /// </summary>
    private void OnPeerConnectionShutdown()
    {
        Debug.Log("Peer connection shutdown!");
    }

    /// <summary>
    /// Evento que se lanza cuando ocurre un error en la conexion.
    /// </summary>
    /// <param name="error">El error ocurrido.</param>
    private void OnPeerConnectionError(string error)
    {
        Debug.LogError("Peer connection error!\n" + error);
        EndCall(true);
    }

    /// <summary>
    /// El metodo <c>Awake</c> revisa que no exista mas de una instancia del script <c>ConnectionManager</c> en la escena.
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
    /// En el metodo <c>Start</c> se busca el componente <c>PeerConnection</c> que haya en la escena y se subscribe a diferentes listeners.
    /// </summary>
    private void Start()
    {
        connection = FindObjectOfType<PeerConnection>();
        if (connection == null)
        {
            Debug.LogError("You need to add a PeerConnection to the scene!!");
        }
        else
        {
            Debug.Log("Found PeerConnection!");
            connection.OnInitialized.AddListener(OnPeerConnectionInitialized);
            connection.OnShutdown.AddListener(OnPeerConnectionShutdown);
            connection.OnError.AddListener(OnPeerConnectionError);
            StartCoroutine(CreateRandomChannel());
        }
    }

    /// <summary>
    /// Evento que ocurre cuando el estado de la busqueda de candidatos ICE cambia. En general suele
    /// pasar a <c>Gathering</c> en el momento que se inicia la llamada y <c>Complete</c> cuando la llamada
    /// se establece correctamente.
    /// </summary>
    /// <param name="newState">Nuevo estado ICE gathering.</param>
    private void IceGatheringStateChangedDelegate(Microsoft.MixedReality.WebRTC.IceGatheringState newState)
    {
        Debug.Log("New ICE gathering state: " + newState);
    }

    /// <summary>
    /// Evento que se lanza cuando cambia el estado ICE de la conexion. Gracias a este estado podemos determinar cuando empieza la llamada
    /// y cuando termina.
    /// </summary>
    /// <param name="newState">Nuevo estado ICE.</param>
    private void IceStateChangedDelegate(Microsoft.MixedReality.WebRTC.IceConnectionState newState)
    {
        Debug.Log("New ICE state: " + newState);

        switch (newState)
        {
            // Estado inicial de la conexion ICE.
            case Microsoft.MixedReality.WebRTC.IceConnectionState.New:
                // No hacer nada
                break;

            // El agente ICE ha recibido uno o mas candidatos remotos y esta comprobando los pares de candidatos locales
            // y remotos entre si para intentar encontrar una coincidencia compatible, pero aun no ha encontrado un par
            // que permita realizar la conexión.
            case Microsoft.MixedReality.WebRTC.IceConnectionState.Checking:
                // No hacer nada
                break;

            // Se han encontrado candidatos remotos para todos los componentes de la conexion. Si estamos en el par
            // que recibe la llamada se llegara a este estado.
            case Microsoft.MixedReality.WebRTC.IceConnectionState.Connected:
                if (!onCall)
                {
                    // Desactivamos la coroutina del timeout
                    onCall = true;
                    ignoreCallButtonPresses = false;
                    // Lanzamos el evento de inicio de llamada
                    MainThreadManager.manager.AddJob(() =>
                    {
                        Debug.Log("Call started!");
                        callStarted.Invoke();
                    });
                }

                break;

            // Se han encontrado candidatos remotos para todos los componentes de la conexion y se ha establecido la conexion. Si estamos en el par
            // que hace la llamada se llegara a este estado.
            case Microsoft.MixedReality.WebRTC.IceConnectionState.Completed:
                if (!onCall)
                {
                    // Desactivamos la coroutina del timeout
                    onCall = true;
                    ignoreCallButtonPresses = false;
                    // Lanzamos el evento de inicio de llamada
                    MainThreadManager.manager.AddJob(() =>
                    {
                        Debug.Log("Call started!");
                        callStarted.Invoke();
                    });
                }

                break;

            // No se han encontrado candidatos ICE compatibles. Imposible iniciar la conexión.
            case Microsoft.MixedReality.WebRTC.IceConnectionState.Failed:
                EndCall(true);
                break;

            // El peer ha colgado la llamada o se ha caido.
            case Microsoft.MixedReality.WebRTC.IceConnectionState.Disconnected:
                EndCall(false);
                break;

            // La conexión se ha cerrado del todo
            case Microsoft.MixedReality.WebRTC.IceConnectionState.Closed:
                EndCall(false);
                break;
        }
    }

    /// <summary>
    /// Cuando cambia el estado de un canal de datos deberan enviarse los mensajes pendientes y eliminar
    /// los canales de datos cerrados.
    /// </summary>
    private void OnDataChannelStateChanged()
    {
        DispatchPendingMessages();
        RemoveClosedDataChannels();
    }

    /// <summary>
    /// Envia los mensajes pendientes, si es posible.
    /// </summary>
    private void DispatchPendingMessages()
    {
        MainThreadManager.manager.AddJob(() =>
        {
            foreach (Tuple<Microsoft.MixedReality.WebRTC.DataChannel, byte[]> messageTuple in pendingMessages.ToArray())
            {
                if (messageTuple != null && messageTuple.Item1 != null && messageTuple.Item2 != null)
                {
                    switch (messageTuple.Item1.State)
                    {
                        case ChannelState.Connecting:
                            // No hacer nada
                            break;

                        case ChannelState.Open:
                            // Enviar mensaje
                            try
                            {
                                Debug.Log("Dispatching pending message...");
                                messageTuple.Item1.SendMessage(messageTuple.Item2);
                            }
                            catch
                            {
                                Debug.LogError("Unable to send pending message...");
                            }
                            finally
                            {
                                pendingMessages.Remove(messageTuple);
                            }

                            break;

                        case ChannelState.Closing:
                            pendingMessages.Remove(messageTuple);
                            break;

                        case ChannelState.Closed:
                            pendingMessages.Remove(messageTuple);
                            break;
                    }
                }
                else
                {
                    pendingMessages.Remove(messageTuple);
                }
            }
        });
    }

    /// <summary>
    /// Delegate que se lanza cuando un canal de datos ha sido correctamente creado.
    /// </summary>
    /// <param name="channel">El canal creado.</param>
    private void DataChannelAddedDelegate(Microsoft.MixedReality.WebRTC.DataChannel channel)
    {
        Debug.Log("Added data channel: " + channel.Label);
        MainThreadManager.manager.AddJob(() =>
        { dataChannelAdded.Invoke(channel); });
    }

    /// <summary>
    /// Elimina los canales de datos cerrados, si es posible.
    /// </summary>
    private void RemoveClosedDataChannels()
    {
        MainThreadManager.manager.AddJob(() =>
        {
            if (connection != null && connection.Peer != null && connection.Peer.DataChannels != null && connection.Peer.DataChannels.Count > 0)
            {
                foreach (Microsoft.MixedReality.WebRTC.DataChannel dataChannel in connection.Peer.DataChannels.ToArray())
                {
                    if (dataChannel != null && dataChannel.State == ChannelState.Closed)
                    {
                        try
                        {
                            connection.Peer.RemoveDataChannel(dataChannel);
                        }
                        catch
                        {
                            // No hacer nada
                        }
                    }
                }
            }
        });
    }

    /// <summary>
    /// Cierra todos los canales de datos.
    /// </summary>
    private void CloseAllDataChannels()
    {
        MainThreadManager.manager.AddJob(() =>
        {
            if (connection != null && connection.Peer != null && connection.Peer.DataChannels != null && connection.Peer.DataChannels.Count > 0)
            {
                foreach (Microsoft.MixedReality.WebRTC.DataChannel dataChannel in connection.Peer.DataChannels.ToArray())
                {
                    try
                    {
                        connection.Peer.RemoveDataChannel(dataChannel);
                    }
                    catch
                    {
                        // No hacer nada
                    }
                }
            }
        });
    }

    /// <summary>
    /// Lanza la rutina para finalizar una llamada.
    /// </summary>
    /// <param name="invokeCallFailed">Si es true se invoca al evento <c>CallFailed</c> en vez de a <c>CallEnded</c>.</param>
    private void EndCall(bool invokeCallFailed)
    {
        Debug.Log("Ending call...");
        onCall = false;

        // Se vacia la cola de mensajes pendientes
        pendingMessages = new List<Tuple<Microsoft.MixedReality.WebRTC.DataChannel, byte[]>>();

        // NO LLAMAR A CLOSE AL ACABAR UNA LLAMADA, PROVOCA CRASHEOS
        // connection.Peer.Close();

        // Cerramos los canales de datos
        CloseAllDataChannels();

        // Si la corrutina del timeout sigue funcionando la paramos
        if (callTimeoutCoroutine != null)
        {
            MainThreadManager.manager.AddJob(() =>
            {
                StopCoroutine(callTimeoutCoroutine);
                callTimeoutCoroutine = null;
            });
        }

        // La unica forma de colgar en MixedReality WebRTC es desactivando y activando el gameobject
        // que contiene la conexion. Es cutre pero es lo que hay:
        // https://github.com/microsoft/MixedReality-WebRTC/issues/375
        MainThreadManager.manager.AddJob(() =>
        {
            if (webcamSource != null)
            {
#if !UNITY_EDITOR
                try
                {
                    if (connection != null)
                    {
                        if (connection.Peer != null)
                        {
                            foreach (var item in connection.Peer.LocalVideoTracks)
                            {
                                item.Enabled = false;
                            }
                        }

                        if (webcamSource != null)
                        {
                            webcamSource.enabled = false;
                        }

                        connection.gameObject.SetActive(false);
                    }

                    if (connection != null)
                    {
                        if (webcamSource != null)
                        {
                            webcamSource.enabled = true;
                        }

                        connection.gameObject.SetActive(true);
                    }
                }
                catch
                {
                    Debug.LogError("Error during desconection procedures!");
                    connection.gameObject.SetActive(false);
                    connection.gameObject.SetActive(true);
                }
#endif

#if UNITY_EDITOR
                connection.gameObject.SetActive(false);
                connection.gameObject.SetActive(true);
#endif
            }
            else
            {
                connection.gameObject.SetActive(false);
                connection.gameObject.SetActive(true);
            }

            StartCoroutine(InvokeEndCallOrCallFailed(invokeCallFailed));
            StartCoroutine(CreateRandomChannel());
        });

        ignoreCallButtonPresses = false;
    }

    /// <summary>
    /// Invoca el evento <c>CallFailed</c> o <c>CallEnded</c> tras esperar un corto periodo de tiempo (0.3s).
    /// Esto es necesario para asegurarnos de que no se reciben mas frames de video despues de invocar el evento.
    /// </summary>
    /// <param name="invokeCallFailed">Si es true se invoca al evento <c>CallFailed</c> en vez de a <c>CallEnded</c>.</param>
    /// <returns>Devuelve el <see cref="IEnumerator"/> que identifica esta corrutina.</returns>
    private IEnumerator InvokeEndCallOrCallFailed(bool invokeCallFailed)
    {
        yield return new WaitForSecondsRealtime(0.3f);
        if (invokeCallFailed)
        {
            callFailed.Invoke();
        }
        else
        {
            callEnded.Invoke();
        }
    }

    /// <summary>
    /// Crea un canal de datos cuyo nombre es un UUID aleatorio. La utilidad de un canal dummy viene dada debido a que
    /// una vez terminada una negociacion ICE se pueden crear otros canales de datos SI Y SOLO SI YA EXISTIA UN CANAL
    /// ANTES DE LA NEGOCIACION, asi que para evitar imprevistos lo mejor es tener siempre al menos un canal disponible.
    /// Usualmente crearemos canales dummy en el metodo <c>Start</c> y al acabar una llamada.
    /// </summary>
    /// <returns>Devuelve el <see cref="IEnumerator"/> que identifica esta corrutina.</returns>
    private IEnumerator CreateRandomChannel()
    {
        // Esperamos hasta que la conexion este presente.
        yield return new WaitUntil(() => connection != null && connection.Peer != null);

        // Generamos un uuid
        Guid uuid = Guid.NewGuid();

        // Creamos un canal de datos
        GetOrCreateDataChannel(uuid.ToString());
    }

    /// <summary>
    /// Termina la llamada pasado un tiempo si no se obtiene respuesta de parte del peer.
    /// </summary>
    /// <returns>Devuelve el <see cref="IEnumerator"/> que identifica esta corrutina.</returns>
    private IEnumerator CallTimeoutCoroutine()
    {
        Debug.Log("Call timeout ready...");
        yield return new WaitForSecondsRealtime(callTimeoutSeconds);

        if (onCall)
        {
            Debug.Log("Call timeout avoided!");
            callTimeoutCoroutine = null;
            ignoreCallButtonPresses = false;
        }
        else
        {
            Debug.LogWarning("Call timeout!");
            EndCall(true);
        }
    }
}
