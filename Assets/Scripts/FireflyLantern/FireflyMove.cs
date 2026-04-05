using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FireflyMove : MonoBehaviour
{
	[SerializeField, Range(0, 10)]
	private float _speed;
	[SerializeField, Range(0, 10)]
	private float _driftLength;
	[SerializeField, Range(0, 10)]
	private float _backToCenterLength;

	private Rigidbody	_rigidBody;
	private Vector3		_direction;

	private void	Awake()
	{
		_rigidBody = GetComponent<Rigidbody>();
		_direction = RandomUtils.GetRandomVector().normalized;
	}

	private void	FixedUpdate()
	{
		Vector3	drift = _driftLength * RandomUtils.GetRandomVector().normalized;
		Vector3	backToCenter = _backToCenterLength * (transform.parent.position - transform.position).normalized;

		_direction = (_direction + (drift + backToCenter) * Time.fixedDeltaTime).normalized;
		_rigidBody.linearVelocity = _direction * _speed;
	}

	private void	OnCollisionEnter(Collision collision)
	{
		_direction = collision.GetContact(0).normal;
	}
}
