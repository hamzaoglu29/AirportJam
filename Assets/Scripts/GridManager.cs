using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject redTilePrefab;
    [SerializeField] private GameObject blueTilePrefab;
    [SerializeField] private GameObject yellowTilePrefab;
    [SerializeField] private GameObject greenTilePrefab;
    [SerializeField] private GameObject redPlanePrefab;
    [SerializeField] private GameObject bluePlanePrefab;
    [SerializeField] private GameObject yellowPlanePrefab;
    [SerializeField] private GameObject greenPlanePrefab;
    [SerializeField] private float gridSpaceSize;

    private Tile[,] _tiles;
    private readonly List<Plane> _planes = new List<Plane>();
    private Dictionary<Tile, Tile[]> _neighborDictionary;
    private LevelData _level;
    private Quaternion _rotation;

    public static int LevelIndex { get; set; } = 1;
    public int NumRows { get; private set; }
    public int NumCols { get; private set; }

    /// <summary>
    /// Gets the neighbors of a given tile.
    /// </summary>
    /// <param name="tile">The tile to get neighbors for.</param>
    /// <returns>An array of neighboring tiles.</returns>
    public Tile[] Neighbors(Tile tile) => _neighborDictionary[tile];
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        LevelIndex = PlayerPrefs.GetInt("LevelIndex", LevelIndex);
        Debug.Log($"Attempting to load level: {LevelIndex}");
        var levelFile = Resources.Load<TextAsset>($"Levels/level{LevelIndex}.txt");
        
        if (levelFile == null)
        {
            Debug.LogWarning($"Level file for level {LevelIndex} not found. Resetting to level 1.");
            // If the level file doesn't exist, reset to level 1
            LevelIndex = 1;
            PlayerPrefs.SetInt("LevelIndex", LevelIndex);
            levelFile = Resources.Load<TextAsset>("Levels/level1.txt");
        }
        
        if (levelFile == null)
        {
            Debug.LogError("Failed to load any level file. Check your Resources folder.");
            return;
        }
        
        Debug.Log($"Successfully loaded level file: {levelFile.name}");
        _level = JsonUtility.FromJson<LevelData>(levelFile.text);

        NumRows = _level.row_count;
        NumCols = _level.column_count;
        
        Debug.Log($"Level dimensions: {NumRows}x{NumCols}");
        Debug.Log($"Number of planes: {_level.planes.Count}");
        Debug.Log($"Number of colored tiles: {_level.colored_tiles.Count}");
        
        _tiles = new Tile[NumRows, NumCols];
        _neighborDictionary = new Dictionary<Tile, Tile[]>();
        
        GenerateGrid();
        SpawnPlanes();
    }

    private void SpawnPlanes()
    {
        Vector3 gridCenter = CalculateGridCenter();
        Vector3 offset = -gridCenter;

        foreach (var planeData in _level.planes)
        {
            var row = planeData.row - 1;
            var col = planeData.column - 1;
            var tile = _tiles[row, col];
            var position = new Vector3(row * gridSpaceSize, 0, col * gridSpaceSize) + offset + Vector3.up * 0.12f;

            _rotation = GetRotationFromDirection(planeData.direction);
            var usedPlanePrefab = GetPlanePrefabFromColor(planeData.color);
            
            var plane = Instantiate(usedPlanePrefab, position, _rotation).GetComponent<Plane>();
            _planes.Add(plane);
            
            plane.InitPlane(row, col, planeData.direction);
            _tiles[row, col].isOccupied = true;
        }
    }

    private Quaternion GetRotationFromDirection(string direction)
    {
        return direction switch
        {
            "N" => Quaternion.Euler(0, -90, 0),
            "E" => Quaternion.Euler(0, 0, 0),
            "S" => Quaternion.Euler(0, 90, 0),
            "W" => Quaternion.Euler(0, 180, 0),
            _ => Quaternion.identity
        };
    }

    private GameObject GetPlanePrefabFromColor(string color)
    {
        return color switch
        {
            "B" => bluePlanePrefab,
            "R" => redPlanePrefab,
            "G" => greenPlanePrefab,
            "Y" => yellowPlanePrefab,
            _ => null
        };
    }
    private Vector3 CalculateGridCenter()
    {
        return new Vector3((NumRows - 1) * gridSpaceSize / 2, 0, (NumCols - 1) * gridSpaceSize / 2);
    }

    private void GenerateGrid()
    {
        Vector3 gridCenter = CalculateGridCenter();
        Vector3 offset = -gridCenter;

        for (var x = 0; x < NumRows; x++)
        {
            for (var z = 0; z < NumCols; z++)
            {
                var tileData = _level.colored_tiles.Find(t => t.row == x + 1 && t.column == z + 1);
                var usedTilePrefab = GetTilePrefabFromColor(tileData?.color ?? "E");
                
                Vector3 position = new Vector3(x * gridSpaceSize, 0, z * gridSpaceSize) + offset;
                _tiles[x, z] = Instantiate(usedTilePrefab, position, Quaternion.identity, transform).GetComponent<Tile>();
               
                _tiles[x, z].isColored = usedTilePrefab != tilePrefab;
                _tiles[x, z].InitTile(x, z);
                _tiles[x, z].gameObject.name = $"Tile ( X: {x} , Y: {z})";
            }
        }

        SetupNeighbors();
    }

    private GameObject GetTilePrefabFromColor(string color)
    {
        return color switch
        {
            "B" => blueTilePrefab,
            "R" => redTilePrefab,
            "G" => greenTilePrefab,
            "Y" => yellowTilePrefab,
            "E" => tilePrefab,
            _ => null
        };
    }

    private void SetupNeighbors()
    {
        for (var y = 0; y < NumCols; y++)
        {
            for (var x = 0; x < NumRows; x++)
            {
                var neighbors = new List<Tile>();
                if (y < NumCols - 1) neighbors.Add(_tiles[x, y + 1]);
                if (x < NumRows - 1) neighbors.Add(_tiles[x + 1, y]);
                if (y > 0) neighbors.Add(_tiles[x, y - 1]);
                if (x > 0) neighbors.Add(_tiles[x - 1, y]);

                _neighborDictionary.Add(_tiles[x, y], neighbors.ToArray());
            }
        }
    }

    /// <summary>
    /// Gets the tile at the specified position.
    /// </summary>
    /// <param name="x">The x-coordinate of the tile.</param>
    /// <param name="y">The y-coordinate of the tile.</param>
    /// <returns>The tile at the specified position.</returns>
    public Tile GetTileAtPos(int x, int y) => _tiles[x, y];

    /// <summary>
    /// Removes a plane from the game and checks for level completion.
    /// </summary>
    /// <param name="plane">The plane to remove.</param>
    public void RemovePlane(Plane plane)
    {
        _planes.Remove(plane);
        IsSuccess();
    }

    private void IsSuccess()
    {
        if (_planes.Count == 0)
        {
            PlayNextLevel();
        }
    }

    private static void PlayNextLevel()
    {
        LevelIndex++;
        // Check if the next level exists
        if (Resources.Load<TextAsset>($"Levels/level{LevelIndex}.txt") == null)
        {
            // If it doesn't exist, reset to level 1
            LevelIndex = 1;
        }
        PlayerPrefs.SetInt("LevelIndex", LevelIndex);
        SceneManager.LoadScene("Gameplay");
    }
}
