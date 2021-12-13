using System;
using System.Collections.Generic;
using Miscellaneous;

namespace DelaunayVoronoi
{
	[Serializable]
	public struct TriangulationSetting
	{
		public float epsilon;
		public bool clockWise;

		public TriangulationSetting(float epsilon, bool clockWise)
		{
			this.epsilon = epsilon;
			this.clockWise = clockWise;
		}

		public static TriangulationSetting Default = new TriangulationSetting
		{
			epsilon = 0.00001f,
			clockWise = false
		};
	}

	public static class DelaunayUtility
    {
		public static float epsilon = 0.00001f;

		private const float oneThird = 1.0f / 3.0f;

		public static bool ToTheLeft(Point2D p, Point2D l0, Point2D l1)
		{
			var v0x = l1.x - l0.x;
			var v0y = l1.y - l0.y;

			var v1x = p.x - l0.x;
			var v1y = p.y - l0.y;

			var det = (v0x * v1y) - (v0y * v1x);

			return det >= -epsilon;
		}
		public static bool OnAnEdge(Point2D p, Point2D l0, Point2D l1)
		{
			var v0x = l1.x - l0.x;
			var v0y = l1.y - l0.y;

			var v1x = p.x - l0.x;
			var v1y = p.y - l0.y;

			var det = (v0x * v1y) - (v0y * v1x);

			return det < epsilon && det > -epsilon;
		}

		public static bool PointInTriangle(Point2D p, Point2D t0, Point2D t1, Point2D t2)
		{
			return ToTheLeft(p, t0, t1) &&
				   ToTheLeft(p, t1, t2) &&
				   ToTheLeft(p, t2, t0);
		}
		public static bool PointOnAnTriangleEdge(Point2D p, Point2D t0, Point2D t1, Point2D t2)
		{
			return OnAnEdge(p, t0, t1) ||
				   OnAnEdge(p, t1, t2) ||
				   OnAnEdge(p, t2, t0);
		}

		public static bool InsideCircumCircle(Point2D p, Point2D c0, Point2D c1, Point2D c2)
        {
            var ax = c0.x - p.x;
            var ay = c0.y - p.y;
            var bx = c1.x - p.x;
            var by = c1.y - p.y;
            var cx = c2.x - p.x;
            var cy = c2.y - p.y;

            var determinant = (ax * ax + ay * ay) * (bx * cy - cx * by) -
                              (bx * bx + by * by) * (ax * cy - cx * ay) +
                              (cx * cx + cy * cy) * (ax * by - bx * ay);

            return determinant > 0.0f;
        }

		public static Point2D CalculateBarycenter(Point2D p0, Point2D p1, Point2D p2)
		{
			var bary = new Point2D
			{
				x = (p0.x + p1.x + p2.x) * oneThird,
				y = (p0.y + p1.y + p2.y) * oneThird
			};

			return bary;
		}
		public static Point2D CalculateCircumcenter(Point2D p0, Point2D p1, Point2D p2)
		{
			var x0 = p1.x - p0.x;
			var y0 = p1.y - p0.y;
			var x1 = p2.x - p0.x;
			var y1 = p2.y - p0.y;

			var bl = x0 * x0 + y0 * y0;
			var cl = x1 * x1 + y1 * y1;
			var d = 0.5f / (x0 * y1 - y0 * x1);

			var x = p0.x + (y1 * bl - y0 * cl) * d;
			var y = p0.y + (x0 * cl - x1 * bl) * d;

			var circumcenter = new Point2D
			{
				x = x,
				y = y
			};

			return circumcenter;
		}
    }

    public class DelaunayCalculator
    {
		private struct TriangleNode
		{
			// The points of the triangle
			public int P0;
			public int P1;
			public int P2;

			// The child triangles of this triangle in the tree
			//
			// A value of -1 means "no child"
			public int C0;
			public int C1;
			public int C2;

