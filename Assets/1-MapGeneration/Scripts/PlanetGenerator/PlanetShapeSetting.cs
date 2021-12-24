using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Geometry;
using Geometry.DataStructure;

[System.Serializable]
public class PlanetShapeSetting
{
    public float planetRadius = 1.0f;
    public int planetRefiningStep = 3;

    public MeshData meshData = null;
    public DualMeshData dualMeshData = null;

    public void GenerateShape()
	{
        meshData = MeshGenerator.CreateIcoSphere(planetRadius, planetRefiningStep);
        DataStructureBuilder.CreateDualMeshData(meshData, out dualMeshData);
	}
}
