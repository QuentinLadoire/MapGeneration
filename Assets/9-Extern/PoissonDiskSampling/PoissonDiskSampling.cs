using System;
using System.Collections.Generic;

namespace PoissonDisk
{
    public struct Point2D
    {
        public float x;
        public float y;

        public Point2D(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public enum StartPointPickMode
	{
        Random,
        Center,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Custom
	}

    public class PoissonDiskSampling
    {
        private const float Pi2 = 6.28318530718f;
        private const float Sqrt2 = 1.41421356237f;

        private int gridWidth = 0;
        private int gridHeight = 0;
        private float cellSize = 0.0f;

        private int seed = 0;
        private Random random = new Random(0);

        private int[,] grid = null;
        private List<int> processingPointList = new List<int>();
        private List<Point2D> outputPointList = new List<Point2D>();

        public float Radius { get; set; } = 1.0f;
        public float AreaWidth { get; set; } = 30.0f;
        public float AreaHeight { get; set; } = 30.0f;
        public int SampleLimitBeforeRejection { get; set; } = 30;
        public int Seed { get => seed; set => SetSeed(value); }

        public Point2D CustomStartPoint { get; set; } = new Point2D(0.0f, 0.0f);
        public StartPointPickMode StartPointPickMode { get; set; } = StartPointPickMode.Random;

        private int CeilToInt(float value)
        {
            return (int)Math.Ceiling(value);
        }
        private int FloorToInt(float value)
        {
            return (int)Math.Floor(value);
        }
        private float Lerp(float start, float end, float t)
        {
            return start + ((end - start) * t);
        }
        private float SqrtDistance(Point2D p1, Point2D p2)
        {
            var x = p2.x - p1.x;
            var y = p2.y - p1.y;

            return x * x + y * y;
        }

        private void SetSeed(int seed)
        {
            this.seed = seed;
            random = new Random(seed);
        }
        private bool IsValidPointPosition(Point2D point)
        {
            //Check if point is in area
            if (point.x < 0.0f || point.x > AreaWidth ||
                point.y < 0.0f || point.y > AreaHeight)
                return false;

            //Get only nearby cells
            var gridX = FloorToInt(point.x / cellSize);
            var gridY = FloorToInt(point.y / cellSize);
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

                        var sqrtDistance = SqrtDistance(pointToTest, point);
                        if (sqrtDistance < Radius * Radius)
                            return false;
                    }
                }

            return true;
        }

        private void Initialize()
        {
            //Determine Grid Size
            cellSize = Radius / Sqrt2;
            gridWidth = CeilToInt(AreaWidth / cellSize);
            gridHeight = CeilToInt(AreaHeight / cellSize);

            //Initialize Grid and List
            grid = new int[gridWidth, gridHeight];
            grid.Fill(-1);

            outputPointList.Clear();
            processingPointList.Clear();
        }
        private Point2D PickStartPoint()
		{
            //Return a point according to the Pickmode
            return StartPointPickMode switch
			{
				StartPointPickMode.Random => new Point2D
                {
                    x = (float)(AreaWidth * random.NextDouble()),
                    y = (float)(AreaHeight * random.NextDouble())
                },
				StartPointPickMode.Center => new Point2D
                {
                    x = AreaWidth * 0.5f,
                    y = AreaHeight * 0.5f
                },
				StartPointPickMode.TopLeft => new Point2D
                {
                    x = 0.0f,
                    y = AreaHeight
                },
				StartPointPickMode.TopRight => new Point2D
                {
                    x = AreaWidth,
                    y = AreaHeight
                },
				StartPointPickMode.BottomLeft => new Point2D
                {
                    x = 0.0f,
                    y = 0.0f
                },
				StartPointPickMode.BottomRight => new Point2D
                {
                    x = AreaWidth,
                    y = 0.0f
                },
				StartPointPickMode.Custom => CustomStartPoint,
				_ => new Point2D()//never happen
			};
		}
        private void AddNewPoint(Point2D point)
		{
            //Add the start point to the output list, processing list and grid
            outputPointList.Add(point);
            processingPointList.Add(outputPointList.Count - 1);

            var gridX = FloorToInt(point.x / cellSize);
            var gridY = FloorToInt(point.y / cellSize);
            grid[gridX, gridY] = outputPointList.Count - 1;
		}
        private Point2D PickRandomPoint(Point2D point)
		{
            //Generate random angle and distance
            var angle = Pi2 * (float)random.NextDouble();
            var distance = Lerp(Radius, 2 * Radius, (float)random.NextDouble());

            //Create new Point
            return new Point2D
            {
                x = point.x + distance * (float)Math.Cos(angle),
                y = point.y + distance * (float)Math.Sin(angle)
            };
        }
        private void Reset()
		{
            random = new Random(Seed);

            grid = null;
            outputPointList.Clear();
            processingPointList.Clear();
        }

        public void ComputePoints(ref Point2D[] points)
        {
            Initialize();

            //Pick and Add the starting point of the process 
            var startPoint = PickStartPoint();
            AddNewPoint(startPoint);

            //Process
            while (processingPointList.Count > 0)
            {
                //Choose a random index from the processing list
                var randomProcessingListIndex = random.Next(processingPointList.Count);
                var outputPointIndex = processingPointList[randomProcessingListIndex];
                var point = outputPointList[outputPointIndex];

                //Generate sampleLimitBeforeRejection Points
                var noValidPoint = true;
                for (int i = 0; i < SampleLimitBeforeRejection; i++)
                {
                    //Pick a random point around the processed point
                    var newPoint = PickRandomPoint(point);

                    //Check if the new point is within distance r of existing points
                    if (IsValidPointPosition(newPoint))
                    {
                        //Add the new point to the process
                        AddNewPoint(newPoint);

                        noValidPoint = false;
                    }
                }

                //If no such point is found, remove point from the processing list
                if (noValidPoint)
                    processingPointList.RemoveAt(randomProcessingListIndex);
            }

            //Set the points array
            points = outputPointList.ToArray();

            Reset();
        }
    }
}
