namespace JujubeMapEditor.Editor {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using UnityEditor.Experimental.SceneManagement;
	using JujubeMapEditor.Core;
	using UnityEditor.SceneManagement;



	// === Main ===
	public partial class JujubeScene : MoenenEditor {




		#region --- SUB ---



		public enum PaletteItemSizeMode {
			Small = 0,
			Middle = 1,
			Large = 2,
		}



		public enum CursorRotationMode {
			Forward = 0,
			Right = 1,
			Back = 2,
			Left = 3,
		}



		public enum JujubeTool {
			Select = 0,
			Wand = 1,
			Brush = 2,
			Erase = 3,
			Paint = 4,
			Pick = 5,
		}



		public enum JujubeLayerMode {
			CurrentLayer = 0,
			AllLayer = 1,
		}


		public enum JujubeWandMode {
			Same = 0,
			Any = 1,
		}


		public enum JujubePaintMode {
			Box = 0,
			Bucket = 1,
		}


		public enum JujubePositionHandleType {
			Quad = 0,
			Box = 1,
			Arrow = 2,
		}


		public enum JujubeWandSpreadMode {
			Straight = 0,
			StraightAndDiagonal = 1,
		}


		#endregion




		#region --- VAR ---


		// Const
		private readonly static Color COORD_COLOR = new Color(0.5f, 0.5f, 0.5f);
		private readonly static Color COORD_COLOR_BACK = new Color(0f, 0f, 0f);
		private readonly static Color COORD_COLOR_X = new Color(0.8588235f, 0.2431373f, 0.1137255f);
		private readonly static Color COORD_COLOR_Z = new Color(0.227451f, 0.4784314f, 0.972549f);
		private readonly static Color HIGHLIGHT_TINT = new Color(0.2f, 0.5f, 0.3f, 0.6f);
		private readonly static Color SELECTING_BLOCK_TINT = new Color(1f, 0.8f, 0.1f, 1f);
		private readonly static Color SELECTING_BLOCK_TINT_ALT = new Color(0.5f, 0.5f, 0.5f, 0.3f);
		private readonly static Color[][] BOX_POS_HANDLE_COLORS = { new Color[] { new Color(0.8588f, 0.2431f, 0.1137f, 1f), new Color(0.6039f, 0.9529f, 0.2823f, 1f), new Color(0.2274f, 0.4784f, 0.9725f, 1f) }, new Color[] { new Color(0.8588f, 0.2431f, 0.1137f, 0.25f), new Color(0.6039f, 0.9529f, 0.2823f, 0.25f), new Color(0.2274f, 0.4784f, 0.9725f, 0.25f) }, };
		private readonly static Color[,] CURSOR_TINT = {
			{ new Color(0.5f, 0.5f, 0.5f, 0.15f), new Color(0.5f, 0.5f, 0.5f, 0.7f), new Color(0.7f, 0.7f, 0.7f, 0.85f), },
			{ new Color(0.5f, 0.5f, 0.5f, 0.15f), new Color(0.5f, 0.5f, 0.5f, 0.7f), new Color(0.7f, 0.7f, 0.7f, 0.85f), },
			{ new Color(0.5f, 0.5f, 0.5f, 0.15f), new Color(0.3f, 0.7f, 0.1f, 0.8f), new Color(0f, 0f, 0f, 0f), },
			{ new Color(1f, 0.3f, 0.3f, 0.2f), new Color(1f, 0.3f, 0.3f, 0.7f), new Color(0.9f, 0.55f, 0.35f, 0.85f), },
			{ new Color(0.5f, 0.5f, 0.5f, 0.15f), new Color(0.3f, 0.7f, 0.1f, 0.8f), new Color(0f, 0f, 0f, 0f), },
			{ new Color(0.5f, 0.5f, 0.5f, 0.15f), new Color(0f, 0f, 0f, 0f), new Color(0.7f, 0.7f, 0.7f, 0.85f), }
		};
		private readonly static float[] BLOCK_SHAKE_CACHE_PARAM = { 0f, 0f, };
		private readonly static float[] BLOCK_SHAKE_CACHE = { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, };
		private readonly static int[] PALETTE_ITEM_COUNT = { 7, 5, 4 };
		private readonly static string[] SCENE_REFRESH_LABELS = { "|       ", " |      ", "  |     ", "   |    ", "    |   ", "     |  ", "      | ", "       |", "      | ", "     |  ", "    |   ", "   |    ", " |      ", };
		private readonly static string[] CURSOR_ROTATION_LABELS = { "▲", "▶", "▼", "◀", "Forward", "Right", "Back", "Left", };
		private readonly static string[] LAYER_MODE_LABELS = { "Current Layer", "All Layers", };
		private readonly static string[] WAND_OPTION_LABELS = { "Same Prefab", "Any Prefab", };
		private readonly static string[] WAND_SPREAD_LABELS = { "Straight", "Diagonal & Straight", };
		private readonly static string[] PAINT_OPTION_LABELS = { "Box", "Bucket", };
		private readonly static string[] TOOL_ICONS = { "Tool_Select_pro.png", "Tool_Wand_pro.png", "Tool_Brush_pro.png", "Tool_Erase_pro.png", "Tool_Paint_pro.png", "Tool_Pick_pro.png", "Tool_Select_per.png", "Tool_Wand_per.png", "Tool_Brush_per.png", "Tool_Erase_per.png", "Tool_Paint_per.png", "Tool_Pick_per.png", };
		private const string JUJUBE_EDITING_ROOT_NAME = "[Jujube Editing Root 9573462]";
		private const string JUJUBE_MENU_ROOT = "Tools/Jujube Map Editor";
		private const float TEMP_WARNING_DURATION = 5f;
		private const int PANEL_WIDTH = 180;
		private const int PANEL_PADDING = 2;
		private const int THUMBNAIL_SIZE = 84;
		private const int PALETTE_CONTENT_HEIGHT = 240;
		private const int LAYER_CONTENT_HEIGHT = 180;
		private const int PANEL_TITLE_HEIGHT = 18;
		private const int PANEL_ITEM_HEIGHT = 18;
		private const int COORD_ADD = 3;
		private const int TOOLBAR_WIDTH = 26;
		private const int TOOLBAR_BUTTON_HEIGHT_S = 22;
		private const int TOOLBAR_BUTTON_HEIGHT_M = 26;
		private const int TOOLBAR_BUTTON_HEIGHT_L = 54;

