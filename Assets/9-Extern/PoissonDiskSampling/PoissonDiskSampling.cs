using System;
using System.Collections.Generic;
using Miscellaneous;

namespace PoissonDisk
{
    public enum StartPointPickMode
	{
        Random,
        Center,
        Custom
	}

    [Serializable]
    public struct PoissonDiskSetting
	{
        public int seed;

        public float radius;
        public float areaWidth;
        public float areaHeight;

        public int sampleLimitBeforeRejection;

        public bool createHull;

        public StartPointPickMode startPointPickMode;
        public Point2D customStartPoint;

		public PoissonDiskSetting(int seed = 0, int sampleLimitBeforeRejection = 30, float radius = 1.0f, float areaWidth = 30.0f, float areaHeight = 30.0f, bool createHull = false, StartPointPickMode startPointPickMode = StartPointPickMode.Center, Point2D customStartPoint = new Point2D())
		{
			this.seed = seed;
			this.sampleLimitBeforeRejection = sampleLimitBeforeRejection;
			this.radius = radius;
			this.areaWidth = areaWidth;
			this.areaHeight = areaHeight;
			this.createHull = createHull;
			this.customStartPoint = customStartPoint;
			this.startPointPickMode = startPointPickMode;
		}

        public static PoissonDiskSetting Default = new PoissonDiskSetting
        {
            seed = 0,
            sampleLimitBeforeRejection = 30,
            radius = 1.0f,
            areaWidth = 30.0f,
            areaHeight = 30.0f,
            createHull = false,
            startPointPickMode = StartPointPickMode.Center,
            customStartPoint = new Point2D(0.0f, 0.0f)
        };
	}

    public static class PoissonDiskUtility
	{
        public const float Pi2 = 6.28318530718f;
        public const float Sqrt2 = 1.41421356237f;

        public static int CeilToInt(float value)
        {
            return (int)Math.Ceiling(value);
        }
        public static int FloorToInt(float value)
        {
            return (int)Math.Floor(value);
        }

        public static float Lerp(float start, float end, float t)
        {
            return start + ((end - start) * t);
        }
        public static float SqrtDistance(Point2D p1, Point2D p2)
        {
            var x = p2.x - p1.x;
            var y = p2.y - p1.y;

            return x * x + y * y;
        }
    }

    public class PoissonDiskSampling
    {
        private int gridWidth = 0;
        private int gridHeight = 0;
        private float cellSize = 0.0f;

        private Random random = new Random(0);
        private PoissonDiskSetting setting = PoissonDiskSetting.Default;

        private int[,] grid = null;
        private Queue<int> processingPointQueue = new Queue<int>();
        private List<Point2D> outputPointList = new List<Point2D>();

        private bool IsValidPointPosition(Point2D point)
        {
            //Check if point is in area
            if (point.x < 0.0f || point.x > setting.areaWidth ||
                point.y < 0.0f || point.y > setting.areaHeight)
                return false;

            //Get only nearby cells
            var gridX = PoissonDiskUtility.FloorToInt(point.x / cellSize);
            var gridY = PoissonDiskUtility.FloorToInt(point.y / cellSize);
            var startX = Math.Max(0, gridX - 2);
            var startY = Math.Max(0, gridY - 2);
            var endX = Math.Min(gridWidth - 1, gridX + 2);
            var endY = Math.Min(gridHeight - 1, gridY + 2);

            //Check if point is within distance r of existing samples
            for (int j = startY; j <= endY; j++)
                for (int i = startX; i <= endX; i++)
                {
                    var pointIndex = grid[i, j];
                    if (pointIndex != -1)
                    {
                        var pointToTest = outputPointList[pointIndex];

                        var sqrtDistance = PoissonDiskUtility.SqrtDistance(pointToTest, point);
                        if (sqrtDistance < setting.radius * setting.radius)
                            return false;
                    }
                }

            return true;
        }

