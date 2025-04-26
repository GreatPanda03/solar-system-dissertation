// manages camera controls across the application

using UnityEngine;

public class CameraController : MonoBehaviour
{

    #region Attributes

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
    public static float followDistance = 30f;

    #endregion Attributes


    #region Start Up

    void Start()
    {
        cameraMode = CameraMode.MainMenu;

        Debug.Log("Camera loaded");
    }

    #endregion Start Up


    #region Auxiliary Methods

    // restricts the given Quaternion's pitch to the given range
    // * mainly to stop the camera from going crazy when looking directly up or down
    public Quaternion RestrictQuaternionPitch(
        Quaternion rotation, float lowerPitch, float upperPitch)
    {
        Vector3 euler = rotation.eulerAngles;
        if (euler.x > 180) euler.x -= 360; // wrap around
        euler.x = Mathf.Clamp(euler.x, lowerPitch, upperPitch);

        return Quaternion.Euler(euler.x, euler.y, 0);
    }

    #endregion


    #region Camera Loop

    public static void SetCameraMode(CameraMode newMode)
    {
        cameraMode = newMode;
    }

    public static CameraMode GetCameraMode()
    {
        return cameraMode;
    }

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

        if (Input.GetMouseButton(1)) {
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
    // * NB: no need for separate WASD binds - automatically done by input!
    private void RunSimulatorMovement()
    {
        if (UIController.FocusedField != null) return;

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

    // updates the camera's rotation with mouse movement
    private void RunSimulatorRotation()
    {
        Vector2 mouseChange = new()
        {
            x = -Input.GetAxis("Mouse Y") * sensitivity,
            y = Input.GetAxis("Mouse X") * sensitivity
        };

        Vector3 cameraOrientation = Camera.main.transform.rotation.eulerAngles;

        Quaternion cameraRotation = Quaternion.Euler(cameraOrientation.x, cameraOrientation.y, 0);
        Quaternion addedRotation = Quaternion.Euler(mouseChange.x, mouseChange.y, 0);
        Quaternion combinedRotation = cameraRotation * addedRotation;

        combinedRotation = RestrictQuaternionPitch(combinedRotation, -80, 80);

        Camera.main.transform.rotation = combinedRotation;
    }

    #endregion Simulator Mode


    #region Following Mode

    public static void SetSubject(Body body)
    {
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

        Vector2 mouseChange = new();

        // rotate on right click
        if (Input.GetMouseButton(1)) {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;

            mouseChange.x = -Input.GetAxis("Mouse Y") * sensitivity;
            mouseChange.y = Input.GetAxis("Mouse X") * sensitivity;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        Vector3 cameraOrientation = Camera.main.transform.rotation.eulerAngles;

        Quaternion cameraRotation = Quaternion.Euler(cameraOrientation.x, cameraOrientation.y, 0);
        Quaternion addedRotation = Quaternion.Euler(mouseChange.x, mouseChange.y, 0);
        Quaternion combinedRotation = cameraRotation * addedRotation;

        combinedRotation = RestrictQuaternionPitch(combinedRotation, -80, 80);

        Vector3 origin = subject.Entity.transform.position;
        Vector3 offset = combinedRotation * new Vector3(0, 0, -followDistance);
        
        Camera.main.transform.position = origin + offset;
        Camera.main.transform.rotation = combinedRotation;
    }

    #endregion Following Mode
}
