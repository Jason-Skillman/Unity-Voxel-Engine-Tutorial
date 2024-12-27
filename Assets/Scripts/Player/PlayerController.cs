namespace VoxelEngine.Player
{
	using Unity.Mathematics;
	using UnityEngine;
	using VoxelEngine.Core;

	public class PlayerController : MonoBehaviour
	{
		[SerializeField]
		private Transform roundedHitPointTarget;
		
		public Camera playerCamera;
		public float speed = 6.0f;
		public float jumpSpeed = 8.0f;
		public float gravity = 20.0f;
		public float mouseSensitivity = 2.0f;

		private CharacterController characterController;
		private Vector3 moveDirection = Vector3.zero;
		private float rotationX = 0.0f;

		private void Start()
		{
			characterController = GetComponent<CharacterController>();
			Cursor.lockState = CursorLockMode.Locked;
			
			roundedHitPointTarget.SetParent(null);
		}

		private void Update()
		{
			HandleMovement();
			HandleMouseLook();
			UpdateRaycast();
		}

		private void HandleMovement()
		{
			if(characterController.isGrounded)
			{
				float moveForward = Input.GetAxis("Vertical");
				float moveRight = Input.GetAxis("Horizontal");
				moveDirection = new Vector3(moveRight, 0, moveForward);
				moveDirection = transform.TransformDirection(moveDirection);
				moveDirection *= speed;

				if(Input.GetButton("Jump"))
				{
					moveDirection.y = jumpSpeed;
				}
			}

			moveDirection.y -= gravity * Time.deltaTime;
			characterController.Move(moveDirection * Time.deltaTime);
		}

		private void HandleMouseLook()
		{
			float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
			float mouseY = -Input.GetAxis("Mouse Y") * mouseSensitivity;

			rotationX += mouseY;
			rotationX = Mathf.Clamp(rotationX, -90, 90);

			playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
			transform.Rotate(Vector3.up * mouseX);
		}

		private void UpdateRaycast()
		{
			const float MaxDistance = 10.0f;
			Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
			Vector3 hitPoint;
			bool didHit = false;
			if(Physics.Raycast(ray, out RaycastHit hit, MaxDistance))
			{
				hitPoint = hit.point;
				didHit = true;
#if UNITY_EDITOR
				Debug.DrawLine(ray.origin, hitPoint, Color.red, 0.1f);
#endif
			}
			else
			{
				hitPoint = ray.origin + ray.direction * MaxDistance;
#if UNITY_EDITOR
				Debug.DrawRay(ray.origin, hitPoint, Color.red, 0.1f);
#endif
			}
			
			Vector3 point = hitPoint + (-hit.normal * 0.5f);
			
			Vector3 vector = point + new Vector3(0.5f, 0.5f, 0.5f);
			
			int3 worldPosition = new(
				(int)math.floor(vector.x), 
				(int)math.floor(vector.y), 
				(int)math.floor(vector.z));

			roundedHitPointTarget.position = new Vector3(worldPosition.x, worldPosition.y, worldPosition.z);
			
			if(Input.GetMouseButtonDown(0))
			{
				if(didHit)
				{
					ChunkWorldManager chunkWorldManager = hit.collider.GetComponentInParent<ChunkWorldManager>();
					if(chunkWorldManager != null)
					{
						chunkWorldManager.RemoveBlock(worldPosition);
					}
				}
			}
		}
	}
}
