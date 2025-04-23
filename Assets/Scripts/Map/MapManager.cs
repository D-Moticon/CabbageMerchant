using UnityEngine;
using System.Collections;

public class MapManager : MonoBehaviour
{
    [HideInInspector] public MapBlueprint currentMapBlueprint;
    [HideInInspector] public Map map;

    public MapCharacter mapCharacterPrefab;
    private MapCharacter mapCharacter;

    [Header("Appearance")]
    public Color normalColor = Color.white;
    public Color disabledColor = new Color(.3f, .3f, .3f, .9f);

    [Header("Scrolling Settings")]
    public float scrollDuration = 1.0f;
    public float yOffset = -3f;

    private void Start()
    {
        MapBlueprint mbp = Singleton.Instance.runManager.startingMapBlueprint;
        map = MapSingleton.Instance.mapGenerator.GenerateMap(mbp);
        currentMapBlueprint = mbp;
        map.InitializeMap(currentMapBlueprint);
        
        UpdateLayerStates();
        CenterOnLayer(0, instant: true);

        if (map.layers.Count > 0 && map.layers[0].mapIcons.Count > 0)
        {
            mapCharacter = Instantiate(mapCharacterPrefab, map.layers[0].mapIcons[0].transform.position, Quaternion.identity, map.transform);
        }
    }

    public void MoveToNextLayer()
    {
        if (Singleton.Instance.playerStats.currentMapLayer >= map.layers.Count - 1) return;

        Singleton.Instance.playerStats.currentMapLayer++;
        UpdateLayerStates();
    }

    public void OnMapIconClicked(MapIcon icon)
    {
        int layerIndex = FindIconLayer(icon);
        if (layerIndex == Singleton.Instance.playerStats.currentMapLayer)
        {
            StartCoroutine(MoveCharacterAndGoToScene(icon));
        }
        else
        {
            Debug.Log($"Icon {icon.name} is not in the current layer. Ignoring click.");
        }
    }

    private IEnumerator MoveCharacterAndGoToScene(MapIcon targetIcon)
    {
        Vector3 startCharPos = mapCharacter.transform.localPosition;
        Vector3 endCharPos = targetIcon.transform.localPosition;

        Vector3 startMapPos = map.transform.localPosition;
        Vector3 endMapPos = new Vector3(startMapPos.x, -FindIconLayer(targetIcon) * map.verticalSpacing + yOffset, 0f);

        mapCharacter.StartWalkingAnimation();
        
        float elapsed = 0f;
        while (elapsed < scrollDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / scrollDuration);

            mapCharacter.transform.localPosition = Vector3.Lerp(startCharPos, endCharPos, t);
            map.transform.localPosition = Vector3.Lerp(startMapPos, endMapPos, t);

            yield return null;
        }

        mapCharacter.transform.localPosition = endCharPos;
        map.transform.localPosition = endMapPos;
        mapCharacter.StopWalkingAnimation();

        if (!string.IsNullOrEmpty(targetIcon.mapPoint.sceneName))
        {
            Singleton.Instance.runManager.GoToScene(targetIcon.mapPoint.sceneName, targetIcon.mapPoint);
        }
    }

    private void UpdateLayerStates()
    {
        for (int i = 0; i < map.layers.Count; i++)
        {
            bool isCurrent = (i == Singleton.Instance.playerStats.currentMapLayer);
            var layer = map.layers[i];

            foreach (var icon in layer.mapIcons)
            {
                icon.spriteRenderer.color = isCurrent ? normalColor : disabledColor;
                if (icon.bc2d != null)
                {
                    icon.bc2d.enabled = isCurrent;
                }
            }
        }
    }

    private void CenterOnLayer(int layerIndex, bool instant = false)
    {
        float targetY = -layerIndex * map.verticalSpacing + yOffset;
        if (instant)
        {
            map.transform.localPosition = new Vector3(map.transform.localPosition.x, targetY, 0f);
        }
    }

    private int FindIconLayer(MapIcon icon)
    {
        for (int i = 0; i < map.layers.Count; i++)
        {
            if (map.layers[i].mapIcons.Contains(icon))
                return i;
        }
        return -1;
    }

    public void SetCurrentLayerIndex(int newLayer)
    {
        Singleton.Instance.playerStats.currentMapLayer = newLayer;
    }
}