		// Api
		public static JujubeRenderer EditingRenderer { get; set; } = null;

		// Short
		private static GUIStyle PanelShadowBoxStyle {
			get {
				if (_PanelShadowBoxStyle == null || _PanelShadowBoxStyle.normal.background == null) {
					_PanelShadowBoxStyle = new GUIStyle() {
						padding = new RectOffset(PANEL_PADDING, PANEL_PADDING, PANEL_PADDING, PANEL_PADDING),
						margin = new RectOffset(0, 0, 0, 0),
					};
					var bg0 = EditorGUIUtility.isProSkin ?
						new Color(0.27f, 0.27f, 0.27f, 0.9f) :
						new Color(0.73f, 0.73f, 0.73f, 0.9f);
					var fr0 = new Color(0f, 0f, 0f, 0.3f);
					var fr1 = new Color(0f, 0f, 0f, 0.15f);
					var texture = new Texture2D(7, 7, TextureFormat.RGBA32, false);
					texture.filterMode = FilterMode.Point;
					texture.alphaIsTransparency = true;
					texture.SetPixels(new Color[7 * 7] {
						fr1, fr1, fr1, fr1, fr1, fr1, fr1,
						fr1, fr0, fr0, fr0, fr0, fr0, fr1,
						fr1, fr0, bg0, bg0, bg0, fr0, fr1,
						fr1, fr0, bg0, bg0, bg0, fr0, fr1,
						fr1, fr0, bg0, bg0, bg0, fr0, fr1,
						fr1, fr0, fr0, fr0, fr0, fr0, fr1,
						fr1, fr1, fr1, fr1, fr1, fr1, fr1,
					});
					texture.Apply();
					_PanelShadowBoxStyle.normal.background = texture;
					_PanelShadowBoxStyle.border = new RectOffset(3, 3, 3, 3);
				}
				return _PanelShadowBoxStyle;
			}
		}
		private static GUIStyle CenterLabelStyle_10 => _CenterLabelStyle_10 != null ? _CenterLabelStyle_10 : (_CenterLabelStyle_10 = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 10, });
		private static int PaletteItemCountX => PALETTE_ITEM_COUNT[Mathf.Clamp(PaletteItemSizeIndex.Value, 0, PALETTE_ITEM_COUNT.Length - 1)];
		private static JujubeTool UsingTool {
			get {
				if (SelectingBlockMap.Count == 0 && Event.current.shift) {
					return JujubeTool.Pick;
				}
				return (JujubeTool)SelectingToolIndex.Value;
			}
			set => SelectingToolIndex.Value = (int)value;
		}
		private static JujubeLayerMode LayerMode {
			get => (JujubeLayerMode)LayerModeIndex.Value;
			set => LayerModeIndex.Value = (int)value;
		}
		private static JujubePositionHandleType PositionHandleType => (JujubePositionHandleType)PositionHandleTypeIndex.Value;
		private static JujubeWandMode WandMode {
			get => (JujubeWandMode)WandModeIndex.Value;
			set => WandModeIndex.Value = (int)value;
		}
		private static JujubePaintMode PaintMode {
			get => (JujubePaintMode)PaintModeIndex.Value;
			set => PaintModeIndex.Value = (int)value;
		}
		private static JujubeWandSpreadMode WandSpreadMode {
			get => (JujubeWandSpreadMode)WandSpreadIndex.Value;
			set => WandSpreadIndex.Value = (int)value;
		}

