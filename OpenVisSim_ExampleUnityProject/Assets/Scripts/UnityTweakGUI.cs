using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityStandardAssets.ImageEffects;
using VisSim;
using ZXing;
using ZXing.QrCode;

///<summary>
/// A simple in-game GUI for Unity that allows tweaking of script fields 
/// and properties marked with [TweakableMember]
///</summary>
public class UnityTweakGUI : MonoBehaviour {

    public Transform[] targetObjects;
    public int winWidth = 350;
    public int winHeight = 500;
    public bool expanded = true;

    private Dictionary<string,List<TweakableParam>> groupParamsMap;
    private string[] sortedGroups;

    private Vector2 scrollPosition;
    private Rect windowRect;

    private int headerHeight = 20;
    private int expandToggleWidth = 75;

    // main GUI formatting
    private GUIStyle groupTextStyle;
    private GUIStyle labelStyle;
    private GUILayoutOption[] labelLayoutOptions = { GUILayout.MaxWidth(70.0f), GUILayout.MinWidth(10.0f) }; // PJ: ensure lefthand labels don't overflow
    private GUIStyle tickboxStyle;
    private GUIStyle enumStyle;
    private GUILayoutOption[] enumLayoutOptions = { GUILayout.MaxWidth(225.0f), GUILayout.MinWidth(100.0f) };
    private GUILayoutOption[] visFieldButtonOptions = { GUILayout.MaxHeight(50.0f), GUILayout.MinHeight(50.0f) };

    // visField params
    private bool showVisFieldExtraGui = false;
    myFieldLoss vf_myFieldLoss;
    int vf_width_px = 425;
    int vf_height_px = 425;
    float vf_width_deg = 60;
    float vf_height_deg = 60;
    Rect vf_Rect;
    GUIStyle vf_textfieldStyle;
    float vf_width_base, vf_height_base, vf_width_deg2px, vf_height_deg2px;
    // combo
    GUIContent[] comboBoxList;
    private ComboBox comboBoxControl;// = new ComboBox();
    private GUIStyle listStyle = new GUIStyle();

    // warping params
    private bool showWarpingExtraGui = false;
    myDistortionMap warp_myDistortionMap;

    // QR code params
    public bool showQRCode = false;
    private Texture2D qrTex = null;

    void Start()
    {
        windowRect = new Rect(0, 0, winWidth, winHeight);
        scrollPosition = new Vector2();

        // init visField GUI params
        vf_Rect = new Rect(30, 25, vf_width_px + 50, vf_height_px + 50);
        vf_width_deg2px = (float)vf_width_px / vf_width_deg;
        vf_width_base = (float)vf_width_px / 2f;
        vf_height_deg2px =  (float)vf_height_px / vf_height_deg;
        vf_height_base = (float)vf_height_px / 2f - 6*vf_height_deg2px; //expecting 1 row fewer than N columns

        // Tweakable params
        InitTweakableParams();

        //
        comboBoxList = new GUIContent[5];
        comboBoxList[0] = new GUIContent("Arbitrary Field");
        comboBoxList[1] = new GUIContent("Healthy");
        comboBoxList[2] = new GUIContent("Glaucoma Example");
        comboBoxList[3] = new GUIContent("AMD Example");
        comboBoxList[4] = new GUIContent("Hemifield Example");

        listStyle.normal.textColor = Color.white;
        listStyle.onHover.background =
        listStyle.hover.background = new Texture2D(2, 2);
        listStyle.padding.left =
        listStyle.padding.right =
        listStyle.padding.top =
        listStyle.padding.bottom = 4;

        comboBoxControl = new ComboBox(new Rect(125, 20, 200, 30), comboBoxList[0], comboBoxList, "button", "box", listStyle);

        //GameObject maincamera = GameObject.Find("Fove Interface 2");
        //vf_grid = maincamera.GetComponent<grid>();
		//warp_myDistortionMap = maincamera.GetComponent<myDistortionMap>();
		GameObject leftEye = GameObject.FindWithTag("LeftEye");
		vf_myFieldLoss = leftEye.GetComponent<myFieldLoss>();
		warp_myDistortionMap = leftEye.GetComponent<myDistortionMap>();
    }

