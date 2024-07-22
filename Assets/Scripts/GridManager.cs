using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject redTilePrefab;
    [SerializeField] private GameObject blueTilePrefab;
    [SerializeField] private GameObject yellowTilePrefab;
    [SerializeField] private GameObject greenTilePrefab;
    [SerializeField] private GameObject redPlanePrefab;
    [SerializeField] private GameObject bluePlanePrefab;
    [SerializeField] private GameObject yellowPlanePrefab;
    [SerializeField] private GameObject greenPlanePrefab;
    
    private Tile[,] _tiles; 
    private readonly List<Plane> _planes = new List<Plane>() ;
    
    //
    private Dictionary<Tile, Tile[]> neighborDictionary;
    public IEnumerable<Tile> Neighbors(Tile tile)
    {
        return neighborDictionary[tile];
    }
    //
    private LevelData _level;
    public static int LevelIndex = 1;
    private Quaternion _rotation;

    public int numRows;
    public int numCols;
    [SerializeField] private float gridSpaceSize;
    
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
        var levelFile = Resources.Load<TextAsset>("Levels/level" + LevelIndex);
        _level = JsonUtility.FromJson<LevelData>(levelFile.text);

        numRows = _level.row_count;
        numCols = _level.column_count;
        
        _tiles = new Tile[numRows, numCols];
        //
        neighborDictionary = new Dictionary<Tile, Tile[]>();
        //
        GenerateGrid();
        
        foreach (var planeData in _level.planes)
        {
            var gridData=new GridData{row = planeData.row-1, col = planeData.column-1};

            var tile = _tiles[gridData.row,gridData.col];
            var position = tile.transform.position+ Vector3.up * 0.12f;

            _rotation = planeData.direction switch
            {
                "N" => Quaternion.Euler(0, -90, 0),
                "E" => Quaternion.Euler(0, 0, 0),
                "S" => Quaternion.Euler(0, 90, 0),
                "W" => Quaternion.Euler(0, 180, 0),
                _ => _rotation
            };

            var usedPlanePrefab = planeData.color switch
            {
                "B" => bluePlanePrefab,
                "R" => redPlanePrefab,
                "G" => greenPlanePrefab,
                "Y" => yellowPlanePrefab,
                _ => null
            };
            
            var plane = Instantiate(usedPlanePrefab, position, _rotation).GetComponent<Plane>();
            _planes.Add(plane);
            
            plane.InitPlane(planeData.row - 1, planeData.column - 1, planeData.direction /*planeData.color*/);
            _tiles[gridData.row, gridData.col].isOccupied = true;
            
            
        }
    }
    //
    public Tile GetTileAtPos(int x, int y)
    {
        return _tiles[x, y];
    }

    public void RemovePlane(Plane plane)
    {
        _planes.Remove(plane);
        IsSuccess();
    } 
    //
    private void GenerateGrid()
    {
        var i=0;
        for (var x = 0; x < numRows; x++) {
            for (var z = 0; z < numCols; z++)
            {
                var tileData = _level.colored_tiles[i];
                var usedTilePrefab = tileData.color switch
                {
                    "B" => blueTilePrefab,
                    "R" => redTilePrefab,
                    "G" => greenTilePrefab,
                    "Y" => yellowTilePrefab,
                    "E" => tilePrefab,
                    _ => null
                };
                
                _tiles[x, z] = Instantiate(usedTilePrefab, new Vector3(x * gridSpaceSize, 0, z * gridSpaceSize),
                    Quaternion.identity, transform).GetComponent<Tile>();
               
                _tiles[x, z].isColored = usedTilePrefab != tilePrefab;
                
                _tiles[x, z].InitTile(x, z);
                _tiles[x, z].gameObject.name = "Tile ( X: " + x + " , Y: " + z + ")";
                
                i += 1;
            }
        }
        //
        for (var y = 0; y < numCols; y++)
        {
            for (var x = 0; x < numRows; x++)
            {
                var neighbors = new List<Tile>();
                if (y < numCols-1)
                    neighbors.Add(_tiles[x, y + 1]);
                if (x < numRows-1)
                    neighbors.Add(_tiles[x + 1, y]);
                if (y > 0)
                    neighbors.Add(_tiles[x, y - 1]);
                if (x > 0)
                    neighbors.Add(_tiles[x - 1, y]);

                neighborDictionary.Add(_tiles[x, y], neighbors.ToArray());
            }
        }
        //
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
        LevelIndex += 1;
        PlayerPrefs.SetInt("LevelIndex", LevelIndex);
        SceneManager.LoadScene("Gameplay");
    }
    
}
