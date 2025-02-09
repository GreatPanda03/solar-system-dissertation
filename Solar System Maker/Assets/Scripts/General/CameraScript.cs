// Handles the camera for the main menu orbit demo

using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public enum Scenes {
        Main_Menu, Simulator
    }

    private int scene;
    public int Scene {
        get => scene;
        set => scene = value;
    }

    // attributes for main menu camera
    public Vector3 centre;
    public Vector3 offset;
    public Vector3 angle;

    // attributes for the simulator camera
    public float sensitivity;
    public float speed;
    public float slowRate, normalRate, fastRate;
    private float currentSpeed;

    // preparing the offset and angle of the camera from the initialised centre
    private void Awake()
    {
        scene = (int) Scenes.Main_Menu;
        Debug.Log("[CAMERA] SUCCESS: Camera Loaded");
    }

    // updates panning per frame (movement & rotation)
    private void Update()
    {
        if (scene == (int) Scenes.Main_Menu)
        {
            transform.position = centre + offset;
            transform.rotation = Quaternion.Euler(angle);
        }
        else if (scene == (int) Scenes.Simulator)
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
        transform.Translate(currentSpeed * Time.deltaTime * input);
    }

    // updates the camera's rotation based on the mouse's turning
    private void Rotation()
    {
        Vector3 mouseInput = new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0);
        transform.Rotate(sensitivity * mouseInput);
        
        Vector3 eulerRotation = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(eulerRotation.x, eulerRotation.y, 0);
    }

    // sets the current scene to the given enum
    public void SetScene(Scenes scene)
    {
        this.scene = (int) scene;
    }
}