			// The triangles adjacent to this triangle
			//
			// A0 is the adjacent triangle opposite to the P0 point (i.e. the A0
			// triangle has (P1, P2) as an edge.
			//
			// A value of -1 means "no adjacent triangle" (only true for
			// triangles with one edge on the bounding triangle).
			public int A0;
			public int A1;
			public int A2;

			/// <summary>
			/// Is this a leaf triangle?
			/// </summary>
			public bool IsLeaf => C0 < 0 && C1 < 0 && C2 < 0;

			/// <summary>
			/// Is this an "inner" triangle, part of the final triangulation, or
			/// is some part of this triangle connected to the bounding triangle.
			/// </summary>
			public bool IsInner => P0 >= 0 && P1 >= 0 && P2 >= 0;

			public TriangleNode(int P0, int P1, int P2)
			{
				this.P0 = P0;
				this.P1 = P1;
				this.P2 = P2;

				this.C0 = -1;
				this.C1 = -1;
				this.C2 = -1;

				this.A0 = -1;
				this.A1 = -1;
				this.A2 = -1;
			}

			/// <summary>
			/// Does this triangle contain this edge?
			/// </summary>
			public bool HasEdge(int e0, int e1)
			{
				if (e0 == P0) return e1 == P1 || e1 == P2;
				else if (e0 == P1) return e1 == P0 || e1 == P2;
				else if (e0 == P2) return e1 == P0 || e1 == P1;

				return false;
			}

			/// <summary>
			/// Assuming p0 and p1 are one of P0 and P1, return the third point.
			/// </summary>
			public int OtherPoint(int p0, int p1)
			{
				if (p0 == P0)
				{
					if (p1 == P1) return P2;
					if (p1 == P2) return P1;
					throw new ArgumentException("p0 and p1 not on triangle");
				}
				if (p0 == P1)
				{
					if (p1 == P0) return P2;
					if (p1 == P2) return P0;
					throw new ArgumentException("p0 and p1 not on triangle");
				}
				if (p0 == P2)
				{
					if (p1 == P0) return P1;
					if (p1 == P1) return P0;
					throw new ArgumentException("p0 and p1 not on triangle");
				}

				throw new ArgumentException("p0 and p1 not on triangle");
			}

			/// <summary>
			/// Get the triangle opposite a certain point.
			/// </summary>
			public int Opposite(int p)
			{
				if (p == P0) return A0;
				if (p == P1) return A1;
				if (p == P2) return A2;
				throw new ArgumentException("p not in triangle");
			}

			/// <summary>
			/// Get the triangle Adjacent a certain edge.
			/// </summary>
			public int Adjacent(int p0, int p1)
			{
				return Opposite(OtherPoint(p0, p1));
			}

			/// <summary>
			/// For debugging purposes.
			/// </summary>
			public override string ToString()
			{
				if (IsLeaf)
					return string.Format("TriangleNode({0}, {1}, {2})", P0, P1, P2);
				else
					return string.Format("TriangleNode({0}, {1}, {2}, {3}, {4}, {5})", P0, P1, P2, C0, C1, C2);
			}
		}

		private int highest = -1;
		private Point2D[] points = null;
		private List<TriangleNode> triangleNodeList = new List<TriangleNode>();

		private bool Higher(int pi0, int pi1)
		{
			if (pi0 == -2) return false;
			else if (pi0 == -1) return true;
			else if (pi1 == -2)	return true;
			else if (pi1 == -1)	return false;
			else
			{
				var p0 = points[pi0];
				var p1 = points[pi1];

				return p0.y < p1.y || (p0.y == p1.y && p0.x < p1.x);
			}
		}