    /// Init or re-init the tweakable params collection
    private void InitTweakableParams()
    {
        if (groupParamsMap == null)
        {
            groupParamsMap = new Dictionary<string, List<TweakableParam>>();
        }
        else
        {
            groupParamsMap.Clear();
        }

        // Search all objects in scene by default if none are specified
        if (targetObjects == null || targetObjects.Length == 0)
        {
            Debug.Log("No target transform set for TweakGUI, using all in scene.");
            targetObjects = FindAllRootTransformsInScene().ToArray();
        }

        // Traverse target transforms
        foreach (Transform t in targetObjects)
        {
            AddTweakableParamsForTransform(t);
        }

        // Sort groups alphabetically
        sortedGroups = new string[groupParamsMap.Count];
        groupParamsMap.Keys.CopyTo(sortedGroups, 0);
        Array.Sort<string>(sortedGroups);
    }

    /// Walks through the transform heirarchy and finds all [Tweakable] members of MonoBehaviours.
    private void AddTweakableParamsForTransform(Transform targetObj)
    {
        if (targetObj != null)
        {
            foreach (MonoBehaviour monoBehaviour in targetObj.GetComponents<MonoBehaviour>())
            {
                foreach (MemberInfo memberInfo in monoBehaviour.GetType().GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                {
                    if (memberInfo is FieldInfo || memberInfo is PropertyInfo)
                    {
                        foreach (object attributeObj in memberInfo.GetCustomAttributes(true))
                        {
                            if (attributeObj is TweakableMemberAttribute)
                            {
                                var attribute = (TweakableMemberAttribute)attributeObj;
                                var tweakableParam = new TweakableParam(attribute, memberInfo, monoBehaviour);
                                List<TweakableParam> paramList;
                                if (groupParamsMap.TryGetValue(attribute.group, out paramList))
                                {
                                    paramList.Add(tweakableParam);
                                }
                                else
                                {
                                    paramList = new List<TweakableParam>();
                                    paramList.Add(tweakableParam);
                                    groupParamsMap.Add(attribute.group, paramList);
                                }
                            }
                        }
                    }
                }
            }

            // Add tweakable fields in child components recursively
            foreach (Transform child in targetObj)
            {
                AddTweakableParamsForTransform(child);
            }
        }
    }

    /// Draw the tweak GUI.
    void OnGUI()
    {
        // styles (can't be done inside Start()?)
        groupTextStyle = new GUIStyle();
        groupTextStyle.normal.textColor = Color.white;
        //groupTextStyle.fontStyle = FontStyle.Bold;
        groupTextStyle.fontSize = 18;
        //groupTextStyle.alignment = TextAnchor.LowerLeft;
        labelStyle = new GUIStyle(GUI.skin.label);
        tickboxStyle = new GUIStyle(GUI.skin.toggle);
        //tickboxStyle.fontStyle = FontStyle.Bold;
        tickboxStyle.fontSize = 18;
        enumStyle = new GUIStyle(GUI.skin.button);
        enumStyle.fontSize = 9;
        vf_textfieldStyle = new GUIStyle(GUI.skin.textField);
        vf_textfieldStyle.alignment = TextAnchor.MiddleCenter;

        // Scale GUI to make it useable on high-res displays (retina etc)
        var scale = 1f;
        if (Screen.dpi > 200) {
            scale = 3f;
        }
        else if (Screen.dpi > 100)
        {
            scale = 2f;
        }
        var scaleVec = new Vector2(scale, scale);
        GUIUtility.ScaleAroundPivot(scaleVec, Vector2.zero);

        if (expanded)
        {
            windowRect.width = winWidth;
            windowRect.height = winHeight;
        }
        else
        {
            windowRect.width = expandToggleWidth + 20;
            windowRect.height = headerHeight + 10;
        }

            // Draw window
            windowRect = GUILayout.Window(0, windowRect, delegate (int id)
            {
            //GUI.DragWindow(new Rect(expandToggleWidth, 0, winWidth - expandToggleWidth, headerHeight));


                if (expanded)
                {
                    if (GUI.Toggle(new Rect(windowRect.width - expandToggleWidth, 0, expandToggleWidth, headerHeight), showQRCode, "Gen QR"))
                    {
                        expanded = false;
                        showQRCode = true;
                        qrTex = null; // set to null to force to regenerate
                    }
                }


                expanded = GUI.Toggle(new Rect(0, 0, expandToggleWidth, headerHeight), expanded, "Expand");

                GUI.DragWindow(new Rect(expandToggleWidth, 0, winWidth - expandToggleWidth, headerHeight));

                GUILayout.BeginVertical();
                if (expanded)
                {
                    scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                    GUILayout.BeginVertical();

                    // Iterate over sorted param groups and lay out the right kind of GUI
                    // controls for each tweakable parameter.
                    foreach (var group in sortedGroups)
                    {

                        List<TweakableParam> tweakableParams;
                        if (groupParamsMap.TryGetValue(group, out tweakableParams))
                        {
                            // PJ -- hack! remove a leading 'z' character (used to ensure 'advanced' options are placed at bottom'
                            String groupname = group;
                            if (group.StartsWith("z "))
                            {
                                groupname = group.TrimStart('z');
                            }
                            GUILayout.Space(18);

                            // PJ -- add tickbox (on the left of the group label) to enable/disable effect
                            //GUILayout.BeginHorizontal();
                            MonoBehaviour imageEffect = (MonoBehaviour)tweakableParams[0].ownerObject;
                            imageEffect.enabled = GUILayout.Toggle((bool)imageEffect.enabled, groupname, tickboxStyle);
                            //GUILayout.Label(group, groupTextStyle);
                            //GUILayout.EndHorizontal();

                            // manually add launch button for visfield
							if (imageEffect.GetType() == typeof(myFieldLoss))
                            {
                                if (GUILayout.Button("Edit Field Map"))
                                {
                                    expanded = false;
                                    showVisFieldExtraGui = true;
                                }
                            }

                            // manually add launch button for warping
                            if (imageEffect.GetType() == typeof(myDistortionMap))
                            {
                                if (GUILayout.Button("Edit Warping Map"))
                                {
                                    expanded = false;
                                    showWarpingExtraGui = true;
                                }
                            }


                            // add tweakable params
                            for (int i = 0; i < tweakableParams.Count; i++)
                            {
                                // reduce vspace
                                GUILayout.Space(-5);
                                
                                TweakableParam tweakableParam = tweakableParams[i];
                                TweakableMemberAttribute attr = tweakableParam.attribute;
                                MemberInfo memberInfo = tweakableParam.memberInfo;

                                object value = tweakableParam.GetMemberValue();
                                Type type = value.GetType();
                                object newValue = null;
                                string paramName = attr.displayName != "" ? attr.displayName : memberInfo.Name;

                                // PJ -- hack! don't display dummy params
                                if (paramName.Equals("dummy"))
                                {
                                    continue;
                                }

                                // Output the right GUI control
                                bool showHeaderLabel = true;
                                bool showValueLabel = true;

                                if (type == typeof(bool))
                                {
                                    showHeaderLabel = false;
                                }

                                GUILayout.BeginHorizontal(); // move up to place label on the left
                                if (showHeaderLabel)
                                {
                                    GUILayout.Label(paramName, labelStyle, labelLayoutOptions);
                                }
                                //GUILayout.BeginHorizontal();

                                // PJ -- hack! nudge down to align vertically with preceding label
                                GUILayout.BeginVertical();
                                GUILayout.Space(9.0f);

                                // Check the type of the member and draw the right control
                                if (type == typeof(float))
                                {
                                    newValue = GUILayout.HorizontalSlider((float)value, attr.minValue, attr.maxValue);
                                }
                                else if (type == typeof(int))
                                {
                                    newValue = (int)GUILayout.HorizontalSlider((int)value, attr.minValue, attr.maxValue);
                                }
                                else if (type == typeof(bool))
                                {
                                    newValue = GUILayout.Toggle((bool)value, paramName);
                                    showValueLabel = false;
                                }
                                else if (type.IsEnum)
                                {
                                    String[] names = Enum.GetNames(type);
                                    newValue = GUILayout.SelectionGrid((int)value, names, names.Length, enumStyle, enumLayoutOptions);
                                    showValueLabel = false;
                                }
                                else
                                {
                                    Debug.Log("Type unknown: " + type);
                                }

                                GUILayout.EndVertical();

                                if (showValueLabel)
                                {
                                    GUILayout.Label(type == typeof(float) ? ((float)value).ToString("F2") : value.ToString(), GUILayout.Width(40));
                                }


                                GUILayout.EndHorizontal();

                                if (newValue != null)
                                {
                                    tweakableParam.SetMemberValue(newValue);
                                }
                            }
                        }
                    }
                    
                    // add launch button for QR Code
                    /*
                    GUILayout.Space(40);
                    if (GUILayout.Button("Generate QR Code"))
                    {
                        expanded = false;
                        showQRCode = true;
                        qrTex = null; // set to null to force to regenerate
                    }*/

                    GUILayout.EndVertical();
                    GUILayout.EndScrollView();
            }

            GUILayout.EndVertical();

        }, "Moorfields Sim v0.0.2");


        if (showVisFieldExtraGui)
        {
            visFieldExtraGui();
        }

        if (showWarpingExtraGui)
        {
            warpingExtraGui();
        }

        if (showQRCode)
        {
           qrCodeExtraGUI();
        }
    }

    private int oldSelectedItem = 0;
    private void visFieldExtraGui()
    {
        // The problem with System.Text.RegularExpressions is that it adds about 900K to web player sizes, because of having to add.dlls that aren't normally included. Also doing regex string replacements every frame in OnGUI may not be ideal for performance. If you're not using a web player then you probably wouldn't care, but if you are, you would likely be better off preventing the unwanted characters from entering the string in the first place and not using regex:
        char chr = Event.current.character;
        if ((chr < 'a' || chr > 'z') && (chr < 'A' || chr > 'Z') && (chr < '0' || chr > '9') && (chr != '-'))
        {
            Event.current.character = '\0';
        }


        GUILayout.Window(1, vf_Rect, delegate (int id1)
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            // draw once (will redraw below to ensure can actually click on it)
            comboBoxControl.Show();

            // get grid
            double[,] xy = vf_myFieldLoss.getGrid();
            switch (comboBoxControl.SelectedItemIndex) {
                case 0:
                    // do nothing
                    break;
                case 1: // healthy
                    xy = new double[,] { { -21, 9, 0 }, { -21, 3, 0 }, { -21, -3, 0 }, { -21, -9, 0 }, { -15, 15, 0 }, { -15, 9, 0 }, { -15, 3, 0 }, { -15, -3, 0 }, { -15, -9, 0 }, { -15, -15, 0 }, { -9, 15, 0 }, { -9, 9, 0 }, { -9, 3, 0 }, { -9, -3, 0 }, { -9, -9, 0 }, { -9, -15, 0 }, { -3, 15, 0 }, { -3, 9, 0 }, { -3, 3, 0 }, { -3, -3, 0 }, { -3, -9, 0 }, { -3, -15, 0 }, { 3, 15, 0 }, { 3, 9, 0 }, { 3, 3, 0 }, { 3, -3, 0 }, { 3, -9, 0 }, { 3, -15, 0 }, { 9, 15, 0 }, { 9, 9, 0 }, { 9, 3, 0 }, { 9, -3, 0 }, { 9, -9, 0 }, { 9, -15, 0 }, { 15, 15, 0 }, { 15, 9, 0 }, { 15, -9, 0 }, { 15, -15, 0 }, { 21, 9, 0 }, { 21, 3, 0 }, { 21, -3, 0 }, { 21, -9, 0 }, { 27, 3, 0 }, { 27, -3, 0 } };
                    break;
                case 2: // glaucoma
                    xy = new double[,] { { -21, 9, -20 }, { -21, 3, -20 }, { -21, -3, -20 }, { -21, -9, -20 }, { -15, 15, -15 }, { -15, 9, -15 }, { -15, 3, -20 }, { -15, -3, -15 }, { -15, -9, -15 }, { -15, -15, -10 }, { -9, 15, -20 }, { -9, 9, -10 }, { -9, 3, 0 }, { -9, -3, 0 }, { -9, -9, 0 }, { -9, -15, -10 }, { -3, 15, -20 }, { -3, 9, -8 }, { -3, 3, 0 }, { -3, -3, 0 }, { -3, -9, 0 }, { -3, -15, -20 }, { 3, 15, 0 }, { 3, 9, 0 }, { 3, 3, 0 }, { 3, -3, 0 }, { 3, -9, -8 }, { 3, -15, -20 }, { 9, 15, -20 }, { 9, 9, -10 }, { 9, 3, 0 }, { 9, -3, -5 }, { 9, -9, -10 }, { 9, -15, -20 }, { 15, 15, -20 }, { 15, 9, -8 }, { 15, -9, -15 }, { 15, -15, -20 }, { 21, 9, -20 }, { 21, 3, -20 }, { 21, -3, -20 }, { 21, -9, -15 }, { 27, 3, -20 }, { 27, -3, -20 } };
                    break;
                case 3: // AMD
                    xy = new double[,] { { -21, 9, 0 }, { -21, 3, 0 }, { -21, -3, 0 }, { -21, -9, 0 }, { -15, 15, 0 }, { -15, 9, 0 }, { -15, 3, 0 }, { -15, -3, 0 }, { -15, -9, 0 }, { -15, -15, 0 }, { -9, 15, 0 }, { -9, 9, 0 }, { -9, 3, -10 }, { -9, -3, -10 }, { -9, -9, 0 }, { -9, -15, 0 }, { -3, 15, 0 }, { -3, 9, -10 }, { -3, 3, -20 }, { -3, -3, -20 }, { -3, -9, -10 }, { -3, -15, 0 }, { 3, 15, 0 }, { 3, 9, -10 }, { 3, 3, -20 }, { 3, -3, -20 }, { 3, -9, -10 }, { 3, -15, 0 }, { 9, 15, 0 }, { 9, 9, 0 }, { 9, 3, -10 }, { 9, -3, -10 }, { 9, -9, 0 }, { 9, -15, 0 }, { 15, 15, 0 }, { 15, 9, 0 }, { 15, -9, 0 }, { 15, -15, 0 }, { 21, 9, 0 }, { 21, 3, 0 }, { 21, -3, 0 }, { 21, -9, 0 }, { 27, 3, 0 }, { 27, -3, 0 } };
                    break;
                case 4: // hemifield
                    xy = new double[,] { { -21, 9, -25 }, { -21, 3, -25 }, { -21, -3, -25 }, { -21, -9, -25 }, { -15, 15, -20 }, { -15, 9, -20 }, { -15, 3, -20 }, { -15, -3, -20 }, { -15, -9, -20 }, { -15, -15, -20 }, { -9, 15, -15 }, { -9, 9, -15 }, { -9, 3, -15 }, { -9, -3, -15 }, { -9, -9, -15 }, { -9, -15, -15 }, { -3, 15, -10 }, { -3, 9, -10 }, { -3, 3, -10 }, { -3, -3, -10 }, { -3, -9, -10 }, { -3, -15, -10 }, { 3, 15, 0 }, { 3, 9, 0 }, { 3, 3, 0 }, { 3, -3, 0 }, { 3, -9, 0 }, { 3, -15, 0 }, { 9, 15, 0 }, { 9, 9, 0 }, { 9, 3, 0 }, { 9, -3, 0 }, { 9, -9, 0 }, { 9, -15, 0 }, { 15, 15, 0 }, { 15, 9, 0 }, { 15, -9, 0 }, { 15, -15, 0 }, { 21, 9, 0 }, { 21, 3, 0 }, { 21, -3, 0 }, { 21, -9, 0 }, { 27, 3, 0 }, { 27, -3, 0 } };
                    break;
            }

            if (oldSelectedItem != comboBoxControl.SelectedItemIndex)
            {
                vf_myFieldLoss.setGrid(xy);
                oldSelectedItem = comboBoxControl.SelectedItemIndex;
            }

            int x, y;
            double result;
            bool changed = false;
            for (int i = 0; i < xy.GetLength(0); i++)
            {
                x = Mathf.RoundToInt(vf_width_base + vf_width_deg2px * (float)xy[i, 0]);
                y = Mathf.RoundToInt(vf_height_base - vf_height_deg2px * (float)xy[i, 1]);

                if (double.TryParse(GUI.TextField(new Rect(x, y, 45, 45), (-xy[i, 2]).ToString("0.0"), vf_textfieldStyle), out result))
                /*String str = GUI.TextField(new Rect(x, y, 45, 45), xy[i, 2].ToString("0.0"), vf_textfieldStyle);
                str = Regex.Replace(str, "-\\.", "-0\\.");
                if (double.TryParse(str, out result))*/
                {
                    result *= -1;
                    if (xy[i, 2] != result)
                    {
                        changed = true;
                        xy[i, 2] = result;
                    }
                }

                /*
                if (double.TryParse(Regex.Replace(GUI.TextField(new Rect(x, y, 45, 45), xy[i, 2].ToString("#.0"), textfieldStyle), "[^0-9\\.]", ""), out result))
                {
                    xy[i,2] = result;
                }
                */
                /*
                Match match = Regex.Match(xy[i, 2].ToString(), "^-?\\d*\\.?\\d?");
                if (match.Success)
                {
                    //print(match.Value);
                    if (double.TryParse(match.Value, out result))
                    {
                        //print(result);
                        xy[i, 2] = result;
                    }
                }
                GUI.TextField(new Rect(x, y, 45, 45), xy[i, 2].ToString("#.0"), textfieldStyle);
                */
            }

            if (changed)
            {
                vf_myFieldLoss.setGrid(xy);
                comboBoxControl.SelectedItemIndex = 0;
            }

            GUILayout.BeginVertical();
            GUILayout.Space(375); // hack!

            // draw again! (for clicking this time, but visually will appear behind textfields)
            comboBoxControl.Show();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Close", visFieldButtonOptions))
            {
                expanded = true;
                showVisFieldExtraGui = false;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }, "Visual Field DLS");

        GUI.BringWindowToFront(1);
        GUI.FocusWindow(1); // set focus on this window
    }
    
    private void warpingExtraGui()
    {
        GUILayout.Window(1, vf_Rect, delegate (int id1)
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            // get
            float[] warp_x = warp_myDistortionMap.getWarpX();
            float[] warp_y = warp_myDistortionMap.getWarpY();
            float[] warp_radius = warp_myDistortionMap.getWarpRadius();
            float[] warp_magnitude = warp_myDistortionMap.getWarpMagnitude();

            // UI
            for (int i = 0; i < warp_x.Length; i++)
            {
                object newValue;

                GUILayout.BeginVertical();

                // header
                String labelString = "Node " + (i+1);
                GUILayout.Label(labelString, groupTextStyle, labelLayoutOptions);

                GUILayout.BeginHorizontal();
                GUILayout.Label("X", labelStyle, labelLayoutOptions);
                newValue = null;
                newValue = GUILayout.HorizontalSlider(warp_x[i], 0f, 1f);
                GUILayout.Label( warp_x[i].ToString("F2"), GUILayout.Width(40));
                if (newValue != null) { warp_x[i] = (float)newValue;  }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Y", labelStyle, labelLayoutOptions);
                newValue = null;
                newValue = GUILayout.HorizontalSlider(warp_y[i], 0f, 1f);
                GUILayout.Label(warp_y[i].ToString("F2"), GUILayout.Width(40));
                if (newValue != null) { warp_y[i] = (float)newValue; }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Radius", labelStyle, labelLayoutOptions);
                newValue = null;
                newValue = GUILayout.HorizontalSlider(warp_radius[i], 0f, 0.5f);
                GUILayout.Label(warp_radius[i].ToString("F2"), GUILayout.Width(40));
                if (newValue != null) { warp_radius[i] = (float)newValue; }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Magnitude", labelStyle, labelLayoutOptions);
                newValue = null;
                newValue = GUILayout.HorizontalSlider(warp_magnitude[i], -1f, 1f);
                GUILayout.Label(warp_magnitude[i].ToString("F2"), GUILayout.Width(40));
                if (newValue != null) { warp_magnitude[i] = (float)newValue; }
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
            }
            
            // buttons
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Compute", visFieldButtonOptions))
            {
                warp_myDistortionMap.compute();
            }
            if (GUILayout.Button("Close", visFieldButtonOptions))
            {
                expanded = true;
                showWarpingExtraGui = false;
            }
            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();
        }, "Warping");

        GUI.BringWindowToFront(1);
        GUI.FocusWindow(1); // set focus on this window
    }

