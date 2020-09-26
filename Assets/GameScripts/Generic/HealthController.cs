using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthController : mMonoBehaviour, IHealthController
{
    [System.Serializable]
    public class OnDead : UnityEngine.Events.UnityEvent<GameObject> { }
    [System.Serializable]
    public class OnReceiveDamage : UnityEngine.Events.UnityEvent<Damage> { }
    protected bool _isDead;
    [SerializeField] 
    protected float _currentHealth;

    public int maxHealth = 100;
    public float currentHealth
    {
        get
        {
            return _currentHealth;
        }
        protected set
        {
            _currentHealth = value;
            if (!_isDead && _currentHealth <= 0)
            {
                _isDead = true;
                onDead.Invoke(gameObject);
            }
        }
    }
    public bool isDead
    {
        get
        {
            if (!_isDead && currentHealth <= 0)
            {
                _isDead = true;
                onDead.Invoke(gameObject);
            }
            return _isDead;
        }
        set
        {
            _isDead = value;
        }
    }

    public float healthRecovery = 0f;
    public float healthRecoveryDelay = 0f;
    [HideInInspector]
    public float currentHealthRecoveryDelay;
    public OnReceiveDamage onReceiveDamage = new OnReceiveDamage();
    public OnDead onDead = new OnDead();

    bool inHealthRecovery;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        currentHealthRecoveryDelay = healthRecoveryDelay;
    }

    protected virtual bool canRecoverHealth
    {
        get
        {
            return !(currentHealth <= 0 || (healthRecovery == 0 && currentHealth < maxHealth));
        }
    }

    protected virtual IEnumerator RecoverHealth()
    {
        inHealthRecovery = true;
        while (canRecoverHealth)
        {
            HealthRecovery();
            yield return null;
        }
        inHealthRecovery = false;
    }

    protected virtual void HealthRecovery()
    {
        if (!canRecoverHealth) return;
        if (currentHealthRecoveryDelay > 0)
            currentHealthRecoveryDelay -= Time.deltaTime;
        else
        {
            if (currentHealth > maxHealth)
                currentHealth = maxHealth;
            if (currentHealth < maxHealth)
                currentHealth += healthRecovery * Time.deltaTime;
        }
    }

    public virtual void ChangeHealth(int value)
    {
        currentHealth += value;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        if (!isDead && currentHealth <= 0)
        {
            isDead = true;
            onDead.Invoke(gameObject);
        }
    }/// Change the currentHealth of Character

    public virtual void ChangeMaxHealth(int value)
    {
        maxHealth += value;
        if (maxHealth < 0)
            maxHealth = 0;
    }/// Change the MaxHealth of Character

    public virtual void TakeDamage(Damage damage)
    {
        if (damage != null)
        {
            currentHealthRecoveryDelay = currentHealth <= 0 ? 0 : healthRecoveryDelay;
            if (damage.damageValue > 0 && !inHealthRecovery)
                StartCoroutine(RecoverHealth());
            onReceiveDamage.Invoke(damage);
            if (currentHealth > 0)
                currentHealth -= damage.damageValue;
        }
    }/// Apply Damage to Current Health
}