		// Data
		private readonly static Dictionary<GameObject, (Texture2D preview, string label)> PaletteItemCacheMap = new Dictionary<GameObject, (Texture2D preview, string label)>();
		private readonly static Dictionary<string, Texture2D> ImageMap = new Dictionary<string, Texture2D>();
		private readonly static Dictionary<(int layerIndex, int blockIndex), JujubeBlock> SelectingBlockMap = new Dictionary<(int layerIndex, int blockIndex), JujubeBlock>();
		private readonly static List<(Vector3Int position, int rotationIndex, int prefabIndex)> CopyList = new List<(Vector3Int position, int rotationIndex, int prefabIndex)>();
		private static GameObject LinkedPrefab = null;
		private static Transform EditingRoot = null;
		private static Transform EditingRoot_Cursor = null;
		private static GUIStyle _PanelShadowBoxStyle = null;
		private static GUIStyle _CenterLabelStyle_10 = null;
		private static Vector3? LerpingCameraPivot = null;
		private static Vector3? CursorPos = null;
		private static Vector2? NeedMoveMouseToPaintPos = null;
		private static Vector3Int? Paint_CursorDownMapPosition = null;
		private static Vector3Int SelectingBlockMin = Vector3Int.one * short.MaxValue;
		private static Vector3Int SelectingBlockMax = Vector3Int.one * short.MinValue;
		private static Vector3? SelectingPivot = null;
		private static Vector3 CursorNormal = default;
		private static Vector3 LayerPanelScrollPosition = default;
		private static Vector3 PalettePanelScrollPosition = default;
		private static Bounds MapLocalBounds = default;
		private static Tool OldUnityTool = Tool.None;
		private static string TempWarningMessage = "";
		private static double AllowPreviewCacheTime = 0f;
		private static float SceneRefreshingLabelAmount = 0f;
		private static float TempWarningTime = float.MinValue;
		private static float CoordAlpha = 1f;
		private static bool CurrentMouseLeftHolding = false;
		private static bool CurrentMouseLeftDragging = false;
		private static bool FocusingTextField = false;
		private static bool OldBoxColliderExpand = false;
		private static bool OldDrawingGizmos = true;
		private static bool MouseInPanelGUI = false;
		private static bool BlockShakeRefreshed = false;
		private static bool AllowRegisteUndoForMoveSelection = true;
		private static bool AllowRotatePaintingBlock = true;
		private static int CurrentMouseButton = -1;
		private static int SelectingPaletteItemIndex = 0;
		private static int SelectingLayerIndex = 0;
		private static int ToolCount = 6;
		private static bool MapDirty = false;
		private static bool ForceMapRespawn = false;
		private static bool MapAssetDirty = false;
		private static bool PaletteAssetDirty = false;
		private static int BlockDirtyCount = -1;

