namespace AdventureGame;

public class AdventureGame
{
    public readonly string GO_NORTH = "W";
    public readonly string GO_SOUTH = "S";
    public readonly string GO_EAST = "D";
    public readonly string GO_WEST = "A";
    public readonly string GET_LAMP = "L";
    public readonly string GET_KEY = "K";
    public readonly string OPEN_CHEST = "O";
    public readonly string QUIT = "Q";

    private Adventurer adventurer = default!;
    private Room[,] dungeon = default!;
    private const char Wall = '#';
    private int aRow;
    private int aCol;
    private int grueRow;
    private int grueCol;
    private bool isChestOpen;
    private bool hasPlayerQuit;
    private bool isAdventureAlive;
    private string statusMessage = ""; // Added status message
    private string lastDirection = default!;

    public AdventureGame() { }

    public void Start()
    {
        Init();
        ShowGameStartScreen();

        string input;
        do
        {
            Console.Clear(); // Clears screen every step
            ShowScene();

            do
            {
                ShowInputOptions();
                input = GetInput();
            }
            while (!IsValidInput(input));

            ProcessInput(input);
            UpdateGameState();
        }
        while (!IsGameOver());

        ShowGameOverScreen();
    }

    private void Init()
    {
        adventurer = new Adventurer();
        dungeon = Load("DungeonTemplate.txt");

        aRow = 1;
        aCol = 1;
        grueRow = 1; // Grue starts far away
        grueCol = 6;

        isChestOpen = false;
        hasPlayerQuit = false;
        isAdventureAlive = true;
        statusMessage = "Find the key and the chest to escape!";
        lastDirection = string.Empty;
    }

    private void ShowGameStartScreen()
    {
        Console.Clear();
        Console.WriteLine("============================");
        Console.WriteLine(" Welcome to Adventure Game! ");
        Console.WriteLine("============================");
        Console.WriteLine("Press any key to begin...");
        Console.ReadKey();
    }

    private void ShowScene()
    {
        PrintDungeonMap();
        var r = dungeon[aRow, aCol];

        if (adventurer.HasLamp() || r.IsLit())
        {
            Console.WriteLine(r.GetDescription());
        }
        else
        {
            Console.WriteLine("This room is pitch black!");
        }

        if (!string.IsNullOrEmpty(statusMessage))
        {
            Console.WriteLine($"\n>>> {statusMessage} <<<");
            statusMessage = ""; 
        }
    }

    private void ShowInputOptions()
    {
        Console.Write($"\nGO: {GO_NORTH}/{GO_SOUTH}/{GO_WEST}/{GO_EAST} | ACTIONS: {GET_LAMP}/{GET_KEY}/{OPEN_CHEST} | {QUIT}: Quit\n> ");
    }

    private string GetInput() => Console.ReadLine()?.ToUpper() ?? "";

    private bool IsValidInput(string input)
    {
        string[] valid = { GO_NORTH, GO_SOUTH, GO_EAST, GO_WEST, GET_LAMP, GET_KEY, OPEN_CHEST, QUIT };
        return valid.Contains(input);
    }

    private void ProcessInput(string input)
    {
        Room r = dungeon[aRow, aCol];
        if (input == GO_NORTH) GoNorth(r);
        else if (input == GO_SOUTH) GoSouth(r);
        else if (input == GO_EAST) GoEast(r);
        else if (input == GO_WEST) GoWest(r);
        else if (input == GET_LAMP) GetLamp(r);
        else if (input == GET_KEY) GetKey(r);
        else if (input == OPEN_CHEST) OpenChest(r);
        else Quit();
    }

    private void UpdateGameState()
    {
        MoveGrue();

        if (grueRow == aRow && grueCol == aCol)
        {
            isAdventureAlive = false;
        }

        if (isChestOpen && (aRow != 8 || aCol != 8))
        {
            // If the message is empty (no collision or wall error), show objective
            if(string.IsNullOrEmpty(statusMessage))
                statusMessage = "THE GRUE IS AWAKE! RUN TO THE EXIT!";
        }
    }

    private bool IsGameOver()
    {
        bool hasReachedExit = (aRow == 1 && aCol == 8);
        return (isChestOpen && hasReachedExit) || hasPlayerQuit || !isAdventureAlive;
    }

    private void ShowGameOverScreen()
    {
        Console.Clear();
        if (isChestOpen && aRow == 1 && aCol == 8)
            Console.WriteLine("VICTORY: You escaped with the treasure!");
        else if (!isAdventureAlive)
            Console.WriteLine("FATALITY: The Grue caught you in the dark.");
        else
            Console.WriteLine("GAME OVER: You abandoned the quest.");
    }

    // --- Movement & Actions ---
    private void GoNorth(Room r) { if (r.HasNorth()) aRow--; else statusMessage = "The path north is blocked."; }
    private void GoSouth(Room r) { if (r.HasSouth()) aRow++; else statusMessage = "The path south is blocked."; }
    private void GoEast(Room r) { if (r.HasEast()) aCol++; else statusMessage = "The path east is blocked."; }
    private void GoWest(Room r) { if (r.HasWest()) aCol--; else statusMessage = "The path west is blocked."; }

