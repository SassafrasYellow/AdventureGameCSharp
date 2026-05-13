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
	private string lastDirection = default!;

	public AdventureGame()
	{

	}

	public void Start()
	{
		Init();

		ShowGameStartScreen();

		string input;

		do
		{
			ShowScene();

			do
			{
				ShowInputOptions();

				input = GetInput();
			}
			while(!IsValidInput(input));

			ProcessInput(input);

			UpdateGameState();
		}
		while(!IsGameOver());

		ShowGameOverScreen();
	}

	private void Init()
	{
		adventurer = new Adventurer();

		 dungeon = Load("DungeonTemplate.txt");

		aRow = 1;
		aCol = 0;
		grueRow = 7;
		grueCol = 5;

		isChestOpen = false;
		hasPlayerQuit = false;
		isAdventureAlive = true;

		lastDirection = string.Empty;
	}

	private void ShowGameStartScreen()
	{
		Console.WriteLine("Welcome to Adventure Game!");
	}

	private void ShowScene()
	{
		PrintDungeonMap();
		var r = dungeon[aRow, aCol];

		if(adventurer.HasLamp() || r.IsLit())
		{
			Console.WriteLine(r.GetDescription());
		}
		else
		{
			Console.WriteLine("This room is pitch black!");
		}
	}

	private void ShowInputOptions()
	{
		string options = ""
		+ $"GO NORTH [{GO_NORTH}] | GO EAST [{GO_EAST}] | GET LAMP [{GET_LAMP}] | OPEN CHEST [{OPEN_CHEST}]\n"
		+ $"GO SOUTH [{GO_SOUTH}] | GO WEST [{GO_WEST}] | GET KEY  [{GET_KEY}] | QUIT       [{QUIT}]\n"
		+ $"> ";

		Console.Write(options);
	}

	private string GetInput()
	{
		return Console.ReadLine()!.ToUpper();
	}

	private bool IsValidInput(string input)
	{
		string[] validInputs = { GO_NORTH, GO_SOUTH, GO_EAST, GO_WEST, GET_LAMP, GET_KEY, OPEN_CHEST, QUIT };

		if(!validInputs.Contains(input))
		{
			Console.WriteLine("ERROR: Invalid input. Please try again.");
			return false;
		}

		return true;
	}

	private void ProcessInput(string input)
	{
		Room r = dungeon[aRow, aCol];

		if(input == GO_NORTH)
		{
			GoNorth(r);
		}
		else if(input == GO_SOUTH)
		{
			GoSouth(r);
		}
		else if(input == GO_EAST)
		{
			GoEast(r);
		}
		else if(input == GO_WEST)
		{
			GoWest(r);
		}
		else if(input == GET_LAMP)
		{
			GetLamp(r);
		}
		else if(input == GET_KEY)
		{
			GetKey(r);
		}
		else if(input == OPEN_CHEST)
		{
			OpenChest(r);
		}
		else// if(input == QUIT)
		{
			Quit();
		}
	}

	private void UpdateGameState()
	{
	MoveGrue();

    if(grueRow == aRow && grueCol == aCol)
    {
        Console.WriteLine("The Grue catches and devours you!");
        isAdventureAlive = false;
    }
	}

	private bool IsGameOver()
	{
		return isChestOpen || hasPlayerQuit || !isAdventureAlive;
	}

	private void ShowGameOverScreen()
	{
		Console.WriteLine("Game Over!");
	}

	private void GoNorth(Room r)
	{
		if(r.HasNorth())
		{
			aRow -= 1;
			lastDirection = GO_SOUTH;
		}
		else
		{
			Console.WriteLine("You cannot go north!\a");
		}
	}

	private void GoSouth(Room r)
	{
		if(r.HasSouth())
		{
			aRow += 1;
			lastDirection = GO_NORTH;
		}
		else
		{
			Console.WriteLine("You cannot go south!\a");
		}
	}

	private void GoEast(Room r)
	{
		if(r.HasEast())
		{
			aCol += 1;
			lastDirection = GO_WEST;
		}
		else
		{
			Console.WriteLine("You cannot go east!\a");
		}
	}

	private void GoWest(Room r)
	{
		if(r.HasWest())
		{
			aCol -= 1;
			lastDirection = GO_EAST;
		}
		else
		{
			Console.WriteLine("You cannot go west!\a");
		}
	}

	private void GetLamp(Room r)
	{
		if(r.HasLamp())
		{
			Console.WriteLine("You got the lamp!");
			adventurer.SetLamp(true);
			r.SetLamp(false);
		}
		else
		{
			Console.WriteLine("There is no lamp in this room.");
		}
	}

	private void GetKey(Room r)
	{
		if(r.HasKey())
		{
			Console.WriteLine("You got the key!");
			adventurer.SetKey(true);
			r.SetKey(false);
		}
		else
		{
			Console.WriteLine("There is no key in this room.");
		}
	}

	private void OpenChest(Room r)
	{
		if(r.HasChest())
		{
			if(adventurer.HasKey())
			{
				Console.WriteLine("You got the treasure!");
				isChestOpen = true;
			}
			else
			{
				Console.WriteLine("You do not have the key!");
			}
		}
		else
		{
			Console.WriteLine("There is no chest in this room.");
		}
	}

	private void Quit()
	{
		Console.WriteLine("You quit the game!");
		hasPlayerQuit = true;
	}

	//Dungeon Loader code
	
	public Room[,] Load(string filePath)
	{
		string[] lines = File.ReadAllLines("C:\\Users\\siriu\\OneDrive\\Documents\\GitHub\\AdventureGameCSharpJorge\\AdventureGameCSharp\\res\\DungeonTemplate.txt"
);

		int rows = int.Parse(lines[0]);
		int cols = int.Parse(lines[1]);

		int exitRow = int.Parse(lines[2]);
		int exitCol = int.Parse(lines[3]);
		int lampRow = int.Parse(lines[4]);
		int lampCol = int.Parse(lines[5]);
		int keyRow = int.Parse(lines[6]);
		int keyCol = int.Parse(lines[7]);
		int chestRow = int.Parse(lines[8]);
		int chestCol = int.Parse(lines[9]);

		// lines[10] and lines[11] are grueRow/grueCol if needed elsewhere

		int layoutStart = 12;
		int descriptionsStart = layoutStart + rows;

		if (lines.Length < descriptionsStart)
			throw new FormatException("File does not contain enough layout rows.");

		Room[,] dungeon = new Room[rows, cols];
		List<(int row, int col)> traversableTiles = new();

		for (int row = 0; row < rows; row++)
		{
			string layoutLine = lines[layoutStart + row];

			if (layoutLine.Length != cols)
				throw new FormatException($"Layout row {row} must contain exactly {cols} characters.");

			for (int col = 0; col < cols; col++)
			{
				if (layoutLine[col] != Wall)
				{
					dungeon[row, col] = new Room();
					traversableTiles.Add((row, col));
				}
			}
		}

		int descriptionCount = lines.Length - descriptionsStart;

		if (descriptionCount != traversableTiles.Count)
		{
			throw new FormatException(
					$"Description count ({descriptionCount}) must match traversable tile count ({traversableTiles.Count})."
			);
		}

		for (int i = 0; i < traversableTiles.Count; i++)
		{
			string[] parts = lines[descriptionsStart + i].Split('|', 2);
			Console.WriteLine(lines[descriptionsStart + i]);
				if (parts.Length != 2)
					throw new FormatException($"Invalid room description line: {lines[descriptionsStart + i]}");

			bool isLit = parts[0] switch
			{
				"1" => true,
				"0" => false,
				_ => throw new FormatException("Room lit value must be 1 or 0.")
			};

			string description = parts[1];

			var (row, col) = traversableTiles[i];
			Room room = dungeon[row, col];

			room.SetLit(isLit);
			room.SetDescription(description);

			room.SetLamp(row == lampRow && col == lampCol);
			room.SetKey(row == keyRow && col == keyCol);
			room.SetChest(row == chestRow && col == chestCol);

			room.SetNorth(IsTraversable(dungeon, row - 1, col));
			room.SetSouth(IsTraversable(dungeon, row + 1, col));
			room.SetEast(IsTraversable(dungeon, row, col + 1));
			room.SetWest(IsTraversable(dungeon, row, col - 1));
		}

		ValidateTraversableTile(dungeon, exitRow, exitCol, "exit");

		return dungeon;
	}

	private bool IsTraversable(Room[,] dungeon, int row, int col)
	{
		return row >= 0 &&
					 row < dungeon.GetLength(0) &&
					 col >= 0 &&
					 col < dungeon.GetLength(1) &&
					 dungeon[row, col] != null;
	}

	private void ValidateTraversableTile(Room[,] dungeon, int row, int col, string name)
	{
		if (!IsTraversable(dungeon, row, col))
			throw new FormatException($"The {name} position must be on a traversable tile.");
	}

	//This is the A* method. It's used for controlling the movement of the grue.

	private void MoveGrue()
{
    (int row, int col) start = (grueRow, grueCol);
    (int row, int col) goal = (aRow, aCol);

    PriorityQueue<(int row, int col), int> openSet = new();

    Dictionary<(int, int), (int, int)> cameFrom = new();

    Dictionary<(int, int), int> gScore = new();
    Dictionary<(int, int), int> fScore = new();

    gScore[start] = 0;
    fScore[start] = Heuristic(start, goal);

    openSet.Enqueue(start, fScore[start]);

    int[] dRows = { -1, 1, 0, 0 };
    int[] dCols = { 0, 0, -1, 1 };

    while(openSet.Count > 0)
    {
        var current = openSet.Dequeue();

        if(current == goal)
        {
            ReconstructPath(cameFrom, current);
            return;
        }

        for(int i = 0; i < 4; i++)
        {
            int newRow = current.row + dRows[i];
            int newCol = current.col + dCols[i];

            bool valid =
                newRow >= 0 &&
                newRow < dungeon.GetLength(0) &&
                newCol >= 0 &&
                newCol < dungeon.GetLength(1) &&
                dungeon[newRow, newCol] != null;

            if(!valid)
                continue;

            var neighbor = (newRow, newCol);

            int tentativeGScore = gScore[current] + 1;

            if(!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
            {
                cameFrom[neighbor] = current;

                gScore[neighbor] = tentativeGScore;

                fScore[neighbor] =
                    tentativeGScore + Heuristic(neighbor, goal);

                openSet.Enqueue(neighbor, fScore[neighbor]);
            }
        }
    }
}
private int Heuristic((int row, int col) a, (int row, int col) b)
{
    return Math.Abs(a.row - b.row)
         + Math.Abs(a.col - b.col);
}
private void ReconstructPath(
    Dictionary<(int, int), (int, int)> cameFrom,
    (int row, int col) current)
{
    List<(int row, int col)> path = new();

    path.Add(current);

    while(cameFrom.ContainsKey(current))
    {
        current = cameFrom[current];
        path.Add(current);
    }

    path.Reverse();

    if(path.Count > 1)
    {
        grueRow = path[1].row;
        grueCol = path[1].col;
    }
}
//Shows Dungeon map
private void PrintDungeonMap()
{
    for(int row = 0; row < dungeon.GetLength(0); row++)
    {
        for(int col = 0; col < dungeon.GetLength(1); col++)
        {
            if(aRow == row && aCol == col)
            {
                Console.Write('P');
            }
            else if(grueRow == row && grueCol == col)
            {
                Console.Write('G');
            }
            else if(dungeon[row, col] == null)
            {
                Console.Write('#');
            }
            else if(dungeon[row, col].HasLamp())
            {
                Console.Write('L');
            }
            else if(dungeon[row, col].HasKey())
            {
                Console.Write('K');
            }
            else if(dungeon[row, col].HasChest())
            {
                Console.Write('C');
            }
            else
            {
                Console.Write('.');
            }
        }

        Console.WriteLine();
    }
}
}
