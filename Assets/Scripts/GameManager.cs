/*
 * =====================================================================================
 *
 * Filename:  GameManager.cs
 *
 * Description:  Script de gesti�n general del juego. Actualmente se encarga de reiniciar el nivel.
 *
 * Authors:  Carlos Hernan Gonzalez Gonzales
 * Eduardo Calderon Trejo
 * Cesar Sasia Zayas
 *
 * Materia:  Inteligencia Artificial e Ingenier�a del Conocimiento
 *
 * =====================================================================================
 */

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Funci�n p�blica que puede ser llamada desde otros scripts o botones de UI
    /// para reiniciar la escena actual.
    /// </summary>
    public void RestartLevel()
    {
        // Carga la escena que est� actualmente activa, obteniendo su nombre din�micamente.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /*
     * ================================================================
     * CONSULTAS A IA
     * ================================================================
     * 1. �C�mo puedo reiniciar el nivel actual en Unity?
     * - Se consult� sobre el uso de `SceneManager.LoadScene` en combinaci�n con
     * `SceneManager.GetActiveScene().name` para recargar la escena activa.
     * ================================================================
     */
}