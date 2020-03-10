using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConvexHull1
{
    //Generate a convex hull with Graham's Scan algorithm
    public List<Vector3> GenerateConvexHull(List<Vector3> unSortedList)
    {
        List<Vector3> convexHullList = new List<Vector3>();

        //Step 1 - Find the vertice with the smallest x coordinate
        //Init with just the first in the list
        float smallestValue = unSortedList[0].x;
        int smallestIndex = 0;

        //Check if we can find a smaller value
        for (int i = 1; i < unSortedList.Count; i++)
        {
            if (unSortedList[i].x < smallestValue)
            {
                smallestValue = unSortedList[i].x;

                smallestIndex = i;
            }
        }

        //Remove the smallest coordinate from the list and add it to the list 
        //with convex hull vertices because this vertex is on the convex hull
        convexHullList.Add(unSortedList[smallestIndex]);

        unSortedList.RemoveAt(smallestIndex);


        //Step 2 - Sort the vertices based on angle to start vertex
        Vector3 firstPoint = convexHullList[0];
        //Need a direction to get an angle with Vector3.Angle()
        Vector3 startVec = (firstPoint + new Vector3(0f, 0f, -1f)) - firstPoint;

        //Important that everything is in 2d space
        firstPoint.y = 0f;
        startVec.y = 0f;

        //Sort from smallest to largest angle
        unSortedList = unSortedList.OrderBy(n => Vector3.Angle(startVec, new Vector3(n.x, 0f, n.z) - firstPoint)).ToList();

        //Reverse because it's faster to remove vertices from the end
        unSortedList.Reverse();


        //Step 3 - The vertex with the smallest angle is also on the convex hull so add it
        convexHullList.Add(unSortedList[unSortedList.Count - 1]);

        unSortedList.RemoveAt(unSortedList.Count - 1);


        //Step 4 - The main algorithm to find the convex hull
        //To avoid infinite loop
        int safety = 0;
        while (unSortedList.Count > 0 && safety < 10000)
        {
            //Get the vertices of the current triangle abc
            Vector3 a = convexHullList[convexHullList.Count - 2];
            Vector3 b = convexHullList[convexHullList.Count - 1];

            Vector3 c = unSortedList[unSortedList.Count - 1];

            unSortedList.RemoveAt(unSortedList.Count - 1);

            convexHullList.Add(c);

            //Is this a clockwise or a counter-clockwise triangle ?
            //May need to back track several steps in case we messed up at an earlier point
            while (isClockWise(a, b, c) && safety < 10000)
            {
                //Remove the next to last vertex because we know it aint on the convex hull
                convexHullList.RemoveAt(convexHullList.Count - 2);

                //Get the vertices of the current triangle abc
                a = convexHullList[convexHullList.Count - 3];
                b = convexHullList[convexHullList.Count - 2];
                c = convexHullList[convexHullList.Count - 1];

                safety += 1;
            }

            safety += 1;
        }

        return convexHullList;
    }


    //Is a triangle in 2d space oriented clockwise or counter-clockwise
    private bool isClockWise(Vector3 a, Vector3 b, Vector3 c)
    {
        float signedArea = (b.x - a.x) * (c.z - a.z) - (b.z - a.z) * (c.x - a.x);

        if (signedArea > 0)
        {
            return false;
        }
        else
        {
            return true;
        }

        //There's also the case when abc is a line (signedArea = 0), but ignore that here!
    }


    //Display in which order the vertices have been added to a list
    //by connecting a line between all vertices
    public void DisplayConvexHull(List<Vector3> verticesList)
    {
        float height = 0.1f;
        
        for (int i = 0; i < verticesList.Count; i++)
        {
            Vector3 start = verticesList[i] + Vector3.up * height;

            //Connect the end with the start
            int endPos = i + 1;

            if (i == verticesList.Count - 1)
            {
                endPos = 0;
            }

            Vector3 end = verticesList[endPos] + Vector3.up * height;

            Debug.DrawLine(start, end, Color.blue);
        }
    }
}