using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof (Renderer))]
[RequireComponent(typeof (VisualEffect))]
public class BurningWand : MonoBehaviour
{
	[SerializeField]
	private float	_dissolveDuration;
	[SerializeField]
	private float	_pauseDuration;

	private Renderer		_renderer;
	private	VisualEffect	_vfx;

	private void	Awake()
	{
		_renderer = GetComponent<Renderer>();
		_vfx = GetComponent<VisualEffect>();
	}

	private void	Update()
	{
		float dissolved = 1 - Mathf.Clamp(Time.time % (_dissolveDuration + _pauseDuration) - (_pauseDuration / 2), 0, _dissolveDuration) / _dissolveDuration;

		_renderer.material.SetFloat("_dissolved", dissolved);
		if (dissolved == 1 || dissolved == 0)
			_vfx.SetFloat("spawnRate", 0);
		else
			_vfx.SetFloat("spawnRate", 1);
		_vfx.SetFloat("spawnRadius", transform.localScale.x / 2);
		_vfx.SetFloat("spawnPointY", dissolved * 2 - 1);
	}
}
