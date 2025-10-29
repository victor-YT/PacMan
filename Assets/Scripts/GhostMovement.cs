using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GhostMovement : MonoBehaviour
{
    public int ghostID = 1;
    public GhostController anim;
    public Tilemap map;
    public PacStudentController pac;
    public List<TileBase> notWalkableTiles;
    public Transform gateA;
    public Transform gateB;
    public float gateBlockDistance = 0.45f;

    public Vector3Int homeCell;
    public Vector2Int exitDirOnRespawn = Vector2Int.up;

    public float normalScaleOfPac = 0.9f;
    public float scaredScaleOfNormal = 0.5f;
    public float deadScaleOfNormal = 0.5f;

    Vector3Int currentCell;
    Vector3Int targetCell;
    Vector3Int lastDir = Vector3Int.right;
    bool hasTarget;

    float speedNormal;
    float speedScared;
    float speedDead;
    bool frozen = false;

    bool spawnReady;
    int yTopExit, yBotExit, xLeftSpawn, xRightSpawn;
    Vector3Int topExitCenterCell, botExitCenterCell;
    Vector3 spawnCenterWorld;
    bool wasOutsideSpawn;

    public void Freeze(bool stop)
    {
        frozen = stop;
        if (stop) hasTarget = false;
    }

    void Start()
    {
        if (!map) map = FindFirstObjectByType<Tilemap>();
        if (!pac) pac = FindFirstObjectByType<PacStudentController>();
        SnapToCell(map.WorldToCell(transform.position));
        float baseSpeed = pac ? pac.moveSpeed : 6f;
        speedNormal = baseSpeed * normalScaleOfPac;
        speedScared = speedNormal * scaredScaleOfNormal;
        speedDead = speedNormal * deadScaleOfNormal;
        ComputeSpawnByGhostExitTiles();
        lastDir = (ghostID == 1 || ghostID == 3) ? Vector3Int.up : Vector3Int.down;
    }

    void Update()
    {
        if (frozen) return;
        if (!anim) return;

        if (anim.State == GhostState.Dead)
        {
            MoveDead();
            return;
        }

        MoveOnGrid();
    }

    void MoveOnGrid()
    {
        if (!hasTarget)
        {
            DecideNextDirection();
            hasTarget = true;
        }

        Vector3 target = map.GetCellCenterWorld(targetCell);
        float spd = GetSpeed();
        transform.position = Vector3.MoveTowards(transform.position, target, spd * Time.deltaTime);

        if ((transform.position - target).sqrMagnitude < 0.0001f)
        {
            SnapToCell(targetCell);
            hasTarget = false;
        }
    }

    void DecideNextDirection()
    {
        List<Vector3Int> dirs = new List<Vector3Int> { Vector3Int.up, Vector3Int.right, Vector3Int.down, Vector3Int.left };
        Vector3Int back = new Vector3Int(-lastDir.x, -lastDir.y, 0);

        List<Vector3Int> candidates = new List<Vector3Int>();
        foreach (var d in dirs)
        {
            if (d == back) continue;
            Vector3Int n = currentCell + d;
            if (IsBlocked(n)) continue;
            if (HitsGate(n)) continue;
            if (spawnReady && wasOutsideSpawn && InsideSpawnRect(n)) continue;
            candidates.Add(d);
        }
        if (candidates.Count == 0)
        {
            if (!IsBlocked(currentCell + back) && !HitsGate(currentCell + back))
            {
                if (!(spawnReady && wasOutsideSpawn && InsideSpawnRect(currentCell + back)))
                    candidates.Add(back);
            }
        }

        if (spawnReady && InsideSpawnRect(currentCell))
        {
            Vector3Int targetExit = (ghostID == 1 || ghostID == 3) ? topExitCenterCell : botExitCenterCell;
            Vector3Int prefer = BestStepToward(currentCell, targetExit, candidates);
            if (prefer != Vector3Int.zero)
            {
                lastDir = prefer;
                targetCell = currentCell + prefer;
                anim.SetFacing(new Vector2(prefer.x, prefer.y));
                return;
            }
        }

        Vector3Int chosen = Vector3Int.zero;

        var state = anim.State;
        bool scaredLike = state == GhostState.Scared || state == GhostState.Recovering || ghostID == 1;
        bool chase = ghostID == 2 && state == GhostState.Normal;
        bool random = ghostID == 3 && state == GhostState.Normal;
        bool clockwise = ghostID == 4 && state == GhostState.Normal;

        if (clockwise)
        {
            chosen = ChooseWallFollowCW(candidates);
            if (chosen == Vector3Int.zero && candidates.Count > 0) chosen = candidates[Random.Range(0, candidates.Count)];
        }
        else if (random)
        {
            if (candidates.Count > 0) chosen = candidates[Random.Range(0, candidates.Count)];
        }
        else
        {
            if (pac)
            {
                Vector3 pacPos = pac.transform.position;
                if (scaredLike)
                {
                    float best = float.NegativeInfinity;
                    foreach (var d in candidates)
                    {
                        float distNow = Vector3.Distance(map.GetCellCenterWorld(currentCell), pacPos);
                        float distNext = Vector3.Distance(map.GetCellCenterWorld(currentCell + d), pacPos);
                        float score = distNext - distNow;
                        if (score > best) { best = score; chosen = d; }
                    }
                }
                else if (chase)
                {
                    float best = float.PositiveInfinity;
                    foreach (var d in candidates)
                    {
                        float dist = Vector3.Distance(map.GetCellCenterWorld(currentCell + d), pacPos);
                        if (dist < best) { best = dist; chosen = d; }
                    }
                }
            }
            if (chosen == Vector3Int.zero && candidates.Count > 0)
                chosen = candidates[Random.Range(0, candidates.Count)];
        }

        if (chosen == Vector3Int.zero) chosen = back;

        lastDir = chosen;
        targetCell = currentCell + chosen;
        anim.SetFacing(new Vector2(chosen.x, chosen.y));

        if (spawnReady && !InsideSpawnRect(targetCell)) wasOutsideSpawn = true;
    }

    Vector3Int ChooseWallFollowCW(List<Vector3Int> candidates)
    {
        Vector3Int f = lastDir == Vector3Int.zero ? Vector3Int.right : lastDir;
        Vector3Int r = new Vector3Int(f.y, -f.x, 0);
        Vector3Int l = new Vector3Int(-f.y, f.x, 0);
        Vector3Int b = new Vector3Int(-f.x, -f.y, 0);
        if (CanGo(r, candidates)) return r;
        if (CanGo(f, candidates)) return f;
        if (CanGo(l, candidates)) return l;
        if (CanGo(b, candidates)) return b;
        return Vector3Int.zero;
    }

    bool CanGo(Vector3Int d, List<Vector3Int> candidates)
    {
        if (!candidates.Contains(d)) return false;
        Vector3Int n = currentCell + d;
        if (IsBlocked(n)) return false;
        if (HitsGate(n)) return false;
        if (spawnReady && wasOutsideSpawn && InsideSpawnRect(n)) return false;
        return true;
    }

    float GetSpeed()
    {
        switch (anim.State)
        {
            case GhostState.Normal: return speedNormal;
            case GhostState.Scared: return speedScared;
            case GhostState.Recovering: return speedScared;
            case GhostState.Dead: return speedDead;
        }
        return speedNormal;
    }

    void MoveDead()
    {
        Vector3 target = spawnReady ? spawnCenterWorld : map.GetCellCenterWorld(homeCell);
        transform.position = Vector3.MoveTowards(transform.position, target, speedDead * Time.deltaTime);
        if ((transform.position - target).sqrMagnitude < 0.05f)
        {
            anim.ResetToSpawn();
            wasOutsideSpawn = false;
            lastDir = spawnReady ? ((ghostID == 1 || ghostID == 3) ? Vector3Int.up : Vector3Int.down)
                                 : new Vector3Int(exitDirOnRespawn.x, exitDirOnRespawn.y, 0);
            SnapToCell(spawnReady ? map.WorldToCell(spawnCenterWorld) : homeCell);
        }
    }

    bool IsBlocked(Vector3Int cell)
    {
        TileBase t = map.GetTile(cell);
        if (t == null) return false;
        if (notWalkableTiles != null && notWalkableTiles.Count > 0)
            return notWalkableTiles.Contains(t);
        return false;
    }

    bool IsWalkable(Vector3Int cell)
{
    return !IsBlocked(cell);
}

    bool HitsGate(Vector3Int cell)
    {
        if (!gateA && !gateB) return false;
        Vector3 w = map.GetCellCenterWorld(cell);
        if (gateA && Vector2.Distance(w, gateA.position) < gateBlockDistance) return true;
        if (gateB && Vector2.Distance(w, gateB.position) < gateBlockDistance) return true;
        return false;
    }

    void SnapToCell(Vector3Int cell)
    {
        currentCell = cell;
        transform.position = map.GetCellCenterWorld(cell);
    }

    Vector3Int BestStepToward(Vector3Int from, Vector3Int to, List<Vector3Int> candidates)
    {
        if (candidates.Count == 0) return Vector3Int.zero;
        float best = float.PositiveInfinity;
        Vector3Int bestDir = Vector3Int.zero;
        foreach (var d in candidates)
        {
            Vector3Int n = from + d;
            float dist = (n - to).sqrMagnitude;
            if (dist < best) { best = dist; bestDir = d; }
        }
        return bestDir;
    }

    bool InsideSpawnRect(Vector3Int cell)
    {
        if (!spawnReady) return false;
        return (cell.x >= xLeftSpawn && cell.x <= xRightSpawn && cell.y <= yTopExit && cell.y >= yBotExit);
    }

    void ComputeSpawnByGhostExitTiles()
    {
        if (!map) { spawnReady = false; return; }

        List<Vector3Int> exits = new List<Vector3Int>();
        BoundsInt b = map.cellBounds;
        foreach (var pos in b.allPositionsWithin)
        {
            TileBase t = map.GetTile(pos);
            if (t == null) continue;
            if (t.name == "ghost_exit") exits.Add(pos);
        }
        if (exits.Count == 0)
        {
            spawnReady = false;
            return;
        }

        int maxY = exits.Max(p => p.y);
        int minY = exits.Min(p => p.y);
        var topRow = exits.FindAll(p => p.y == maxY);
        var botRow = exits.FindAll(p => p.y == minY);
        topRow.Sort((a, c) => a.x.CompareTo(c.x));
        botRow.Sort((a, c) => a.x.CompareTo(c.x));

        yTopExit = maxY;
        yBotExit = minY;

        int topMidX = topRow[topRow.Count / 2].x;
        int botMidX = botRow[botRow.Count / 2].x;
        topExitCenterCell = new Vector3Int(topMidX, yTopExit, 0);
        botExitCenterCell = new Vector3Int(botMidX, yBotExit, 0);

        xLeftSpawn = ScanEdgeX(topMidX, yTopExit, -1);
        xRightSpawn = ScanEdgeX(topMidX, yTopExit, +1);

        Vector3 topW = map.GetCellCenterWorld(topExitCenterCell);
        Vector3 botW = map.GetCellCenterWorld(botExitCenterCell);
        spawnCenterWorld = (topW + botW) * 0.5f;

        spawnReady = true;
    }

    int ScanEdgeX(int startX, int y, int step)
    {
        int x = startX;
        while (true)
        {
            Vector3Int c = new Vector3Int(x, y, 0);
            if (!IsWalkable(c)) return x - step;
            x += step;
            if (x < map.cellBounds.xMin || x > map.cellBounds.xMax) return x - step;
        }
    }
}