using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PacStudentController : MonoBehaviour
{
    [Header("Maps")]
    public Tilemap levelMap;
    public List<TileBase> notWalkableTiles;
    public List<TileBase> pelletTiles;
    public List<TileBase> powerPelletTiles;

    [Header("Movement")]
    public float moveSpeed = 6f;
    public KeyCode upKey = KeyCode.W;
    public KeyCode downKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;

    [Header("Teleport Gates")]
    public Transform gateA;
    public Transform gateB;
    public float gateDistance = 0.4f;
    public float exitOffset = 0.55f;
    public float teleportCooldown = 0.15f;

    [Header("Animation")]
    public Animator animator;
    public string animUp = "player_move_up";
    public string animDown = "player_move_down";
    public string animLeft = "player_move_left";
    public string animRight = "player_move_right";
    public string animDeath = "player_death";

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip moveClip;
    public AudioClip eatClip;

    [Header("FX")]
    public ParticleSystem dust;

    Vector2Int lastInput = Vector2Int.right;
    Vector2Int currentInput = Vector2Int.right;

    Vector3 targetWorld;
    bool isMoving;
    bool canTeleport = true;
    bool controlEnabled = true;

    GameManager gm;

    void Awake()
    {
        gm = FindFirstObjectByType<GameManager>();
    }

    void Start()
    {
        Vector3Int startCell = levelMap.WorldToCell(transform.position);
        targetWorld = levelMap.GetCellCenterWorld(startCell);
        transform.position = targetWorld;
        PlayDirAnim(currentInput);
        SetMoving(false);
    }

    void Update()
    {
        if (!controlEnabled)
        {
            isMoving = false;
            SetMoving(false);
            return;
        }

        ReadInput();
        HandleTeleport();

        if (!isMoving)
        {
            Vector3Int cell = levelMap.WorldToCell(transform.position);
            if (TryBeginMove(cell, lastInput)) {}
            else if (TryBeginMove(cell, currentInput)) {}
            else SetMoving(false);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, targetWorld, moveSpeed * Time.deltaTime);
            if ((transform.position - targetWorld).sqrMagnitude < 0.0001f)
            {
                transform.position = targetWorld;
                isMoving = false;
                ConsumeTileAt(levelMap.WorldToCell(transform.position));
                SetMoving(false);
            }
        }
    }

    void ReadInput()
    {
        if (Input.GetKeyDown(upKey)) lastInput = Vector2Int.up;
        else if (Input.GetKeyDown(downKey)) lastInput = Vector2Int.down;
        else if (Input.GetKeyDown(leftKey)) lastInput = Vector2Int.left;
        else if (Input.GetKeyDown(rightKey)) lastInput = Vector2Int.right;
    }

    bool TryBeginMove(Vector3Int curCell, Vector2Int dir)
    {
        if (dir == Vector2Int.zero) return false;
        Vector3Int nextCell = curCell + new Vector3Int(dir.x, dir.y, 0);
        if (IsWalkable(nextCell))
        {
            currentInput = dir;
            targetWorld = levelMap.GetCellCenterWorld(nextCell);
            isMoving = true;
            PlayDirAnim(dir);
            SetMoving(true);
            PickMoveAudio(nextCell);
            return true;
        }
        return false;
    }

    bool IsWalkable(Vector3Int cell)
    {
        TileBase t = levelMap.GetTile(cell);
        if (t == null) return true;
        if (notWalkableTiles != null && notWalkableTiles.Count > 0)
            return !notWalkableTiles.Contains(t);
        return true;
    }

    void HandleTeleport()
    {
        if (!canTeleport) return;
        Vector3 pos = transform.position;

        if (gateA && Vector2.Distance(pos, gateA.position) < gateDistance)
        {
            TeleportTo(gateB, currentInput);
            return;
        }

        if (gateB && Vector2.Distance(pos, gateB.position) < gateDistance)
        {
            TeleportTo(gateA, currentInput);
            return;
        }
    }

    void TeleportTo(Transform dst, Vector2Int dir)
    {
        if (!dst) return;

        canTeleport = false;
        StartCoroutine(TeleportCooldownRoutine());

        Vector3Int dstCell = levelMap.WorldToCell(dst.position);
        Vector3 dstCenter = levelMap.GetCellCenterWorld(dstCell);

        Vector2 pushDir = dir != Vector2Int.zero ? (Vector2)dir : Vector2.right;
        Vector3 outPos = dstCenter + (Vector3)(pushDir * exitOffset);

        transform.position = outPos;

        Vector3Int nextCell = levelMap.WorldToCell(outPos) + new Vector3Int((int)pushDir.x, (int)pushDir.y, 0);
        if (IsWalkable(nextCell))
        {
            targetWorld = levelMap.GetCellCenterWorld(nextCell);
            isMoving = true;
            PlayDirAnim(dir);
            SetMoving(true);
            PickMoveAudio(nextCell);
        }
        else
        {
            isMoving = false;
            SetMoving(false);
        }
    }

    IEnumerator TeleportCooldownRoutine()
    {
        yield return new WaitForSeconds(teleportCooldown);
        canTeleport = true;
    }

    void PlayDirAnim(Vector2Int dir)
    {
        if (!animator || animator.runtimeAnimatorController == null) return;
        string state = animRight;
        if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y)) state = dir.x > 0 ? animRight : animLeft;
        else state = dir.y > 0 ? animUp : animDown;
        animator.Play(state, 0, 0f);
    }

    void SetMoving(bool moving)
    {
        if (audioSource && !moving && audioSource.isPlaying) audioSource.Stop();
        if (dust)
        {
            var em = dust.emission;
            em.enabled = moving;
        }
    }

    void PickMoveAudio(Vector3Int nextCell)
    {
        if (!audioSource) return;
        TileBase t = levelMap.GetTile(nextCell);
        bool aboutToEat =
            (pelletTiles != null && pelletTiles.Contains(t)) ||
            (powerPelletTiles != null && powerPelletTiles.Contains(t));
        AudioClip clip = aboutToEat ? eatClip : moveClip;
        if (!clip) return;
        if (audioSource.clip != clip) audioSource.clip = clip;
        if (!audioSource.isPlaying) audioSource.Play();
    }

    void ConsumeTileAt(Vector3Int cell)
    {
        TileBase t = levelMap.GetTile(cell);
        if (t == null) return;

        if (pelletTiles != null && pelletTiles.Contains(t))
        {
            levelMap.SetTile(cell, null);
            if (gm) gm.OnPelletEaten();
            return;
        }

        if (powerPelletTiles != null && powerPelletTiles.Contains(t))
        {
            levelMap.SetTile(cell, null);
            if (gm) gm.OnPowerPelletEaten();
            return;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Cherry"))
        {
            if (gm) gm.OnCherryEaten();
            Destroy(other.gameObject);
        }
    }

    public void EnableControl(bool enable)
    {
        controlEnabled = enable;
        if (!enable)
        {
            isMoving = false;
            SetMoving(false);
        }
    }

    public void PlayDeath()
    {
        EnableControl(false);
        if (animator && !string.IsNullOrEmpty(animDeath)) animator.Play(animDeath, 0, 0f);
    }

    public int RemainingPelletCount()
    {
        if (!levelMap) return 0;
        int count = 0;
        BoundsInt bounds = levelMap.cellBounds;
        foreach (var pos in bounds.allPositionsWithin)
        {
            TileBase t = levelMap.GetTile(pos);
            if (t == null) continue;
            if ((pelletTiles != null && pelletTiles.Contains(t)) ||
                (powerPelletTiles != null && powerPelletTiles.Contains(t)))
                count++;
        }
        return count;
    }
}