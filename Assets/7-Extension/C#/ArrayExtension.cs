using System;
using System.Collections;
using System.Collections.Generic;

public static class ArrayExtension
{
    public static void Fill<T>(this T[] array, T value)
	{
		for (int i = 0; i < array.Length; i++)
			array[i] = value;
	}
	public static void Fill<T>(this T[,] array, T value)
	{
		for (int j = 0; j < array.GetLength(1); j++)
			for (int i = 0; i < array.GetLength(0); i++)
				array[i, j] = value;
	}
}
