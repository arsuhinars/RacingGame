using UnityEngine;

[SelectionBase]
[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    private const string SparksPoolName = "Sparks";
    private const string DebrisPoolName = "Debris";
    private const string HitSoundPoolName = "HitSound";
    private const string CrashSoundPoolName = "CrashSound";
    private const float SparksSpawnDelay = 0.1f;
    private const float GasSmoothTime = 0f;
    private const float VelocitySmoothTime = 0.1f;
    private const float WheelRotationSmoothTime = 0.1f;

    public bool isOncoming = false;

    public float Speed
    {
        get => rb.velocity.z + MapManager.Instance.MoveSpeed;
        set
        {
            var vel = rb.velocity;
            vel.z = (!isOncoming ? value : -value) - MapManager.Instance.MoveSpeed;

            rb.velocity = vel;
        }
    }
    public Vector3 Velocity => rb.velocity + Vector3.forward * MapManager.Instance.MoveSpeed;
    public float Acceleration { get; private set; }
    [HideInInspector] public float gasValue;
    [HideInInspector] public float turnValue;
    [HideInInspector] public bool isWorking = true;     // Работает ли двигатель автомобиля

    private Rigidbody rb;
    private CarConfig carConfig;

    private float currGasValue;
    private float gasValueVel;

    private float angleVelocity;
    private Vector3 velAccel;

    private float currWheelAngle;
    private float wheelAngleVel;
    private Quaternion wheelRot = Quaternion.identity;

    private static readonly Quaternion rotateOnPI = Quaternion.Euler(0.0f, 180.0f, 0.0f);

    private static float sparksSpawnTime;
    
    public void SetCarConfigInstance(CarConfig carConfigInstance)
    {
        carConfig = carConfigInstance;

        if (carConfig != null)
        {
            if (carConfig.engineAudio != null)
            {
                carConfig.engineAudio.Play();
            }
            carConfig.transform.SetParent(transform);
            carConfig.transform.localPosition = Vector3.zero;
            carConfig.transform.localRotation = Quaternion.identity;
        }
    }

    public CarConfig GetCarConfig() => carConfig;

    public void ResetState()
    {
        if (carConfig != null && carConfig.engineAudio != null)
            carConfig.engineAudio.Play();

        currGasValue = 0f;
        gasValueVel = 0f;
        angleVelocity = 0f;
        velAccel = Vector3.zero;
        wheelAngleVel = 0f;
        currWheelAngle = 0f;
        rb.velocity = Vector3.zero;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        SetCarConfigInstance(GetComponentInChildren<CarConfig>());

        MapManager.Instance.OnInstantSpeedChange += OnLevelsInstantSpeedChange;
    }

    private void OnDisable()
    {
        if (carConfig != null)
        {
            carConfig.engineAudio.Stop();
        }
    }

    private void OnDestroy()
    {
        MapManager.Instance.OnInstantSpeedChange -= OnLevelsInstantSpeedChange;
    }

    private void FixedUpdate()
    {
        if (carConfig == null)
        {
            rb.velocity = Vector3.zero;
            return;
        }

        float currSpeed = rb.velocity.z + MapManager.Instance.MoveSpeed;
        if (CompareTag("Player"))
            currSpeed = MapManager.Instance.MoveSpeed;

        if (isOncoming)
            currSpeed *= -1.0f;

        // Находим текущее ускорение
        if (Mathf.Abs(currSpeed / Time.fixedDeltaTime) > carConfig.frictionDeceleration)
            Acceleration = Mathf.Sign(currSpeed) * -carConfig.frictionDeceleration;
        else
            Acceleration = -currSpeed / Time.fixedDeltaTime;

        if (isWorking)
        {
            currGasValue = Mathf.SmoothDamp(currGasValue, gasValue, ref gasValueVel, GasSmoothTime);
            if (currGasValue > Mathf.Epsilon && currSpeed < currGasValue * carConfig.maxSpeed)
            {
                Acceleration = carConfig.gasAcceleration;
            }

            if (currGasValue < -Mathf.Epsilon)
            {
                if (Mathf.Abs(currSpeed / Time.fixedDeltaTime) > carConfig.breaksDeceleration)
                    Acceleration = Mathf.Sign(currSpeed) * -carConfig.breaksDeceleration;
                else
                    Acceleration = -currSpeed / Time.fixedDeltaTime;
            }
        }

        if (isOncoming)
            Acceleration *= -1.0f;

        // Придаем автомобилю ускорение
        rb.AddForce(Vector3.forward * Acceleration, ForceMode.Acceleration);
        rb.AddForce(-Vector3.forward * MapManager.Instance.MoveAcceleration, ForceMode.Acceleration);

        // Изменяем скорость автомобиля в соотвествии с его направлением
        var vel = rb.velocity + Vector3.forward * MapManager.Instance.MoveSpeed;

        var targetVel = Mathf.Sign(vel.z) * vel.magnitude * transform.forward;
        if (isOncoming)
            targetVel *= -1.0f;

        vel = Vector3.SmoothDamp(vel, targetVel, ref velAccel, VelocitySmoothTime);
        vel -= Vector3.forward * MapManager.Instance.MoveSpeed;
        rb.AddForce(vel - rb.velocity, ForceMode.VelocityChange);

        // Получаем угол поворота авто
        float currAngle = rb.rotation.eulerAngles.y;
        if (!isOncoming)
        {
            if (currAngle > 180.0f)
                currAngle -= 360.0f;
        }
        else currAngle -= 180.0f;
        currAngle = Mathf.Clamp(currAngle, -carConfig.maxTurnAngle, carConfig.maxTurnAngle);

        float deltaTime = Time.fixedDeltaTime * Mathf.Abs(currSpeed) / carConfig.maxSpeed;

        if (!Mathf.Approximately(deltaTime, 0.0f))
        {
            // Поворачиваем авто согласно повороту руля
            currAngle = Mathf.SmoothDamp(
                currAngle,
                turnValue * carConfig.maxTurnAngle,
                ref angleVelocity,
                carConfig.angleSmoothTime * TimeCycleManager.Instance.CarAngleSmoothTimeFactor,
                Mathf.Infinity,
                deltaTime
            );
        }

        if (isOncoming)
            currAngle += 180.0f;

        // Применяем поворот
        var rot = Quaternion.Euler(0.0f, currAngle, 0.0f);
        rb.MoveRotation(rot);

        // Поворачиваем колеса согласно рулю
        currWheelAngle = Mathf.SmoothDamp(currWheelAngle, turnValue * carConfig.maxTurnAngle, ref wheelAngleVel, WheelRotationSmoothTime);
        var wheelVerticalRot = Quaternion.Euler(0.0f, currWheelAngle, 0.0f);

        // Вращаем колеса
        wheelRot *= Quaternion.Euler(currSpeed * Time.fixedDeltaTime * 180.0f / Mathf.PI / carConfig.wheelRadius, 0.0f, 0.0f);

        if (!isOncoming)
        {
            carConfig.wheelFR.localRotation = wheelVerticalRot * wheelRot;
            carConfig.wheelFL.localRotation = rotateOnPI * wheelVerticalRot * Quaternion.Inverse(wheelRot);
        }
        else
        {
            carConfig.wheelFR.localRotation = wheelVerticalRot * wheelRot;
            carConfig.wheelFL.localRotation = rotateOnPI * wheelVerticalRot * Quaternion.Inverse(wheelRot);
        }

        carConfig.wheelBR.localRotation = wheelRot;
        carConfig.wheelBL.localRotation = rotateOnPI * Quaternion.Inverse(wheelRot);

        // Обновляем звук двигателя
        if (carConfig.engineAudio != null)
        {
            carConfig.engineAudio.pitch = Mathf.LerpUnclamped(
                carConfig.engineMinPitch,
                carConfig.engienMaxPitch,
                currSpeed / carConfig.maxSpeed
            );
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        var pos = Vector3.zero;
        for (int i = 0; i < collision.contactCount; i++)
        {
            pos += collision.GetContact(i).point;
        }
        pos /= collision.contactCount;

        float deltaSpeed = Mathf.Abs(collision.impulse.z / rb.mass * 0.5f);
        if (deltaSpeed > 5f)
        {
            var crashSoundPool = PoolsManager.Instance.GetPool(CrashSoundPoolName);
            if (crashSoundPool != null)
            {
                var crashSound = crashSoundPool.Get();
                crashSound.transform.position = pos;
            }

            var debrisPool = PoolsManager.Instance.GetPool(DebrisPoolName);
            if (debrisPool != null)
            {
                var debris = debrisPool.Get();

                debris.transform.position = pos;

                var particleMain = debris.ParticleSystem.main;
                particleMain.customSimulationSpace = MapManager.Instance.GetLastSection().transform;
                particleMain.startColor = new ParticleSystem.MinMaxGradient(carConfig.BodyColor);

                debris.ParticleSystem.Play();
            }
        }
        else
        {
            // Создаем звук удара
            var hitPool = PoolsManager.Instance.GetPool(HitSoundPoolName);
            if (hitPool != null)
            {
                var hitSound = hitPool.Get();
                hitSound.transform.position = pos;
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (Time.time - sparksSpawnTime < SparksSpawnDelay)
            return;

        sparksSpawnTime = Time.time;

        var relativeVel = collision.rigidbody == null ? Velocity : collision.relativeVelocity;
        if (relativeVel.sqrMagnitude < 25f)
            return;

        var normal = Vector3.zero;
        var pos = Vector3.zero;

        for (int i = 0; i < collision.contactCount; i++)
        {
            var contact = collision.GetContact(i);
            normal += contact.normal;
            pos += contact.point;
        }

        normal /= collision.contactCount;
        pos /= collision.contactCount;

        var normalAngle = Vector3.Angle(Velocity, -normal);
        if (normalAngle < 75f || normalAngle > 105f)
            return;

        // Создаем частицу искр
        var sparksPool = PoolsManager.Instance.GetPool(SparksPoolName);
        if (sparksPool != null)
        {
            var sparks = sparksPool.Get();

            sparks.transform.SetPositionAndRotation(
                pos,
                Quaternion.LookRotation(-Velocity)
            );

            var particleMain = sparks.ParticleSystem.main;
            particleMain.customSimulationSpace = MapManager.Instance.GetLastSection().transform;
            particleMain.startSpeed = new ParticleSystem.MinMaxCurve(Speed * 0.8f, Speed * 1.2f);

            sparks.ParticleSystem.Play();
        }
    }

    private void OnLevelsInstantSpeedChange(float speed, float old)
    {
        if (isActiveAndEnabled && carConfig != null && !CompareTag("Player"))
            rb.AddForce(Vector3.forward * (old - speed), ForceMode.VelocityChange);
    }
}
