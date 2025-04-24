// Handles the camera for the main menu orbit demo

// LIBRARIES
using UnityEngine;


// CLASS DEFINITION
public class CameraController : MonoBehaviour
{

    // a mode for each situation
    public enum Mode { MainMenu, Simulator, Following }

    public static Mode mode { get; private set; }

    [Header("Main Menu Mode")]
    public Vector3 centre;
    public Vector3 offset;
    public Vector3 angle;

    [Header("Simulator Mode")]
    public float sensitivity;
    public float speed;
    public float slowRate, normalRate, fastRate;
    private float currentSpeed;

    [Header("Follow Mode")]
    public static Body subject;
    public static float followDistance;
    public static Vector3 followRotation;


    // retrieves the main camera
    private void Initialise()
    {
        mode = (int) Mode.MainMenu;

        Debug.Log("[CAMERA] SUCCESS: Camera Loaded");
    }


    #region Mode Control

    // sets the current scene to the given enum
    public static void SetScene(Mode newMode)
    {
        mode = newMode;
    }

    // updates panning per frame (movement & rotation)
    private void Update()
    {
        if (mode == Mode.MainMenu)
        {
            RunMainMenuMode();
        } 
        else if (mode == Mode.Simulator)
        {
            RunSimulatorMode();
        } 
        else if (mode == Mode.Following)
        {
            RunFollowingMode();
        }
        else
        {
            return;
        }
    }

    #endregion


    #region Main Menu Mode

    // runs the camera in Main Menu mode
    private void RunMainMenuMode()
    {
        Camera.main.transform.SetPositionAndRotation(
            centre + offset, Quaternion.Euler(angle));
    }

    #endregion


    #region Simulator Mode

    // runs the camera in Simulator mode
    private void RunSimulatorMode()
    {
        Movement();

        if (Input.GetMouseButton(1)) { // turn on right mouse button click
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
        if (UIController.IsTextFieldFocused()) return;

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

    #endregion Simulator Mode


    #region Body Following Mode

    public static void SetSubject(Body body)
    {
        followDistance = 30f;
        followRotation = Camera.main.transform.forward;
        subject = body;
    }

    public static Body GetSubject()
    {
        return subject;
    }

    private void RunFollowingMode()
    {
        if (subject == null) return;

        Vector3 subjectPos = subject.Entity.transform.position;
        Vector3 cameraRot = Camera.main.transform.forward;
        Vector3 targetPos = subjectPos + (cameraRot * -followDistance);
        
        Camera.main.transform.position = targetPos;
        // Vector3.Lerp(cameraPos, targetPos, 0.1f);

        // TODO: make rotation smoother! rotate around the subject rather than rotating the camera

        // rotating on right click
        if (Input.GetMouseButton(1)) {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;
            
            Vector3 mouseInput = new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0);
            Camera.main.transform.Rotate(sensitivity * mouseInput);
            
            Vector3 eulerRotation = Camera.main.transform.rotation.eulerAngles;
            Camera.main.transform.rotation = Quaternion.Euler(eulerRotation.x, eulerRotation.y, 0);
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    #endregion Body Following Mode
}
