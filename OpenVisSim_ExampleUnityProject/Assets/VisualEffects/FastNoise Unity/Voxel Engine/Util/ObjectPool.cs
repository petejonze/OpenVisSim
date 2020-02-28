using System.Collections.Generic;

namespace VoxelEngine
{
	// Generic pool class to hold and create objects
	public class ObjectPool<T>
	{
		private int poolSize;
		private Stack<T> poolObjects;

		public int Count
		{
			get { return poolObjects.Count; }
		}

		public ObjectPool(int poolSize)
		{
			this.poolSize = poolSize;

			poolObjects = new Stack<T>(poolSize);
		}

		public bool Add(T obj)
		{
			if (poolObjects.Count >= poolSize)
				return false;

			poolObjects.Push(obj);
			return true;
		}

		public T Get()
		{
			return poolObjects.Count > 0 ? poolObjects.Pop() : System.Activator.CreateInstance<T>();
		}
	}
}