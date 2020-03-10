using UnityEngine;
using System.Linq;
using VisSim;
using System.Collections.Generic;
using System;
using ConvexHull;

// TODO: insert proper x/y limits (need to find viewing angle data for screen??)
// TODO: compute DIFFERENCE from normative values (pointwise? or after interpolation? latter would allow for arbitrary grids)
// TODO: scale amount of blurring to some arbitrary dB range
// TODO: investigate smoothing in RBF interpolation
// TODO: consider what to do with points outside of measured range
//    |- http://stackoverflow.com/questions/25813778/how-can-i-set-the-default-value-for-an-alglib-rbf-interpolator

// NB: uses Radial Basis Function for interpolation (allowing for scattered data): http://www.alglib.net/interpolation/fastrbf.php
// NB: see tmp_forUnity.m for data/comparison

public class GridInterpolator : ScriptableObject
{

    private static int width_px = 128; // The width_px of the image matrix in n-pixels
    private static int height_px = 128; // The height_px of the image matrix in n-pixels

	// NB: twice the viewable range, since the field loss overlay needs to be 2X bigger than the viewable area
    // NB: rough estimates!
    // https://www.reddit.com/r/oculus/comments/4at20n/field_of_view_for_vr_headsets_explained/
    private double fov_x_min = -55; // -21; // field of view left of the midline (on the retina), in degrees
	private double fov_x_max = 55; //21;
	private double fov_y_min = -55; //-15;
	private double fov_y_max = 55; //15;

	// MIN/MAX
	public static double ffMin = -30;
	public static double ffMax = 0;

    alglib.rbfmodel model;
	public double[,] xy;

    private static GridInterpolator instance; // Singleton instance


    public static GridInterpolator Instance
    {
        get
        {
            if (instance == null)
            {
                //instance = new GridInterpolator();
                instance = ScriptableObject.CreateInstance("GridInterpolator") as GridInterpolator;
                //instance = GameObject.AddComponent<GridInterpolator>();
            }
            return instance;
        }
    }

    // Constructor
    private GridInterpolator() {

        // Sequence of steps is as follows (*implemented in later methods):
        // 1. Create RBF model object and tune algorithm settings
        // *2. Attach our dataset to the RBF model                 
        // *3. Build RBF model using QNN algorithm
        // *4. Use RBF model to derive (interpolated) values for each pixel in a matrix
        // *5. Generate a texture using the pixel matrix, & set it as mipmapblur overlay
        //

        //
        // Step 1: RBF model creation.
        //
        // We have to specify dimensionality of the space (2 or 3) and
        // dimensionality of the function (scalar or vector).
        //
        alglib.rbfcreate(2, 1, out model);

        // i.e., What to do with points outside of measured range
        //alglib.rbfsetzeroterm(model); // i.e., will be black (full blur)
        //alglib.rbfsetconstterm(model); // i.e., will be average of all points
        alglib.rbfsetlinterm(model); // default, i.e., will try to continue on edge points, but often causes a 'dip' overshoot / edge effect
    }
    
    /*
    public double[,] getGrid()
    {
        return xy;
    }

    public void setGrid(double[,] xy)
    {
        this.xy = xy;
		interpolateGridAndMakeTexture();
    }

	public void setQRGrid(string filterVar)
	{

		// Remove any rubbish from the array
		String s = filterVar;
		//s = s.Replace("},{", "/").Replace("{", "").Replace("}", "");
		s = s.Replace("{", "").Replace("}", "");

		// Parse the string into a double 2d array
		int i = 0, j = 0;
		double[,] result = new double[44,3];
		foreach (var row in s.Split('/'))
		{
			j = 0;
			foreach (var col in row.Trim().Split(','))
			{
				result[i, j] = double.Parse(col.Trim());
				j++;
			}
			i++;
		};
        
		this.xy = result;
		interpolateGridAndMakeTexture();
	}
    */

