using UnityEngine;
using System.Collections.Generic;

public class PetEquippedRequirement : Requirement
{
    public PetDefinition pet;
    public override bool IsRequirementMet()
    {
        if (Singleton.Instance.petManager.currentPet == pet)
        {
            return true;
        }

        return false;
    }
}
