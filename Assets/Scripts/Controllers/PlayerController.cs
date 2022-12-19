using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CarController))]
public class PlayerController : MonoBehaviour
{
    private CarController car;
    private Rigidbody rb;
    private float startPosZ;

    public void HandleGasInput(InputAction.CallbackContext context)
    {
        car.gasValue = context.ReadValue<float>(); ;
    }

    public void HandleTurnInput(InputAction.CallbackContext context)
    {
        car.turnValue = context.ReadValue<float>();
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            GameManager.Instance.IsPaused = !GameManager.Instance.IsPaused;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        car = GetComponent<CarController>();
    }

    private void Start()
    {
        startPosZ = rb.position.z;

        GameManager.Instance.OnLoaded += () =>
        {
            car.SetCarConfigInstance(GameManager.Instance.PlayerCarConfig);
            car.GetCarConfig().BodyColor = GameManager.Instance.LevelData.playerCarColor;
        };

        GameManager.Instance.OnGameStart += () => 
        {
            car.isWorking = true;
            car.ResetState();
            car.turnValue = 0.0f;
            car.gasValue = 0.0f;
            transform.SetPositionAndRotation(
                Vector3.Scale(transform.position, new Vector3(0f, 1f, 1f)),
                Quaternion.Euler(0f, 0f, 0f)
            );
            MapManager.Instance.MoveSpeed = 0f;
        };
        GameManager.Instance.OnGameEnd += (reason) =>
        {
            car.isWorking = false;
        };
    }

    private void FixedUpdate()
    {
        MapManager.Instance.MoveAcceleration = car.Acceleration;

        if (!Mathf.Approximately(rb.velocity.z, 0f))
        {
            var vel = rb.velocity;
            //MapManager.Instance.MoveSpeed += rb.velocity.z;
            vel.z = 0f;
            rb.velocity = vel;
        }

        var pos = rb.position;
        pos.z = startPosZ;
        rb.MovePosition(pos);
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        HandleCollision(collision);
    }

    private void HandleCollision(Collision collision)
    {
        float deltaVel = rb.velocity.z;
        MapManager.Instance.MoveSpeed += deltaVel;

        var vel = rb.velocity;
        vel.z = 0f;
        rb.velocity = vel;

        //var pos = rb.position;
        //pos.z = originPosZ;
        //rb.MovePosition(pos);

        if (Mathf.Abs(deltaVel) >= 8f)
        {
            GameManager.Instance.EndGame(GameManager.EndReasonCrash);
        }
    }
}
