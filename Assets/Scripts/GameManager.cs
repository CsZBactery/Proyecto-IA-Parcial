/*
 * =====================================================================================
 *
 * Filename:  GameManager.cs
 *
 * Description:  Script de gestión general del juego. Actualmente se encarga de reiniciar el nivel.
 *
 * Authors:  Carlos Hernan Gonzalez Gonzales
 * Eduardo Calderon Trejo
 * Cesar Sasia Zayas
 *
 * Materia:  Inteligencia Artificial e Ingeniería del Conocimiento
 *
 * =====================================================================================
 */

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Función pública que puede ser llamada desde otros scripts o botones de UI
    /// para reiniciar la escena actual.
    /// </summary>
    public void RestartLevel()
    {
        // Carga la escena que está actualmente activa, obteniendo su nombre dinámicamente.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /*
     * ================================================================
     * CONSULTAS A IA
     * ================================================================
     * 1. ¿Cómo puedo reiniciar el nivel actual en Unity?
     * - Se consultó sobre el uso de `SceneManager.LoadScene` en combinación con
     * `SceneManager.GetActiveScene().name` para recargar la escena activa.
     * ================================================================
     */
}