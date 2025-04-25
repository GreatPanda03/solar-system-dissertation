// Handles the camera for the main menu orbit demo

// LIBRARIES
using UnityEngine;


// CLASS DEFINITION
public class CameraController : MonoBehaviour
{

    // a mode for each situation
    public enum CameraMode { MainMenu, Simulator, Following }

    private static CameraMode cameraMode;

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
    void Start()
    {
        cameraMode = CameraMode.MainMenu;

        Debug.Log("Camera loaded");
    }


    #region Camera Loop

    // sets the current scene to the given enum
    public static void SetCameraMode(CameraMode newMode)
    {
        cameraMode = newMode;
    }

    // updates panning per frame (movement & rotation)
    private void Update()
    {
        if (cameraMode == CameraMode.MainMenu)
        {
            RunMainMenuMode();
        } 
        else if (cameraMode == CameraMode.Simulator)
        {
            RunSimulatorMode();
        } 
        else if (cameraMode == CameraMode.Following)
        {
            RunFollowingMode();
        }
        else
        {
            Debug.Log($"'{cameraMode}' is invalid!");
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
        RunSimulatorMovement();

        if (Input.GetMouseButton(1)) { // turn on right mouse button click
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;
            RunSimulatorRotation();
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    // updates the camera's movement based on key presses
    // * NOTE: no need for separate WASD binds - automatically done by input!!!
    private void RunSimulatorMovement()
    {
        if (UIController.IsTextFieldFocused()) return;

        Vector3 input = new(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        
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
    private void RunSimulatorRotation()
    {
        Vector3 mouseInput = new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0);
        Camera.main.transform.Rotate(sensitivity * mouseInput);
        
        Vector3 eulerRotation = Camera.main.transform.rotation.eulerAngles;
        Camera.main.transform.rotation = Quaternion.Euler(eulerRotation.x, eulerRotation.y, 0);
    }

    #endregion Simulator Mode


    #region Body Following Mode

    // sets the given body as the subject to follow
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

    // handles following mode
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