		// Saving
		public static EditorSavingInt PaletteItemSizeIndex = new EditorSavingInt("JujubeScene.PaletteItemSizeIndex", 1);
		public static EditorSavingInt CursorRotationIndex = new EditorSavingInt("JujubeScene.CursorRotationIndex", 0);
		public static EditorSavingInt WandModeIndex = new EditorSavingInt("JujubeScene.WandModeIndex", 0);
		public static EditorSavingInt PaintModeIndex = new EditorSavingInt("JujubeScene.PaintModeIndex", 0);
		public static EditorSavingInt SelectingToolIndex = new EditorSavingInt("JujubeScene.SelectingToolIndex", 0);
		public static EditorSavingInt LayerModeIndex = new EditorSavingInt("JujubeScene.LayerModeIndex", 1);
		public static EditorSavingInt WandSpreadIndex = new EditorSavingInt("JujubeScene.WandSpreadIndex", 0);
		public static EditorSavingInt PositionHandleTypeIndex = new EditorSavingInt("JujubeScene.PositionHandleTypeIndex", 0);
		public static EditorSavingInt MaxSelectionCount = new EditorSavingInt("JujubeScene.MaxSelectionCount", 4096);
		public static EditorSavingInt MaxPaintCount = new EditorSavingInt("JujubeScene.MaxPaintCount", 4096);
		public static EditorSavingBool ShowStateLabel = new EditorSavingBool("JujubeScene.ShowStateLabel", false);
		public static EditorSavingBool UseBlockShake = new EditorSavingBool("JujubeScene.UseBlockShake", true);
		public static EditorSavingBool ShowEditingHierarchyLabel = new EditorSavingBool("JujubeScene.ShowEditingHierarchyLabel", true);


		#endregion




		#region --- MSG ---



		[InitializeOnLoadMethod]
		public static void EditorInit () {
			SceneView.beforeSceneGui += BeforeSceneGUI;
			SceneView.duringSceneGui += DuringSceneGUI;
			EditorSceneManager.activeSceneChangedInEditMode += (sceneA, sceneB) => {
				if (EditingRenderer != null) {
					EndEdit();
				}
			};
			EditorApplication.update += OnUpdate;
			EditorApplication.hierarchyWindowItemOnGUI += HierarchyGUI;
			Undo.undoRedoPerformed += UndoRedoPerformed;
			RemoveAllEditingRoot();
			ToolCount = System.Enum.GetNames(typeof(JujubeTool)).Length;
		}


