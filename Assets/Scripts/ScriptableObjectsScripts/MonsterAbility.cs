using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Monster Ability")]
public class MonsterAbility : BaseAbility
{
    public int rageLevel;
    public float roarCooldown;
    public int areaDamage;

    private void OnEnable()
    {
        characterType = CharacterType.Monster;
    }
    public override void Attack(GameObject target)
    {
        Debug.Log($"ArmyMan attacks with {areaDamage} power.");
        // Apply damage to target
    }

    public override void TakeDamage(float amount)
    {
        Debug.Log($"ArmyMan takes {amount} damage.");
        // Reduce health, play hit animation, etc.
    }
}

