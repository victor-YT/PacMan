using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CherrySpawner : MonoBehaviour
{
    public GameObject cherryPrefab;
    public Transform levelMask;
    public Tilemap levelMap;
    public float spawnDelay = 5f;
    public float moveSpeed = 3f;
    public float margin = 1.0f;
    public float sizeInCells = 0.9f;

    public string sortingLayerName = "Default";
    public int sortingOrder = 100;

    Transform _maskTf;
    SpriteMask _mask;
    Bounds _bounds;

    void Awake()
    {
        if (!levelMask || !levelMap)
        {
            enabled = false;
            return;
        }
        _maskTf = levelMask;
        _mask = levelMask.GetComponent<SpriteMask>();
    }

    void OnEnable()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(spawnDelay);

        while (true)
        {
            _bounds = GetMaskWorldBounds();

            GameObject cherry = Instantiate(cherryPrefab);
            cherry.name = "Cherry";

            var sr = cherry.GetComponentInChildren<SpriteRenderer>();
            if (sr)
            {
                sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                sr.sortingLayerName = sortingLayerName;
                sr.sortingOrder = sortingOrder;

                var grid = levelMap.layoutGrid ? levelMap.layoutGrid : levelMap.GetComponentInParent<Grid>();
                float worldCellX = grid.cellSize.x * grid.transform.lossyScale.x;

                float targetWorldWidth = worldCellX * sizeInCells;
                float spriteWorldWidth = sr.sprite.rect.width / sr.sprite.pixelsPerUnit * sr.transform.lossyScale.x;
                float s = targetWorldWidth / Mathf.Max(0.0001f, spriteWorldWidth);

                sr.transform.localScale = new Vector3(s, s, 1f);
            }

            Vector2 start, end;
            GetRandomLineAcrossBounds(_bounds, out start, out end);
            cherry.transform.position = start;

            float t = 0f;
            float totalDist = Vector2.Distance(start, end);
            while (t < 1f)
            {
                if (!cherry) break;
                float step = moveSpeed * Time.deltaTime / Mathf.Max(0.0001f, totalDist);
                t += step;
                cherry.transform.position = Vector2.Lerp(start, end, t);
                if (Vector2.Distance(cherry.transform.position, end) < 0.05f) break;
                yield return null;
            }

            if (cherry) Destroy(cherry);
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    Bounds GetMaskWorldBounds()
    {
        if (_mask) return _mask.bounds;
        var rend = _maskTf.GetComponent<Renderer>();
        if (rend) return rend.bounds;
        Vector3 size = _maskTf.lossyScale;
        return new Bounds(_maskTf.position, size);
    }

    void GetRandomLineAcrossBounds(Bounds b, out Vector2 start, out Vector2 end)
    {
        int side = Random.Range(0, 4);
        float xMin = b.min.x, xMax = b.max.x, yMin = b.min.y, yMax = b.max.y;

        switch (side)
        {
            case 0:
                start = new Vector2(xMin - margin, Random.Range(yMin, yMax));
                end   = new Vector2(xMax + margin, start.y);
                break;
            case 1:
                start = new Vector2(xMax + margin, Random.Range(yMin, yMax));
                end   = new Vector2(xMin - margin, start.y);
                break;
            case 2:
                start = new Vector2(Random.Range(xMin, xMax), yMin - margin);
                end   = new Vector2(start.x, yMax + margin);
                break;
            default:
                start = new Vector2(Random.Range(xMin, xMax), yMax + margin);
                end   = new Vector2(start.x, yMin - margin);
                break;
        }
    }
}