		// GUI
		private static void BeforeSceneGUI (SceneView sceneView) {

			if (EditingRenderer == null) { return; }

			if (EditingRenderer.Map == null || EditorApplication.isPlaying || EditingRenderer.Mode != JujubeRendererMode.Develop) {
				EndEdit();
				return;
			}

			// Tool
			if (Tools.current != Tool.None) {
				Tools.current = Tool.None;
			}

			// 2D Mode
			if (SceneView.currentDrawingSceneView.in2DMode) {
				SceneView.currentDrawingSceneView.in2DMode = false;
			}

			if (SceneView.currentDrawingSceneView.drawGizmos) {
				SceneView.currentDrawingSceneView.drawGizmos = false;
			}

			// Cancel Control
			var e = Event.current;
			if (e.type == EventType.KeyDown && !e.control && !e.shift && !e.alt) {
				if (e.keyCode == KeyCode.Return) {
					GUI.FocusControl("");
					e.Use();
				}
			}

			// Coord
			SceneGUI_Control_Before();
			SceneGUI_Coordinate(sceneView, true);

		}


		private static void DuringSceneGUI (SceneView sceneView) {

			if (EditingRenderer == null || EditingRenderer.Map == null || EditingRenderer.Palette == null) { return; }

			// Prefab Stage Check
			if (PrefabStageUtility.GetCurrentPrefabStage() != null || EditingRenderer.Map == null || EditorApplication.isPlaying || EditingRenderer.Mode != JujubeRendererMode.Develop) {
				EndEdit();
				return;
			}

			// Default Control
			HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
			FocusingTextField = false;

			// GUI - System
			SceneGUI_ValueFix();
			SceneGUI_MouseButtonCache(sceneView);
			// GUI - Gizmos
			SceneGUI_Camera(sceneView);
			SceneGUI_Coordinate(sceneView, false);
			// GUI - Paint
			SceneGUI_Cursor(sceneView);
			SceneGUI_Selection(sceneView);
			SceneGUI_Paint();
			SceneGUI_Focus();
			// GUI - Panel
			SceneGUI_Panel(sceneView);
			SceneGUI_Toolbar(sceneView);
			SceneGUI_State(sceneView);
			SceneGUI_Warning(sceneView);
			// GUI - Control
			SceneGUI_Control(sceneView);

			if (Event.current.type == EventType.MouseDown) {
				GUI.FocusControl("");
				sceneView.Repaint();
			}

		}


		private static void OnUpdate () {

			// Editing Root Check
			if (EditingRenderer == null || EditingRenderer.Map == null || EditingRenderer.Palette == null) {
				if (EditingRoot != null) {
					Object.DestroyImmediate(EditingRoot.gameObject, false);
				}
				return;
			}

			// Block Rotation XZ
			RefreshBlockShake();

			// Map Dirty
			if (MapDirty) {
				RefreshMapBound();
				EditingRenderer.ReloadAll(ForceMapRespawn);
				MapDirty = false;
				ForceMapRespawn = false;
			}

			// Asset Dirty
			if (MapAssetDirty) {
				EditorUtility.SetDirty(EditingRenderer.Map);
				MapAssetDirty = false;
			}
			if (PaletteAssetDirty) {
				if (EditingRenderer.Palette != null) {
					EditorUtility.SetDirty(EditingRenderer.Palette);
				}
				PaletteAssetDirty = false;
			}

		}