	public Texture2D interpolateGridAndMakeTexture(double[,] xy, bool extrapolate)
    {
        // example data
        //xy = new double[,] { { -21, 9, 20.3125 }, { -21, 3, 21.1094 }, { -21, -3, 20.8125 }, { -21, -9, 20.3281 }, { -15, 15, 19.3281 }, { -15, 9, 20.0625 }, { -15, 3, 18.8281 }, { -15, -9, 20.1719 }, { -15, -15, 19.2813 }, { -9, 15, 19.8438 }, { -9, 9, 19.2031 }, { -9, 3, 21.5938 }, { -9, -3, 21.4219 }, { -9, -9, 19.6875 }, { -9, -15, 18.9688 }, { -3, 15, 19.6250 }, { -3, 9, 20.6094 }, { -3, 3, 21.7031 }, { -3, -3, 21.8125 }, { -3, -9, 20.4688 }, { -3, -15, 19.2344 }, { 3, 15, 20.3125 }, { 3, 9, 21.4844 }, { 3, 3, 21.8594 }, { 3, -3, 21.5625 }, { 3, -9, 20.6250 }, { 3, -15, 19.1875 }, { 9, 15, 20.4844 }, { 9, 9, 20.8281 }, { 9, 3, 21.3906 }, { 9, -3, 21.3125 }, { 9, -9, 20.6875 }, { 9, -15, 18.9063 }, { 15, 15, 20.1094 }, { 15, 9, 20.6875 }, { 15, 3, 20.9063 }, { 15, -3, 20.3906 }, { 15, -9, 20.4375 }, { 15, -15, 18.6094 }, { 21, 9, 20.2656 }, { 21, 3, 20.8750 }, { 21, -3, 20.7656 }, { 21, -9, 19.6094 }, { 27, 3, 20.0469 }, { 27, -3, 19.8750 } };
        // bottom-left corner attenauted:
        //double[,] xy = new double[,] { { -21, 9, 20.3125 }, { -21, 3, 21.1094 }, { -21, -3, 17.3438 }, { -21, -9, 16.9401 }, { -15, 15, 19.3281 }, { -15, 9, 20.0625 }, { -15, 3, 18.8281 }, { -15, -9, 16.8099 }, { -15, -15, 16.0678 }, { -9, 15, 19.8438 }, { -9, 9, 19.2031 }, { -9, 3, 21.5938 }, { -9, -3, 17.8516 }, { -9, -9, 16.4063 }, { -9, -15, 15.8073 }, { -3, 15, 19.6250 }, { -3, 9, 20.6094 }, { -3, 3, 21.7031 }, { -3, -3, 18.1771 }, { -3, -9, 17.0573 }, { -3, -15, 16.0287 }, { 3, 15, 20.3125 }, { 3, 9, 21.4844 }, { 3, 3, 21.8594 }, { 3, -3, 21.5625 }, { 3, -9, 20.6250 }, { 3, -15, 19.1875 }, { 9, 15, 20.4844 }, { 9, 9, 20.8281 }, { 9, 3, 21.3906 }, { 9, -3, 21.3125 }, { 9, -9, 20.6875 }, { 9, -15, 18.9063 }, { 15, 15, 20.1094 }, { 15, 9, 20.6875 }, { 15, 3, 20.9063 }, { 15, -3, 20.3906 }, { 15, -9, 20.4375 }, { 15, -15, 18.6094 }, { 21, 9, 20.2656 }, { 21, 3, 20.8750 }, { 21, -3, 20.7656 }, { 21, -9, 19.6094 }, { 27, 3, 20.0469 }, { 27, -3, 19.8750 } };
        // normalised relative to normal vision
        //double[,] xy = new double[,] { { -21, 9, 2.2192 }, { -21, 3, 2.2161 }, { -21, -3, 1.3192 }, { -21, -9, 1.4348 }, { -15, 15, 1.7348 }, { -15, 9, 0.3692 }, { -15, 3, -1.7652 }, { -15, -9, 0.1786 }, { -15, -15, 0.1880 }, { -9, 15, 1.5505 }, { -9, 9, -1.8902 }, { -9, 3, 0.3005 }, { -9, -3, -0.3714 }, { -9, -9, -2.2058 }, { -9, -15, -0.7245 }, { -3, 15, 0.9317 }, { -3, 9, -0.1839 }, { -3, 3, -0.4902 }, { -3, -3, -0.4808 }, { -3, -9, -0.9245 }, { -3, -15, -1.3589 }, { 3, 15, 2.0192 }, { 3, 9, 1.2911 }, { 3, 3, -0.1339 }, { 3, -3, -0.7308 }, { 3, -9, -0.6683 }, { 3, -15, -0.6058 }, { 9, 15, 1.0911 }, { 9, 9, 1.1348 }, { 9, 3, 0.6973 }, { 9, -3, 0.1192 }, { 9, -9, -0.6058 }, { 9, -15, -0.4870 }, { 15, 15, 1.2161 }, { 15, 9, 1.5942 }, { 15, -9, 0.8442 }, { 15, -15, -2.2839 }, { 21, 9, 1.3723 }, { 21, 3, 1.0817 }, { 21, -3, 0.7723 }, { 21, -9, -0.4839 }, { 27, 3, -0.3464 }, { 27, -3, 0.0817 } };
        // normalised & bottom-left corner attenauted:
        //double[,] xy = new double[,] { { -21, 9, 2.2192 }, { -21, 3, 2.2161 }, { -21, -3, -2.1496 }, { -21, -9, -1.9532 }, { -15, 15, 1.7348 }, { -15, 9, 0.3692 }, { -15, 3, -1.7652 }, { -15, -9, -3.1834 }, { -15, -15, -3.0256 }, { -9, 15, 1.5505 }, { -9, 9, -1.8902 }, { -9, 3, 0.3005 }, { -9, -3, -3.9417 }, { -9, -9, -5.4871 }, { -9, -15, -3.8860 }, { -3, 15, 0.9317 }, { -3, 9, -0.1839 }, { -3, 3, -0.4902 }, { -3, -3, -4.1162 }, { -3, -9, -4.3360 }, { -3, -15, -4.5647 }, { 3, 15, 2.0192 }, { 3, 9, 1.2911 }, { 3, 3, -0.1339 }, { 3, -3, -0.7308 }, { 3, -9, -0.6683 }, { 3, -15, -0.6058 }, { 9, 15, 1.0911 }, { 9, 9, 1.1348 }, { 9, 3, 0.6973 }, { 9, -3, 0.1192 }, { 9, -9, -0.6058 }, { 9, -15, -0.4870 }, { 15, 15, 1.2161 }, { 15, 9, 1.5942 }, { 15, -9, 0.8442 }, { 15, -15, -2.2839 }, { 21, 9, 1.3723 }, { 21, 3, 1.0817 }, { 21, -3, 0.7723 }, { 21, -9, -0.4839 }, { 27, 3, -0.3464 }, { 27, -3, 0.0817 } };
        // normalised & bottom-left corner *strongly* attenauted:
        //xy = new double[,] { { -21, 9, 2.2192 }, { -21, 3, 2.2161 }, { -21, -3, -9.0871 }, { -21, -9, -8.7293 }, { -15, 15, 1.7348 }, { -15, 9, 0.3692 }, { -15, 3, -1.7652 }, { -15, -3, -2.6652 }, { -15, -9, -9.9074 }, { -15, -15, -9.4527 }, { -9, 15, 1.5505 }, { -9, 9, -1.8902 }, { -9, 3, 0.3005 }, { -9, -3, -11.0824 }, { -9, -9, -12.0496 }, { -9, -15, -10.2089 }, { -3, 15, 0.9317 }, { -3, 9, -0.1839 }, { -3, 3, -0.4902 }, { -3, -3, -11.3871 }, { -3, -9, -11.1589 }, { -3, -15, -10.9761 }, { 3, 15, 2.0192 }, { 3, 9, 1.2911 }, { 3, 3, -0.1339 }, { 3, -3, -0.7308 }, { 3, -9, -0.6683 }, { 3, -15, -0.6058 }, { 9, 15, 1.0911 }, { 9, 9, 1.1348 }, { 9, 3, 0.6973 }, { 9, -3, 0.1192 }, { 9, -9, -0.6058 }, { 9, -15, -0.4870 }, { 15, 15, 1.2161 }, { 15, 9, 1.5942 }, { 15, -9, 0.8442 }, { 15, -15, -2.2839 }, { 21, 9, 1.3723 }, { 21, 3, 1.0817 }, { 21, -3, 0.7723 }, { 21, -9, -0.4839 }, { 27, 3, -0.3464 }, { 27, -3, 0.0817 } };

        //
        // Step 2: we add dataset.
        //   

        // Debugging
        //Debug.Log ("Grid array: " + xy.GetLength(0));
        
		// PJ: Trying to improve the stability at the edges by expanding outwards -- doesn't always work well however
		if (extrapolate) {

			//Debug.Log ("STARTING HULL");
			List<Point> listPoints = new List<Point>();
			for (int i = 0; i < xy.GetLength (0); i++) {
				listPoints.Add (new Point ((int)xy [i, 0], (int)xy [i, 1], (float)xy[i,2])); // HACK : convert double to int!
			}
			JarvisMatch jm = new JarvisMatch();
			List<Point> hull = jm.convexHull(listPoints);

			List<double[]> tmp1 = new List<double[]> ();
			foreach (Point p in hull) {
				//Debug.Log ("> " +  p.getX() + ", " + p.getY());
				tmp1.Add (new double[] { p.getX() * 1.25, p.getY() * 1.25, p.v * 1.1 }); // would be better to use the difference between the last two points to scale the gradient by
				//tmp1.Add (new double[] { p.getX(), p.getY() * 1.25, p.v * 1.1 });
				//tmp1.Add (new double[] { p.getX() * 1.25, p.getY(), p.v * 1.1 });
				tmp1.Add (new double[] { p.getX() * 1.5, p.getY() * 1.5, p.v * 1.2 });
				//tmp1.Add (new double[] { p.getX(), p.getY() * 1.5, p.v * 1.2 });
				//tmp1.Add (new double[] { p.getX() * 1.5, p.getY(), p.v * 1.2 });
			}
			//Debug.Log ("ENDING HULL");

			/*
			// find most extreme points (assuming a regular grid -- otherwise would have to use some kind of convex hull)
			double minX = xy [0, 0];
			double minY = xy [0, 1];
			double maxX = xy [0, 0];
			double maxY = xy [0, 1];
			for (int i = 0; i < xy.GetLength (0); i++) {
				minX = Math.Min (minX, xy [i, 0]);
				minY = Math.Min (minY, xy [i, 1]);
				maxX = Math.Max (maxX, xy [i, 0]);
				maxY = Math.Max (maxY, xy [i, 1]);
			}
			// duplicate outwards
			List<double[]> tmp1 = new List<double[]> ();
			for (int i = 0; i < xy.GetLength (0); i++) {
				if (xy [i, 0] == minX || xy [i, 0] == maxX || xy [i, 1] == minY || xy [i, 1] == maxY) {
					tmp1.Add (new double[] { xy [i, 0] * 1.25, xy [i, 1] * 1.25, xy [i, 2] * 1.1 }); // would be better to use the difference between the last two points to scale the gradient by
					tmp1.Add (new double[] { xy [i, 0] * 1.5, xy [i, 1] * 1.5, xy [i, 2] * 1.2 });
				}
			}
			*/


			// concatenate new points with existing (xy), to make a new xy1
			double[,] xy1 = new double[xy.GetLength (0) + tmp1.Count, 3];
			for (int i = 0; i < xy.GetLength (0); i++) {
				xy1 [i, 0] = xy [i, 0];
				xy1 [i, 1] = xy [i, 1];
				xy1 [i, 2] = xy [i, 2];
			}
			int ii = xy.GetLength (0);
			foreach (double[] d in tmp1) {
				xy1 [ii, 0] = d [0];
				xy1 [ii, 1] = d [1];
				xy1 [ii, 2] = d [2];
				ii++;
			}

			alglib.rbfsetpoints (model, xy1);
		} else {
			alglib.rbfsetpoints (model, xy);
		}

        //
        // Step 3: build model
        //
        // After we've configured model, we should build it -
        // this will change coefficients stored internally in the
        // rbfmodel structure.
        //
        // By default, RBF uses QNN algorithm, which works well with
        // relatively uniform datasets (all points are well separated,
        // average distance is approximately same for all points).
        // This default algorithm is perfectly suited for our simple
        // made up data.
        //
        // NOTE: we recommend you to take a look at example of RBF-ML,
        // multilayer RBF algorithm, which sometimes is a better
        // option than QNN.
        //
        alglib.rbfreport rep;

        // set model type
        //alglib.rbfsetalgoqnn(model);

		// used in pilot:
        alglib.rbfsetalgomultilayer(model, 5.0, 1, 1.0e-3); // Manual example, use 5.0+ as 2nd input param for more reasonable smoothness

		// new mess:
		//alglib.rbfsetalgomultilayer(model, 2.0, 2, 1.0e-3); // Manual example, use 5.0+ as 2nd input param for more reasonable smoothness

        // build model
        alglib.rbfbuildmodel(model, out rep);

        // check built successfully
        /*Rep.TerminationType:
                  *-5 - non - distinct basis function centers were detected,
                         interpolation aborted
                  *-4 - nonconvergence of the internal SVD solver
                  *  1 - successful termination
        */
        if (rep.terminationtype != 1)
        {
            throw new System.ArgumentException("RBF model failed to converge (could revert to some default?)");
        }

        //
        // Step 4: use the now-built model to interpolate pixel values
        //
        // After call of rbfbuildmodel(), rbfcalc2() will return
        // value of the new model.
        //

        // PERFORM INTERPOLATION
        // create xx grid array (think: meshgrid)
        int nsteps = width_px;
        double[] xx = Enumerable.Range(0, nsteps).Select(i => fov_x_min + (fov_x_max - fov_x_min) * ((double)i / (nsteps - 1))).ToArray();
        nsteps = height_px;
        double[] yy = Enumerable.Range(0, nsteps).Select(i => fov_y_min + (fov_y_max - fov_y_min) * ((double)i / (nsteps - 1))).ToArray();

        // interp
        double[,] ff = new double[width_px, height_px];
        alglib.rbfgridcalc2(model, xx, width_px, yy, height_px, out ff);

        // Debugging
        /*
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                Debug.Log("grid (" + xx[i] + "," + yy[j] + "): " + ff[i, j]);
            }
        }
        Debug.Log("grid min: " + ff.Cast<double>().Min());
        Debug.Log("grid max: " + ff.Cast<double>().Max());
        */

        // make an image matrix, scaled so values vary from 0 and 1
        // convert 2D matrix of double-precision sensitivity values to 1D array of scaled colors
        //double ffMin = -30; // ff.Cast<double>().Min();
        //double ffMax = 0; // ff.Cast<double>().Max();
        double ffDiff = ffMax - ffMin;

        Color[] imgMatrix = new Color[width_px * height_px];
        for (int x = 0; x < width_px; x++)
        {
            for (int y = 0; y < height_px; y++)
            {
                float k = (float)((ff[x, y] - ffMin) / ffDiff);
                k = Mathf.Clamp(k, 0, 1);
                //k = 1; // [no blur -- for debugging]
				imgMatrix[x + y * height_px] = Color.white * (k);
            }
        }

        //
        // Step 5: make texture and set it as mipmapblur overlay
        //
        // After call of rbfbuildmodel(), rbfcalc2() will return
        // value of the new model.
        //

        // make texture
		Texture2D tex = new Texture2D(width_px, height_px, TextureFormat.RGB24, false); // Create a new texture RGB24 (24 bit without alpha) and no mipmap
		tex.wrapMode = TextureWrapMode.Clamp; // to avoid wrapping round at sides!
		tex.SetPixels(imgMatrix);
		tex.Apply(false, false); // actually apply all SetPixels, don't recalculate mip levels

        // for debugging: draw texture to screen!
        //GameObject currentLoc = GameObject.Find("Cube (Street Scene)"); //("Cube (Texture2D)");
        //currentLoc.GetComponent<Renderer>().material.mainTexture = tex;

        // return final output (the texture!)
        return tex;

        /*
        //GameObject leftEye = GameObject.FindWithTag("LeftEye");
        myFieldLoss sn = this.gameObject.GetComponent<myFieldLoss>();
        sn.SetOverlay(tex);
        */
    }
}