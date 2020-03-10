using UnityEngine;
using System.Collections;
using System;
using System.IO;
using VisSim;

public class PAMELA_main : MonoBehaviour 
{

    public GameObject TextLeft;
    public GameObject TextRight;

    public myFieldLoss myFieldLoss_left;
    public myFieldLoss myFieldLoss_right;
    public myInpainter2 myInpainter_left;
    public myInpainter2 myInpainter_right;

    private float leftEyeVIindex = 0.5f;
    private float rightEyeVIindex = 0.5f;

    public enum FieldLossType
    {
        Uniform = 0,
		Central = 1,
		Peripheral = 2,
		Blindspot = 3,
		TS_Superior = 4,
		TS_Inferior = 5,
		UK_walk_in = 6,
        Tanzania_walk_in = 7,
        Timecourse_mild = 8,
        Timecourse_moderate = 9,
        Timecourse_severe = 10,
        None = 11
    }

	// More more internal variables
	private double[,] grid_basic_xy; // field loss grid
	private double[,] grid_blindspot_left_xy;
	private double[,] grid_blindspot_right_xy;
	private double[,] grid_superior_left_xy;
	private double[,] grid_superior_right_xy;
	private double[,] grid_inferior_left_xy;
	private double[,] grid_inferior_right_xy;
    private double[,] grid_UKwalkIn_left_xy;
    private double[,] grid_UKwalkIn_right_xy;
    private double[,] grid_TanzaniaWalkIn_left_xy;
    private double[,] grid_TanzaniaWalkIn_right_xy;
    private double[,] grid_PID1269767_mild_left_xy;
    private double[,] grid_PID1269767_mild_right_xy;
    private double[,] grid_PID1269767_moderate_left_xy;
    private double[,] grid_PID1269767_moderate_right_xy;
    private double[,] grid_PID1269767_severe_left_xy;
    private double[,] grid_PID1269767_severe_right_xy;
    //
    private FieldLossType prevGridType;
	private float prevGridLevel_LE;
	private float prevGridLevel_RE;

