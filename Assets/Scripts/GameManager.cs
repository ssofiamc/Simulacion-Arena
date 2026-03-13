using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [Header("Configuración de la Simulación")]
    public int width = 50;
    public int height = 30;
    public float updateTime = 0.0001f;
    public GameObject cellPrefab;

    private bool[,] grid;
    private bool[,] nextGrid;
    private GameObject[,] cellObjects;
    private float timer;
    private int generations = 0; 
    private bool isPaused = false;

    void Start()
    {
        grid = new bool[width, height];
        cellObjects = new GameObject[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject cell = Instantiate(cellPrefab, new Vector3(x, y, 0), Quaternion.identity);
                cellObjects[x, y] = cell;

                cellObjects[x, y].GetComponent<SpriteRenderer>().color = Color.white;
            }
        }

        InputManager.Instance.OnToggleCell += HandleMouseClick;
        InputManager.Instance.OnPause += () => isPaused = !isPaused;
        InputManager.Instance.OnClear += ClearGrid;

        Debug.Log("Simulación iniciada: Tablero limpio. Haz click para poner arena.");
    }

    void Update()
    {
        if (isPaused) return;

        timer += Time.deltaTime;
        if (timer >= updateTime)
        {
            timer = 0;
            UpdateSandPhysics();
        }
    }

    void UpdateSandPhysics()
    {
        nextGrid = new bool[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[x, y]) // Si hay un grano de arena
                {
                    // Si ya está en el suelo, se queda ahí
                    if (y == 0)
                    {
                        nextGrid[x, y] = true;
                        continue;
                    }

                    // 1. Intentar caer directo hacia abajo
                    if (!grid[x, y - 1])
                    {
                        nextGrid[x, y - 1] = true;
                    }
                    // 2. Si abajo está ocupado, intentar diagonales
                    else
                    {
                        bool canMoveLeft = (x > 0 && !grid[x - 1, y - 1]);
                        bool canMoveRight = (x < width - 1 && !grid[x + 1, y - 1]);

                        if (canMoveLeft && canMoveRight)
                        {
                            // Comportamiento aleatorio
                            int dir = Random.value < 0.5f ? -1 : 1;
                            nextGrid[x + dir, y - 1] = true;
                        }
                        else if (canMoveLeft)
                        {
                            nextGrid[x - 1, y - 1] = true;
                        }
                        else if (canMoveRight)
                        {
                            nextGrid[x + 1, y - 1] = true;
                        }
                        else
                        {
                            // 3. Si no hay espacio, se acumula (se queda donde está)
                            nextGrid[x, y] = true;
                        }
                    }
                }
            }
        }

        grid = nextGrid;
        generations++;

        // Mostrar evolución en consola día a día
        Debug.Log("Simulación Evolucionando - Día: " + generations);

        UpdateVisuals();
    }

    void HandleMouseClick()
    {
        Vector3 mousePos = Mouse.current.position.ReadValue();
        mousePos.z = -Camera.main.transform.position.z;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

        int x = Mathf.RoundToInt(worldPos.x);
        int y = Mathf.RoundToInt(worldPos.y);

        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            grid[x, y] = true; 
            UpdateVisuals();
        }
    }

    void ClearGrid()
    {
        grid = new bool[width, height];
        generations = 0;
        UpdateVisuals();
        Debug.Log("Tablero reiniciado por el usuario.");
    }

    void UpdateVisuals()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var rend = cellObjects[x, y].GetComponent<SpriteRenderer>();

                if (grid[x, y])
                    rend.color = new Color(1.0f, 0.6f, 0.0f); 
                else
                    rend.color = Color.white; 
            }
        }
    }
}