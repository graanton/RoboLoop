using UnityEngine;

public sealed class LaserArmModule : ArmModuleBase
{
    [SerializeField] private float _damagePerSecond = 25f;
    [SerializeField] private float _range = 50f;
    [SerializeField] private float _heatPerSecond = 20f;
    [SerializeField] private float _coolPerSecond = 15f;
    [SerializeField] private float _maxHeat = 100f;

    private float _currentHeat;
    private bool _isFiring;

    public override void Tick()
    {
        if (_isFiring && _currentHeat < _maxHeat)
        {
            FireLaser();
            _currentHeat += _heatPerSecond * Time.deltaTime;
        }
        else
        {
            _currentHeat -= _coolPerSecond * Time.deltaTime;
        }

        _currentHeat = Mathf.Clamp(_currentHeat, 0, _maxHeat);
    }

    public override void OnPrimaryPressed()
    {
        _isFiring = true;
    }

    public override void OnPrimaryReleased()
    {
        _isFiring = false;
    }

    private void FireLaser()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out var hit, _range))
        {
            var damageable = hit.collider.GetComponent<IDamageable>();
            damageable?.TakeDamage(_damagePerSecond * Time.deltaTime);
        }
    }
}