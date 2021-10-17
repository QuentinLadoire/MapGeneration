using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoissonDiskSamplingTest : MonoBehaviour
{
	[SerializeField] private float displayRadius = 0.1f;

	PoissonDiskSampling poissonDiskSampling = new PoissonDiskSampling();
	Point2D[] points = null;

	private void Start()
	{
		var startTime = Time.realtimeSinceStartup;
		poissonDiskSampling.GeneratePoints();
		var endTime = Time.realtimeSinceStartup;

		Debug.Log("Process time : " + (endTime - startTime));

		points = poissonDiskSampling.Points;
		
		//Debug.Log(points.Length);
		//foreach (var point in points)
		//{
		//	Debug.Log("x : " + point.x + " - y : " + point.y);
		//}
	}

	private void OnDrawGizmos()
	{
		if (points == null)
			return;

		Gizmos.color = Color.blue;
		foreach (var point in points)
		{
			var position = new Vector3(point.x, point.y);
			Gizmos.DrawSphere(position, displayRadius);
		}
	}
}
