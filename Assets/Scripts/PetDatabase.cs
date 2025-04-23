using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PetDatabase", menuName = "Pets/PetDatabase")]
public class PetDatabase : ScriptableObject
{
    public List<PetDefinition> allPets;
}