	public void Start()
	{
		// initialise basic field grid
		//double[,]  xy = new double[,] { { -21, 9, -25 }, { -21, 3, -25 }, { -21, -3, -25 }, { -21, -9, -25 }, { -15, 15, -20 }, { -15, 9, -20 }, { -15, 3, -20 }, { -15, -3, -20 }, { -15, -9, -20 }, { -15, -15, -20 }, { -9, 15, -15 }, { -9, 9, -15 }, { -9, 3, -15 }, { -9, -3, -15 }, { -9, -9, -15 }, { -9, -15, -15 }, { -3, 15, -10 }, { -3, 9, -10 }, { -3, 3, -10 }, { -3, -3, -10 }, { -3, -9, -10 }, { -3, -15, -10 }, { 3, 15, 0 }, { 3, 9, 0 }, { 3, 3, 0 }, { 3, -3, 0 }, { 3, -9, 0 }, { 3, -15, 0 }, { 9, 15, 0 }, { 9, 9, 0 }, { 9, 3, 0 }, { 9, -3, 0 }, { 9, -9, 0 }, { 9, -15, 0 }, { 15, 15, 0 }, { 15, 9, 0 }, { 15, -9, 0 }, { 15, -15, 0 }, { 21, 9, 0 }, { 21, 3, 0 }, { 21, -3, 0 }, { 21, -9, 0 }, { 27, 3, 0 }, { 27, -3, 0 } };
		grid_basic_xy = new double[25*17,3];
		int kk = 0;
		for (int y = -16; y <= 16; y += 2) {
			for (int x = -24; x < 24; x += 2) {
				grid_basic_xy [kk, 0] = x;
				grid_basic_xy [kk, 1] = y;
				grid_basic_xy [kk, 2] = -30;
				kk++;
			}
		}

		// pre-cache the VF loss texture (only necessary in coniditions that are linked to an explicit grid)
		// blindspot
		grid_blindspot_left_xy = new double[,]{{-9, 21, 31}, {-3, 21, 31}, {3, 21, 31}, {9, 21, 31}, {-15, 15, 31}, {-9, 15, 31}, {-3, 15, 31}, {3, 15, 31}, {9, 15, 31}, {15, 15, 31}, {-21, 9, 31}, {-15, 9, 31}, {-9, 9, 31}, {-3, 9, 31}, {3, 9, 31}, {9, 9, 31}, {15, 9, 31}, {21, 9, 31}, {-21, 3, 31}, {-15, 3, 31}, {-9, 3, 31}, {-3, 3, 31}, {3, 3, 31}, {9, 3, 31}, {15, 3, 31}, {21, 3, 31}, {27, 3, 31}, {-21, -3, 31}, {-15, -3, 0}, {-9, -3, 31}, {-3, -3, 31}, {3, -3, 31}, {9, -3, 31}, {15, -3, 31}, {21, -3, 31}, {27, -3, 31}, {-21, -9, 31}, {-15, -9, 31}, {-9, -9, 31}, {-3, -9, 31}, {3, -9, 31}, {9, -9, 31}, {15, -9, 31}, {21, -9, 31}, {-15, -15, 31}, {-9, -15, 31}, {-3, -15, 31}, {3, -15, 31}, {9, -15, 31}, {15, -15, 31}, {-9, -21, 31}, {-3, -21, 31}, {3, -21, 31}, {9, -21, 31}};
		grid_blindspot_right_xy = new double[,]{{-9, 21, 31}, {-3, 21, 31}, {3, 21, 31}, {9, 21, 31}, {-15, 15, 31}, {-9, 15, 31}, {-3, 15, 31}, {3, 15, 31}, {9, 15, 31}, {15, 15, 31}, {-21, 9, 31}, {-15, 9, 31}, {-9, 9, 31}, {-3, 9, 31}, {3, 9, 31}, {9, 9, 31}, {15, 9, 31}, {21, 9, 31}, {-27, 3, 31}, {-21, 3, 31}, {-15, 3, 31}, {-9, 3, 31}, {-3, 3, 31}, {3, 3, 31}, {9, 3, 31}, {15, 3, 31}, {21, 3, 31}, {-27, -3, 31}, {-21, -3, 31}, {-15, -3, 31}, {-9, -3, 31}, {-3, -3, 31}, {3, -3, 31}, {9, -3, 31}, {15, -3, 0}, {21, -3, 31}, {-21, -9, 31}, {-15, -9, 31}, {-9, -9, 31}, {-3, -9, 31}, {3, -9, 31}, {9, -9, 31}, {15, -9, 31}, {21, -9, 31}, {-15, -15, 31}, {-9, -15, 31}, {-3, -15, 31}, {3, -15, 31}, {9, -15, 31}, {15, -15, 31}, {-9, -21, 31}, {-3, -21, 31}, {3, -21, 31}, {9, -21, 31}};
        // superior (flipped for inferior)
        grid_superior_left_xy = new double[,] { { -9, 21, 23 }, { -3, 21, 13 }, { 3, 21, 6 }, { 9, 21, 7 }, { -15, 15, 19 }, { -9, 15, 19 }, { -3, 15, 9 }, { 3, 15, 7 }, { 9, 15, 4 }, { 15, 15, 0 }, { -21, 9, 27 }, { -15, 9, 8 }, { -9, 9, 0 }, { -3, 9, 0 }, { 3, 9, 0 }, { 9, 9, 0 }, { 15, 9, 0 }, { 21, 9, 0 }, { -21, 3, 23 }, { -15, 3, 0 }, { -9, 3, 23 }, { -3, 3, 9 }, { 3, 3, 10 }, { 9, 3, 8 }, { 15, 3, 0 }, { 21, 3, 0 }, { 27, 3, 0 }, { -21, -3, 23 }, { -9, -3, 31 }, { -3, -3, 30 }, { 3, -3, 29 }, { 9, -3, 28 }, { 15, -3, 17 }, { 21, -3, 14 }, { 27, -3, 18 }, { -21, -9, 19 }, { -15, -9, 27 }, { -9, -9, 30 }, { -3, -9, 28 }, { 3, -9, 25 }, { 9, -9, 25 }, { 15, -9, 26 }, { 21, -9, 24 }, { -15, -15, 23 }, { -9, -15, 29 }, { -3, -15, 24 }, { 3, -15, 23 }, { 9, -15, 21 }, { 15, -15, 19 }, { -9, -21, 21 }, { -3, -21, 16 }, { 3, -21, 15 }, { 9, -21, 19 } };
        grid_superior_right_xy = new double[,] { { -9, 21, 0 }, { -3, 21, 0 }, { 3, 21, 20 }, { 9, 21, 23 }, { -15, 15, 0 }, { -9, 15, 0 }, { -3, 15, 0 }, { 3, 15, 0 }, { 9, 15, 21 }, { 15, 15, 17 }, { -21, 9, 0 }, { -15, 9, 0 }, { -9, 9, 0 }, { -3, 9, 0 }, { 3, 9, 0 }, { 9, 9, 0 }, { 15, 9, 14 }, { 21, 9, 13 }, { -27, 3, 3 }, { -21, 3, 0 }, { -15, 3, 0 }, { -9, 3, 0 }, { -3, 3, 0 }, { 3, 3, 12 }, { 9, 3, 14 }, { 15, 3, 21 }, { 21, 3, 23 }, { -27, -3, 20 }, { -21, -3, 26 }, { -15, -3, 27 }, { -9, -3, 28 }, { -3, -3, 30 }, { 3, -3, 30 }, { 9, -3, 27 }, { 21, -3, 28 }, { -21, -9, 24 }, { -15, -9, 29 }, { -9, -9, 29 }, { -3, -9, 30 }, { 3, -9, 30 }, { 9, -9, 28 }, { 15, -9, 27 }, { 21, -9, 27 }, { -15, -15, 28 }, { -9, -15, 29 }, { -3, -15, 30 }, { 3, -15, 29 }, { 9, -15, 28 }, { 15, -15, 28 }, { -9, -21, 22 }, { -3, -21, 27 }, { 3, -21, 28 }, { 9, -21, 28 } };
        // UK walk-in
        grid_UKwalkIn_left_xy = new double[,] { { -9, 21, 20 }, { -3, 21, 20 }, { 3, 21, 21 }, { 9, 21, 20 }, { -15, 15, 23 }, { -9, 15, 24 }, { -3, 15, 23 }, { 3, 15, 24 }, { 9, 15, 24 }, { 15, 15, 22 }, { -21, 9, 23 }, { -15, 9, 25 }, { -9, 9, 25 }, { -3, 9, 26 }, { 3, 9, 26 }, { 9, 9, 26 }, { 15, 9, 25 }, { 21, 9, 22 }, { -21, 3, 25 }, { -15, 3, 21 }, { -9, 3, 26 }, { -3, 3, 28 }, { 3, 3, 28 }, { 9, 3, 27 }, { 15, 3, 26 }, { 21, 3, 23 }, { 27, 3, 20 }, { -21, -3, 25 }, { -9, -3, 27 }, { -3, -3, 29 }, { 3, -3, 29 }, { 9, -3, 28 }, { 15, -3, 26 }, { 21, -3, 24 }, { 27, -3, 20 }, { -21, -9, 25 }, { -15, -9, 26 }, { -9, -9, 27 }, { -3, -9, 27 }, { 3, -9, 27 }, { 9, -9, 27 }, { 15, -9, 26 }, { 21, -9, 23 }, { -15, -15, 25 }, { -9, -15, 27 }, { -3, -15, 26 }, { 3, -15, 26 }, { 9, -15, 26 }, { 15, -15, 24 }, { -9, -21, 24 }, { -3, -21, 25 }, { 3, -21, 24 }, { 9, -21, 23 } };
        grid_UKwalkIn_right_xy = new double[,] { { -9, 21, 21 }, { -3, 21, 21 }, { 3, 21, 20 }, { 9, 21, 20 }, { -15, 15, 22 }, { -9, 15, 24 }, { -3, 15, 24 }, { 3, 15, 23 }, { 9, 15, 24 }, { 15, 15, 23 }, { -21, 9, 22 }, { -15, 9, 25 }, { -9, 9, 26 }, { -3, 9, 26 }, { 3, 9, 25 }, { 9, 9, 25 }, { 15, 9, 25 }, { 21, 9, 23 }, { -27, 3, 20 }, { -21, 3, 23 }, { -15, 3, 26 }, { -9, 3, 27 }, { -3, 3, 28 }, { 3, 3, 28 }, { 9, 3, 26 }, { 15, 3, 21 }, { 21, 3, 25 }, { -27, -3, 20 }, { -21, -3, 24 }, { -15, -3, 26 }, { -9, -3, 28 }, { -3, -3, 29 }, { 3, -3, 29 }, { 9, -3, 28 }, { 21, -3, 25 }, { -21, -9, 23 }, { -15, -9, 26 }, { -9, -9, 27 }, { -3, -9, 28 }, { 3, -9, 28 }, { 9, -9, 27 }, { 15, -9, 26 }, { 21, -9, 25 }, { -15, -15, 25 }, { -9, -15, 26 }, { -3, -15, 26 }, { 3, -15, 26 }, { 9, -15, 27 }, { 15, -15, 26 }, { -9, -21, 23 }, { -3, -21, 24 }, { 3, -21, 25 }, { 9, -21, 28 } };
        // Tanzania walk-in
        grid_TanzaniaWalkIn_left_xy = new double[,] { { -9, 21, 16 }, { -3, 21, 15 }, { 3, 21, 16 }, { 9, 21, 15 }, { -15, 15, 18 }, { -9, 15, 19 }, { -3, 15, 18 }, { 3, 15, 19 }, { 9, 15, 19 }, { 15, 15, 18 }, { -21, 9, 19 }, { -15, 9, 20 }, { -9, 9, 20 }, { -3, 9, 20 }, { 3, 9, 21 }, { 9, 9, 21 }, { 15, 9, 20 }, { 21, 9, 17 }, { -21, 3, 20 }, { -15, 3, 16 }, { -9, 3, 21 }, { -3, 3, 22 }, { 3, 3, 22 }, { 9, 3, 22 }, { 15, 3, 21 }, { 21, 3, 18 }, { 27, 3, 15 }, { -21, -3, 20 }, { -9, -3, 23 }, { -3, -3, 24 }, { 3, -3, 23 }, { 9, -3, 23 }, { 15, -3, 21 }, { 21, -3, 18 }, { 27, -3, 15 }, { -21, -9, 19 }, { -15, -9, 20 }, { -9, -9, 21 }, { -3, -9, 22 }, { 3, -9, 22 }, { 9, -9, 21 }, { 15, -9, 21 }, { 21, -9, 17 }, { -15, -15, 20 }, { -9, -15, 21 }, { -3, -15, 20 }, { 3, -15, 20 }, { 9, -15, 20 }, { 15, -15, 19 }, { -9, -21, 18 }, { -3, -21, 18 }, { 3, -21, 18 }, { 9, -21, 17 } };
        grid_TanzaniaWalkIn_right_xy = new double[,] { { -9, 21, 16 }, { -3, 21, 16 }, { 3, 21, 16 }, { 9, 21, 16 }, { -15, 15, 18 }, { -9, 15, 20 }, { -3, 15, 19 }, { 3, 15, 18 }, { 9, 15, 19 }, { 15, 15, 18 }, { -21, 9, 17 }, { -15, 9, 20 }, { -9, 9, 21 }, { -3, 9, 21 }, { 3, 9, 21 }, { 9, 9, 20 }, { 15, 9, 20 }, { 21, 9, 19 }, { -27, 3, 15 }, { -21, 3, 18 }, { -15, 3, 21 }, { -9, 3, 22 }, { -3, 3, 23 }, { 3, 3, 23 }, { 9, 3, 22 }, { 15, 3, 16 }, { 21, 3, 20 }, { -27, -3, 15 }, { -21, -3, 18 }, { -15, -3, 21 }, { -9, -3, 23 }, { -3, -3, 24 }, { 3, -3, 24 }, { 9, -3, 23 }, { 21, -3, 20 }, { -21, -9, 18 }, { -15, -9, 21 }, { -9, -9, 21 }, { -3, -9, 22 }, { 3, -9, 22 }, { 9, -9, 21 }, { 15, -9, 20 }, { 21, -9, 20 }, { -15, -15, 19 }, { -9, -15, 21 }, { -3, -15, 21 }, { 3, -15, 21 }, { 9, -15, 21 }, { 15, -15, 20 }, { -9, -21, 17 }, { -3, -21, 18 }, { 3, -21, 19 }, { 9, -21, 19 } };
        // mild, moderate, severe timecourse
        grid_PID1269767_mild_left_xy = new double[,] { { -9, 21, 23 }, { -3, 21, 23 }, { 3, 21, 26 }, { 9, 21, 25 }, { -15, 15, 28 }, { -9, 15, 28 }, { -3, 15, 28 }, { 3, 15, 28 }, { 9, 15, 27 }, { 15, 15, 24 }, { -21, 9, 26 }, { -15, 9, 29 }, { -9, 9, 26 }, { -3, 9, 29 }, { 3, 9, 27 }, { 9, 9, 27 }, { 15, 9, 16 }, { 21, 9, 10 }, { -21, 3, 27 }, { -15, 3, 26 }, { -9, 3, 28 }, { -3, 3, 30 }, { 3, 3, 26 }, { 9, 3, 9 }, { 15, 3, 18 }, { 21, 3, 8 }, { 27, 3, 5 }, { -21, -3, 27 }, { -9, -3, 30 }, { -3, -3, 31 }, { 3, -3, 29 }, { 9, -3, 29 }, { 15, -3, 28 }, { 21, -3, 23 }, { 27, -3, 16 }, { -21, -9, 26 }, { -15, -9, 27 }, { -9, -9, 29 }, { -3, -9, 28 }, { 3, -9, 27 }, { 9, -9, 28 }, { 15, -9, 26 }, { 21, -9, 25 }, { -15, -15, 25 }, { -9, -15, 27 }, { -3, -15, 28 }, { 3, -15, 25 }, { 9, -15, 28 }, { 15, -15, 27 }, { -9, -21, 23 }, { -3, -21, 26 }, { 3, -21, 26 }, { 9, -21, 25 } };
        grid_PID1269767_mild_right_xy = new double[,] { { -9, 21, 27 }, { -3, 21, 23 }, { 3, 21, 25 }, { 9, 21, 24 }, { -15, 15, 27 }, { -9, 15, 25 }, { -3, 15, 25 }, { 3, 15, 27 }, { 9, 15, 26 }, { 15, 15, 24 }, { -21, 9, 20 }, { -15, 9, 25 }, { -9, 9, 25 }, { -3, 9, 25 }, { 3, 9, 26 }, { 9, 9, 23 }, { 15, 9, 26 }, { 21, 9, 26 }, { -27, 3, 15 }, { -21, 3, 20 }, { -15, 3, 24 }, { -9, 3, 15 }, { -3, 3, 0 }, { 3, 3, 26 }, { 9, 3, 27 }, { 15, 3, 10 }, { 21, 3, 25 }, { -27, -3, 23 }, { -21, -3, 25 }, { -15, -3, 30 }, { -9, -3, 31 }, { -3, -3, 29 }, { 3, -3, 30 }, { 9, -3, 29 }, { 21, -3, 26 }, { -21, -9, 25 }, { -15, -9, 29 }, { -9, -9, 31 }, { -3, -9, 30 }, { 3, -9, 31 }, { 9, -9, 31 }, { 15, -9, 29 }, { 21, -9, 29 }, { -15, -15, 27 }, { -9, -15, 30 }, { -3, -15, 31 }, { 3, -15, 31 }, { 9, -15, 29 }, { 15, -15, 30 }, { -9, -21, 26 }, { -3, -21, 29 }, { 3, -21, 30 }, { 9, -21, 30 } };
        grid_PID1269767_moderate_left_xy = new double[,] { { -9, 21, 23 }, { -3, 21, 13 }, { 3, 21, 6 }, { 9, 21, 7 }, { -15, 15, 19 }, { -9, 15, 19 }, { -3, 15, 9 }, { 3, 15, 7 }, { 9, 15, 4 }, { 15, 15, 0 }, { -21, 9, 27 }, { -15, 9, 8 }, { -9, 9, 0 }, { -3, 9, 0 }, { 3, 9, 0 }, { 9, 9, 0 }, { 15, 9, 0 }, { 21, 9, 0 }, { -21, 3, 23 }, { -15, 3, 0 }, { -9, 3, 23 }, { -3, 3, 9 }, { 3, 3, 10 }, { 9, 3, 8 }, { 15, 3, 0 }, { 21, 3, 0 }, { 27, 3, 0 }, { -21, -3, 23 }, { -9, -3, 31 }, { -3, -3, 30 }, { 3, -3, 29 }, { 9, -3, 28 }, { 15, -3, 17 }, { 21, -3, 14 }, { 27, -3, 18 }, { -21, -9, 19 }, { -15, -9, 27 }, { -9, -9, 30 }, { -3, -9, 28 }, { 3, -9, 25 }, { 9, -9, 25 }, { 15, -9, 26 }, { 21, -9, 24 }, { -15, -15, 23 }, { -9, -15, 29 }, { -3, -15, 24 }, { 3, -15, 23 }, { 9, -15, 21 }, { 15, -15, 19 }, { -9, -21, 21 }, { -3, -21, 16 }, { 3, -21, 15 }, { 9, -21, 19 } };
        grid_PID1269767_moderate_right_xy = new double[,] { { -9, 21, 0 }, { -3, 21, 0 }, { 3, 21, 20 }, { 9, 21, 23 }, { -15, 15, 0 }, { -9, 15, 0 }, { -3, 15, 0 }, { 3, 15, 0 }, { 9, 15, 21 }, { 15, 15, 17 }, { -21, 9, 0 }, { -15, 9, 0 }, { -9, 9, 0 }, { -3, 9, 0 }, { 3, 9, 0 }, { 9, 9, 0 }, { 15, 9, 14 }, { 21, 9, 13 }, { -27, 3, 3 }, { -21, 3, 0 }, { -15, 3, 0 }, { -9, 3, 0 }, { -3, 3, 0 }, { 3, 3, 12 }, { 9, 3, 14 }, { 15, 3, 21 }, { 21, 3, 23 }, { -27, -3, 20 }, { -21, -3, 26 }, { -15, -3, 27 }, { -9, -3, 28 }, { -3, -3, 30 }, { 3, -3, 30 }, { 9, -3, 27 }, { 21, -3, 28 }, { -21, -9, 24 }, { -15, -9, 29 }, { -9, -9, 29 }, { -3, -9, 30 }, { 3, -9, 30 }, { 9, -9, 28 }, { 15, -9, 27 }, { 21, -9, 27 }, { -15, -15, 28 }, { -9, -15, 29 }, { -3, -15, 30 }, { 3, -15, 29 }, { 9, -15, 28 }, { 15, -15, 28 }, { -9, -21, 22 }, { -3, -21, 27 }, { 3, -21, 28 }, { 9, -21, 28 } };
        grid_PID1269767_severe_left_xy = new double[,] { { -9, 21, 0 }, { -3, 21, 0 }, { 3, 21, 0 }, { 9, 21, 0 }, { -15, 15, 7 }, { -9, 15, 0 }, { -3, 15, 0 }, { 3, 15, 0 }, { 9, 15, 0 }, { 15, 15, 0 }, { -21, 9, 5 }, { -15, 9, 0 }, { -9, 9, 0 }, { -3, 9, 0 }, { 3, 9, 0 }, { 9, 9, 0 }, { 15, 9, 0 }, { 21, 9, 0 }, { -21, 3, 10 }, { -15, 3, 0 }, { -9, 3, 2 }, { -3, 3, 3 }, { 3, 3, 8 }, { 9, 3, 3 }, { 15, 3, 0 }, { 21, 3, 0 }, { 27, 3, 0 }, { -21, -3, 0 }, { -9, -3, 31 }, { -3, -3, 27 }, { 3, -3, 27 }, { 9, -3, 13 }, { 15, -3, 0 }, { 21, -3, 0 }, { 27, -3, 0 }, { -21, -9, 0 }, { -15, -9, 12 }, { -9, -9, 22 }, { -3, -9, 21 }, { 3, -9, 14 }, { 9, -9, 9 }, { 15, -9, 0 }, { 21, -9, 0 }, { -15, -15, 0 }, { -9, -15, 0 }, { -3, -15, 5 }, { 3, -15, 5 }, { 9, -15, 0 }, { 15, -15, 0 }, { -9, -21, 8 }, { -3, -21, 0 }, { 3, -21, 0 }, { 9, -21, 0 } };
        grid_PID1269767_severe_right_xy = new double[,] { { -9, 21, 0 }, { -3, 21, 0 }, { 3, 21, 0 }, { 9, 21, 6 }, { -15, 15, 0 }, { -9, 15, 0 }, { -3, 15, 2 }, { 3, 15, 0 }, { 9, 15, 3 }, { 15, 15, 2 }, { -21, 9, 0 }, { -15, 9, 0 }, { -9, 9, 0 }, { -3, 9, 0 }, { 3, 9, 0 }, { 9, 9, 0 }, { 15, 9, 0 }, { 21, 9, 23 }, { -27, 3, 0 }, { -21, 3, 0 }, { -15, 3, 0 }, { -9, 3, 2 }, { -3, 3, 0 }, { 3, 3, 28 }, { 9, 3, 3 }, { 15, 3, 0 }, { 21, 3, 21 }, { -27, -3, 21 }, { -21, -3, 26 }, { -15, -3, 28 }, { -9, -3, 29 }, { -3, -3, 29 }, { 3, -3, 30 }, { 9, -3, 28 }, { 21, -3, 17 }, { -21, -9, 25 }, { -15, -9, 27 }, { -9, -9, 28 }, { -3, -9, 30 }, { 3, -9, 28 }, { 9, -9, 28 }, { 15, -9, 25 }, { 21, -9, 31 }, { -15, -15, 24 }, { -9, -15, 24 }, { -3, -15, 28 }, { 3, -15, 28 }, { 9, -15, 27 }, { 15, -15, 26 }, { -9, -21, 21 }, { -3, -21, 24 }, { 3, -21, 24 }, { 9, -21, 24 } };

		// flip superior vertically to make inferior
		grid_inferior_left_xy = new double[grid_superior_left_xy.GetLength (0), 3];
		grid_inferior_right_xy = new double[grid_superior_left_xy.GetLength (0), 3];
		for (int i = 0; i < grid_superior_left_xy.GetLength (0); i++) {
			grid_inferior_left_xy [i, 0] = grid_superior_left_xy [i, 0];
			grid_inferior_left_xy [i, 1] = -grid_superior_left_xy [i, 1];
			grid_inferior_left_xy [i, 2] = grid_superior_left_xy [i, 2];
			grid_inferior_right_xy[i, 0] = grid_superior_right_xy [i, 0];
			grid_inferior_right_xy[i, 1] = -grid_superior_right_xy [i, 1];
			grid_inferior_right_xy[i, 2] = grid_superior_right_xy [i, 2];
		}

        // init impairment
        SetImpairment(FieldLossType.None);
    }