		private bool ToTheLeft(int pi, int li0, int li1)
		{
			if      (li0 == -2) return Higher(li1, pi);
			else if (li0 == -1) return Higher(pi, li1);
			else if (li1 == -2) return Higher(pi, li0);
			else if (li1 == -1) return Higher(li0, pi);
			else 
				return DelaunayUtility.ToTheLeft(points[pi], points[li0], points[li1]);
		}
		private bool OnTheEdge(int pi, int li0, int li1)
		{
			if		(li0 == -2) return false;
			else if (li0 == -1) return false;
			else if (li1 == -2) return false;
			else if (li1 == -1) return false;
			else
				return DelaunayUtility.OnAnEdge(points[pi], points[li0], points[li1]);
		}

		private bool PointInTriangle(int pi, int ti)
		{
			var t = triangleNodeList[ti];
			return ToTheLeft(pi, t.P0, t.P1)
				&& ToTheLeft(pi, t.P1, t.P2)
				&& ToTheLeft(pi, t.P2, t.P0);
		}
		private bool PointIsOnTriangleEdges(int pi, int ti, out int ei0, out int ei1)
		{
			var t = triangleNodeList[ti];

			ei0 = -1;
			ei1 = -1;

			if (OnTheEdge(pi, t.P0, t.P1))
			{
				ei0 = t.P0;
				ei1 = t.P1;
			}
			else if (OnTheEdge(pi, t.P1, t.P2))
			{
				ei0 = t.P1;
				ei1 = t.P2;
			}
			else if (OnTheEdge(pi, t.P2, t.P0))
			{
				ei0 = t.P2;
				ei1 = t.P0;
			}

			return ei0 != -1;
		}

		/// <summary>
		/// Find the leaf triangle in the triangle tree containing a certain point.
		/// </summary>
		private int FindTriangleNode(int pi)
		{
			var curr = 0;
			while (!triangleNodeList[curr].IsLeaf)
			{
				var t = triangleNodeList[curr];
				if (t.C0 >= 0 && PointInTriangle(pi, t.C0))
					curr = t.C0;
				else if (t.C1 >= 0 && PointInTriangle(pi, t.C1))
					curr = t.C1;
				else
					curr = t.C2;
			}

			return curr;
		}

		/// <summary>
		/// Find the leaf of the triangles[ti] subtree that contains a given
		/// edge.
		///
		/// We need this because when we split or flip triangles, the adjacency
		/// references don't update, so even if the adjacency triangles were
		/// leaves when the node was created, they might not be leaves later.
		/// If they aren't, they're going to be the ancestor of the correct
		/// leaf, so this method goes down the tree finding the right leaf.
		/// </summary>
		int LeafWithEdge(int ti, int e0, int e1)
		{
			while (!triangleNodeList[ti].IsLeaf)
			{
				var t = triangleNodeList[ti];
				if (t.C0 != -1 && triangleNodeList[t.C0].HasEdge(e0, e1))
					ti = t.C0;
				else if (t.C1 != -1 && triangleNodeList[t.C1].HasEdge(e0, e1))
					ti = t.C1;
				else if (t.C2 != -1 && triangleNodeList[t.C2].HasEdge(e0, e1))
					ti = t.C2;
				else
					throw new ArgumentException(string.Format("The triangle {0}, does not contain a Child with the edge ({1} - {2})", ti, e0, e1));
			}

			return ti;
		}

		/// <summary>
		/// Is the edge legal, or does it need to be flipped?
		/// </summary>
		bool LegalEdge(int k, int l, int i, int j)
		{
			var lMagic = l < 0;
			var iMagic = i < 0;
			var jMagic = j < 0;

			if (lMagic)
			{
				return true;
			}
			else if (iMagic)
			{
				var p = points[l];
				var l0 = points[k];
				var l1 = points[j];

				return DelaunayUtility.ToTheLeft(p, l0, l1);
			}
			else if (jMagic)
			{
				var p = points[l];
				var l0 = points[k];
				var l1 = points[i];

				return DelaunayUtility.OnAnEdge(p, l0, l1) || !DelaunayUtility.ToTheLeft(p, l0, l1);
			}
			else
			{
				var p = points[l];
				var c0 = points[k];
				var c1 = points[i];
				var c2 = points[j];

				return !DelaunayUtility.InsideCircumCircle(p, c0, c1, c2);
			}
		}

