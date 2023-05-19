using UnityEngine;
using UnityEngine.InputSystem;
public class FirstPerson : MonoBehaviour
{
    [SerializeField] Transform cameraTrasform;
    [SerializeField] float walkingSpeed = 5f;
    [SerializeField] float flyingSpeed = 10f;
    [SerializeField] float mouseSensitivity= 3f;
    [SerializeField] float mass = 1f;
    [SerializeField] float jumpSpeed = 5f;
    [SerializeField] float acceleration= 20f;
    Vector2 look;
    CharacterController controller;
    Vector3 velocity;

    PlayerInput playerInput;
    InputAction moveAction;
    InputAction lookAction;
    InputAction jumpAction;
    InputAction sprintAction;
    InputAction flyUpDownAction;
    public State state;

    public enum State
    {
        Walking,
        Flying
    }
    void start(){
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Awake(){
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["move"];
        lookAction = playerInput.actions["look"];
        jumpAction = playerInput.actions["jump"];
        sprintAction = playerInput.actions["sprint"];
        flyUpDownAction = playerInput.actions["flyUpDown"];
    }

    void Update(){
        switch(state){
            case State.Walking:
                updateGravity();
                UpdateMovement();
                UpdateLook();
                break;
            case State.Flying:
                UpdateMovementFlying();
                UpdateLook();
                break;
            }
    }
    void updateGravity(){
        var gravity = Physics.gravity * mass * Time.deltaTime;
        velocity.y = controller.isGrounded ? -1f : velocity.y + gravity.y;
    }
    Vector3 GetMovementInput(float speed, bool horizontal = true){
        var moveInput = moveAction.ReadValue<Vector2>();
        var flyUpDownInput = flyUpDownAction.ReadValue<float>();
        var input = new Vector3();
        var referenceTransform = horizontal ? transform : cameraTrasform;
        input += referenceTransform.forward * moveInput.y;
        input += referenceTransform.right * moveInput.x;
        if(!horizontal){
            input += transform.up * flyUpDownInput;
        }
        input = Vector3.ClampMagnitude(input,1f);
        var sprintInput =  sprintAction.ReadValue<float>();
        var multiplier = sprintInput > 0 ? 1.5f : 1f; 
        input *= speed * multiplier;
        return input;
    }
    void UpdateMovement(){
        var input = GetMovementInput(walkingSpeed);

        var factor = acceleration * Time.deltaTime;
        velocity.x = Mathf.Lerp(velocity.x , input.x , factor);
        velocity.z = Mathf.Lerp(velocity.z , input.z , factor);

        var jumpInput = jumpAction.ReadValue<float>();
        if(jumpInput > 0 && controller.isGrounded){
            velocity.y += jumpSpeed;
        }

        controller.Move(velocity * Time.deltaTime);   
    }
    void UpdateMovementFlying(){
        var input = GetMovementInput(flyingSpeed,false);

        var factor = acceleration* Time.deltaTime;
        velocity = Vector3.Lerp(velocity,input,factor);

        controller.Move(velocity * Time.deltaTime);
    }
    
    void UpdateLook(){
        var lookInput = lookAction.ReadValue<Vector2>();
        look.x += lookInput.x * mouseSensitivity;
        look.y += lookInput.y * mouseSensitivity;

        cameraTrasform.localRotation = Quaternion.Euler(-look.y,0,0);
        transform.localRotation = Quaternion.Euler(0,look.x,0);
    }

    void OnToogleFlying(){
        state = state == State.Flying ? State.Walking : State.Flying;
    }

}