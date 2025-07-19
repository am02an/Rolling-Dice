using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/ArmyMan Ability")]
public class ArmyManAbility : BaseAbility
{
    public float speed;
    public int attackPower;
    public float reloadTime;

    private void OnEnable()
    {
        characterType = CharacterType.ArmyMan;
    }
    public override void Attack(GameObject target)
    {
        Debug.Log($"ArmyMan attacks with {attackPower} power.");
        // Apply damage to target
    }

    public override void TakeDamage(float amount)
    {
        Debug.Log($"ArmyMan takes {amount} damage.");
        // Reduce health, play hit animation, etc.
    }
}
