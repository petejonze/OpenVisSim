using System;
using System.Collections;
using UnityEngine;

namespace VisSim
{
    
    public class myFloaters : LinkableBaseEffect
    {
        [TweakableMember(0.0f, 1.0f, "Intensity", "Vitreous Floaters")]
        [Linkable, Range(0.0f, 1.0f)]
        public float intensity = 1.0f;

        [TweakableMember(1, 3, "Size", "Vitreous Floaters")]
        [Linkable, Range(1, 3)]
        public int floaterSize = 1;

        [TweakableMember(0.0f, 1.0f, "Density", "Vitreous Floaters")]
        [Linkable, Range(0.0f, 1.0f)]
        public float floaterDensity = 0.5f;

        // Shader params
        public Texture2D texture = null;

        // Scintillation params
        private float ScStrength = 1f;
        private float ScLumContribution = 1f;

        public enum FloaterType
        {
            Dark = 0,
            Light = 1,
            Scintillating = 2,
        }
        [Linkable, TweakableMember(0, 1, "Density", "Vitreous Floaters")]
        public FloaterType floaterType = FloaterType.Dark;
        [TweakableMember(0f, 3f, "Speed", "Vitreous Floaters")]
        [Linkable, Range(0.0f, 3.0f), Tooltip("Wave animation speed.")]
        public float Speed = 1f;
        [Linkable, Tooltip("Wave frequency (higher means more waves).")]
        private float Frequency = 12f;
        [Linkable, Tooltip("Wave amplitude (higher means bigger waves).")]
        private float Amplitude = 0.01f;

        // internal
        private float Timer = 0f;
        private int _old_floaterSize = 1;
        private float _old_floaterDensity = 0.5f;

        // cellular automata (Game of Life) params
        private static bool[,] golBoard; // Holds the current state of the board.
        private static int golBoardWidth = 256; // The width of the board in n-cells.
        private static int golBoardHeight = 256; // The height of the board in n-cells.
        private static bool golLoopEdges = false; // True if cell rules can loop around edges.
        private static float[,] smoothedBoard; // Holds the current state of the board.
        
        public new void OnEnable()
        {
            // init floater texture
            generateOverlayTexture();

            // call base method to enable effect
            base.OnEnable();
        }

        // Update is called once per frame
		protected override void OnUpdate()
        {
            // Reset the timer after a while, some GPUs don't like big numbers
            if (Timer > 1000f)
                Timer -= 1000f;

            // Increment timer
            Timer += Speed * Time.deltaTime;
        }

        // Called by camera to apply image effect
        protected override void OnRenderImage (RenderTexture source, RenderTexture destination)
		{
            Vector4 UV_Transform = new  Vector4(1, 0, 0, 1);

            #if UNITY_WP8
	    	    // WP8 has no OS support for rotating screen with device orientation,
	    	    // so we do those transformations ourselves.
			    if (Screen.orientation == ScreenOrientation.LandscapeLeft) {
				    UV_Transform = new Vector4(0, -1, 1, 0);
			    }
			    if (Screen.orientation == ScreenOrientation.LandscapeRight) {
				    UV_Transform = new Vector4(0, 1, -1, 0);
			    }
			    if (Screen.orientation == ScreenOrientation.PortraitUpsideDown) {
				    UV_Transform = new Vector4(-1, 0, 0, -1);
			    }
            #endif
            
            if (floaterSize != _old_floaterSize || floaterDensity != _old_floaterDensity)
            {
                generateOverlayTexture();
            }

            // set params
            Material.SetVector("_UV_Transform", UV_Transform);
            Material.SetFloat ("_Intensity", intensity);
            Material.SetTexture ("_Overlay", texture);
            Material.SetVector("_WarpParams", new Vector3(Frequency, Amplitude, Timer));

            // update params
            Material.SetVector("_ScintillateParams", new Vector3(Timer, ScStrength, ScLumContribution));

            // Blit
            switch (floaterType)
            {
                case FloaterType.Dark:
                    Graphics.Blit(source, destination, Material, 0);
                    break;
                case FloaterType.Light:
                    Graphics.Blit(source, destination, Material, 1);
                    break;
                case FloaterType.Scintillating:
                    Graphics.Blit(source, destination, Material, 2);
                    break;
                default:
                    Console.WriteLine("??????");
                    break;
            };
        }





        // -----------------------------------------------------------------------------
        // GENERATING VITREOUS-FLOATERS OVERLAY TEXTURE
        // -----------------------------------------------------------------------------

        private void generateOverlayTexture()
        {
            Debug.Log("Initialising Game Of Life" + UnityEngine.Random.Range(-10.0f, 10.0f));

            // generate golBoard
            runGameOfLife();

            // use golBoard to generate a texture
            var tex = new Texture2D(golBoardWidth, golBoardHeight, TextureFormat.RGB24, false); // Create a new texture RGB24 (24 bit without alpha) and no mipmaps
            Color[] imgMatrix = new Color[golBoardWidth * golBoardHeight];
            for (int x = 0; x < golBoardWidth; x++)
            {
                for (int y = 0; y < golBoardHeight; y++)
                {
                    imgMatrix[x + y * golBoardWidth] = golBoard[x, y] ? Color.black : Color.white;
                }
            }
            tex.SetPixels(imgMatrix);
            tex.Apply(false); // actually apply all SetPixels, don't recalculate mip levels

            // smooth and set
            Material material = new Material(Shader.Find("Hidden/BlurEffectConeTap"));
            RenderTexture rt = RenderTexture.GetTemporary(64, 64); // downsample to blur
            Graphics.Blit(tex, rt, material);
            // Create a new Texture2D and read the RenderTexture image into it
            RenderTexture.active = rt; // Set the supplied RenderTexture as the active one
            texture = new Texture2D(rt.width, rt.height);
            texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            texture.Apply(false);

            // if in editor store values so know whether effect has been updated
            _old_floaterSize = floaterSize;
            _old_floaterDensity = floaterDensity;

            // done
            Debug.Log("Game Of Life Ready!" + UnityEngine.Random.Range(-10.0f, 10.0f));
        }

