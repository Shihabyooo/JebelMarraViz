using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamerFly : MonoBehaviour
{
    Vector3 originalPos;
    Quaternion originalRot;

    Vector3 velocity;
    Vector3 velocityWorld;
    public float moveSpeed = 50.0f;

    public float lookSpead = 10.0f;

    bool flightMode = true;

    // Start is called before the first frame update
    void Start()
    {
        originalPos = this.transform.position;
        originalRot = this.transform.rotation;
    }


    // Update is called once per frame
    void Update()
    {
        velocity = Vector3.zero;
        velocityWorld = Vector3.zero;
        if (flightMode)
        {
            Cursor.lockState = CursorLockMode.Locked;
            GetMovementInput();
            GetLookInput();
        }

        GetOtherInput();
        Displace();
    }


    void GetMovementInput()
    {

        if (Input.GetKey(KeyCode.W))
        {
            velocity.z += 1.0f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            velocity.z -= 1.0f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            velocity.x += 1.0f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            velocity.x -= 1.0f;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            velocityWorld.y += 1.0f;
        }
        if (Input.GetKey(KeyCode.C))
        {
            velocityWorld.y -= 1.0f;
        }

    }

    void GetLookInput()
    {
        float xRot = Input.GetAxisRaw("Mouse X");
        float yRot = Input.GetAxisRaw("Mouse Y");

        this.transform.Rotate(Vector3.up, xRot * lookSpead * Time.deltaTime, Space.World);
        this.transform.Rotate(Vector3.right, yRot * -1.0f * lookSpead * Time.deltaTime, Space.Self);

        Compass.compass.RotateCompass(xRot * lookSpead * Time.deltaTime);

        if (Input.GetKey(KeyCode.Q))
        {
            this.transform.Rotate(Vector3.forward, lookSpead * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.E))
        {
            this.transform.Rotate(Vector3.forward, -1.0f * lookSpead * Time.deltaTime);
        }
        //print("xRot: " + xRot);
    }

    void GetOtherInput()
    {
        if (Input.GetKeyDown(KeyCode.R))
            ResetPositionAndView();
        if (Input.GetKeyDown(KeyCode.P))
            if (PrecipitationManager.precMan != null)
                PrecipitationManager.precMan.UpdateVisualization();
        if (Input.GetKeyDown(KeyCode.O))
            if (GroundCreator.grCreator != null)
                GroundCreator.grCreator.UpdateTopography();
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (flightMode)
                Cursor.lockState = CursorLockMode.Confined;
            flightMode = !flightMode;
        }
        if (Input.GetKeyDown(KeyCode.RightBracket))
            PrecipitationManager.precMan.SwitchMonthOffset(1);
        else if (Input.GetKeyDown(KeyCode.LeftBracket))
            PrecipitationManager.precMan.SwitchMonthOffset(-1);

        if (Input.GetKeyDown(KeyCode.Comma))
            GroundCreator.grCreator.SwitchResolutionOffset(-1);
        else if (Input.GetKeyDown(KeyCode.Period))
            GroundCreator.grCreator.SwitchResolutionOffset(1);

    }

    void Displace()
    {
        this.transform.Translate(velocity * moveSpeed * Time.deltaTime);
        this.transform.Translate(velocityWorld * moveSpeed * Time.deltaTime, Space.World);
    }

    public void ResetPositionAndView()
    {
        this.transform.position = originalPos;
        this.transform.rotation = originalRot;
        Compass.compass.ResetRotation();
    }
}
