using UnityEngine;


namespace VisSim
{
    public static class VSUtils
    {
        
        // assert equality between two 2D arrays (compare dimensions, then compare contents elementwise)
        public static bool arraysEqual<T, S>(T[,] arrayA, S[,] arrayB)
        {
            if (arrayA.GetLength(0) != arrayB.GetLength(0)) return false;
            if (arrayA.GetLength(1) != arrayB.GetLength(1)) return false;

            for (int i = 0; i < arrayA.GetLength(0); i++)
            {
                for (int j = 0; j < arrayA.GetLength(1); j++)
                {
                    if (!arrayA[i,j].Equals(arrayB[i,j])) return false;
                }
            }
            return true;
        }

    }
} 