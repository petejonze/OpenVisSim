using UnityEngine;

namespace VoxelEngine
{
	[ExecuteInEditMode]
	public abstract class TerrainGeneratorBase : MonoBehaviour
	{
		protected float minHeight = float.MinValue;
		protected float maxHeight = float.MaxValue;

		protected Voxel minVoxel = Voxel.Solid;
		protected Voxel maxVoxel = Voxel.Empty;

		protected int interpBitStep;
		protected int interpSize;
		protected int interpSizeSq;
		protected float interpScale;

		public abstract void GenerateChunk(Chunk chunk);
		public abstract Color32 DensityColor(Voxel voxel);

		public float MinHeight()
		{
			return minHeight;
		}

		public float MaxHeight()
		{
			return maxHeight;
		}

		public Voxel MinVoxel()
		{
			return minVoxel;
		}

		public Voxel MaxVoxel()
		{
			return maxVoxel;
		}

		public static void ChunkFillUpdate(Chunk chunk, Voxel voxel)
		{
			switch (chunk.fillType)
			{
				case Chunk.FillType.Empty:
					if (voxel.IsSolid())
						chunk.fillType = Chunk.FillType.Mixed;
					break;

				case Chunk.FillType.Solid:
					if (!voxel.IsSolid())
						chunk.fillType = Chunk.FillType.Mixed;
					break;

				case Chunk.FillType.Null:
					chunk.fillType = voxel.IsSolid() ? Chunk.FillType.Solid : Chunk.FillType.Empty;
					break;
			}
		}

		public virtual void Awake()
		{
		}

		// The higher the interpBitStep the less noise samples are taken and more interpolation is used, this is faster but can create less detailed terrain
		protected void SetInterpBitStep(int interpBitStep)
		{
			this.interpBitStep = interpBitStep;
			interpSize = (Chunk.SIZE >> interpBitStep) + 1;
			interpSizeSq = interpSize*interpSize;
			interpScale = 1f/(1 << interpBitStep);
		}

		protected int InterpLookupIndex(int interpX, int interpY, int interpZ)
		{
			return interpZ + interpY*interpSize + interpX*interpSizeSq;
		}

		protected float VoxelInterpLookup(int localX, int localY, int localZ, float[] interpLookup)
		{
			float xs = (localX + 0.5f)*interpScale;
			float ys = (localY + 0.5f)*interpScale;
			float zs = (localZ + 0.5f)*interpScale;

			int x0 = FastFloor(xs);
			int y0 = FastFloor(ys);
			int z0 = FastFloor(zs);

			xs = (xs - x0);
			ys = (ys - y0);
			zs = (zs - z0);

			int lookupIndex = InterpLookupIndex(x0, y0, z0);

			return Lerp(Lerp(
				Lerp(interpLookup[lookupIndex], interpLookup[lookupIndex + interpSizeSq], xs),
				Lerp(interpLookup[lookupIndex + interpSize], interpLookup[lookupIndex + interpSizeSq + interpSize], xs),
				ys), Lerp(
					Lerp(interpLookup[++lookupIndex], interpLookup[lookupIndex + interpSizeSq], xs),
					Lerp(interpLookup[lookupIndex + interpSize], interpLookup[lookupIndex + interpSizeSq + interpSize], xs),
					ys), zs);
		}

		private static float Lerp(float a, float b, float t)
		{
			return a + t*(b - a);
		}

		private static int FastFloor(float f)
		{
			return (f >= 0.0f ? (int) f : (int) f - 1);
		}
	}
}