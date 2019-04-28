using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour {


    public ObjectPool platformPool;
    public ObjectPool upPool;
    public ObjectPool upLongPool;
    public ObjectPool downPool;
    public ObjectPool downLongPool;
    public ObjectPool ladderPool;
    public ObjectPool movingPool;

    public GameObject chest;

    List<Platform> platforms;
    static readonly bool terrainDebugLogging = false;

    delegate void Generator(ref Vector2 start, ref float length, List<Platform> plfs);
    enum Difficulty { V_EASY = 0, EASY = 1, MODERATE = 2, HARD = 3, V_HARD = 4 }
    List<System.Tuple<Generator, Difficulty>> generators = new List<System.Tuple<Generator, Difficulty>>() {
            new System.Tuple<Generator, Difficulty>(FlatShort, Difficulty.V_EASY),
            new System.Tuple<Generator, Difficulty>(SmallPit, Difficulty.EASY),
            new System.Tuple<Generator, Difficulty>(WidePit, Difficulty.EASY),
            new System.Tuple<Generator, Difficulty>(ClimableWall, Difficulty.MODERATE),
            new System.Tuple<Generator, Difficulty>(NoReturnDrop, Difficulty.MODERATE),
            new System.Tuple<Generator, Difficulty>(StairsRight, Difficulty.HARD),
            new System.Tuple<Generator, Difficulty>(StairsSteep, Difficulty.V_HARD)
        };


    void Awake()
    {
        //terrainPool = GetComponent<ObjectPool>();
        //Random.InitState(20);
        Platform.AssignPools(platformPool, upPool,upLongPool, downPool, downLongPool, ladderPool, movingPool);
    }

    int currentLevel = 0;
    List<System.Tuple<int, Difficulty>> levels = new List<System.Tuple<int, Difficulty>>() {
        new System.Tuple<int, Difficulty>(60, Difficulty.V_EASY),
        new System.Tuple<int, Difficulty>(100, Difficulty.EASY),
        new System.Tuple<int, Difficulty>(200, Difficulty.V_EASY),
        new System.Tuple<int, Difficulty>(100, Difficulty.MODERATE),
        new System.Tuple<int, Difficulty>(200, Difficulty.EASY),
        new System.Tuple<int, Difficulty>(200, Difficulty.MODERATE),
        new System.Tuple<int, Difficulty>(100, Difficulty.HARD),
        new System.Tuple<int, Difficulty>(300, Difficulty.EASY),
        new System.Tuple<int, Difficulty>(200, Difficulty.HARD),
        new System.Tuple<int, Difficulty>(300, Difficulty.MODERATE),
        new System.Tuple<int, Difficulty>(200, Difficulty.V_HARD),
        new System.Tuple<int, Difficulty>(300, Difficulty.HARD),
        new System.Tuple<int, Difficulty>(300, Difficulty.V_HARD),
        new System.Tuple<int, Difficulty>(500, Difficulty.MODERATE),
        new System.Tuple<int, Difficulty>(500, Difficulty.HARD),
        new System.Tuple<int, Difficulty>(500, Difficulty.V_HARD)
    };

    public void GenerateNextLevel()
    {
        Platform.DeactivateAllObjects();

        platforms = MakeLevel(new Vector2(0, 0), levels[currentLevel].Item1, levels[currentLevel].Item2);
        platforms.ForEach(p => p.Make());
        currentLevel++;
    }

    public void GenerateCustomLevel(int length, int difficulty)
    {
        platformPool.DeactivateAllObjects();

        platforms = MakeLevel(new Vector2(0, 0), length, (Difficulty)difficulty);
        platforms.ForEach(p => p.Make());
    }

    public System.Tuple<string,string> InfoNextLevel()
    {
        switch (levels[currentLevel].Item2)
        {
        case Difficulty.V_EASY:
            return new System.Tuple<string, string>(levels[currentLevel].Item1.ToString(), "Very easy");
        case Difficulty.EASY:
            return new System.Tuple<string, string>(levels[currentLevel].Item1.ToString(), "Easy");
        case Difficulty.MODERATE:
            return new System.Tuple<string, string>(levels[currentLevel].Item1.ToString(), "Moderate");
        case Difficulty.HARD:
            return new System.Tuple<string, string>(levels[currentLevel].Item1.ToString(), "Hard");
        case Difficulty.V_HARD:
            return new System.Tuple<string, string>(levels[currentLevel].Item1.ToString(), "Very hard");
        }
        throw new System.Exception("Bad Difficulty");
    }

    public int GetCurrentLevel()
    {
        return currentLevel - 1;
    }

    public bool IsFinalLevel()
    {
        return levels.Count <= currentLevel;
    }

    public void ResetLevel()
    {
        currentLevel = 0;
    }

    List<Platform> MakeLevel(Vector2 start, float length, Difficulty levelDifficulty)
    {
        // return MakeFlatPath(start, length);
        List<Platform> plfs = new List<Platform>();
        StagingGround(ref start, ref length, plfs);

        int totalWeight = generators.Sum(tpl => GetDifficultyWeight(levelDifficulty, tpl.Item2));

        while(length > 0)
        {
            int randomNumber = Random.Range(0, totalWeight);

            Generator selectedGenerator = null;
            foreach (System.Tuple<Generator, Difficulty> generator in generators)
            {
                int generatorWeight = GetDifficultyWeight(levelDifficulty, generator.Item2);
                if (randomNumber <= generatorWeight)
                {
                    selectedGenerator = generator.Item1;
                    break;
                }
                randomNumber -= generatorWeight;
            }
            selectedGenerator(ref start, ref length, plfs);
            GenerateSpawner(start, levelDifficulty);
        }
        EndGround(ref start, ref length, plfs);
        return plfs;
    }

    int GetDifficultyWeight(Difficulty levelDifficulty, Difficulty generatorDifficulty)
    {
        return Mathf.Clamp(3 - Mathf.Abs(levelDifficulty - generatorDifficulty), 0, 3);
    }

    // Not used
    List<Platform> MakeFlatPath(Vector2 start, float length)
    {
        List<Platform> plfs = new List<Platform>();
        StagingGround(ref start, ref length, plfs);
        WidePit(ref start, ref length, plfs);
        while (length > 0)
        {
            switch (Random.Range(6, 7))
            {
            case 0:     // flat land with a platform that sticks out (up or down)
                FlatShort(ref start, ref length, plfs);
                break;
            case 1:     // a small pit you have to jump over, maybe with a hight difference
                SmallPit(ref start, ref length, plfs);
                break;
            case 2:     // a wide pit you have to jump over
                WidePit(ref start, ref length, plfs);
                break;
            case 3:     // a wall you have to scale
                ClimableWall(ref start, ref length, plfs);
                break;
            case 4:
                NoReturnDrop(ref start, ref length, plfs);
                break;
            case 5:
                StairsRight(ref start, ref length, plfs);
                break;
            case 6:
                StairsSteep(ref start, ref length, plfs);
                break;
            }
            GenerateSpawner(start);
        }
        EndGround(ref start, ref length, plfs);


        return plfs;
    }

    private static void GenerateSpawner(Vector2 start, Difficulty difficulty = Difficulty.EASY)
    {
        float r = Random.value + (int)difficulty * .2f;
        if (r < 0.8f)
            new Spawner(start + new Vector2(0, 1), 3, Spawner.EnemyType.CHARGER);
        else if (r < 1.3f)
            new Spawner(start + new Vector2(0, 1), 3, Spawner.EnemyType.SHOOTER);
        else
            new Spawner(start + new Vector2(0, 1), 3, Spawner.EnemyType.SHIELD);
    }

    #region L1 Generators

    /// <summary>
    /// Long flat land with a wall on the front to start on
    /// </summary>
    private static void StagingGround(ref Vector2 start, ref float length, List<Platform> plfs)
    {
        // There are 3 platforms: Starting wall, safe ground, and the bit lower starting ground
        plfs.Add(new Platform(start + new Vector2(-5, 10), new Vector2(5, 13)));
        plfs.Add(new Platform(start, new Vector2(8, 4)));
        plfs.Add(new Platform(start + new Vector2(8, -3), new Vector2(5, 2)));
        plfs.Add(Platform.Ladder(start + new Vector2(8, 0), 3));
        plfs.Add(new Platform(start + new Vector2(8, 0), start + new Vector2(8, 4), new Vector2(3, 1)));
        //Debug.Log("Flat: " + l1 + l2 + l3 + up);
        start += new Vector2(13, -3);
        length -= (13);
    }

    /// <summary>
    /// Long flat land with a wall on the front to start on
    /// </summary>
    private void EndGround(ref Vector2 start, ref float length, List<Platform> plfs)
    {
        // There are 3 platforms: End platform, wall ad podium
        plfs.Add(new Platform(start, new Vector2(8, 2)));
        plfs.Add(new Platform(start + new Vector2(8, 10), new Vector2(5, 12)));
        plfs.Add(new Platform(start + new Vector2(3, 1), new Vector2(2, 1)));
        //Debug.Log("Flat: " + l1 + l2 + l3 + up);
        length -= (8);

        chest.transform.position = start + new Vector2(3.5f, 1);
    }

    /// <summary>
    /// flat land with a platform that sticks out (up or down)
    /// V Easy
    /// </summary>
    private static void FlatShort(ref Vector2 start, ref float length, List<Platform> plfs)
    {
        // There are 3 platforms, so determine the length of these platforms
        int l1 = Random.Range(4, 9);
        int l2 = Random.Range(4, 7);
        int l3 = Random.Range(4, 9);
        // Does the platform go up or down?
        bool up = Random.value > 0.5f;
        plfs.Add(new Platform(start, new Vector2(l1, up ? 2 : 3)));
        plfs.Add(new Platform(start + new Vector2(l1 - 1, up ? 1 : -1), new Vector2(l2 + 1, up ? 4 : 2)));  // TODO: Extra random for asteathic positioning
        plfs.Add(new Platform(start + new Vector2(l1 + l2, 0), new Vector2(l3, up ? 2 : 3)));
        if (up)
        {
            plfs.Add(new Platform(start + new Vector2(l1 - 2, 1), Platform.PlatformType.UP));
            plfs.Add(new Platform(start + new Vector2(l1 + l2 , 1), Platform.PlatformType.DOWNLONG));
        } else
        {
            plfs.Add(new Platform(start + new Vector2(l1, 0), Platform.PlatformType.DOWN));
            plfs.Add(new Platform(start + new Vector2(l1 + l2 - 1, 0), Platform.PlatformType.UP));
        }
        if(terrainDebugLogging) Debug.Log("Flat: " + l1 + l2 + l3 + up);
        start += new Vector2(l1 + l2 + l3, 0);
        length -= (l1 + l2 + l3);
    }

    /// <summary>
    /// a small pit you have to jump over, maybe with a hight difference
    /// Easy
    /// </summary>
    private static void SmallPit(ref Vector2 start, ref float length, List<Platform> plfs)
    {
        // There are two platforms: at the bottom and on the other side. The starting platform is reused from the last template
        // length of the pit and length of the landing
        int l1 = Random.Range(1, 3);
        int l2 = Random.Range(3, 8);
        // up, flat or down?
        int h = Random.Range(-1, 2);
        plfs.Add(new Platform(start + new Vector2(0, Mathf.Min(-1, h - 1)), new Vector2(l1, 1)));
        plfs.Add(new Platform(start + new Vector2(l1, h), new Vector2(l2, 3)));
        if (terrainDebugLogging) Debug.Log("Small jump: " + l1 + l2 + h);
        start += new Vector2(l1 + l2, h);
        length -= (l1 + l2);
    }

    /// <summary>
    /// a wide pit you have to jump over
    /// Easy
    /// </summary>
    private static void WidePit(ref Vector2 start, ref float length, List<Platform> plfs)
    {
        // There are 3 platforms: one at the bottom, the landing and the recup. The recup is a ladder. The hole is 2 deep.
        // length of the pit and length of the landing
        int l1 = Random.Range(3, 5);
        int l2 = Random.Range(3, 8);
        plfs.Add(new Platform(start + new Vector2(0, -2), new Vector2(l1, 2)));
        plfs.Add(Platform.Ladder(start + new Vector2(0, 0), 2));
        plfs.Add(new Platform(start + new Vector2(l1, 0), new Vector2(l2, 3)));
        if (terrainDebugLogging) Debug.Log("Big jump: " + l1 + l2);
        start += new Vector2(l1 + l2, 0);
        length -= (l1 + l2);
    }

    /// <summary>
    /// a wall you have to scale
    /// Moderate
    /// </summary>
    private static void ClimableWall(ref Vector2 start, ref float length, List<Platform> plfs)
    {
        // 3 platforms: startpit, between (above start) and landing
        // 1 ladder to the right of the between
        // Length of the startpit, between and landing
        int l1 = Random.Range(5, 9);
        int l2 = Random.Range(2, l1 - 3);
        int l3 = Random.Range(3, 5);
        // startpit goes down
        bool down = Random.value > 0.5f;
        // Height of the wall and the between
        int h1 = Random.Range(down ? 3 : 4, 6);
        // int h2 = Random.Range(down ? 2 : 3, h1);
        int h2 = h1 - 1;
        plfs.Add(new Platform(start + new Vector2(0, down ? -1 : 0), new Vector2(l1, 2)));
        plfs.Add(new Platform(start + new Vector2(l1 - l2 - 2, h2 + (down ? -1 : 0)), new Vector2(l2, 1)));
        plfs.Add(new Platform(start + new Vector2(l1, h1 + (down ? -1 : 0)), new Vector2(l3, h1 + 2)));
        plfs.Add(Platform.Ladder(start + new Vector2(l1 - 2, h2 + (down ? -1 : 0)), h2));
        if (terrainDebugLogging) Debug.Log("Climb: " + l1 + l2 + l3 + down + h1 + h2);
        start += new Vector2(l1 + l3, h1 + (down ? -1 : 0));
        length -= (l1 + l3);
    }

    /// <summary>
    /// A drop that is hard to scale back up
    /// Moderate
    /// </summary>
    private static void NoReturnDrop(ref Vector2 start, ref float length, List<Platform> plfs)
    {
        // 3 platforms: The high one from wich to drop, the landing and the continuation, which may or not be the same hight.
        // Length of the drop, landing and continuation
        int l1 = Random.Range(2, 6);
        int l2 = Random.Range(2, 6);
        int l3 = Random.Range(4, 9);
        // The hight of the drop and of the step of the continuation
        int h1 = Random.Range(4, 10);
        int h2 = Random.Range(-1, 2);
        plfs.Add(new Platform(start, new Vector2(l1, h1 + 1)));
        plfs.Add(new Platform(start + new Vector2(l1, -h1), new Vector2(l2, 2)));
        plfs.Add(Platform.Ladder(start + new Vector2(l1, 0), h1));
        plfs.Add(new Platform(start + new Vector2(l1 + l2, -h1 + h2), new Vector2(l3, h2 == -1 ? 1 : 2)));
        if (terrainDebugLogging) Debug.Log("No return drop: " + l1 + l2 + l2 + h1 + h2);
        start += new Vector2(l1 + l2 + l3, -h1 + h2);
        length -= (l1 + l2 + l3);
    }

    /// <summary>
    /// A couple of stairs to climb to the right
    /// Hard
    /// </summary>
    private static void StairsRight(ref Vector2 start, ref float length, List<Platform> plfs)
    {
        // There are 3 fixed platforms, the starting pillar, the catcher and the back pillar. Other then that, there are between 2 and 5 steps
        // A step can be 1 or 2 heigher then the previous one. If two, it always begins with a slope, which may be long if there is space (>4).
        // If one, it can also have no slope.
        // There is a chance that there is a ladder at the end
        int steps = Random.Range(2, 6);
        bool ladder = Random.value > 0.5f;
        // The starting piller
        plfs.Add(new Platform(start, new Vector2(2, 3)));
        int totalLength = 0;
        int totalHeight = 0;
        for (int i = 0; i < steps; i++)
        {
            int gap = Random.Range(1, 4);
            int l = Random.Range(2, 6);
            int heightDiff = Random.Range(1, 3);
            int slopeLength = Random.Range(heightDiff == 2 ? 1 : 0, l > 3 ? 3 : 2);
            Vector2 stepPos = new Vector2(2 + totalLength + gap, totalHeight + heightDiff);
            plfs.Add(new Platform(start + stepPos + new Vector2(slopeLength, 0), new Vector2(l - slopeLength, 1)));
            switch (slopeLength)
            {
            case 0: break;
            case 1: plfs.Add(new Platform(start + stepPos, Platform.PlatformType.UP)); break;
            case 2: plfs.Add(new Platform(start + stepPos, Platform.PlatformType.UPLONG)); break;
            }
            totalLength += (gap + l);
            totalHeight += heightDiff;
        }
        plfs.Add(new Platform(start + new Vector2(2, -1), new Vector2(totalLength + 2, 2)));    // Catcher
        plfs.Add(new Platform(start + new Vector2(2 + totalLength + 2, totalHeight + 1), new Vector2(2, totalHeight + 4)));   // Back pillar
        new Spawner(start + new Vector2(2 + totalLength + 2 + .5f, totalHeight + 1), 1, Spawner.EnemyType.GUARD);
        if (ladder) plfs.Add(Platform.Ladder(start + new Vector2(2 + totalLength + 1, totalHeight + 1), totalHeight + 2));
        if (terrainDebugLogging) Debug.Log("Stairs Right: " + steps + ", " + totalLength);
        start += new Vector2(totalLength + 6, totalHeight + 1);
        length -= (totalLength + 6);
    }

    /// <summary>
    /// A couple of stairs to climb mostly up
    /// Very Hard
    /// </summary>
    private static void StairsSteep(ref Vector2 start, ref float length, List<Platform> plfs)
    {
        // There are 3 fixed platforms, the starting pillar, the catcher and the back pillar. Other then that, there are between 2 and 3 switchbacks
        int switchbacks = Random.Range(2, 4);
        // The starting piller
        plfs.Add(new Platform(start, new Vector2(2, 3)));
        int mostRightPoint = 0;
        int currentx = 2;
        string debug = "";
        for (int i = 0; i < switchbacks; i++)
        {
            // Each switchback is made of 4 platforms
            //  4-----
            //          3-----
            //                  2------
            //          1-----
            // Nr1 has a up at the left, nr3 has a down on the right
            int gap1 = Random.Range(3, 5);
            int l1 = Random.Range(3, 5);
            int slope1 = Random.Range(1, l1 > 3 ? 3 : 2);
            int gap2 = Random.Range(2, 4);
            int l2 = Random.Range(2, 4);
            int gap3 = Random.Range(gap2, 5);
            int l3 = Random.Range(2, 5);
            int slope3 = Random.Range(1, l3 > 3 ? 3 : 2);
            int gap4 = Random.Range(2, 4);
            int l4 = Random.Range(1, 3);
            plfs.Add(new Platform(start + new Vector2(currentx + gap1 + slope1, i * 6 + 2), new Vector2(l1 - slope1, 1)));
            plfs.Add(new Platform(start + new Vector2(currentx + gap1, i * 6 + 2), slope1 == 1 ? Platform.PlatformType.UP : Platform.PlatformType.UPLONG));
            plfs.Add(new Platform(start + new Vector2(currentx + gap1 + l1 + gap2, i * 6 + 3), new Vector2(l2, 1)));
            plfs.Add(new Platform(start + new Vector2(currentx + gap1 + l1 + gap2 - gap3 - l3, i * 6 + 5), new Vector2(l3 - slope3, 1)));
            plfs.Add(new Platform(start + new Vector2(currentx + gap1 + l1 + gap2 - gap3 - slope3, i * 6 + 5), slope3 == 1 ? Platform.PlatformType.DOWN : Platform.PlatformType.DOWNLONG));
            plfs.Add(new Platform(start + new Vector2(currentx + gap1 + l1 + gap2 - gap3 - l3 - gap4 - l4, i * 6 + 6), new Vector2(l4, 1)));
            new Spawner(start + new Vector2(currentx + gap1 + l1 + gap2 - gap3 - l3 - gap4 - l4 + .5f, i * 6 + 6), 1, Spawner.EnemyType.GUARD);
            mostRightPoint = Mathf.Max(mostRightPoint, currentx + gap1 + l1 + gap2 + l2);
            currentx = currentx + gap1 + l1 + gap2 - gap3 - l3 - gap4;
            debug += "(" + gap1 + l1 + gap2 + l2 + gap3 + l3 + gap4 + l4 + ")";
        }
        // Finishing up the last two platforms of the half-switchback
        {
            int gap1 = Random.Range(2, 5);
            int l1 = Random.Range(2, 5);
            int slope1 = Random.Range(1, l1 > 3 ? 3 : 2);
            int gap2 = Random.Range(1, 4);
            int l2 = Random.Range(2, 4);
            plfs.Add(new Platform(start + new Vector2(currentx + gap1 + slope1, switchbacks * 6 + 2), new Vector2(l1, 1)));
            plfs.Add(new Platform(start + new Vector2(currentx + gap1, switchbacks * 6 + 2), slope1 == 1 ? Platform.PlatformType.UP : Platform.PlatformType.UPLONG));
            plfs.Add(new Platform(start + new Vector2(currentx + gap1 + l1 + gap2, switchbacks * 6 + 3), new Vector2(l2, 1)));
            mostRightPoint = Mathf.Max(mostRightPoint, currentx + gap1 + l1 + gap2 + l2);
            debug += "(" + gap1 + l1 + gap2 + l2 + ")";
        }
        // Finally the base and the end piller
        plfs.Add(new Platform(start + new Vector2(2, -1), new Vector2(mostRightPoint - 2, 2)));
        plfs.Add(new Platform(start + new Vector2(mostRightPoint, switchbacks * 6 + 4), new Vector2(2, switchbacks * 6 + 6)));
        if (terrainDebugLogging) Debug.Log("Stars Steep: " + debug);
        start += new Vector2(mostRightPoint + 2, switchbacks * 6 + 3);
        length -= (mostRightPoint + 2);
    }

    #endregion

    class Platform
    {
        /// <summary>
        /// Top left of the platform
        /// </summary>
        public Vector2 position;
        public Vector2 position2;   // Used for moving platforms
        public Vector2 size;
        public enum PlatformType { BOX, UP, UPLONG, DOWN, DOWNLONG, LADDER, MOVING};
        public PlatformType platformType;
        static ObjectPool[] platformPools = new ObjectPool[7];

        public Platform(Vector2 position, Vector2 size)
        {
            this.position = position;
            this.size = size;
            platformType = PlatformType.BOX;
        }

        public Platform(float posx, float posy, float sizex, float sizey) :
            this(new Vector2(posx, posy), new Vector2(sizex, sizey))
        {
        }

        public Platform(Vector2 position, PlatformType platformType)
        {
            this.position = position;
            this.platformType = platformType;
            switch (platformType)
            {
            case PlatformType.BOX:
                throw new System.Exception("Do not use box with this constructor");
            case PlatformType.UP:
            case PlatformType.DOWN:
                size = new Vector2(1, 1); break;
            case PlatformType.UPLONG:
            case PlatformType.DOWNLONG:
                size = new Vector2(2, 1); break;
            case PlatformType.LADDER:
                throw new System.Exception("Do not use ladder with this constructor");
            }
        }

        public Platform(Vector2 position1, Vector2 position2, Vector2 size)
        {
            this.position = position1;
            this.position2 = position2;
            this.size = size;
            platformType = PlatformType.MOVING;
        }

        Platform() { }

        public static Platform Ladder(Vector2 position, float height)
        {
            Platform platform = new Platform {
                position = position,
                size = new Vector2(1, height),
                platformType = PlatformType.LADDER
            };
            return platform;
        }

        public void Make()
        {
            GameObject go = platformPools[(int)platformType].GetNextObject();
            if (platformType == PlatformType.BOX || platformType == PlatformType.LADDER || platformType == PlatformType.MOVING)
            {
                SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
                sr.size = size;
                BoxCollider2D bc = go.GetComponent<BoxCollider2D>();
                bc.size = size;
                bc.offset = new Vector2(size.x / 2, -size.y / 2);
                if(platformType == PlatformType.MOVING)
                {
                    MovingPlatform mp = go.GetComponent<MovingPlatform>();
                    mp.position1 = position;
                    mp.position2 = position2;
                }
            }
            go.transform.position = position;
        }

        static public void AssignPools(ObjectPool boxPool, ObjectPool upPool, ObjectPool upLongPool, 
            ObjectPool downPool, ObjectPool downLongPool, ObjectPool ladderPool, ObjectPool movingPool)
        {
            platformPools[0] = boxPool; platformPools[1] = upPool; platformPools[2] = upLongPool;
            platformPools[3] = downPool; platformPools[4] = downLongPool; platformPools[5] = ladderPool;
            platformPools[6] = movingPool;
        }

        internal static void DeactivateAllObjects()
        {
            foreach (ObjectPool pool in platformPools)
                pool.DeactivateAllObjects();
        }
    }

    public class Spawner
    {
        public Vector2 position;
        public int amount;
        public enum EnemyType { CHARGER, SHOOTER, SHIELD, GUARD}
        public EnemyType type;
        public int difficulty;

        static ObjectPool enemyPool;
        static List<SpawnerGO> spawners = new List<SpawnerGO>();

        public Spawner(Vector2 position, int amount, EnemyType type, int difficulty = 1)
        {
            if (enemyPool == null)
                enemyPool = GameObject.Find("EnemyPool").GetComponent<ObjectPool>();
            this.position = position; this.amount = amount; this.type = type; this.difficulty = difficulty;
            var spawnergo = new GameObject("Spawner " + position.x).AddComponent<SpawnerGO>();
            spawnergo.parent = this;
            spawners.Add(spawnergo);
        }

        public static void RemoveAllSpawners()
        {
            spawners.ForEach(go => Destroy(go.gameObject));
            spawners = new List<SpawnerGO>();
        }


        class SpawnerGO : MonoBehaviour
        {
            const float DISTANCE_UNTILL_ACTIVATION = 23;
            const float TIME_TO_SPAWN_ALL = 6;
            const float DISTANCE_OF_DEACTIVATION = 10;

            public Spawner parent;
            Transform player;
            bool isactive = false;
            float timeUntillNext;
            float timeBetweenTwo;


            private void Awake()
            {
                player = GameObject.Find("Wizard").transform;
            }

            private void Update()
            {
                if (isactive == false)
                {
                    if (Mathf.Abs(player.position.x - parent.position.x) < DISTANCE_UNTILL_ACTIVATION)
                    {
                        isactive = true;
                        timeBetweenTwo = TIME_TO_SPAWN_ALL / parent.amount;
                    }
                } else
                {
                    timeUntillNext -= Time.deltaTime;
                    if(timeUntillNext < 0)
                    {
                        timeUntillNext = timeBetweenTwo;
                        if (((Vector2)player.position - parent.position).magnitude < DISTANCE_OF_DEACTIVATION)
                        {
                            parent.amount--;
                            if (parent.amount <= 0)
                            {
                                spawners.Remove(this);
                                Destroy(gameObject);
                            }
                            return;
                        }
                        GameObject enemy = enemyPool.GetNextObject();
                        enemy.transform.position = parent.position;
                        switch (parent.type)
                        {
                        case EnemyType.CHARGER:
                            break;
                        case EnemyType.SHOOTER:
                            enemy.GetComponent<Enemy>().projectiles = 3;
                            break;
                        case EnemyType.SHIELD:
                            enemy.GetComponent<Enemy>().shieldMagic = 10;
                            enemy.GetComponent<Enemy>().shieldMagicLeft = 10;
                            break;
                        case EnemyType.GUARD:
                            enemy.GetComponent<Enemy>().projectiles = 12;
                            enemy.GetComponent<Enemy>().SetAsGuard();
                            break;
                        }
                        parent.amount--;
                        if (parent.amount <= 0)
                        {
                            spawners.Remove(this);
                            Destroy(gameObject);
                        }
                    }
                }
            }
        }
    }

}
