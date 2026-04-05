using UnityEngine;

public class Lantern : MonoBehaviour
{
	[SerializeField]
	private FireflyMove	_fireflyPrefab;
	[SerializeField]
	private uint		_fireflyCount;
	[SerializeField, Range(0, 10)]
	private float		_spawnRange;

	private void	Awake()
	{
		for (int i = 0; i < _fireflyCount; i++)
		{
			Vector3	spawnPosition = RandomUtils.GetRandomVector().normalized * _spawnRange;

		    Instantiate(_fireflyPrefab.gameObject, transform.position + spawnPosition, Quaternion.identity, transform);
		}
	}
}