		/// <summary>
		/// Key part of the algorithm. Flips edges if they need to be flipped,
		/// and recurses.
		///
		/// pi is the newly inserted point, creating a new triangle ti0.
		/// The adjacent triangle opposite pi in ti0 is ti1. The edge separating
		/// the two triangles is li0 and li1.
		///
		/// Checks if the (li0, li1) edge needs to be flipped. If it does,
		/// creates two new triangles, and recurses to check if the newly
		/// created triangles need flipping.
		/// <summary>
		void LegalizeEdge(int ti0, int ti1, int pi, int li0, int li1)
		{
			// ti1 might not be a leaf node (ti0 is guaranteed to be, it was
			// just created), so find the current correct leaf.
			ti1 = LeafWithEdge(ti1, li0, li1);

			var t0 = triangleNodeList[ti0];
			var t1 = triangleNodeList[ti1];
			var qi = t1.OtherPoint(li0, li1);

			if (!LegalEdge(pi, qi, li0, li1))
			{
				var ti2 = triangleNodeList.Count;
				var ti3 = ti2 + 1;

				var t2 = new TriangleNode(pi, li0, qi);
				var t3 = new TriangleNode(pi, qi, li1);

				t2.A0 = t1.Opposite(li1);
				t2.A1 = ti3;
				t2.A2 = t0.Opposite(li1);

				t3.A0 = t1.Opposite(li0);
				t3.A1 = t0.Opposite(li0);
				t3.A2 = ti2;

				triangleNodeList.Add(t2);
				triangleNodeList.Add(t3);

				var nt0 = triangleNodeList[ti0];
				var nt1 = triangleNodeList[ti1];

				nt0.C0 = ti2;
				nt0.C1 = ti3;

				nt1.C0 = ti2;
				nt1.C1 = ti3;

				triangleNodeList[ti0] = nt0;
				triangleNodeList[ti1] = nt1;

				if (t2.A0 != -1) LegalizeEdge(ti2, t2.A0, pi, li0, qi);
				if (t3.A0 != -1) LegalizeEdge(ti3, t3.A0, pi, qi, li1);
			}
		}

