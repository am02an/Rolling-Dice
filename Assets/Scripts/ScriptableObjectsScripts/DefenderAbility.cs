using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Defender Ability")]
public class DefenderAbility : BaseAbility
{
    public int defensePoints;
    public float regenRate;

    private void OnEnable()
    {
        characterType = CharacterType.Defender;
    }
   
    public override void TakeDamage(float amount)
    {
        Debug.Log($"ArmyMan takes {amount} damage.");
        // Reduce health, play hit animation, etc.
    }
}