        // logic copied from
        // c# code modelled on https://rosettacode.org/wiki/Conway%27s_Game_of_Life#C.23
        private void runGameOfLife()
        {
            // init params
            int nPhase1iterations = 1;
            int nPhase2iterations = 2;

            // Generate a  new randomly seeded map
            initializeRandomBoard();

            // Phase 1
            for (var i = 0; i < nPhase1iterations; i++)
            {
                updateBoard1();
            }

            // Phase 2
            for (var i = 0; i < nPhase2iterations; i++)
            {
                updateBoard2();
            }
        }

        // Creates the initial board with a random state.
        private void initializeRandomBoard()
        {
            // compute param(s) based on user input
            double probCellStartsOn = 0.0075 + (0.02 - 0.0075) * floaterDensity; // 0.0075 to 0.02[Controlling Density]

            // init
            var rand = new System.Random();

            // iterate through board, turning on cells randomly
            golBoard = new bool[golBoardWidth, golBoardHeight];
            for (var y = 0; y < golBoardHeight; y++)
            {
                for (var x = 0; x < golBoardWidth; x++)
                {
                    golBoard[x, y] = rand.NextDouble() < probCellStartsOn; // A double-precision floating point number that is greater than or equal to 0.0, and less than 1.0
                }
            }
        }

        // Moves the board to the next state based on Conway's rules.
        private void updateBoard1()
        {
            // compute param(s) based on user input
            int padCheckEmpty = floaterSize + 1;

            // A temp variable to hold the next state while it's being calculated.
            bool[,] newBoard = new bool[golBoardWidth, golBoardHeight];

            for (var y = 0; y < golBoardHeight; y++)
            {
                for (var x = 0; x < golBoardWidth; x++)
                {
                    var nSurroundingOn1 = countLiveNeighbors(x, y, 1);
                    var nSurroundingOn2 = countLiveNeighbors(x, y, padCheckEmpty);
                    newBoard[x, y] = (nSurroundingOn1 >= 5) || (nSurroundingOn2 <= 1);
                }
            }

            // Set the board to its new state.
            golBoard = newBoard;
        }

        // Moves the board to the next state based on Conway's rules.
        private void updateBoard2()
        {
            // A temp variable to hold the next state while it's being calculated.
            bool[,] newBoard = new bool[golBoardWidth, golBoardHeight];

            for (var y = 0; y < golBoardHeight; y++)
            {
                for (var x = 0; x < golBoardWidth; x++)
                {
                    var nSurroundingOn1 = countLiveNeighbors(x, y, 1);
                    newBoard[x, y] = nSurroundingOn1 >= 5;
                }
            }

            // Set the board to its new state.
            golBoard = newBoard;
        }

        // smooth
        //private static readonly int[] kerneli = { -3, -2, -1, 0, 1, 2, 3};
        //private static readonly float[] kernel = { 0.005f, 0.054f, 0.242f, 0.398f, 0.242f, 0.054f, 0.005f };
        //private static readonly int[] kerneli = { -6, -5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5, 6 };
        //private static readonly float[] kernel = { 0.002f, 0.009f, 0.027f, 0.065f, 0.121f, 0.176f, 0.200f, 0.176f, 0.121f, 0.065f, 0.027f, 0.009f, 0.002f };
        
        // Returns the number of live neighbors around the cell at position (x,y).
        // by default, n==1
        private int countLiveNeighbors(int x, int y, int n)
        {
            // The number of live neighbors.
            int value = 0;
            int nvalues = 0;

            // This nested loop enumerates the 9 cells in the specified cells neighborhood.
            for (var j = -n; j <= n; j++)
            {
                // If loopEdges is set to false and y+j is off the board, continue.
                if (!golLoopEdges && y + j < 0 || y + j >= golBoardHeight)
                {
                    continue;
                }

                // Loop around the edges if y+j is off the board.
                int k = (y + j + golBoardHeight) % golBoardHeight;

                for (var i = -n; i <= n; i++)
                {
                    // If loopEdges is set to false and x+i is off the board, continue.
                    if (!golLoopEdges && x + i < 0 || x + i >= golBoardWidth)
                    {
                        continue;
                    }

                    // Loop around the edges if x+i is off the board.
                    int h = (x + i + golBoardWidth) % golBoardWidth;

                    // Count the neighbor cell at (h,k) if it is alive.
                    value += golBoard[h, k] ? 1 : 0;
                    nvalues++;
                }
            }

            // Subtract 1 if (x,y) is alive since we (erroneously) counted it as a neighbor in the above.
            value -= (golBoard[x, y] ? 1 : 0);
            // HACK!
            if (nvalues <= 5)
            {
                value = 9;
            }
            return value;
        }
		
		protected override string GetShaderName()
		{
			return "Hidden/VisSim/myFloaters";
		}
    }
}