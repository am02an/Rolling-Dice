using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Missile Ability")]
public class MissileAbility : BaseAbility
{
    public float explosionRadius;
    public int damage;
    public float launchDelay;

    private void OnEnable()
    {
        characterType = CharacterType.Missile;
    }
    public override void Attack(GameObject target)
    {
        Debug.Log($"ArmyMan attacks with {damage} power.");
        // Apply damage to target
    }

    public override void TakeDamage(float amount)
    {
        Debug.Log($"ArmyMan takes {amount} damage.");
        // Reduce health, play hit animation, etc.
    }
}

