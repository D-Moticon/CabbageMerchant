using UnityEngine;
using TMPro;

public class MapSign : MonoBehaviour
{
    public TMP_Text signText;

    public void SetSignTextFromBiome(Biome biome)
    {
        signText.text = biome.biomeName;
    }
}
