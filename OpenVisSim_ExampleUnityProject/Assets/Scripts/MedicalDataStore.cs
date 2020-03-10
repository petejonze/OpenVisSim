using UnityEngine;


namespace VisSim
{
    public class MedicalDataStore
    {
        public static int myVar = 0;
        public static double[,] L_rawGrid_xy;
        public static double[,] R_rawGrid_xy;


        /*
         * if want to listen for updates: https://forum.unity.com/threads/variable-listener.468721/
        public static event OnVariableChangeDelegate OnVariableChange;
        public delegate void OnVariableChangeDelegate(float newVal);
        private static float _myFloat = 0f;
        public static float MyFloat
        {
            get
            {
                return _myFloat;
            }
            set
            {
                if (_myFloat == value) return;
                _myFloat = value;
                Debug.Log("AAAA");
                if (OnVariableChange != null)
                    OnVariableChange(_myFloat);
            }
        }
        */

        /*
    public static event OnVariableChangeDelegate OnVariableChange;
    public delegate void OnVariableChangeDelegate(double[,] newVal);
    public static double[,] rawGrid_xy
    {
        get { return rawGrid_xy; }
        set
        {
            rawGrid_xy = value;
            OnVariableChange(rawGrid_xy);
        }
    }
    */

    }

}
 