        private void AddNewPoint(Point2D point, bool process = true)
		{
            //Add the start point to the output list, processing list and grid
            outputPointList.Add(point);
            if (process)
                processingPointQueue.Enqueue(outputPointList.Count - 1);

            var gridX = PoissonDiskUtility.FloorToInt(point.x / cellSize);
            var gridY = PoissonDiskUtility.FloorToInt(point.y / cellSize);
            grid[gridX, gridY] = outputPointList.Count - 1;
		}
        private void MakeHull()
		{
            //Determine the hull division
            var countX = (int)(setting.areaWidth / setting.radius) + 1;
            var countY = (int)(setting.areaHeight / setting.radius) + 1;

            var ratioX = setting.areaWidth / (countX - 1);
            var ratioY = setting.areaHeight / (countY - 1);

            for (int i = 0; i < countX; i++)
			{
                if (i == 0 || i == countX - 1) //Create vertical hull edge
                {
                    for (int j = 0; j < countY; j++)
					{
                        AddNewPoint(new Point2D
                        {
                            x = ratioX * i,
                            y = ratioY * j
                        }, false);
					}
                }
                else //Create horizontal hull edge
                {
                    AddNewPoint(new Point2D
                    {
                        x = ratioX * i,
                        y = 0.0f
                    }, false);
                    AddNewPoint(new Point2D
                    {
                        x = ratioX * i,
                        y = setting.areaHeight
                    }, false);
                }
            }
		}
        private Point2D PickStartingPoint()
		{
            return setting.startPointPickMode switch
			{
                StartPointPickMode.Random => new Point2D
                {
                    x = (float)(setting.areaWidth * random.NextDouble()),
                    y = (float)(setting.areaHeight * random.NextDouble())
                },
                StartPointPickMode.Center => new Point2D
                {
                    x = setting.areaWidth * 0.5f,
                    y = setting.areaHeight * 0.5f
                },
                StartPointPickMode.Custom => setting.customStartPoint,
                _ => new Point2D() //Never happen
                };
		}
        private Point2D PickRandomPoint(Point2D point)
		{
            //Generate random angle and distance
            var angle = PoissonDiskUtility.Pi2 * (float)random.NextDouble();
            var distance = PoissonDiskUtility.Lerp(setting.radius, 2 * setting.radius, (float)random.NextDouble());

            //Create new Point
            return new Point2D
            {
                x = point.x + distance * (float)Math.Cos(angle),
                y = point.y + distance * (float)Math.Sin(angle)
            };
        }

        private void Initialize()
        {
            random = new Random(setting.seed);

            //Determine Grid Size
            cellSize = setting.radius / PoissonDiskUtility.Sqrt2;
            gridWidth = PoissonDiskUtility.CeilToInt(setting.areaWidth / cellSize);
            gridHeight = PoissonDiskUtility.CeilToInt(setting.areaHeight / cellSize);

            //Initialize Grid and List
            grid = new int[gridWidth, gridHeight];
            grid.Fill(-1);

            outputPointList.Clear();
            processingPointQueue.Clear();
        }
        private void Process()
		{
            //Process
            while (processingPointQueue.Count > 0)
            {
                //Get the processed point
                var index = processingPointQueue.Dequeue();
                var point = outputPointList[index];

                //Generate `sampleLimitBeforeRejection` Points
                for (int i = 0; i < setting.sampleLimitBeforeRejection; i++)
                {
                    //Pick a random point around the processed point
                    var newPoint = PickRandomPoint(point);

                    //Check if the new point is within distance r of existing points
                    if (IsValidPointPosition(newPoint))
                    {
                        //Add the new point to the process
                        AddNewPoint(newPoint);
                    }
                }
            }
        }
        private void Reset()
		{
            grid = null;
            outputPointList.Clear();
            processingPointQueue.Clear();
        }

        public void ComputePoints(PoissonDiskSetting setting, out Point2D[] outputPoints)
        {
            this.setting = setting;

            Initialize();

            //Pick and Add the starting point of the process 
            var startingPoint = PickStartingPoint();
            AddNewPoint(startingPoint);

            if (setting.createHull)
                MakeHull();

            Process();

            //Set the out points array
            outputPoints = outputPointList.ToArray();

            Reset();
        }
    }
}
