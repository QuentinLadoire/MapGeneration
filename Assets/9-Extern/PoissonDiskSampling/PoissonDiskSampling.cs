using System;
using System.Collections.Generic;

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

public class PoissonDiskSampling
{
    private const float Sqrt2 = 1.41421356237f;

    private Random random = new Random(0);

    private int sampleLimitBeforeRejection = 30;

    private float radius = 1.0f;
    private float areaWidth = 30.0f;
    private float areaHeight = 30.0f;

    private float cellSize = 0.0f;
    private int gridWidth = 0;
    private int gridHeight = 0;

    private int[,] grid = null;
    private List<int> processingPointList = new List<int>();
    private List<Point2D> outputPointList = new List<Point2D>();

    public Point2D[] Points => outputPointList.ToArray();

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

    private bool IsValidPointPosition(Point2D point)
	{
        if (point.x < 0.0f || point.x > areaWidth ||
            point.y < 0.0f || point.y > areaHeight)
            return false;

        var gridX = FloorToInt(point.x / cellSize);
        var gridY = FloorToInt(point.y / cellSize);
        var startX = Math.Max(0, gridX - 2);
        var startY = Math.Max(0, gridY - 2);
        var endX = Math.Min(gridWidth - 1, gridX + 2);
        var endY = Math.Min(gridHeight - 1, gridY + 2);

        for (int j = startY; j <= endY; j++)
            for (int i = startX; i <= endX; i++)
            {
                var pointIndex = grid[i, j];
                if (pointIndex != -1)
                {
                    var pointToTest = outputPointList[pointIndex];

                    var sqrtDistance = SqrtDistance(pointToTest, point);
                    if (sqrtDistance < radius * radius)
                        return false;
                }
            }

        return true;
	}

    public void GeneratePoints()
	{
        //Determine Grid Size
        cellSize = radius / Sqrt2;
        gridWidth = CeilToInt(areaWidth / cellSize);
        gridHeight = CeilToInt(areaHeight / cellSize);

        //Initialize Grid and List
        grid = new int[gridWidth, gridHeight];
        grid.Fill(-1);

        outputPointList.Clear();
        processingPointList.Clear();

        //Pick a Random Point
        var startPoint = new Point2D
        {
            x = (float)(areaWidth * random.NextDouble()),
            y = (float)(areaHeight * random.NextDouble())
        };

        //Add the start point to the output list, processing list and grid
        outputPointList.Add(startPoint);
        processingPointList.Add(0);
        grid[FloorToInt(startPoint.x / cellSize), FloorToInt(startPoint.y / cellSize)] = 0;

        //Process
        while (processingPointList.Count > 0)
		{
            //Choose a random index from the processing list
            var randomProcessingListIndex = random.Next(processingPointList.Count);
            var outputPointIndex = processingPointList[randomProcessingListIndex];
            var point = outputPointList[outputPointIndex];

            //Generate sampleLimitBeforeRejection Points
            var noValidPoint = true;
            for (int i = 0; i < sampleLimitBeforeRejection; i++)
			{
                //Generate random angle and distance
                var angle = 2.0f * (float)Math.PI * (float)random.NextDouble();
                var distance = Lerp(radius, 2 * radius, (float)random.NextDouble());

                //Create new Point
                var newPoint = new Point2D
                {
                    x = point.x + distance * (float)Math.Cos(angle),
                    y = point.y + distance * (float)Math.Sin(angle)
                };

                //Check if it is within distance r of existing points
                if (IsValidPointPosition(newPoint))
				{
                    //Add the new point to the output list, processing list and grid
                    var gridX = FloorToInt(newPoint.x / cellSize);
                    var gridY = FloorToInt(newPoint.y / cellSize);

                    outputPointList.Add(newPoint);
                    grid[gridX, gridY] = outputPointList.Count - 1;
                    processingPointList.Add(outputPointList.Count - 1);

                    noValidPoint = false;
                }
			}

            //If no such point is found, remove point from the processing list
            if (noValidPoint)
                processingPointList.RemoveAt(randomProcessingListIndex);
		}
	}
}
