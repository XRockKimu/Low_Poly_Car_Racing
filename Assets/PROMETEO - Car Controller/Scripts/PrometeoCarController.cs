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
    
    [Space(10)]
    [Range(5000f, 50000f)]
    public float enginePower = 50000f;   // greatly increased for maximum acceleration


    //CAR SETUP
    [Space(20)]
    [Space(10)]
    [Range(20, 5000)]
    public int maxSpeed = 5000; // km/h (set very high for maximum top speed)
    [Range(10, 500)]
    public int maxReverseSpeed = 200; // km/h (increased reverse for consistency)
    [Range(1, 50)]
    public int accelerationMultiplier = 50; // set to maximum for extreme acceleration
    [Space(10)]
    [Range(10, 45)]
    public int maxSteeringAngle = 27;
    [Range(0.1f, 1f)]
    public float steeringSpeed = 0.5f;
    [Space(10)]
    [Range(100, 600)]
    public int brakeForce = 350;
    [Range(1, 10)]
    public int decelerationMultiplier = 2;
    [Range(1, 10)]
    public int handbrakeDriftMultiplier = 5;

    [Space(10)]
    public Vector3 bodyMassCenter;

    // --- TUNING (Less drift + better braking) ---
    [Space(20)]
    [Range(0.5f, 3f)] public float baseSidewaysStiffness = 1.35f;  // more = more grip
    [Range(0.5f, 3f)] public float baseForwardStiffness = 1.15f;   // more = more grip
    [Range(1f, 20f)]  public float brakeToReverseSpeedKPH = 8f;    // brake first above this speed
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
    [Space(10)]
    public bool useEffects = false;

    public ParticleSystem RLWParticleSystem;
    public ParticleSystem RRWParticleSystem;

    [Space(10)]
    public TrailRenderer RLWTireSkid;
    public TrailRenderer RRWTireSkid;

    //SPEED TEXT (UI)
    [Space(20)]
    [Space(10)]
    public bool useUI = false;
    public Text carSpeedText;

    //SOUNDS
    [Space(20)]
    [Space(10)]
    public bool useSounds = false;
    public AudioSource carEngineSound;
    public AudioSource tireScreechSound;
    float initialCarEngineSoundPitch;

    //CONTROLS
    [Space(20)]
    [Space(10)]
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

    // New brake state
    bool isBraking = false;

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

        // Downforce (helps stick to track)
        carRigidbody.AddForce(-transform.up * downforce * carRigidbody.velocity.magnitude);

        // Hard speed limiter (fixes "wrong" speed / runaway acceleration)
        float speedKph = carRigidbody.velocity.magnitude * 3.6f;
        float localZ = transform.InverseTransformDirection(carRigidbody.velocity).z;

        float maxKph = (localZ >= 0f) ? maxSpeed : maxReverseSpeed;
        if (speedKph > maxKph)
        {
            carRigidbody.velocity = carRigidbody.velocity.normalized * (maxKph / 3.6f);
        }
    }

    void Start()
    {
        carRigidbody = gameObject.GetComponent<Rigidbody>();
        carRigidbody.centerOfMass = bodyMassCenter;

        // Add more grip so the car doesn't drift too much
        ApplyBaseGrip();

        // Save default wheel friction values (after base grip tuning)
        FLwheelFriction = new WheelFrictionCurve();
        FLwheelFriction.extremumSlip = frontLeftCollider.sidewaysFriction.extremumSlip;
        FLWextremumSlip = frontLeftCollider.sidewaysFriction.extremumSlip;
        FLwheelFriction.extremumValue = frontLeftCollider.sidewaysFriction.extremumValue;
        FLwheelFriction.asymptoteSlip = frontLeftCollider.sidewaysFriction.asymptoteSlip;
        FLwheelFriction.asymptoteValue = frontLeftCollider.sidewaysFriction.asymptoteValue;
        FLwheelFriction.stiffness = frontLeftCollider.sidewaysFriction.stiffness;

        FRwheelFriction = new WheelFrictionCurve();
        FRwheelFriction.extremumSlip = frontRightCollider.sidewaysFriction.extremumSlip;
        FRWextremumSlip = frontRightCollider.sidewaysFriction.extremumSlip;
        FRwheelFriction.extremumValue = frontRightCollider.sidewaysFriction.extremumValue;
        FRwheelFriction.asymptoteSlip = frontRightCollider.sidewaysFriction.asymptoteSlip;
        FRwheelFriction.asymptoteValue = frontRightCollider.sidewaysFriction.asymptoteValue;
        FRwheelFriction.stiffness = frontRightCollider.sidewaysFriction.stiffness;

        RLwheelFriction = new WheelFrictionCurve();
        RLwheelFriction.extremumSlip = rearLeftCollider.sidewaysFriction.extremumSlip;
        RLWextremumSlip = rearLeftCollider.sidewaysFriction.extremumSlip;
        RLwheelFriction.extremumValue = rearLeftCollider.sidewaysFriction.extremumValue;
        RLwheelFriction.asymptoteSlip = rearLeftCollider.sidewaysFriction.asymptoteSlip;
        RLwheelFriction.asymptoteValue = rearLeftCollider.sidewaysFriction.asymptoteValue;
        RLwheelFriction.stiffness = rearLeftCollider.sidewaysFriction.stiffness;

        RRwheelFriction = new WheelFrictionCurve();
        RRwheelFriction.extremumSlip = rearRightCollider.sidewaysFriction.extremumSlip;
        RRWextremumSlip = rearRightCollider.sidewaysFriction.extremumSlip;
        RRwheelFriction.extremumValue = rearRightCollider.sidewaysFriction.extremumValue;
        RRwheelFriction.asymptoteSlip = rearRightCollider.sidewaysFriction.asymptoteSlip;
        RRwheelFriction.asymptoteValue = rearRightCollider.sidewaysFriction.asymptoteValue;
        RRwheelFriction.stiffness = rearRightCollider.sidewaysFriction.stiffness;

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
        else if (!useUI)
        {
            if (carSpeedText != null) carSpeedText.text = "0";
        }

        if (useSounds)
        {
            InvokeRepeating("CarSounds", 0f, 0.1f);
        }
        else if (!useSounds)
        {
            if (carEngineSound != null) carEngineSound.Stop();
            if (tireScreechSound != null) tireScreechSound.Stop();
        }

        if (!useEffects)
        {
            if (RLWParticleSystem != null) RLWParticleSystem.Stop();
            if (RRWParticleSystem != null) RRWParticleSystem.Stop();
            if (RLWTireSkid != null) RLWTireSkid.emitting = false;
            if (RRWTireSkid != null) RRWTireSkid.emitting = false;
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

        //INPUT + PHYSICS
        if (useTouchControls && touchControlsSetup)
        {
            if (throttlePTI.buttonPressed)
            {
                CancelInvoke("DecelerateCar");
                deceleratingCar = false;
                ReleaseBrakes();
                GoForward();
            }

            if (reversePTI.buttonPressed)
            {
                CancelInvoke("DecelerateCar");
                deceleratingCar = false;

                // Brake first when still moving forward
                if (localVelocityZ > 1f && Mathf.Abs(carSpeed) > brakeToReverseSpeedKPH)
                {
                    ThrottleOff();
                    ApplyBrakes(1f);
                }
                else
                {
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

            if (w)
            {
                CancelInvoke("DecelerateCar");
                deceleratingCar = false;
                ReleaseBrakes();
                GoForward();
            }

            if (s)
            {
                CancelInvoke("DecelerateCar");
                deceleratingCar = false;

                // Brake first when still moving forward
                if (localVelocityZ > 1f && Mathf.Abs(carSpeed) > brakeToReverseSpeedKPH)
                {
                    ThrottleOff();
                    ApplyBrakes(1f);
                }
                else
                {
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
        if (useUI)
        {
            try
            {
                float absoluteCarSpeed = Mathf.Abs(carSpeed);
                carSpeedText.text = Mathf.RoundToInt(absoluteCarSpeed).ToString();
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
            }
        }
    }

    // Sounds
    public void CarSounds()
    {
        if (useSounds)
        {
            try
            {
                if (carEngineSound != null)
                {
                    float engineSoundPitch = initialCarEngineSoundPitch + (Mathf.Abs(carRigidbody.velocity.magnitude) / 25f);
                    carEngineSound.pitch = engineSoundPitch;
                }
                if ((isDrifting) || (isTractionLocked && Mathf.Abs(carSpeed) > 12f))
                {
                    if (!tireScreechSound.isPlaying) tireScreechSound.Play();
                }
                else if ((!isDrifting) && (!isTractionLocked || Mathf.Abs(carSpeed) < 12f))
                {
                    tireScreechSound.Stop();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
            }
        }
        else if (!useSounds)
        {
            if (carEngineSound != null && carEngineSound.isPlaying) carEngineSound.Stop();
            if (tireScreechSound != null && tireScreechSound.isPlaying) tireScreechSound.Stop();
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
        try
        {
            Quaternion FLWRotation;
            Vector3 FLWPosition;
            frontLeftCollider.GetWorldPose(out FLWPosition, out FLWRotation);
            frontLeftMesh.transform.position = FLWPosition;
            frontLeftMesh.transform.rotation = FLWRotation;

            Quaternion FRWRotation;
            Vector3 FRWPosition;
            frontRightCollider.GetWorldPose(out FRWPosition, out FRWRotation);
            frontRightMesh.transform.position = FRWPosition;
            frontRightMesh.transform.rotation = FRWRotation;

            Quaternion RLWRotation;
            Vector3 RLWPosition;
            rearLeftCollider.GetWorldPose(out RLWPosition, out RLWRotation);
            rearLeftMesh.transform.position = RLWPosition;
            rearLeftMesh.transform.rotation = RLWRotation;

            Quaternion RRWRotation;
            Vector3 RRWPosition;
            rearRightCollider.GetWorldPose(out RRWPosition, out RRWRotation);
            rearRightMesh.transform.position = RRWPosition;
            rearRightMesh.transform.rotation = RRWRotation;
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex);
        }
    }

    //
    //ENGINE AND BRAKING METHODS
    //
    public void GoForward()
    {
        if (Mathf.Abs(localVelocityX) > 2.5f)
        {
            isDrifting = true;
            DriftCarPS();
        }
        else
        {
            isDrifting = false;
            DriftCarPS();
        }

        throttleAxis = throttleAxis + (Time.deltaTime * 3f);
        if (throttleAxis > 1f) throttleAxis = 1f;

        if (localVelocityZ < -1f)
        {
            Brakes();
        }
        else
        {
            if (Mathf.RoundToInt(carSpeed) < maxSpeed)
            {
                ReleaseBrakes();

                frontLeftCollider.motorTorque = (accelerationMultiplier * enginePower) * throttleAxis;
                frontRightCollider.motorTorque = (accelerationMultiplier * enginePower) * throttleAxis;
                rearLeftCollider.motorTorque = (accelerationMultiplier * enginePower) * throttleAxis;
                rearRightCollider.motorTorque = (accelerationMultiplier * enginePower) * throttleAxis;
            }
            else
            {
                frontLeftCollider.motorTorque = 0;
                frontRightCollider.motorTorque = 0;
                rearLeftCollider.motorTorque = 0;
                rearRightCollider.motorTorque = 0;
            }
        }
    }

    public void GoReverse()
    {
        if (Mathf.Abs(localVelocityX) > 2.5f)
        {
            isDrifting = true;
            DriftCarPS();
        }
        else
        {
            isDrifting = false;
            DriftCarPS();
        }

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

                frontLeftCollider.motorTorque = (accelerationMultiplier * enginePower) * throttleAxis;
                frontRightCollider.motorTorque = (accelerationMultiplier * enginePower) * throttleAxis;
                rearLeftCollider.motorTorque = (accelerationMultiplier * enginePower) * throttleAxis;
                rearRightCollider.motorTorque = (accelerationMultiplier * enginePower) * throttleAxis;
            }
            else
            {
                frontLeftCollider.motorTorque = 0;
                frontRightCollider.motorTorque = 0;
                rearLeftCollider.motorTorque = 0;
                rearRightCollider.motorTorque = 0;
            }
        }
    }

    public void ThrottleOff()
    {
        frontLeftCollider.motorTorque = 0;
        frontRightCollider.motorTorque = 0;
        rearLeftCollider.motorTorque = 0;
        rearRightCollider.motorTorque = 0;
    }

    public void DecelerateCar()
    {
        if (Mathf.Abs(localVelocityX) > 2.5f)
        {
            isDrifting = true;
            DriftCarPS();
        }
        else
        {
            isDrifting = false;
            DriftCarPS();
        }

        if (throttleAxis != 0f)
        {
            if (throttleAxis > 0f) throttleAxis = throttleAxis - (Time.deltaTime * 10f);
            else if (throttleAxis < 0f) throttleAxis = throttleAxis + (Time.deltaTime * 10f);

            if (Mathf.Abs(throttleAxis) < 0.15f) throttleAxis = 0f;
        }

        carRigidbody.velocity = carRigidbody.velocity * (1f / (1f + (0.025f * decelerationMultiplier)));

        frontLeftCollider.motorTorque = 0;
        frontRightCollider.motorTorque = 0;
        rearLeftCollider.motorTorque = 0;
        rearRightCollider.motorTorque = 0;

        if (carRigidbody.velocity.magnitude < 0.25f)
        {
            carRigidbody.velocity = Vector3.zero;
            CancelInvoke("DecelerateCar");
        }
    }

    // === REAL BRAKES ===
    public void ApplyBrakes(float strength01)
    {
        float s = Mathf.Clamp01(strength01);
        isBraking = s > 0f;

        // Front bias braking (stable)
        frontLeftCollider.brakeTorque = brakeForce * 1.3f * s;
        frontRightCollider.brakeTorque = brakeForce * 1.3f * s;
        rearLeftCollider.brakeTorque = brakeForce * 0.7f * s;
        rearRightCollider.brakeTorque = brakeForce * 0.7f * s;
    }

    public void ReleaseBrakes()
    {
        if (!isBraking) return;
        isBraking = false;

        frontLeftCollider.brakeTorque = 0f;
        frontRightCollider.brakeTorque = 0f;
        rearLeftCollider.brakeTorque = 0f;
        rearRightCollider.brakeTorque = 0f;
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
        if (useEffects)
        {
            try
            {
                if (isDrifting)
                {
                    RLWParticleSystem.Play();
                    RRWParticleSystem.Play();
                }
                else
                {
                    RLWParticleSystem.Stop();
                    RRWParticleSystem.Stop();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
            }

            try
            {
                if ((isTractionLocked || Mathf.Abs(localVelocityX) > 5f) && Mathf.Abs(carSpeed) > 12f)
                {
                    RLWTireSkid.emitting = true;
                    RRWTireSkid.emitting = true;
                }
                else
                {
                    RLWTireSkid.emitting = false;
                    RRWTireSkid.emitting = false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
            }
        }
        else
        {
            if (RLWParticleSystem != null) RLWParticleSystem.Stop();
            if (RRWParticleSystem != null) RRWParticleSystem.Stop();
            if (RLWTireSkid != null) RLWTireSkid.emitting = false;
            if (RRWTireSkid != null) RRWTireSkid.emitting = false;
        }
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
