using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorExtensions
{
	public static Vector3[] ToVector3Array(this List<Vector2> list)
	{
		if (list == null)
			return null;

		var array = new Vector3[list.Count];
		for (int i = 0; i < list.Count; i++)
			array[i] = list[i];

		return array;
	}
    public static List<Vector3> ToVector3List(this List<Vector2> list)
	{
		if (list == null)
			return null;

		var vector3List = new List<Vector3>(list.Count);
		for (int i = 0; i < list.Count; i++)
			vector3List[i] = list[i];

		return vector3List;
	}
}