		private static void HierarchyGUI (int instanceID, Rect rect) {
			if (!ShowEditingHierarchyLabel.Value || EditingRenderer == null || EditingRenderer.Map == null) { return; }
			Object obj = EditorUtility.InstanceIDToObject(instanceID);
			if (!obj || obj != EditingRenderer.gameObject) { return; }
			rect.x += rect.width - 48;
			rect.width = 48;
			var oldC = GUI.color;
			GUI.color = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.5f) : new Color(0f, 0f, 0f, 0.5f);
			GUI.Box(rect, "Editing", GUI.skin.label);
			GUI.color = oldC;
		}


		private static void UndoRedoPerformed () {
			SetMapDirty(true);
			//SetJblockDirty();
			SelectingBlockMin = Vector3Int.one * short.MaxValue;
			SelectingBlockMax = Vector3Int.one * short.MinValue;
			SelectingPivot = null;
		}


		#endregion




		#region --- API ---


		public static void StartEdit (JujubeRenderer renderer) {

			if (EditingRenderer != null || renderer == null || renderer.Mode != JujubeRendererMode.Develop) {
				EndEdit();
			}

			// No Map
			if (renderer.Map == null) {
				Debug.LogWarning("[Jujube] Can not edit this map because no map was assigned.");
				return;
			}

			// No Palette
			if (renderer.Map != null && renderer.Palette == null) {
				CreatePaletteFile(renderer);
			}

			// Dialog
			try {

				EditorUtil.ProgressBar("Prepare for Edit Map...", "Starting", 0.5f);

				// === Start ===
				renderer.gameObject.SetActive(true);

				// Get Linked Prefab
				LinkedPrefab = null;
				var nRoot = PrefabUtility.GetNearestPrefabInstanceRoot(renderer);
				if (nRoot != null) {
					string rootPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(nRoot);
					if (EditorUtil.FileExists(rootPath)) {
						LinkedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(rootPath);
					}
				}

				// Unpack
				if (PrefabUtility.IsPartOfAnyPrefab(renderer)) {
					var root = PrefabUtility.GetOutermostPrefabInstanceRoot(renderer);
					if (root != null) {
						PrefabUtility.UnpackPrefabInstance(
							root,
							PrefabUnpackMode.OutermostRoot,
							InteractionMode.AutomatedAction
						);
					}
				}

				// Load
				renderer.ReloadAll(false);

				// Camera
				var mapLocalBounds = renderer.Map.GetMapBounds(renderer.CellSize);
				var sceneView = SceneView.lastActiveSceneView;
				if (sceneView != null) {
					SetLerpingCameraPivot(renderer.transform.position + mapLocalBounds.center);
				}

				// Editing Root
				RemoveAllEditingRoot();
				CreateEditingRoot();

				// Flag
				renderer.transform.hideFlags = HideFlags.NotEditable;

				// Component Expand
				OldBoxColliderExpand = EditorUtil.GetExpandComponent<BoxCollider>();
				EditorUtil.SetExpandComponent<BoxCollider>(false);

				// Draw Gizmos
				foreach (SceneView view in SceneView.sceneViews) {
					OldDrawingGizmos = view.drawGizmos;
					view.drawGizmos = false;
				}

				// Done
				EditingRenderer = renderer;
				MapLocalBounds = mapLocalBounds;
				OldUnityTool = Tools.current;
				SelectingLayerIndex = 0;
				SelectingPaletteItemIndex = 0;
				CurrentMouseLeftHolding = false;
				CurrentMouseLeftDragging = false;
				Selection.activeGameObject = renderer.gameObject;
				ClearSelection();
				ClearJujubeCache();
				SetBlockCountDirty();
				CopyList.Clear();
				EditorWindow.FocusWindowIfItsOpen<SceneView>();
				UnityEditorInternal.InternalEditorUtility.RepaintAllViews();

			} catch (System.Exception ex) {
				Debug.LogError(ex);
				EditorUtil.ClearProgressBar();
			}

			EditorUtil.ClearProgressBar();
		}


		public static void EndEdit () {

			// Relink
			if (EditingRenderer != null && LinkedPrefab != null) {
				string path = AssetDatabase.GetAssetPath(LinkedPrefab);
				if (EditorUtil.FileExists(path)) {
					PrefabUtility.SaveAsPrefabAssetAndConnect(
						EditingRenderer.gameObject,
						path,
						InteractionMode.AutomatedAction
					);
				}
				LinkedPrefab = null;
			}

			// Editing Root
			RemoveAllEditingRoot();

			// Component Expand
			EditorUtil.SetExpandComponent<BoxCollider>(OldBoxColliderExpand);

			// Draw Gizmos
			foreach (SceneView view in SceneView.sceneViews) {
				view.drawGizmos = OldDrawingGizmos;
			}

			// Flag, Cache
			if (EditingRenderer != null) {
				EditingRenderer.transform.hideFlags = HideFlags.None;
			}

			// Done
			ClearSelection();
			RefreshBlockShake(true);
			MapLocalBounds = default;
			EditingRenderer = null;
			LerpingCameraPivot = null;
			CurrentMouseLeftHolding = false;
			CurrentMouseLeftDragging = false;
			CopyList.Clear();
			Tools.current = OldUnityTool;
			UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
		}


		// Block
		public static void ClearBlockSelection () => ClearSelection();


		public static void RefreshMapBound () => MapLocalBounds = EditingRenderer.Map.GetMapBounds(EditingRenderer.CellSize);


		// Dirty
		public static void SetMapDirty (bool forceRespawnBlocks) {
			MapDirty = true;
			ForceMapRespawn = forceRespawnBlocks;
		}


		public static void SetMapAssetDirty () => MapAssetDirty = true;


		public static void SetPaletteAssetDirty () => PaletteAssetDirty = true;


		public static void SetBlockCountDirty () => BlockDirtyCount = -1;


		#endregion




		#region --- LGC ---


		// Workflow
		private static void CreatePaletteFile (JujubeRenderer renderer) {
			string mapPath = AssetDatabase.GetAssetPath(renderer.Map);
			if (EditorUtil.FileExists(mapPath)) {
				JujubePalette pal = null;
				string palName = $"{renderer.Map.name}_Pal";
				string palPath = EditorUtil.CombinePaths(
					EditorUtil.FixedRelativePath(EditorUtil.GetParentPath(mapPath)),
					palName + ".asset"
				);
				// Get Exist Palette
				if (EditorUtil.FileExists(palPath)) {
					pal = AssetDatabase.LoadAssetAtPath<JujubePalette>(palPath);
				}
				// Create New Palette
				if (pal == null) {
					pal = ScriptableObject.CreateInstance<JujubePalette>();
					pal.name = palName;
					AssetDatabase.CreateAsset(pal, palPath);
					AssetDatabase.Refresh();
					AssetDatabase.SaveAssets();
				}
				renderer.SetPalette(pal);
			}
		}


		// Editing Root
		private static void CreateEditingRoot () {

			// Root
			EditingRoot = new GameObject(JUJUBE_EDITING_ROOT_NAME).transform;
			EditingRoot.position = Vector3.zero;
			EditingRoot.rotation = Quaternion.identity;
			EditingRoot.localScale = Vector3.one;
			EditingRoot.SetParent(null);
			EditingRoot.SetAsLastSibling();
			EditingRoot.gameObject.hideFlags = HideFlags.HideAndDontSave;

			// Cursor
			EditingRoot_Cursor = new GameObject("Cursor").transform;
			EditingRoot_Cursor.SetParent(EditingRoot);
			EditingRoot_Cursor.localPosition = Vector3.zero;
			EditingRoot_Cursor.localRotation = Quaternion.identity;
			EditingRoot_Cursor.localScale = Vector3.one;
			EditingRoot_Cursor.SetAsLastSibling();
			EditingRoot_Cursor.gameObject.hideFlags = HideFlags.HideAndDontSave;



		}


		private static void RemoveAllEditingRoot () {
			for (int safe = 0; safe < 64; safe++) {
				var root = GameObject.Find(JUJUBE_EDITING_ROOT_NAME);
				if (root != null) {
					EditorUtil.DestroyAllChirldrenImmediate(root.transform);
					Object.DestroyImmediate(root.gameObject, false);
				} else {
					break;
				}
			}
		}


		// Cache
		private static (Texture2D preview, string label) GetJujubePaletteCache (GameObject prefab) {
			if (prefab == null) { return (null, string.Empty); }
			if (PaletteItemCacheMap.ContainsKey(prefab)) {
				return PaletteItemCacheMap[prefab];
			} else if (EditorApplication.timeSinceStartup > AllowPreviewCacheTime) {
				AllowPreviewCacheTime = EditorApplication.timeSinceStartup + 0.01f;
				var texture = EditorUtil.GetFixedAssetPreview(prefab.gameObject);
				if (texture == null) { return (null, string.Empty); }
				string label = string.Empty;
				var jblock = prefab.GetComponent<JBlock>();
				if (jblock != null) {
					label = jblock.GetPaletteLabel();
				}
				PaletteItemCacheMap.Add(prefab, (texture, label));
				return (texture, label);
			} else {
				SceneView.RepaintAll();
				return (null, string.Empty);
			}
		}


		private static Texture2D GetJujubeImage (string fileName) {
			if (ImageMap.ContainsKey(fileName)) {
				return ImageMap[fileName];
			} else {
				var image = EditorUtil.GetImage(fileName);
				if (image != null) {
					ImageMap.Add(fileName, image);
				}
				return image;
			}
		}


		private static void RegisterJujubeUndo () => Undo.RegisterCompleteObjectUndo(new Object[] {
			EditingRenderer, EditingRenderer.Map, EditingRenderer.Palette
		}, "Jujube");


		// Block Shake
		private static void RefreshBlockShake (bool forceRefresh = false) {

			if (!UseBlockShake.Value) { return; }

			bool hasSelection = SelectingBlockMap.Count > 0;
			if (!forceRefresh && !hasSelection && BlockShakeRefreshed) { return; }

			// Cache
			float time = (float)EditorApplication.timeSinceStartup;
			int cacheLength = BLOCK_SHAKE_CACHE.Length;
			float angle = BLOCK_SHAKE_CACHE_PARAM[0];
			float speed = BLOCK_SHAKE_CACHE_PARAM[1];
			for (int i = 0; i < cacheLength; i++) {
				BLOCK_SHAKE_CACHE[i] = i % 2 == 0 ?
					Mathf.Clamp(Mathf.PingPong((time + i) * speed, angle * 2f) - angle, -angle / 2f, angle / 2f) :
					Mathf.PingPong((time + i) * speed, angle) - angle / 2f;
			}

			// Do Shake
			int cacheIndex = 0;
			for (int layerIndex = 0; layerIndex < EditingRenderer.Map.Layers.Count; layerIndex++) {
				var layer = EditingRenderer.Map.Layers[layerIndex];
				for (int blockIndex = 0; blockIndex < layer.Blocks.Count; blockIndex++) {
					var blockTF = EditingRenderer.GetBlockTF(layerIndex, blockIndex);
					if (blockTF == null) { continue; }
					blockTF.localRotation = hasSelection && SelectingBlockMap.ContainsKey((layerIndex, blockIndex)) ?
						Quaternion.Euler(
							BLOCK_SHAKE_CACHE[cacheIndex],
							blockTF.localRotation.eulerAngles.y,
							BLOCK_SHAKE_CACHE[cacheIndex + 1]
						) :
						Quaternion.Euler(0f, blockTF.localRotation.eulerAngles.y, 0f);
					cacheIndex = (cacheIndex + 1) % (cacheLength - 1);
				}
			}

			// End
			BlockShakeRefreshed = !hasSelection;
			SceneView.RepaintAll();

		}


		private static void RefreshBlockShakeCache () {
			if (!UseBlockShake.Value) { return; }
			BLOCK_SHAKE_CACHE_PARAM[0] = Random.Range(6f, 8f);
			BLOCK_SHAKE_CACHE_PARAM[1] = Random.Range(64f, 72f);
		}


		// Misc
		private static void SetLerpingCameraPivot (Vector3 pos) => LerpingCameraPivot = pos;


		#endregion




	}
}