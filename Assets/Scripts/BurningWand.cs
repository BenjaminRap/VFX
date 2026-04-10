using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof (Renderer))]
[RequireComponent(typeof (VisualEffect))]
public class BurningWand : MonoBehaviour
{
	[SerializeField]
	private float			_dissolveDuration;
	[SerializeField]
	private float			_pauseDuration;
	[SerializeField]
	private float			_dissolveParticleRange;
	[SerializeField, Range(0, 1)]
	private float			_dissolved;
	[SerializeField]
	private bool			_animate;

	private Renderer		_renderer;
	private	VisualEffect	_vfx;
	private	Vector2			_defaultSpawnRate;
	private float			_previousDissolved;
	private float			_particleSpawnSubtractY;

	private void	Awake()
	{
		_renderer = GetComponent<Renderer>();
		_vfx = GetComponent<VisualEffect>();
		_defaultSpawnRate = _vfx.GetVector2("spawnRate");
		_previousDissolved = -1;
		_particleSpawnSubtractY = _renderer.material.GetFloat("_burnWidth") + _renderer.material.GetFloat("_fireWidth") / 2;
	}

	private void	Update()
	{
		if (_animate)
			_dissolved = 1 - Mathf.Clamp(Time.time % (_dissolveDuration + _pauseDuration) - (_pauseDuration / 2), 0, _dissolveDuration) / _dissolveDuration;

		if (_dissolved == _previousDissolved)
			return ;
		_renderer.material.SetFloat("_dissolved", _dissolved);
		if (_dissolved < 1 - _dissolveParticleRange || _dissolved > _dissolveParticleRange)
			_vfx.SetVector2("spawnRate", Vector2.zero);
		else
			_vfx.SetVector2("spawnRate", _defaultSpawnRate);
		_vfx.SetFloat("spawnPointY", _dissolved * 2 - 1 - _particleSpawnSubtractY);
		_previousDissolved = _dissolved;
	}
}
