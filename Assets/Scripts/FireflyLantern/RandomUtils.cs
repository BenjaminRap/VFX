using UnityEngine;

public static class RandomUtils
{
    public static Vector3	GetRandomVector()
	{
		return new (Random.value - 0.5f, Random.value - 0.5f, Random.value - 0.5f);
	}
}
