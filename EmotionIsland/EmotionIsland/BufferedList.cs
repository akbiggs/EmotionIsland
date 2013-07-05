using System.Collections.Generic;

namespace EmotionIsland
{
    /// <summary>
    /// A subclass of the list that supports add and remove buffers, so changes
    /// can be scheduled to the list while it is being processed, and applied
    /// afterwards.
    /// </summary>
    public class BufferedList<T> : List<T>
    {
        private List<T> AddBuffer = new List<T>();
        private List<T> RemoveBuffer = new List<T>();

        public void BufferAdd(T obj)
        {
            this.AddBuffer.Add(obj);
        }

        public void BufferRemove(T obj)
        {
            this.RemoveBuffer.Add(obj);
        }

        public void ApplyBuffers()
        {
            this.AddRange(this.AddBuffer);
            foreach (T thing in this.RemoveBuffer)
                this.Remove(thing);

            this.AddBuffer.Clear();
            this.RemoveBuffer.Clear();
        }

        /// <summary>
        /// Count of everything, including objects in add buffer.
        /// </summary>
        /// <returns></returns>
        public int TotalCount()
        {
            return this.AddBuffer.Count + this.Count;
        }

        private bool InRemoveBuffer(T obj)
        {
            return this.RemoveBuffer.Contains(obj);
        }
    }
}
