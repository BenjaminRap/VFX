using UnityEngine;

[RequireComponent(typeof (Cloth))]
public class AnimateClotheWind : MonoBehaviour
{
	[SerializeField]
	private Vector3	_wind;
	[SerializeField]
	private float	_windSpeed;

	private Cloth	_cloth;

	protected void	Awake()
	{
		_cloth = GetComponent<Cloth>();
	}

	protected void Update()
	{
		_cloth.externalAcceleration = _wind * Mathf.PerlinNoise1D(Time.time * _windSpeed);
	}
}
