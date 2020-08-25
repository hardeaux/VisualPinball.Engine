using System;
using System.Collections.Generic;
using System.Linq;

namespace VisualPinball.Engine.Math.ProgMesh
{
	public class Triangle
	{
		/// <summary>
		/// the 3 points that make this tri
		/// </summary>
		public Vertex[] vertex = new Vertex[3];

		/// <summary>
		/// unit vector othogonal to this face
		/// </summary>
		public Vertex3D normal;

		public Triangle(Vertex v0, Vertex v1, Vertex v2)
		{
			Util.Assert(v0 != v1 && v1 != v2 && v2 != v0, "[Triangle] Vertices must be different.");

			if (v0 == v1 || v1 == v2 || v2 == v0) {
				throw new ArgumentException();
			}

			vertex[0] = v0;
			vertex[1] = v1;
			vertex[2] = v2;
			ComputeNormal();
			ProgMesh.triangles.Add(this);

			for (var i = 0; i < 3; i++) {
				vertex[i].face.Add(this);
				for (var j = 0; j < 3; j++) {
					if (i != j) {
						Util.AddUnique(vertex[i].neighbor, vertex[j]);
					}
				}
			}
		}

		~Triangle()
		{
			Util.RemoveFillWithBack(ProgMesh.triangles, this);
			for (var i = 0; i < 3; i++) {
				if (vertex[i] != null) {
					Util.RemoveFillWithBack(vertex[i].face, this);
				}
			}

			for (var i = 0; i < 3; i++) {
				var i2 = (i + 1) % 3;
				if (vertex[i] != null && vertex[i2] != null) {
					vertex[i].RemoveIfNonNeighbor(vertex[i2]);
					vertex[i2].RemoveIfNonNeighbor(vertex[i]);
				}
			}
		}

		public void ComputeNormal()
		{
			var v0 = vertex[0].position;
			var v1 = vertex[1].position;
			var v2 = vertex[2].position;
			normal = Vertex3D.CrossProduct(v1.Clone().Sub(v0), v2.Clone().Sub(v1));
			var l =  normal.Clone().Magnitude();
			if (l > float.MinValue) {
				normal.DivideScalar(l);
			}
		}

		public void ReplaceVertex(Vertex vold, Vertex vnew)
		{
			Util.Assert(vold != null && vnew != null, "[ProgMeshTriangle.replaceVertex] Arguments must not be null.");
			Util.Assert(vold == vertex[0] || vold == vertex[1] || vold == vertex[2], "[ProgMeshTriangle.replaceVertex] vold must not be included in this.vertex.");
			Util.Assert(vnew != vertex[0] && vnew != vertex[1] && vnew != vertex[2], "[ProgMeshTriangle.replaceVertex] vnew must not be included in this.vertex.");

			if (vold == vertex[0]) {
				vertex[0] = vnew;

			} else if (vold == vertex[1]) {
				vertex[1] = vnew;

			} else {
				Util.Assert(vold == vertex[2], "[ProgMeshTriangle.replaceVertex] vold == vertex[2]");
				vertex[2] = vnew;
			}
			Util.RemoveFillWithBack(vold.face, this);
			Util.Assert(!vnew.face.Contains(this), "[ProgMeshTriangle.replaceVertex] !Contains(vnew->face, this)");

			vnew.face.Add(this);

			for (var i = 0; i < 3; i++) {
				vold.RemoveIfNonNeighbor(vertex[i]);
				vertex[i].RemoveIfNonNeighbor(vold);
			}

			for (var i = 0; i < 3; i++) {
				Util.Assert(this.vertex[i].face.Count(f => f == this) == 1, "[ProgMeshTriangle.replaceVertex] Contains(vertex[i]->face, this) == 1");
				for (var j = 0; j < 3; j++) {
					if (i != j) {
						Util.AddUnique(vertex[i].neighbor, vertex[j]);
					}
				}
			}
			ComputeNormal();
		}

		public bool HasVertex(Vertex v)
		{
			return v == vertex[0] || v == vertex[1] || v == vertex[2];
		}
	}


	internal struct tridata {
		internal int[] v;

		public tridata(int v1, int v2, int v3)
		{
			v = new[] {v1, v2, v3};
		}
	}
}
