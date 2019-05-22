using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimulationSpeedManager : MonoBehaviour {

    public Image pauseImage;
    public Image playImage;
    public Image speedUpImage;

    public float speedUpMultiplier = 2f;

    private void Start()
    {
        PlaySimulation();
    }

    public void PauseSimulation()
    {
        Time.timeScale = 0.0f;
        pauseImage.color = Color.green;
        playImage.color = Color.white;
        speedUpImage.color = Color.white;
    }

    public void PlaySimulation()
    {
        Time.timeScale = 1f;
        pauseImage.color = Color.white;
        playImage.color = Color.green;
        speedUpImage.color = Color.white;
    }

    public void SpeedUpSimulation()
    {
        if (speedUpMultiplier < 1)
        {
            speedUpMultiplier = 2;
        }

        Time.timeScale = speedUpMultiplier;
        pauseImage.color = Color.white;
        playImage.color = Color.white;
        speedUpImage.color = Color.green;
    }
}
