using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiController : MonoBehaviour
{
    public JetController jetController;

    public Text speedText;
    public Text heightText;
    public Text throttleText;

    void Update()
    {
        if (jetController == null)
        {
            Debug.LogError("Jet controller is missing");
            return;
        }

        speedText.text = $"Speed: {jetController.speed:n0}";
        heightText.text = $"Height: {jetController.height:n0}";
        throttleText.text = $"Throttle: {jetController.throttle:n0}%";
    }
}