		private void ProcessWhenPointIsOnTriangleEdges(int pi, int ti, int ei0, int ei1)
		{
			var t = triangleNodeList[ti];

			var p0 = t.P0;
			var p1 = t.P1;
			var p2 = t.P2;

			// Indices of the newly created triangles.
			var nti0 = triangleNodeList.Count;
			var nti1 = nti0 + 1;

			var nt0 = new TriangleNode(pi, p0, p1);
			var nt1 = new TriangleNode(pi, p1, p2);

			nt0.A0 = t.A2;
			nt0.A1 = nti1;
			nt0.A2 = t.A1;

			nt1.A0 = t.A0;
			nt1.A1 = t.A1;
			nt1.A2 = nti0;

			t.C0 = nti0;
			t.C1 = nti1;

			triangleNodeList[ti] = t;

			triangleNodeList.Add(nt0);
			triangleNodeList.Add(nt1);

			var oti = t.A1;
			if (oti != -1)
			{
				oti = LeafWithEdge(oti, ei0, ei1);
				var ot = triangleNodeList[oti];

				var op0 = ot.P0;
				var op1 = ot.P1;
				var op2 = ot.P2;

				var onti0 = triangleNodeList.Count;
				var onti1 = onti0 + 1;

				var ont0 = new TriangleNode(pi, op1, op2);
				var ont1 = new TriangleNode(pi, op2, op0);

				ont0.A0 = ot.A0;
				ont0.A1 = onti1;
				ont0.A2 = nti1; 

				ont1.A0 = ot.A1;
				ont1.A1 = nti0; 
				ont1.A2 = onti0;

				ot.C0 = onti0;
				ot.C1 = onti1;

				triangleNodeList[oti] = ot;

				nt0.A2 = onti1;
				nt1.A1 = onti0;

				triangleNodeList[nti0] = nt0;
				triangleNodeList[nti1] = nt1;

				triangleNodeList.Add(ont0);
				triangleNodeList.Add(ont1);

				if (ont0.A0 != -1) LegalizeEdge(onti0, ont0.A0, pi, op1, op2);
				if (ont1.A0 != -1) LegalizeEdge(onti1, ont1.A0, pi, op2, op0);
			}

			if (nt0.A0 != -1) LegalizeEdge(nti0, nt0.A0, pi, p0, p1);
			if (nt1.A0 != -1) LegalizeEdge(nti1, nt1.A0, pi, p1, p2);
		}
		private void ProcessWhenPointIsInTriangle(int pi, int ti)
		{
			var t = triangleNodeList[ti];

			// The points of the containing triangle in CCW order
			var p0 = t.P0;
			var p1 = t.P1;
			var p2 = t.P2;

			// Indices of the newly created triangles.
			var nti0 = triangleNodeList.Count;
			var nti1 = nti0 + 1;
			var nti2 = nti0 + 2;

			// The new triangles! All in CCW order
			var nt0 = new TriangleNode(pi, p0, p1);
			var nt1 = new TriangleNode(pi, p1, p2);
			var nt2 = new TriangleNode(pi, p2, p0);

			// Setting the adjacency triangle references.  Only way to make
			// sure you do this right is by drawing the triangles up on a
			// piece of paper.
			nt0.A0 = t.A2;
			nt1.A0 = t.A0;
			nt2.A0 = t.A1;

			nt0.A1 = nti1;
			nt1.A1 = nti2;
			nt2.A1 = nti0;

			nt0.A2 = nti2;
			nt1.A2 = nti0;
			nt2.A2 = nti1;

			// The new triangles are the children of the old one.
			t.C0 = nti0;
			t.C1 = nti1;
			t.C2 = nti2;

			triangleNodeList[ti] = t;

			triangleNodeList.Add(nt0);
			triangleNodeList.Add(nt1);
			triangleNodeList.Add(nt2);

			if (nt0.A0 != -1) LegalizeEdge(nti0, nt0.A0, pi, p0, p1);
			if (nt1.A0 != -1) LegalizeEdge(nti1, nt1.A0, pi, p1, p2);
			if (nt2.A0 != -1) LegalizeEdge(nti2, nt2.A0, pi, p2, p0);
		}

