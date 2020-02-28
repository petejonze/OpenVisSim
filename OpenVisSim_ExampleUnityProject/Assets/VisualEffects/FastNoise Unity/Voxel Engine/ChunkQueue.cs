using System;
using System.Collections.Generic;

namespace VoxelEngine
{
	public class ChunkQueue
	{
		// Max queue size stops queue items becoming old, since the are sorted based on
		// the original distance they were added with and the target may have moved since then
		private const int MAX_QUEUE_SIZE = 256;

		// Sorted list of chunk locations
		private List<ChunkQueueItem> items = new List<ChunkQueueItem>(MAX_QUEUE_SIZE);
		// Hash set of queuing/processing chunk positions (faster contains check)
		private HashSet<Vector3i> hashSet = new HashSet<Vector3i>();

		public int Count
		{
			get { return items.Count; }
		}

		public void Enqueue(float distance, Vector3i pos)
		{
			// Add as first positions if list is empty
			if (items.Count == 0)
			{
				items.Add(new ChunkQueueItem(distance, pos));
				hashSet.Add(pos);
				return;
			}

			// If the queue is full and it's further than the last element don't add
			if (items.Count >= MAX_QUEUE_SIZE)
			{ 
				if (items[items.Count - 1].distance < distance)
					return;

				// Remove last element if over max size
				hashSet.Remove(items[MAX_QUEUE_SIZE-1].pos);
				items.RemoveAt(MAX_QUEUE_SIZE-1);
			}

			// Binary search to find position for new item in list
			ChunkQueueItem cqi = new ChunkQueueItem(distance, pos);
			int i = items.BinarySearch(cqi);

			if (i >= 0)
				items.Insert(i, cqi);
			else
				items.Insert(~i, cqi);
			
			hashSet.Add(pos);
		}

		// Dequeue doesn't remove positions from the hashSet
		public bool Dequeue(out Vector3i pos)
		{
			if (items.Count == 0)
			{
				pos = new Vector3i();
				return false;
			}

			pos = items[0].pos;
			items.RemoveAt(0);
			return true;
		}

		// Remove should be called after chunk is added to chunkMap, this stops
		// chunk positions being added to the chunk queue again if they are mid generation
		public void Remove(Vector3i pos)
		{
			hashSet.Remove(pos);
		}

		public bool IsFull()
		{
			return hashSet.Count >= MAX_QUEUE_SIZE;
		}

		// Hash Set lookup much faster than binary search through linked list
		public bool Contains(Vector3i pos)
		{
			return hashSet.Contains(pos);
		}

		public void Clear()
		{
			items.Clear();
			hashSet.Clear();
		}

		// Item for linked list
		private struct ChunkQueueItem : IComparable<ChunkQueueItem>
		{
			public float distance;
			public Vector3i pos;

			public ChunkQueueItem(float distance, Vector3i pos)
			{
				this.distance = distance;
				this.pos = pos;
			}

			public int CompareTo(ChunkQueueItem other)
			{
				return distance.CompareTo(other.distance);
			}
		}
	}
}