    private void GetLamp(Room r) 
    { 
        if (r.HasLamp()) { adventurer.SetLamp(true); r.SetLamp(false); statusMessage = "You got the lamp!"; } 
        else statusMessage = "No lamp here."; 
    }
    private void GetKey(Room r) 
    { 
        if (r.HasKey()) { adventurer.SetKey(true); r.SetKey(false); statusMessage = "You got the key!"; } 
        else statusMessage = "No key here."; 
    }
    private void OpenChest(Room r) 
   {
    if (r.HasChest())
    {
        if (adventurer.HasKey())
        {
            isChestOpen = true;
            r.SetChest(false); // Remove the chest from the room so you can't open it twice
            statusMessage = "!!! TREASURE CLAIMED !!! The ground shakes... something is coming!";
        }
        else
        {
            statusMessage = "The chest is heavy and locked. You need a key.";
        }
    }
    else
    {
        statusMessage = "There is no chest here.";
    }
}

    private void Quit() { hasPlayerQuit = true; }

    // --- Loading & Map ---
    public Room[,] Load(string filePath)
    {
        // Note: Used relative path to avoid local machine errors
        string[] lines = File.ReadAllLines("AdventureGame\\" + filePath);

        int rows = int.Parse(lines[0]);
        int cols = int.Parse(lines[1]);
        int lampRow = int.Parse(lines[4]), lampCol = int.Parse(lines[5]);
        int keyRow = int.Parse(lines[6]), keyCol = int.Parse(lines[7]);
        int chestRow = int.Parse(lines[8]), chestCol = int.Parse(lines[9]);

        int layoutStart = 12;
        Room[,] dungeon = new Room[rows, cols];
        List<(int row, int col)> traversableTiles = new();

        for (int row = 0; row < rows; row++)
        {
            string layoutLine = lines[layoutStart + row];
            for (int col = 0; col < cols; col++)
            {
                if (layoutLine[col] != Wall)
                {
                    dungeon[row, col] = new Room();
                    traversableTiles.Add((row, col));
                }
            }
        }

        int descriptionsStart = layoutStart + rows + 1;
        for (int i = 0; i < traversableTiles.Count; i++)
        {
            string[] parts = lines[descriptionsStart + i].Split('|', 2);
            var (row, col) = traversableTiles[i];
            Room room = dungeon[row, col];
            room.SetLit(parts[0] == "1");
            room.SetDescription(parts[1]);
            room.SetLamp(row == lampRow && col == lampCol);
            room.SetKey(row == keyRow && col == keyCol);
            room.SetChest(row == chestRow && col == chestCol);
            room.SetNorth(IsTraversable(dungeon, row - 1, col));
            room.SetSouth(IsTraversable(dungeon, row + 1, col));
            room.SetEast(IsTraversable(dungeon, row, col + 1));
            room.SetWest(IsTraversable(dungeon, row, col - 1));
        }
        return dungeon;
    }

    private bool IsTraversable(Room[,] d, int r, int c) => r >= 0 && r < d.GetLength(0) && c >= 0 && c < d.GetLength(1) && d[r, c] != null;

    // --- Grue A* Pathfinding ---
    private void MoveGrue()
    {
        if (!isChestOpen) return; // Grue only moves when chest is open

        (int row, int col) start = (grueRow, grueCol);
        (int row, int col) goal = (aRow, aCol);
        PriorityQueue<(int row, int col), int> openSet = new();
        Dictionary<(int, int), (int, int)> cameFrom = new();
        Dictionary<(int, int), int> gScore = new() { [start] = 0 };
        openSet.Enqueue(start, Heuristic(start, goal));

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();
            if (current == goal) { ReconstructPath(cameFrom, current); return; }

            int[] dr = { -1, 1, 0, 0 };
            int[] dc = { 0, 0, -1, 1 };

            for (int i = 0; i < 4; i++)
            {
                int nr = current.row + dr[i], nc = current.col + dc[i];
                if (IsTraversable(dungeon, nr, nc))
                {
                    var neighbor = (nr, nc);
                    int tentG = gScore[current] + 1;
                    if (!gScore.ContainsKey(neighbor) || tentG < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentG;
                        openSet.Enqueue(neighbor, tentG + Heuristic(neighbor, goal));
                    }
                }
            }
        }
    }

    private int Heuristic((int row, int col) a, (int row, int col) b) => Math.Abs(a.row - b.row) + Math.Abs(a.col - b.col);

    private void ReconstructPath(Dictionary<(int, int), (int, int)> cameFrom, (int row, int col) current)
    {
        List<(int row, int col)> path = new() { current };
        while (cameFrom.ContainsKey(current)) { current = cameFrom[current]; path.Add(current); }
        path.Reverse();
        if (path.Count > 1) { grueRow = path[1].row; grueCol = path[1].col; }
    }

    private void PrintDungeonMap()
    {
        for (int row = 0; row < dungeon.GetLength(0); row++)
        {
            for (int col = 0; col < dungeon.GetLength(1); col++)
            {
                if (aRow == row && aCol == col) Console.Write('P');
                else if (grueRow == row && grueCol == col) Console.Write('G');
                else if (dungeon[row, col] == null) Console.Write('#');
                else if (dungeon[row, col].HasLamp()) Console.Write('L');
                else if (dungeon[row, col].HasKey()) Console.Write('K');
                else if (dungeon[row, col].HasChest()) Console.Write('C');
                else if (row == 1 && col == 8) Console.Write('E');
                else Console.Write('.');
            }
            Console.WriteLine();
        }
    }
}