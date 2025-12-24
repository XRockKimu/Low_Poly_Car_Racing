/*
MESSAGE FROM CREATOR: This script was coded by Mena. You can use it in your games either these are commercial or
personal projects. You can even add or remove functions as you wish. However, you cannot sell copies of this
script by itself, since it is originally distributed as a free product.
I wish you the best for your project. Good luck!

P.S: If you need more cars, you can check my other vehicle assets on the Unity Asset Store, perhaps you could find
something useful for your game. Best regards, Mena.
*/

using System;
using UnityEngine;
using UnityEngine.UI;

public class PrometeoCarController : MonoBehaviour
{
    public float downforce = 140f;

    //CAR SETUP
    [Space(20)]
    [Range(20, 1000)]
    public int maxSpeed = 400; // km/h
    [Range(20, 200)]
    public int maxReverseSpeed = 80; // km/h
    [Range(1, 10)]
    public int accelerationMultiplier = 10;

    [Space(10)]
    [Tooltip("Multiplier applied to torque when sprinting (e.g., 1.3 = 30% more)")]
    [Range(1f, 3f)]
    public float sprintMultiplier = 1.35f;

    [Tooltip("Maximum forward speed while sprinting (km/h)")]
    [Range(20, 2000)]
    public int sprintMaxSpeed = 500;

