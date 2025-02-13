// Handles the camera for the main menu orbit demo

// LIBRARIES
using UnityEngine;


// CLASS DEFINITION
public class CameraController : MonoBehaviour
{

    // a mode for each situation
    public enum Mode { MainMenu, Simulator }

    public int mode { get; private set; }

    [Header("Main Menu Mode")]
    public Vector3 centre;
    public Vector3 offset;
    public Vector3 angle;

    [Header("Simulator Mode")]
    public float sensitivity;
    public float speed;
    public float slowRate, normalRate, fastRate;
    private float currentSpeed;

    // retrieves the main camera
    private void Initialise()
    {
        mode = (int) Mode.MainMenu;

        Debug.Log("[CAMERA] SUCCESS: Camera Loaded");
    }

    // updates panning per frame (movement & rotation)
    private void Update()
    {
        if (mode == (int) Mode.MainMenu)
        {
            RunMainMenuMode();
        } 
        else if (mode == (int) Mode.Simulator)
        {
            RunSimulatorMode();
        } 
        else 
        {
            return;
        }
    }

    // runs the camera in Main Menu mode
    private void RunMainMenuMode()
    {
        Camera.main.transform.SetPositionAndRotation(
            centre + offset, Quaternion.Euler(angle));
    }

    // runs the camera in Simulator mode
    private void RunSimulatorMode()
    {
        Movement();
        if (Input.GetMouseButton(1)) {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;
            Rotation();
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    // updates the camera's movement based on key presses
    // * NOTE: no need for separate WASD binds - automatically done by input!!!
    private void Movement()
    {
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        
        if (Input.GetKey(KeyCode.LeftControl))
        {
            currentSpeed = slowRate;
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = fastRate;
        }
        else
        {
            currentSpeed = normalRate;
        }

        currentSpeed *= speed;
        Camera.main.transform.Translate(currentSpeed * Time.deltaTime * input);
    }

    // updates the camera's rotation based on the mouse's turning
    private void Rotation()
    {
        Vector3 mouseInput = new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0);
        Camera.main.transform.Rotate(sensitivity * mouseInput);
        
        Vector3 eulerRotation = Camera.main.transform.rotation.eulerAngles;
        Camera.main.transform.rotation = Quaternion.Euler(eulerRotation.x, eulerRotation.y, 0);
    }

    // sets the current scene to the given enum
    public void SetScene(Mode mode)
    {
        this.mode = (int) mode;
    }
}
