using UnityEngine;
using UnityEngine;
public class Walkiing : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        bool walk = false;
        bool sit = false;
        bool sleep = false;
        bool type = false;
        
        walk = Input.GetKey(KeyCode.A);
        sit = Input.GetKey(KeyCode.W);
        sleep = Input.GetKey(KeyCode.S);
        type = Input.GetKey(KeyCode.D);
		float input = Input.GetAxis("Horizontal") * 1;

        _animator.SetBool("Typing", type);
        _animator.SetBool("Sitting", sit);
		_animator.SetFloat("Speed", input);
    }
}
