using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorExtensions
{
    public static List<Vector3> ToVector3List(this List<Vector2> list)
	{
		if (list == null)
			return null;

		var vector3List = new List<Vector3>();
		foreach (var vector in list)
			vector3List.Add(vector);

		return vector3List;
	}
}
