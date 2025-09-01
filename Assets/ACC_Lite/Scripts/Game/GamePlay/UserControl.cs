using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;

/// <summary>
/// For user multiplatform control or AI-controlled vehicles.
/// </summary>
[RequireComponent(typeof(CarController))]
public class UserControl : MonoBehaviour
{
    CarController ControlledCar;

    public float Horizontal { get; private set; }
    public float Vertical { get; private set; }
    public bool Brake { get; private set; }

    private PhotonView photonView;
    public static MobileControlUI CurrentUIControl { get; set; }

    [Header("AI Settings")]
    public bool isAI = false;
    public float aiSteering = 0f;
    public float aiThrottle = 1f;
    public bool aiBrake = false;

    private void Awake()
    {
        ControlledCar = GetComponent<CarController>();
        CurrentUIControl = FindObjectOfType<MobileControlUI>();
    }

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    void Update()
    {
        if (isAI)
        {
            // AI logic: move forward constantly, steering optionally
            Horizontal = aiSteering;
            Vertical = aiThrottle;
            Brake = aiBrake;

            ControlledCar.UpdateControls(Horizontal, Vertical, Brake);
        }
        else if (PhotonManager.Instance.isFreeRoam)
        {
            // FreeRoam mode - always update controls regardless of race start or Photon ownership
            if (CurrentUIControl != null && CurrentUIControl.ControlInUse)
            {
                Horizontal = CurrentUIControl.GetHorizontalAxis;
                Vertical = CurrentUIControl.GetVerticalAxis;
                // Optionally set Brake if you have it in UI controls for FreeRoam
               // Brake = CurrentUIControl.GetBrakeButtonState ? true : false;
            }
            else
            {
                Horizontal = Input.GetAxis("Horizontal");
                Vertical = Input.GetAxis("Vertical");
                Brake = Input.GetButton("Jump");
            }

            ControlledCar.UpdateControls(Horizontal, Vertical, Brake);
        }
        else if (photonView.IsMine && RC_UIManager.Instance.CanStartRace)
        {
            if (CurrentUIControl != null && CurrentUIControl.ControlInUse)
            {
                Horizontal = CurrentUIControl.GetHorizontalAxis;
                Vertical = CurrentUIControl.GetVerticalAxis;
            }
            else
            {
                Horizontal = Input.GetAxis("Horizontal");
                Vertical = Input.GetAxis("Vertical");
                Brake = Input.GetButton("Jump");
            }

            ControlledCar.UpdateControls(Horizontal, Vertical, Brake);
        }
    }

}
