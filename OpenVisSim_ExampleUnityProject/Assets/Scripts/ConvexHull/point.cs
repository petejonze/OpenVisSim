/*
	@ masphei
	email : masphei@gmail.com
*/
// --------------------------------------------------------------------------
// 2016-05-11 <oss.devel@searchathing.com> : created csprj and splitted Main into a separate file
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace ConvexHull
{

    public class Point
    {
        private int y;
        private int x;
		public float v;
		public Point(int _x, int _y, float _v)
        {
            x = _x;
            y = _y;
			v = _v;
        }
        public int getX()
        {
            return x;
        }
        public int getY()
        {
            return y;
        }
    }
     
}