using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using TMPro;
public class GameManager : MonoBehaviour
{
    public bool croakEnabled;

    public static GameManager instance;
    public TextMeshProUGUI alert;
    public Slider healthSlider;
    public Slider energySlider;
    public TextMeshProUGUI fireflycount;
    public FrogCharacter myFrog;
    public FrogSon mySon;
    public bool hudUpdate = false;
    public float timeMultiplyer = 1.0f;
    public UnityEngine.Rendering.Universal.Vignette vignette;
    public VolumeProfile volumeProfile;
    bool vignetteGoDown;
    float alertDuration;

    public bool lowHealth = false;
    // Basic Singleton
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(instance);
    }

    private void Start()
    {
        vignetteGoDown = true;
        vignette =(UnityEngine.Rendering.Universal.Vignette) volumeProfile.components[2];
        Cursor.lockState = CursorLockMode.Locked;
        UpdateHUD();
    }

    // Update is called once per frame
    void Update()
    {
        if(healthSlider.value < 20)
            lowHealth = true;
        else
            lowHealth = false;

        if (hudUpdate)
            UpdateHUD();
        if(lowHealth)
            LowHealth();
        else
            vignette.intensity.value = 0;


        if (alertDuration > 0.0f)
        {
            alertDuration -= Time.deltaTime;
            if (alertDuration <= 0.0f)
            {
                alert.text = "";
            }
        }
    }

    void LowHealth()
    {
        if(vignetteGoDown)
        {
            vignette.intensity.value -= 0.3f * Time.deltaTime;
            if (vignette.intensity.value < 0.3f)
                vignetteGoDown = false;
        }
        else
        {
            if (vignette.intensity.value > 0.6f)
                vignetteGoDown = true;
            vignette.intensity.value += 5 * Time.deltaTime;
        }
          
    }

    IEnumerator LowHealth(float time)
    {
        yield return new WaitForSeconds(time);
        vignetteGoDown = !vignetteGoDown;
    }

    /// <summary>
    /// Updates HUD when told
    /// </summary>
    private void UpdateHUD()
    {
        healthSlider.value = myFrog.currentHealth;
        energySlider.value = myFrog.currentEnergy;
        fireflycount.text = myFrog.fireflies.ToString();
        hudUpdate = false;
    }

    /// <summary>
    /// prints an alert to the HUD
    /// </summary>
    /// <param name="message"></param>
    public void Alert(string message)
    {
        alert.text = message;
        alertDuration = 1.5f;
    }

    public void Alert(string message, float duration)
    {
        alert.text = message;
        alertDuration = duration;
    }
}