    IEnumerator SetGUIText(String txt)
    {
        Debug.Log(txt);
        //TextLeft.GetComponent<UnityEngine.UI.Text>().text = txt;
        //TextRight.GetComponent<UnityEngine.UI.Text>().text = txt;
        yield return new WaitForSeconds(5);
        //TextLeft.GetComponent<UnityEngine.UI.Text>().text = "";
        //TextRight.GetComponent<UnityEngine.UI.Text>().text = "";
    }

    public void Update()
    {
        if (Input.GetKeyDown("1")) {
            StartCoroutine(SetGUIText("Mild Glaucoma (Patient X: 2 years)"));
            SetImpairment(FieldLossType.Timecourse_mild);
        } else if (Input.GetKeyDown("2")) {
            StartCoroutine(SetGUIText("Moderate Glaucoma (Patient X: 4 years)"));
            SetImpairment(FieldLossType.Timecourse_moderate);
        } else if (Input.GetKeyDown("3")) {
            StartCoroutine(SetGUIText("Severe Glaucoma (Patient X: 6 years)"));
            SetImpairment(FieldLossType.Timecourse_severe);
        } else if (Input.GetKeyDown("4")) {
            StartCoroutine(SetGUIText("Average UK Patient at point of detection"));
            SetImpairment(FieldLossType.UK_walk_in);
        } else if (Input.GetKeyDown("5")) {
            StartCoroutine(SetGUIText("Average Tanzania Patient at point of detection"));
            SetImpairment(FieldLossType.Tanzania_walk_in);
        } else if (Input.GetKeyDown("6")) {
            StartCoroutine(SetGUIText("TS_Superior"));
            SetImpairment(FieldLossType.TS_Superior);
        } else if (Input.GetKeyDown("7")) {
            StartCoroutine(SetGUIText("TS_Inferior"));
            SetImpairment(FieldLossType.TS_Inferior);
        } else if (Input.GetKeyDown("8")) {
            StartCoroutine(SetGUIText("Whole field blur: Severe"));
            leftEyeVIindex = 0.4f;
            rightEyeVIindex = leftEyeVIindex;
            SetImpairment(FieldLossType.Uniform);
        } else if (Input.GetKeyDown("9")) {
            StartCoroutine(SetGUIText("Central vision loss (AMD)"));
            leftEyeVIindex = 0.7f;
            SetImpairment(FieldLossType.Central);
        } else if (Input.GetKeyDown("0")) {
            StartCoroutine(SetGUIText("No Impairment"));
            SetImpairment(FieldLossType.None);
        }
    }

