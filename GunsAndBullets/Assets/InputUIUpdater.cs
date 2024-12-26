using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class InputUIUpdater : MonoBehaviour
{
    public TextMeshProUGUI inputText;
    PlayerMovement pm;
    Swinging swinging;
    Grappling grappling;
    void Start()
    {
        grappling = GetComponent<Grappling>();
        swinging = GetComponent<Swinging>();
        pm = GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        if(pm)
        {
            switch(pm.state)
            {
                case PlayerMovement.MovementState.air:
                inputText.text = "WASD - Direction";
                if(grappling && (pm.state != PlayerMovement.MovementState.grappliing|| pm.state != PlayerMovement.MovementState.freeze))
                    inputText.text+= "\nRMB - To Grapple";
                break;

                case PlayerMovement.MovementState.walking:
                inputText.text = "WASD - Direction\nSpace - Jump";
                break;

                case PlayerMovement.MovementState.crouching:
                inputText.text = "WASD - Direction";

                break;

                case PlayerMovement.MovementState.climbing:
                inputText.text = "W - Go up\nS - Go Down\nA and D - To Move Sideways\nSpace - Wall Jump";
                break;

                case PlayerMovement.MovementState.wallrunning:
                inputText.text = "W - Continue Wall Run\nSpace - Wall Jump";
                break;

                case PlayerMovement.MovementState.swinging:
                inputText.text = "W,A and D - Direction\nS - Reel It Out\nSpace - Reel It IN";
                break;

                case PlayerMovement.MovementState.grappliing:
                inputText.text = "";
                break;

                case PlayerMovement.MovementState.freeze:
                inputText.text = "";
                break;

                case PlayerMovement.MovementState.sliding:
                inputText.text = "WASD - Direction\nSpace - Jump";
                break;

                case PlayerMovement.MovementState.sprinting:
                inputText.text = "WASD - Direction\nSpace - Jump\nCTRL - Sliding";
                break;
            }
        }

        string extraText = "";

        if(swinging&&swinging.swingPoint!=Vector3.zero && pm.state != PlayerMovement.MovementState.swinging)
        {
            extraText = "\nLMB - To Swing";
        }
        
        inputText.text+=extraText;
    }
}