    [Tooltip("Key used to sprint")]
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Space(10)]
    [Range(5000f, 20000f)]
    public float enginePower = 12000f; // FAST: increase this for stronger acceleration (replaces old 7000f)

    [Space(10)]
    [Range(10, 45)]
    public int maxSteeringAngle = 27;
    [Range(0.1f, 1f)]
    public float steeringSpeed = 0.5f;

    [Space(10)]
    [Range(100, 800)]
    public int brakeForce = 650;
    [Range(1, 10)]
    public int decelerationMultiplier = 2;
    [Range(1, 10)]
    public int handbrakeDriftMultiplier = 5;

    [Space(10)]
    public Vector3 bodyMassCenter;

    // --- TUNING (Less drift + better braking) ---
    [Space(20)]
    [Range(0.5f, 3f)] public float baseSidewaysStiffness = 1.6f;  // more = more grip
    [Range(0.5f, 3f)] public float baseForwardStiffness = 1.5f;   // more = more grip
    [Range(0.5f, 20f)]  public float brakeToReverseSpeedKPH = 2f;    // brake first above this speed
    [Range(0.3f, 1f)] public float rearHandbrakeStiffnessFactor = 0.75f; // 0.75 slight drift, 0.6 more drift

    //WHEELS
    public GameObject frontLeftMesh;
    public WheelCollider frontLeftCollider;
    [Space(10)]
    public GameObject frontRightMesh;
    public WheelCollider frontRightCollider;
    [Space(10)]
    public GameObject rearLeftMesh;
    public WheelCollider rearLeftCollider;
    [Space(10)]
    public GameObject rearRightMesh;
    public WheelCollider rearRightCollider;

    //PARTICLE SYSTEMS
    [Space(20)]
    public bool useEffects = false;

    public ParticleSystem RLWParticleSystem;
    public ParticleSystem RRWParticleSystem;

    [Space(10)]
    public TrailRenderer RLWTireSkid;
    public TrailRenderer RRWTireSkid;

    //SPEED TEXT (UI)
    [Space(20)]
    public bool useUI = false;
    public Text carSpeedText;

    //SOUNDS
    [Space(20)]
    public bool useSounds = false;
    public AudioSource carEngineSound;
    public AudioSource tireScreechSound;
    float initialCarEngineSoundPitch;

    //CONTROLS
    [Space(20)]
    public bool useTouchControls = false;
    public GameObject throttleButton;
    PrometeoTouchInput throttlePTI;
    public GameObject reverseButton;
    PrometeoTouchInput reversePTI;
    public GameObject turnRightButton;
    PrometeoTouchInput turnRightPTI;
    public GameObject turnLeftButton;
    PrometeoTouchInput turnLeftPTI;
    public GameObject handbrakeButton;
    PrometeoTouchInput handbrakePTI;

    //CAR DATA
    [HideInInspector] public float carSpeed;
    [HideInInspector] public bool isDrifting;
    [HideInInspector] public bool isTractionLocked;

    //PRIVATE VARIABLES
    Rigidbody carRigidbody;
    float steeringAxis;
    float throttleAxis;
    float driftingAxis;
    float localVelocityZ;
    float localVelocityX;
    bool deceleratingCar;
    bool touchControlsSetup = false;

    // Sprint state
    bool isSprinting = false;

    // Reverse/brake helper
    bool waitingForReverse = false;

    // Brake state
    bool isBraking = false;
    float currentBrakeStrength = 0f;  // Smooth brake application

    WheelFrictionCurve FLwheelFriction;
    float FLWextremumSlip;
    WheelFrictionCurve FRwheelFriction;
    float FRWextremumSlip;
    WheelFrictionCurve RLwheelFriction;
    float RLWextremumSlip;
    WheelFrictionCurve RRwheelFriction;
    float RRWextremumSlip;

    // Base (after grip tuning)
    float RLWstiffnessBase;
    float RRWstiffnessBase;

    void FixedUpdate()
    {
        if (carRigidbody == null) return;

        // Smooth brake strength application (prevents lockup)
        float targetBrakeStrength = isBraking ? 1f : 0f;
        currentBrakeStrength = Mathf.Lerp(currentBrakeStrength, targetBrakeStrength, Time.deltaTime * 20f);  // Fast brake application

        // Apply smoothed brake strength to wheels
        if (currentBrakeStrength > 0.01f)
        {
            frontLeftCollider.brakeTorque = brakeForce * 1.3f * currentBrakeStrength;
            frontRightCollider.brakeTorque = brakeForce * 1.3f * currentBrakeStrength;
            rearLeftCollider.brakeTorque = brakeForce * 0.7f * currentBrakeStrength;
            rearRightCollider.brakeTorque = brakeForce * 0.7f * currentBrakeStrength;
        }
        else
        {
            frontLeftCollider.brakeTorque = 0f;
            frontRightCollider.brakeTorque = 0f;
            rearLeftCollider.brakeTorque = 0f;
            rearRightCollider.brakeTorque = 0f;
        }

        // Downforce (helps stick to track)
        carRigidbody.AddForce(-transform.up * downforce * carRigidbody.velocity.magnitude);

        // Soft speed limiter (prevents runaway speed but decelerates smoothly)
        float speedKph = carRigidbody.velocity.magnitude * 3.6f;
        float localZ = transform.InverseTransformDirection(carRigidbody.velocity).z;

        float forwardMaxKph = (localZ >= 0f) ? (isSprinting ? sprintMaxSpeed : maxSpeed) : maxReverseSpeed;
        if (speedKph > forwardMaxKph)
        {
            // Smoothly lerp the velocity down towards the allowed max instead of snapping
            Vector3 targetVel = carRigidbody.velocity.normalized * (forwardMaxKph / 3.6f);
            float smoothing = 2.0f; // higher = faster deceleration to cap
            carRigidbody.velocity = Vector3.Lerp(carRigidbody.velocity, targetVel, Time.deltaTime * smoothing);
        }
    }

    void Start()
    {
        carRigidbody = gameObject.GetComponent<Rigidbody>();
        carRigidbody.centerOfMass = bodyMassCenter;

        // Add more grip so the car doesn't drift too much
        ApplyBaseGrip();

        // Initialize wheel friction curves
        InitializeWheelFriction(frontLeftCollider, out FLwheelFriction, out FLWextremumSlip);
        InitializeWheelFriction(frontRightCollider, out FRwheelFriction, out FRWextremumSlip);
        InitializeWheelFriction(rearLeftCollider, out RLwheelFriction, out RLWextremumSlip);
        InitializeWheelFriction(rearRightCollider, out RRwheelFriction, out RRWextremumSlip);

        // Save base stiffness for rear wheels (used by handbrake tuning)
        RLWstiffnessBase = rearLeftCollider.sidewaysFriction.stiffness;
        RRWstiffnessBase = rearRightCollider.sidewaysFriction.stiffness;

        if (carEngineSound != null)
        {
            initialCarEngineSoundPitch = carEngineSound.pitch;
        }

        if (useUI)
        {
            InvokeRepeating("CarSpeedUI", 0f, 0.1f);
        }
        else
        {
            if (carSpeedText != null) carSpeedText.text = "0";
        }

        if (useSounds)
        {
            InvokeRepeating("CarSounds", 0f, 0.1f);
        }
        else
        {
            if (carEngineSound != null) carEngineSound.Stop();
            if (tireScreechSound != null) tireScreechSound.Stop();
        }

        if (!useEffects)
        {
            StopAllEffects();
        }

        if (useTouchControls)
        {
            if (throttleButton != null && reverseButton != null &&
                turnRightButton != null && turnLeftButton != null &&
                handbrakeButton != null)
            {
                throttlePTI = throttleButton.GetComponent<PrometeoTouchInput>();
                reversePTI = reverseButton.GetComponent<PrometeoTouchInput>();
                turnLeftPTI = turnLeftButton.GetComponent<PrometeoTouchInput>();
                turnRightPTI = turnRightButton.GetComponent<PrometeoTouchInput>();
                handbrakePTI = handbrakeButton.GetComponent<PrometeoTouchInput>();
                touchControlsSetup = true;
            }
            else
            {
                string ex = "Touch controls are not completely set up. You must drag and drop your scene buttons in the" +
                            " PrometeoCarController component.";
                Debug.LogWarning(ex);
            }
        }
    }

    void Update()
    {
        //CAR DATA
        carSpeed = carRigidbody.velocity.magnitude * 3.6f;
        localVelocityX = transform.InverseTransformDirection(carRigidbody.velocity).x;
        localVelocityZ = transform.InverseTransformDirection(carRigidbody.velocity).z;

        // Sprint state: only enabled for keyboard controls
        isSprinting = !useTouchControls && Input.GetKey(sprintKey);

        //INPUT + PHYSICS
        if (useTouchControls && touchControlsSetup)
        {
            if (throttlePTI.buttonPressed)
            {
                CancelInvoke("DecelerateCar");
                deceleratingCar = false;
                // Cancel any pending reverse request
                waitingForReverse = false;
                ReleaseBrakes();
                GoForward();
            }

            if (reversePTI.buttonPressed)
            {
                CancelInvoke("DecelerateCar");
                deceleratingCar = false;

                // Brake first when still moving forward, set waiting flag so we don't flip immediately
                if (localVelocityZ > 0.2f && Mathf.Abs(carSpeed) > brakeToReverseSpeedKPH)
                {
                    ThrottleOff();
                    ApplyBrakes(1f);
                    waitingForReverse = true;
                }
                else
                {
                    waitingForReverse = false;
                    ReleaseBrakes();
                    GoReverse();
                }
            }

            if (turnLeftPTI.buttonPressed) TurnLeft();
            if (turnRightPTI.buttonPressed) TurnRight();

            if (handbrakePTI.buttonPressed)
            {
                CancelInvoke("DecelerateCar");
                deceleratingCar = false;
                Handbrake();
            }
            if (!handbrakePTI.buttonPressed)
            {
                RecoverTraction();
            }

            if ((!throttlePTI.buttonPressed && !reversePTI.buttonPressed))
            {
                // If user released reverse while waiting, clear waiting flag
                waitingForReverse = false;
                ReleaseBrakes();
                ThrottleOff();
            }

            if ((!reversePTI.buttonPressed && !throttlePTI.buttonPressed) && !handbrakePTI.buttonPressed && !deceleratingCar)
            {
                InvokeRepeating("DecelerateCar", 0f, 0.1f);
                deceleratingCar = true;
            }

            if (!turnLeftPTI.buttonPressed && !turnRightPTI.buttonPressed && steeringAxis != 0f)
            {
                ResetSteeringAngle();
            }
        }
        else
        {
            bool w = Input.GetKey(KeyCode.W);
            bool s = Input.GetKey(KeyCode.S);
            bool a = Input.GetKey(KeyCode.A);
            bool d = Input.GetKey(KeyCode.D);
            bool space = Input.GetKey(KeyCode.Space);

            // W has priority: if pressed, cancel any reverse/brake operations
            if (w)
            {
                CancelInvoke("DecelerateCar");
                deceleratingCar = false;
                // Cancel any pending reverse request
                waitingForReverse = false;
                ReleaseBrakes();
                GoForward();
            }
            else if (s)  // Only check S if W is not pressed
            {
                CancelInvoke("DecelerateCar");
                deceleratingCar = false;

                // Brake first when still moving forward, set waiting flag so we don't flip immediately
                if (localVelocityZ > 0.2f && Mathf.Abs(carSpeed) > brakeToReverseSpeedKPH)
                {
                    ThrottleOff();
                    ApplyBrakes(1f);
                    waitingForReverse = true;
                }
                else
                {
                    waitingForReverse = false;
                    ReleaseBrakes();
                    GoReverse();
                }
            }

            if (a) TurnLeft();
            if (d) TurnRight();

            if (space)
            {
                CancelInvoke("DecelerateCar");
                deceleratingCar = false;
                Handbrake();
            }

            if (Input.GetKeyUp(KeyCode.Space))
            {
                RecoverTraction();
            }

            if (!w && !s)
            {
                // If user released reverse while waiting, clear waiting flag
                waitingForReverse = false;
                ReleaseBrakes();
                ThrottleOff();
            }

            if (!w && !s && !space && !deceleratingCar)
            {
                InvokeRepeating("DecelerateCar", 0f, 0.1f);
                deceleratingCar = true;
            }

            if (!a && !d && steeringAxis != 0f)
            {
                ResetSteeringAngle();
            }
        }

        AnimateWheelMeshes();
    }

    // === GRIP HELPERS ===
    void StopAllEffects()
    {
        if (RLWParticleSystem != null) RLWParticleSystem.Stop();
        if (RRWParticleSystem != null) RRWParticleSystem.Stop();
        if (RLWTireSkid != null) RLWTireSkid.emitting = false;
        if (RRWTireSkid != null) RRWTireSkid.emitting = false;
    }

    void InitializeWheelFriction(WheelCollider wc, out WheelFrictionCurve friction, out float extremumSlip)
    {
        var sf = wc.sidewaysFriction;
        friction = new WheelFrictionCurve()
        {
            extremumSlip = sf.extremumSlip,
            extremumValue = sf.extremumValue,
            asymptoteSlip = sf.asymptoteSlip,
            asymptoteValue = sf.asymptoteValue,
            stiffness = sf.stiffness
        };
        extremumSlip = sf.extremumSlip;
    }

    void SetWheelTorque(float torque)
    {
        frontLeftCollider.motorTorque = torque;
        frontRightCollider.motorTorque = torque;
        rearLeftCollider.motorTorque = torque;
        rearRightCollider.motorTorque = torque;
    }

    void ApplyBaseGrip()
    {
        ApplyGripToWheel(frontLeftCollider);
        ApplyGripToWheel(frontRightCollider);
        ApplyGripToWheel(rearLeftCollider);
        ApplyGripToWheel(rearRightCollider);
    }

    void ApplyGripToWheel(WheelCollider wc)
    {
        if (wc == null) return;

        var sf = wc.sidewaysFriction;
        sf.stiffness *= baseSidewaysStiffness;
        wc.sidewaysFriction = sf;

        var ff = wc.forwardFriction;
        ff.stiffness *= baseForwardStiffness;
        wc.forwardFriction = ff;
    }

    // UI
    public void CarSpeedUI()
    {
        if (!useUI || carSpeedText == null) return;
        carSpeedText.text = Mathf.RoundToInt(Mathf.Abs(carSpeed)).ToString();
    }

    // Sounds
    public void CarSounds()
    {
        if (!useSounds)
        {
            if (carEngineSound != null && carEngineSound.isPlaying) carEngineSound.Stop();
            if (tireScreechSound != null && tireScreechSound.isPlaying) tireScreechSound.Stop();
            return;
        }

        if (carEngineSound != null)
        {
            float engineSoundPitch = initialCarEngineSoundPitch + (Mathf.Abs(carRigidbody.velocity.magnitude) / 25f);
            carEngineSound.pitch = engineSoundPitch;
        }

        bool shouldScratch = (isDrifting) || (isTractionLocked && Mathf.Abs(carSpeed) > 12f);
        if (tireScreechSound != null)
        {
            if (shouldScratch && !tireScreechSound.isPlaying) tireScreechSound.Play();
            else if (!shouldScratch && tireScreechSound.isPlaying) tireScreechSound.Stop();
        }
    }

    //
    //STEERING METHODS
    //
    public void TurnLeft()
    {
        steeringAxis = steeringAxis - (Time.deltaTime * 10f * steeringSpeed);
        if (steeringAxis < -1f) steeringAxis = -1f;

        var steeringAngle = steeringAxis * maxSteeringAngle;
        frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, steeringAngle, steeringSpeed);
        frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, steeringAngle, steeringSpeed);
    }

    public void TurnRight()
    {
        steeringAxis = steeringAxis + (Time.deltaTime * 10f * steeringSpeed);
        if (steeringAxis > 1f) steeringAxis = 1f;

        var steeringAngle = steeringAxis * maxSteeringAngle;
        frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, steeringAngle, steeringSpeed);
        frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, steeringAngle, steeringSpeed);
    }

    public void ResetSteeringAngle()
    {
        if (steeringAxis < 0f) steeringAxis = steeringAxis + (Time.deltaTime * 10f * steeringSpeed);
        else if (steeringAxis > 0f) steeringAxis = steeringAxis - (Time.deltaTime * 10f * steeringSpeed);

        if (Mathf.Abs(frontLeftCollider.steerAngle) < 1f) steeringAxis = 0f;

        var steeringAngle = steeringAxis * maxSteeringAngle;
        frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, steeringAngle, steeringSpeed);
        frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, steeringAngle, steeringSpeed);
    }

    void AnimateWheelMeshes()
    {
        UpdateWheelMesh(frontLeftCollider, frontLeftMesh);
        UpdateWheelMesh(frontRightCollider, frontRightMesh);
        UpdateWheelMesh(rearLeftCollider, rearLeftMesh);
        UpdateWheelMesh(rearRightCollider, rearRightMesh);
    }

    void UpdateWheelMesh(WheelCollider wc, GameObject mesh)
    {
        if (mesh == null) return;
        wc.GetWorldPose(out Vector3 pos, out Quaternion rot);
        mesh.transform.position = pos;
        mesh.transform.rotation = rot;
    }

    //
    //ENGINE AND BRAKING METHODS
    //
    public void GoForward()
    {
        isDrifting = Mathf.Abs(localVelocityX) > 2.5f;
        DriftCarPS();

        throttleAxis = throttleAxis + (Time.deltaTime * 3f);
        if (throttleAxis > 1f) throttleAxis = 1f;

        if (localVelocityZ < -1f)
        {
            Brakes();
        }
        else
        {
            int activeMax = isSprinting ? sprintMaxSpeed : maxSpeed;
            if (Mathf.RoundToInt(carSpeed) < activeMax)
            {
                ReleaseBrakes();
                float torque = (accelerationMultiplier * enginePower) * throttleAxis * (isSprinting ? sprintMultiplier : 1f);
                SetWheelTorque(torque);
            }
            else
            {
                SetWheelTorque(0);
            }
        }
    }

    public void GoReverse()
    {
        isDrifting = Mathf.Abs(localVelocityX) > 2.5f;
        DriftCarPS();

        throttleAxis = throttleAxis - (Time.deltaTime * 3f);
        if (throttleAxis < -1f) throttleAxis = -1f;

        if (localVelocityZ > 1f)
        {
            Brakes();
        }
        else
        {
            if (Mathf.Abs(Mathf.RoundToInt(carSpeed)) < maxReverseSpeed)
            {
                ReleaseBrakes();
                float torque = (accelerationMultiplier * enginePower) * throttleAxis;
                SetWheelTorque(torque);
            }
            else
            {
                SetWheelTorque(0);
            }
        }
    }

    public void ThrottleOff()
    {
        SetWheelTorque(0);
    }

    public void DecelerateCar()
    {
        isDrifting = Mathf.Abs(localVelocityX) > 2.5f;
        DriftCarPS();

        if (throttleAxis != 0f)
        {
            if (throttleAxis > 0f) throttleAxis = throttleAxis - (Time.deltaTime * 10f);
            else if (throttleAxis < 0f) throttleAxis = throttleAxis + (Time.deltaTime * 10f);

            if (Mathf.Abs(throttleAxis) < 0.15f) throttleAxis = 0f;
        }

        carRigidbody.velocity = carRigidbody.velocity * (1f / (1f + (0.025f * decelerationMultiplier)));
        SetWheelTorque(0);

        if (carRigidbody.velocity.magnitude < 0.25f)
        {
            carRigidbody.velocity = Vector3.zero;
            CancelInvoke("DecelerateCar");
        }
    }

    // === REAL BRAKES ===
    public void ApplyBrakes(float strength01)
    {
        isBraking = Mathf.Clamp01(strength01) > 0f;
    }

    public void ReleaseBrakes()
    {
        isBraking = false;
        // Brake torque is handled smoothly in FixedUpdate
    }

    // Compatibility for old calls
    public void Brakes()
    {
        ApplyBrakes(1f);
    }

    // Handbrake (rear wheels only, softer drift)
    public void Handbrake()
    {
        CancelInvoke("RecoverTraction");

        driftingAxis = driftingAxis + Time.deltaTime;

        float secureStartingPoint = driftingAxis * RLWextremumSlip * handbrakeDriftMultiplier;
        if (secureStartingPoint < RLWextremumSlip)
        {
            driftingAxis = RLWextremumSlip / (RLWextremumSlip * handbrakeDriftMultiplier);
        }
        if (driftingAxis > 1f) driftingAxis = 1f;

        isDrifting = Mathf.Abs(localVelocityX) > 2.5f;

        if (driftingAxis < 1f)
        {
            // Rear wheels lose grip, front stays stable
            RLwheelFriction.extremumSlip = RLWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            RLwheelFriction.stiffness = RLWstiffnessBase * Mathf.Lerp(1f, rearHandbrakeStiffnessFactor, driftingAxis);
            rearLeftCollider.sidewaysFriction = RLwheelFriction;

            RRwheelFriction.extremumSlip = RRWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            RRwheelFriction.stiffness = RRWstiffnessBase * Mathf.Lerp(1f, rearHandbrakeStiffnessFactor, driftingAxis);
            rearRightCollider.sidewaysFriction = RRwheelFriction;
        }

        isTractionLocked = true;
        DriftCarPS();
    }

    public void DriftCarPS()
    {
        if (!useEffects) return;

        // Particle systems
        if (isDrifting)
        {
            if (RLWParticleSystem != null) RLWParticleSystem.Play();
            if (RRWParticleSystem != null) RRWParticleSystem.Play();
        }
        else
        {
            if (RLWParticleSystem != null) RLWParticleSystem.Stop();
            if (RRWParticleSystem != null) RRWParticleSystem.Stop();
        }

        // Tire skid trails
        bool shouldSkid = (isTractionLocked || Mathf.Abs(localVelocityX) > 5f) && Mathf.Abs(carSpeed) > 12f;
        if (RLWTireSkid != null) RLWTireSkid.emitting = shouldSkid;
        if (RRWTireSkid != null) RRWTireSkid.emitting = shouldSkid;
    }

    public void RecoverTraction()
    {
        isTractionLocked = false;
        driftingAxis = driftingAxis - (Time.deltaTime / 1.5f);
        if (driftingAxis < 0f) driftingAxis = 0f;

        // Recover rear traction only
        if (RLwheelFriction.extremumSlip > RLWextremumSlip)
        {
            RLwheelFriction.extremumSlip = RLWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            RLwheelFriction.stiffness = RLWstiffnessBase * Mathf.Lerp(1f, rearHandbrakeStiffnessFactor, driftingAxis);
            rearLeftCollider.sidewaysFriction = RLwheelFriction;

            RRwheelFriction.extremumSlip = RRWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            RRwheelFriction.stiffness = RRWstiffnessBase * Mathf.Lerp(1f, rearHandbrakeStiffnessFactor, driftingAxis);
            rearRightCollider.sidewaysFriction = RRwheelFriction;

            Invoke("RecoverTraction", Time.deltaTime);
        }
        else if (RLwheelFriction.extremumSlip < RLWextremumSlip)
        {
            RLwheelFriction.extremumSlip = RLWextremumSlip;
            RLwheelFriction.stiffness = RLWstiffnessBase;
            rearLeftCollider.sidewaysFriction = RLwheelFriction;

            RRwheelFriction.extremumSlip = RRWextremumSlip;
            RRwheelFriction.stiffness = RRWstiffnessBase;
            rearRightCollider.sidewaysFriction = RRwheelFriction;

            driftingAxis = 0f;
        }
    }
}