    private void SetImpairment(FieldLossType fieldLossType)
    {
		// Set/update impairment
		if (fieldLossType==prevGridType && prevGridLevel_LE==leftEyeVIindex && prevGridLevel_RE==rightEyeVIindex)
		{
			// nothing has changed, do nothing!
		} else {
            // record details for future reference
            prevGridType = fieldLossType;
            prevGridLevel_LE = leftEyeVIindex;
            prevGridLevel_RE = rightEyeVIindex;

            // Get grid
            double[,] grid_LE;
			double[,] grid_RE;
			switch (fieldLossType) {
			case FieldLossType.Uniform:
				grid_LE = new double[grid_basic_xy.GetLength (0), 3];
				grid_RE = new double[grid_basic_xy.GetLength (0), 3];
				for (int i = 0; i < grid_basic_xy.GetLength(0); i++) {
					// set location
					grid_LE [i,0] = grid_basic_xy[i, 0];
					grid_LE [i,1] = grid_basic_xy[i, 1];
					grid_RE [i,0] = grid_basic_xy[i, 0];
					grid_RE [i,1] = grid_basic_xy[i, 1];
					// set value
					float val = 1;
					grid_LE [i,2] = GridInterpolator.ffMin * leftEyeVIindex * val;
					grid_RE [i,2] = GridInterpolator.ffMin * rightEyeVIindex * val;
				}
				break;
			case FieldLossType.Central:
				grid_LE = new double[grid_basic_xy.GetLength (0), 3];
				grid_RE = new double[grid_basic_xy.GetLength (0), 3];
				for (int i = 0; i < grid_basic_xy.GetLength(0); i++) {
					// set location
					grid_LE [i,0] = grid_basic_xy[i, 0];
					grid_LE [i,1] = grid_basic_xy[i, 1];
					grid_RE [i,0] = grid_basic_xy[i, 0];
					grid_RE [i,1] = grid_basic_xy[i, 1];
					// set value
					float d = Mathf.Sqrt (Mathf.Pow ((float)grid_basic_xy[i, 0], 2) + Mathf.Pow ((float)grid_basic_xy[i, 1], 2));
					//float val = (d <= (9 * 0.95)) ? 1 : 0; // 9 to correspond to macular; 0.95 since the fove FOV is 95 degrees (correction validated based on physiological localization of the blindspot using the gazeContingentBlob script)
					float val = d <= 16 ? 1 : 0; // 9 to correspond to the half-angle of the macula: (google image: visual field degrees macula paracentral)
					grid_LE [i,2] = GridInterpolator.ffMin * leftEyeVIindex * val;
					grid_RE [i,2] = GridInterpolator.ffMin * rightEyeVIindex * val;
				}
				break;
			case FieldLossType.Peripheral:
				grid_LE = new double[grid_basic_xy.GetLength (0), 3];
				grid_RE = new double[grid_basic_xy.GetLength (0), 3];
				for (int i = 0; i < grid_basic_xy.GetLength(0); i++) {
					// set location
					grid_LE [i,0] = grid_basic_xy[i, 0];
					grid_LE [i,1] = grid_basic_xy[i, 1];
					grid_RE [i,0] = grid_basic_xy[i, 0];
					grid_RE [i,1] = grid_basic_xy[i, 1];
					// set value
					float d = Mathf.Sqrt (Mathf.Pow ((float)grid_basic_xy[i, 0], 2) + Mathf.Pow ((float)grid_basic_xy[i, 1], 2));
					//float val = (d <= (9 * 0.95)) ? 1 : 0; // 9 to correspond to macular; 0.95 since the fove FOV is 95 degrees (correction validated based on physiological localization of the blindspot using the gazeContingentBlob script)
					float val = d > 9 ? 1 : 0; // 9 to correspond to the half-angle of the macula: (google image: visual field degrees macula paracentral)
					grid_LE [i,2] = GridInterpolator.ffMin * leftEyeVIindex * val;
					grid_RE [i,2] = GridInterpolator.ffMin * rightEyeVIindex * val;
				}
				break;
			case FieldLossType.Blindspot:
				grid_LE = grid_blindspot_left_xy;
				grid_RE = grid_blindspot_right_xy;
				break;
			case FieldLossType.TS_Superior:
				grid_LE = new double[grid_superior_left_xy.GetLength (0), 3];
				grid_RE = new double[grid_superior_right_xy.GetLength (0), 3];
				for (int i = 0; i < grid_superior_left_xy.GetLength(0); i++) {
					// set location
					grid_LE [i,0] = grid_superior_left_xy[i, 0];
					grid_LE [i,1] = grid_superior_left_xy[i, 1];
					grid_RE [i,0] = grid_superior_right_xy[i, 0];
					grid_RE [i,1] = grid_superior_right_xy[i, 1];
                    // set value
                    grid_LE[i, 2] = grid_superior_left_xy[i, 2] + GridInterpolator.ffMin; // hack: should be relative to normative
                    grid_RE[i, 2] = grid_superior_right_xy[i, 2] + GridInterpolator.ffMin;
                }
				break;
			case FieldLossType.TS_Inferior:
				grid_LE = new double[grid_inferior_left_xy.GetLength (0), 3];
				grid_RE = new double[grid_inferior_right_xy.GetLength (0), 3];
				for (int i = 0; i < grid_inferior_left_xy.GetLength(0); i++) {
					// set location
					grid_LE [i,0] = grid_inferior_left_xy[i, 0];
					grid_LE [i,1] = grid_inferior_left_xy[i, 1];
					grid_RE [i,0] = grid_inferior_right_xy[i, 0];
					grid_RE [i,1] = grid_inferior_right_xy[i, 1];
					// set value
					grid_LE [i,2] = grid_inferior_left_xy [i,2] + GridInterpolator.ffMin; // hack: should be relative to normative
                    grid_RE [i,2] = grid_inferior_right_xy [i,2] + GridInterpolator.ffMin;
                    }
				break;
            case FieldLossType.UK_walk_in:
                    grid_LE = new double[grid_UKwalkIn_left_xy.GetLength(0), 3];
                    grid_RE = new double[grid_UKwalkIn_right_xy.GetLength(0), 3];
                    for (int i = 0; i < grid_UKwalkIn_left_xy.GetLength(0); i++)
                    {
                        // set location
                        grid_LE[i, 0] = grid_UKwalkIn_left_xy[i, 0];
                        grid_LE[i, 1] = grid_UKwalkIn_left_xy[i, 1];
                        grid_RE[i, 0] = grid_UKwalkIn_right_xy[i, 0];
                        grid_RE[i, 1] = grid_UKwalkIn_right_xy[i, 1];
                        // set value
                        grid_LE[i, 2] = grid_UKwalkIn_left_xy[i, 2] + GridInterpolator.ffMin; // hack: should be relative to normative
                        grid_RE[i, 2] = grid_UKwalkIn_right_xy[i, 2] + GridInterpolator.ffMin;
                    }
                    break;
            case FieldLossType.Tanzania_walk_in:
                    grid_LE = new double[grid_TanzaniaWalkIn_left_xy.GetLength(0), 3];
                    grid_RE = new double[grid_TanzaniaWalkIn_right_xy.GetLength(0), 3];
                    for (int i = 0; i < grid_TanzaniaWalkIn_left_xy.GetLength(0); i++)
                    {
                        // set location
                        grid_LE[i, 0] = grid_TanzaniaWalkIn_left_xy[i, 0];
                        grid_LE[i, 1] = grid_TanzaniaWalkIn_left_xy[i, 1];
                        grid_RE[i, 0] = grid_TanzaniaWalkIn_right_xy[i, 0];
                        grid_RE[i, 1] = grid_TanzaniaWalkIn_right_xy[i, 1];
                        // set value
                        grid_LE[i, 2] = grid_TanzaniaWalkIn_left_xy[i, 2] + GridInterpolator.ffMin; // hack: should be relative to normative
                        grid_RE[i, 2] = grid_TanzaniaWalkIn_right_xy[i, 2] + GridInterpolator.ffMin;
                    }
                    break;
                case FieldLossType.Timecourse_mild:
                    grid_LE = new double[grid_PID1269767_mild_left_xy.GetLength(0), 3];
                    grid_RE = new double[grid_PID1269767_mild_right_xy.GetLength(0), 3];
                    for (int i = 0; i < grid_PID1269767_mild_left_xy.GetLength(0); i++)
                    {
                        // set location
                        grid_LE[i, 0] = grid_PID1269767_mild_left_xy[i, 0];
                        grid_LE[i, 1] = grid_PID1269767_mild_left_xy[i, 1];
                        grid_RE[i, 0] = grid_PID1269767_mild_right_xy[i, 0];
                        grid_RE[i, 1] = grid_PID1269767_mild_right_xy[i, 1];
                        // set value
                        grid_LE[i, 2] = grid_PID1269767_mild_left_xy[i, 2] + GridInterpolator.ffMin; // hack: should be relative to normative
                        grid_RE[i, 2] = grid_PID1269767_mild_right_xy[i, 2] + GridInterpolator.ffMin;
                    }
                    break;
                case FieldLossType.Timecourse_moderate:
                    grid_LE = new double[grid_PID1269767_moderate_left_xy.GetLength(0), 3];
                    grid_RE = new double[grid_PID1269767_moderate_right_xy.GetLength(0), 3];
                    for (int i = 0; i < grid_PID1269767_moderate_left_xy.GetLength(0); i++)
                    {
                        // set location
                        grid_LE[i, 0] = grid_PID1269767_moderate_left_xy[i, 0];
                        grid_LE[i, 1] = grid_PID1269767_moderate_left_xy[i, 1];
                        grid_RE[i, 0] = grid_PID1269767_moderate_right_xy[i, 0];
                        grid_RE[i, 1] = grid_PID1269767_moderate_right_xy[i, 1];
                        // set value
                        grid_LE[i, 2] = grid_PID1269767_moderate_left_xy[i, 2] + GridInterpolator.ffMin; // hack: should be relative to normative
                        grid_RE[i, 2] = grid_PID1269767_moderate_right_xy[i, 2] + GridInterpolator.ffMin;
                    }
                    break;
                case FieldLossType.Timecourse_severe:
                    grid_LE = new double[grid_PID1269767_severe_left_xy.GetLength(0), 3];
                    grid_RE = new double[grid_PID1269767_severe_right_xy.GetLength(0), 3];
                    for (int i = 0; i < grid_PID1269767_severe_left_xy.GetLength(0); i++)
                    {
                        // set location
                        grid_LE[i, 0] = grid_PID1269767_severe_left_xy[i, 0];
                        grid_LE[i, 1] = grid_PID1269767_severe_left_xy[i, 1];
                        grid_RE[i, 0] = grid_PID1269767_severe_right_xy[i, 0];
                        grid_RE[i, 1] = grid_PID1269767_severe_right_xy[i, 1];
                        // set value
                        grid_LE[i, 2] = grid_PID1269767_severe_left_xy[i, 2] + GridInterpolator.ffMin; // hack: should be relative to normative
                        grid_RE[i, 2] = grid_PID1269767_severe_right_xy[i, 2] + GridInterpolator.ffMin;
                    }
                    break;
                case FieldLossType.None:
                    myFieldLoss_left.enabled = false;
                    myFieldLoss_right.enabled = false;
                    myInpainter_left.enabled = false;
                    myInpainter_right.enabled = false;
                    return;
                default:
				    throw new Exception(String.Format("Unknown field type: {0}", fieldLossType.ToString()));
				    break;
			};

			// set grid
			myFieldLoss_left.setGrid(grid_LE, false); // true to attempt extrapolation
			myFieldLoss_right.setGrid(grid_RE, false);
            //myFieldLoss_left.setGrid(grid_LE, false);
            //myFieldLoss_right.setGrid(grid_RE, false);

            // turn on VI
            Debug.Log("enabling...");
            myFieldLoss_left.enabled = true;
            myFieldLoss_right.enabled = true;
            if (fieldLossType == FieldLossType.Central)
            {
                myInpainter_left.enabled = true;
                myInpainter_right.enabled = true;
            } else
            {
                myInpainter_left.enabled = false;
                myInpainter_right.enabled = false;
            }


        }

    }
}