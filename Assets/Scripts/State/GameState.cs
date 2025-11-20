using System.Collections.Generic;
using State;
using UnityEngine;
using UnityEngine.Serialization;

public class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }

    public Wall[,] WallMap { get; private set; }
    public List<PlayerSpawn> PlayerSpawns { get; private set; }
    public GameObject destructibleWallPrefab;
    public GameObject nonDestructibleWallPrefab;

    private int _gridTileWidth = 2;
    private int _gameWidthInTiles;
    private int _gameHeightInTiles;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        Load(@"
XXXXX
XP  X
X XDX
XDD X
XXXXX
"
            , 5, 5);
    }

    private void Load(string mapData, int width, int height)
    {
        _gameHeightInTiles = height;
        _gameWidthInTiles = width;
        WallMap = new Wall[height, width];
        PlayerSpawns = new List<PlayerSpawn>();

        string[] lines = mapData.Split('\n');
        var validLines = new List<string>();

        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();
            if (!string.IsNullOrEmpty(trimmedLine))
            {
                validLines.Add(trimmedLine);
            }
        }

        int y = 0;
        int lineIndex = 0;

        while (lineIndex < validLines.Count && y < height)
        {
            string currentLine = validLines[lineIndex];
            int x = 0;
            int charIndex = 0;

            while (charIndex < currentLine.Length && x < width)
            {
                char currentChar = currentLine[charIndex];

                switch (currentChar)
                {
                    case 'X':

                        var wallGo = Instantiate(nonDestructibleWallPrefab,
                            new Vector3(x * _gridTileWidth, 0, y * _gridTileWidth), Quaternion.identity);
                        WallMap[y, x] = new Wall(wallGo, false);
                        break;

                    case 'P':
                        PlayerSpawns.Add(new PlayerSpawn(x, y));
                        //todo spawn player and track GO
                        GameObject.FindGameObjectWithTag("Player").transform.position =
                            new Vector3(x * _gridTileWidth, 1, y * _gridTileWidth);
                        break;

                    case 'D':
                        var destructibleWallGo = Instantiate(destructibleWallPrefab,
                            new Vector3(x * _gridTileWidth, 0, y * _gridTileWidth),
                            Quaternion.identity);
                        WallMap[y, x] = new Wall(destructibleWallGo, true);
                        break;

                    case ' ':

                        break;

                    default:
                        Debug.LogWarning($"Neznámy znak '{currentChar}' na pozícii [{x}, {y}]");
                        break;
                }

                charIndex++;
                x++;
            }

            lineIndex++;
            y++;
        }

        // todo generate GO from map data
    }


    void Update()
    {
    }
}