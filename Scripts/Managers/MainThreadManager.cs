using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ejecuta porciones de codigo en el hilo principal de Unity.
/// </summary>
internal class MainThreadManager : MonoBehaviour
{
    /// <summary>
    /// Instancia de este manager.
    /// </summary>
    internal static MainThreadManager manager;

    /// <summary>
    /// Cola de acciones a realizar.
    /// </summary>
    private Queue<Action> jobs = new Queue<Action>();

    /// <summary>
    /// Awake se ejecuta justo cuando se instancia el objeto.
    /// </summary>
    private void Awake()
    {
        manager = this;
    }

    /// <summary>
    /// Update se ejecuta en cada frame.
    /// </summary>
    private void Update()
    {
        while (jobs.Count > 0)
        {
            jobs.Dequeue().Invoke();
        }
    }

    /// <summary>
    /// Añade una accion a la cola.
    /// </summary>
    /// <param name="newJob">La accion <see cref="Action"/> a ejecutar.</param>
    internal void AddJob(Action newJob)
    {
        jobs.Enqueue(newJob);
    }
}
