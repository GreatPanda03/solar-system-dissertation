// manages camera controls across the application

using UnityEngine;

public class CameraController : MonoBehaviour
{

    #region Attributes

    public enum Mode { Fixed, Free, Follow }
    private static Mode cameraMode;
    public static Mode CameraMode {
        get => cameraMode;
        set => cameraMode = value;
    }

    // these properties can be set from the inspector
    [Header("Fixed Mode")]
    public Vector3 centre;
    public Vector3 offset;
    public Vector3 angle;

    [Header("Free Mode")]
    public float sensitivity;
    public float speed;
    public float slowRate, normalRate, fastRate;
    private float currentSpeed;

    [Header("Follow Mode")]
    public float followDistance;

    private static RenderableBody subject;
    [HideInInspector] public static RenderableBody Subject {
        get => subject;
        set => subject = value;
    }

    #endregion Attributes


    #region Start Up

    void Start()
    {
        cameraMode = Mode.Fixed;
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

    private void Update()
    {
        if (cameraMode == Mode.Fixed)
        {
            RunFixedMode();
        } 
        else if (cameraMode == Mode.Free)
        {
            RunFreeMode();
        } 
        else if (cameraMode == Mode.Follow)
        {
            RunFollowMode();
        }
        else
        {
            Debug.Log($"'{cameraMode}' is invalid!");
            return;
        }
    }

    #endregion


    #region Fixed Mode

    // fixes the camera at the specified point at an angled offset
    private void RunFixedMode()
    {
        Camera.main.transform.SetPositionAndRotation(
            centre + offset, Quaternion.Euler(angle));
    }

    #endregion Fixed Mode


    #region Free Mode

    // allows the camera to be moved and turned freely in all directions
    private void RunFreeMode()
    {
        RunFreeMovement();

        if (Input.GetMouseButton(1)) {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;
            RunFreeRotation();
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    // updates the camera's movement based on key presses
    private void RunFreeMovement()
    {
        if (UIController.FocusedField != null) return;

        Vector3 input = new(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        
        if (Input.GetKey(AppControls.keybinds["Move Slower"]))
        {
            currentSpeed = slowRate;
        }
        else if (Input.GetKey(AppControls.keybinds["Move Faster"]))
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
    private void RunFreeRotation()
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

    #endregion Free Mode


    #region Follow Mode

    // sets the camera to follow the given subject
    public static void SetToFollow(RenderableBody newSubject)
    {
        subject = newSubject;
        cameraMode = Mode.Follow;
    }

    // sets the camera to stop following the current subject
    public static void SetToUnfollow()
    {
        cameraMode = Mode.Free;
        subject = null;
    }

    // only allows rotation around a subject
    private void RunFollowMode()
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

    #endregion Follow Mode
}
