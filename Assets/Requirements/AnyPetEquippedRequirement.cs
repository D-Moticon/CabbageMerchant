using UnityEngine;

public class AnyPetEquippedRequirement : Requirement
{
    public override bool IsRequirementMet()
    {
        if (Singleton.Instance.petManager.currentPet != null)
        {
            return true;
        }

        return false;
    }
}