		private void Initialize()
		{
			highest = 0;
			for (int i = 0; i < points.Length; i++)
				if (Higher(highest, i))
					highest = i;

			// Add first triangle, the bounding triangle.
			triangleNodeList.Add(new TriangleNode(-2, -1, highest));
		}
		private void RunBowyerWatson()
		{
			// For each point, find the containing triangle, split it into three
			// new triangles, call LegalizeEdge on all edges opposite the newly
			// inserted points
			for (int i = 0; i < points.Length; i++)
			{
				var pi = i;

				if (pi == highest) continue;

				// Index of the containing triangle
				var ti = FindTriangleNode(pi);

				if (PointIsOnTriangleEdges(pi, ti, out int ei0, out int ei1))
					ProcessWhenPointIsOnTriangleEdges(pi, ti, ei0, ei1);
				else
					ProcessWhenPointIsInTriangle(pi, ti);
			}
		}
		private void CalculateDataStruct(DelaunayTriangulation triangulation)
		{
			var triangleCount = triangulation.indices.Length / 3;
			triangulation.triangles = new DelaunayTriangulation.Triangle[triangleCount];

			var halfEdgeCount = triangulation.indices.Length;
			triangulation.halfEdges = new DelaunayTriangulation.HalfEdge[halfEdgeCount];

			triangulation.barycenters = new Point2D[triangleCount];
			triangulation.circumcenters = new Point2D[triangleCount];

			var processlist = new List<int>();

			var index = 0;
			for (int i = 0; i < triangulation.indices.Length; i += 3)
			{
				processlist.Add(i);
				processlist.Add(i + 1);
				processlist.Add(i + 2);

				triangulation.halfEdges[i] = new DelaunayTriangulation.HalfEdge
				{
					pi0 = triangulation.indices[i],
					pi1 = triangulation.indices[i + 1],

					ti = index
				};
				triangulation.halfEdges[i + 1] = new DelaunayTriangulation.HalfEdge
				{
					pi0 = triangulation.indices[i + 1],
					pi1 = triangulation.indices[i + 2],

					ti = index
				};
				triangulation.halfEdges[i + 2] = new DelaunayTriangulation.HalfEdge
				{
					pi0 = triangulation.indices[i + 2],
					pi1 = triangulation.indices[i],

					ti = index
				};

				triangulation.barycenters[index] = DelaunayUtility.CalculateBarycenter(points[triangulation.indices[i]], points[triangulation.indices[i + 1]], points[triangulation.indices[i + 2]]);
				triangulation.circumcenters[index] = DelaunayUtility.CalculateCircumcenter(points[triangulation.indices[i]], points[triangulation.indices[i + 1]], points[triangulation.indices[i + 2]]);

				triangulation.triangles[index] = new DelaunayTriangulation.Triangle
				{
					hei0 = i,
					hei1 = i + 1,
					hei2 = i + 2,

					bci = index,
					cci = index
				};

				index++;
			}

			while (processlist.Count > 0)
			{
				var current = processlist[0];

				for (int i = 0; i < processlist.Count; i++)
				{
					var tmp = processlist[i];
					if (triangulation.halfEdges[current].pi0 == triangulation.halfEdges[tmp].pi1 &&
						triangulation.halfEdges[current].pi1 == triangulation.halfEdges[tmp].pi0)
					{
						triangulation.halfEdges[current].ohei = tmp;
						triangulation.halfEdges[tmp].ohei = current;

						processlist.RemoveAt(i);

						break;
					}
				}

				processlist.RemoveAt(0);
			}
		}
		private void GenerateResult(out DelaunayTriangulation result, bool clockWise)
		{
			var tmp = new List<int>();
			for (int i = 0; i < this.triangleNodeList.Count; i++)
			{
				var triangle = triangleNodeList[i];
				if (triangle.IsLeaf && triangle.IsInner)
				{
					if (clockWise)
					{
						tmp.Add(triangle.P0);
						tmp.Add(triangle.P2);
						tmp.Add(triangle.P1);
					}
					else
					{
						tmp.Add(triangle.P0);
						tmp.Add(triangle.P1);
						tmp.Add(triangle.P2);
					}
				}
			}

			result = new DelaunayTriangulation(points, tmp.ToArray());
			CalculateDataStruct(result);
		}

		private void Clear()
		{
			this.points = null;
			triangleNodeList.Clear();
		}

		public void CalculateTriangulation(Point2D[] verts, out DelaunayTriangulation result, bool clockWise = false)
		{
			if (verts == null)
				throw new ArgumentNullException("points");

			if (verts.Length < 3)
				throw new ArgumentException("You need at least 3 points for a triangulation");

			this.points = verts;

			Initialize();
						
			try
			{
				RunBowyerWatson();
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc.Message);
			}

			GenerateResult(out result, clockWise);

			Clear();
			}
		public void CalculateTriangulation(Point2D[] verts, out DelaunayTriangulation result, TriangulationSetting setting)
		{
			DelaunayUtility.epsilon = setting.epsilon;
			CalculateTriangulation(verts, out result, setting.clockWise);
		}
	}
}