    // TODO - obviously shouldn't be generate QR code on every refresh!
    private void qrCodeExtraGUI()
    {
        GUILayout.Window(1, vf_Rect, delegate (int id1)
        {
            // params
            int width_px = 256; // (int)vf_Rect.width - 50; // 256;
            int height_px = 256; // width_px; //  (int)vf_Rect.height - 50; // 256;

            if (qrTex == null)
            { 
                String textForEncoding = "hello world";

                // UI - courtesy of https://medium.com/@adrian.n/reading-and-generating-qr-codes-with-c-in-unity-3d-the-easy-way-a25e1d85ba51
                var writer = new BarcodeWriter
                {
                    Format = BarcodeFormat.QR_CODE,
                    Options = new QrCodeEncodingOptions
                    {
                        Height = height_px,
                        Width = width_px
                    }
                };
                Color32[] qrPixels = writer.Write(textForEncoding);

                // generate texture
                qrTex = new Texture2D(width_px, height_px);
                qrTex.SetPixels32(qrPixels);
                qrTex.Apply();
            }

            // display QR code as button
            GUILayout.BeginArea(new Rect(0, 30, vf_Rect.width, vf_Rect.width));
            GUILayout.Button(qrTex);
            GUILayout.EndArea();

            // add close button
            GUILayout.BeginVertical();
            GUILayout.Space(30 + height_px + 15);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Close", visFieldButtonOptions))
            {
                expanded = true;
                showQRCode = false;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

        }, "QR Code");

        GUI.BringWindowToFront(1);
        GUI.FocusWindow(1); // set focus on this window
    }







    private List<Transform> FindAllRootTransformsInScene()
    {
        List<Transform> roots = new List<Transform>();
        foreach (GameObject obj in UnityEngine.Object.FindObjectsOfType(typeof(GameObject)))
        {
            if (obj.transform.parent == null)
            {
                roots.Add(obj.transform);
            }
        }
        return roots;
    }


    /// Private class that wraps a tweakable member
    private class TweakableParam
    {
        public TweakableMemberAttribute attribute;
        public MemberInfo memberInfo;
        public object ownerObject;
        private bool isField;

        public TweakableParam(TweakableMemberAttribute attr, MemberInfo memberInfo, object ownerObject)
        {
            this.attribute = attr;
            this.memberInfo = memberInfo;
            this.ownerObject = ownerObject;
            this.isField = memberInfo is FieldInfo;
            if (!(memberInfo is FieldInfo || memberInfo is PropertyInfo))
            {
                throw new ArgumentException("Member " + memberInfo.ToString() + " not supported.");
            }
        }

        public object GetMemberValue()
        {
            if (isField)
            {
                return ((FieldInfo)memberInfo).GetValue(ownerObject);
            }
            else
            {
                return ((PropertyInfo)memberInfo).GetValue(ownerObject, null);
            }
        }

        public void SetMemberValue(object value)
        {
            if (isField)
            {
                ((FieldInfo)memberInfo).SetValue(ownerObject, value);
            }
            else
            {
                ((PropertyInfo)memberInfo).SetValue(ownerObject, value, null);
            }
        }
    }
}


/// Attribute that can be used to mark fields or properties on MonoBehaivours as Tweakable,
/// letting them be tweaked in an in-game GUI.
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
class TweakableMemberAttribute : Attribute
{
    public float maxValue;
    public float minValue;
    public string displayName;
    public string group;

    private const string DEFAULT_GROUP = "Default";

    public TweakableMemberAttribute()
        : this(0, 100)
    {
    }

    public TweakableMemberAttribute(string displayname, string group = DEFAULT_GROUP)
        : this(0, 100, displayname, group)
    {
    }

    public TweakableMemberAttribute(float minValue, float maxValue, string displayName = "", string group = DEFAULT_GROUP)
    {
        this.minValue = minValue;
        this.maxValue = maxValue;
        this.displayName = displayName;
        this.group = group;
    }

}
