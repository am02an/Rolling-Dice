using UnityEngine;

public abstract class BaseAbility : ScriptableObject
{
    public CharacterType characterType;
    public virtual void Attack(GameObject target) { }
    public virtual void TakeDamage(float amount) { }
}
public enum CharacterType
{
    ArmyMan,
    Defender,
    Missile,
    Monster
}
