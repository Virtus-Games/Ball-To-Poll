namespace JujubeMapEditor.Editor {
	using UnityEngine;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using System.Linq;
	using System.Runtime.Serialization.Formatters.Binary;
	using UnityEditor;


	public struct EditorUtil {




		#region --- File ---


		public static string FileToText (string path) {
			StreamReader sr = File.OpenText(path);
			string data = sr.ReadToEnd();
			sr.Close();
			return data;
		}


		public static void TextToFile (string data, string path) {
			FileStream fs = new FileStream(path, FileMode.Create);
			StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
			sw.Write(data);
			sw.Close();
			fs.Close();
		}


		public static void CreateFolder (string path) {
			if (!string.IsNullOrEmpty(path) && !DirectoryExists(path)) {
				string pPath = GetParentPath(path);
				if (!DirectoryExists(pPath)) {
					CreateFolder(pPath);
				}
				Directory.CreateDirectory(path);
			}
		}


		public static byte[] FileToByte (string path) => File.ReadAllBytes(path);


		public static void ByteToFile (byte[] bytes, string path) {
			string parentPath = GetParentPath(path);
			CreateFolder(parentPath);
			FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
			fs.Write(bytes, 0, bytes.Length);
			fs.Close();
			fs.Dispose();
		}


		public static byte[] ObjectToBytes (object obj) {
			if (obj == null) { return new byte[0]; }
			try {
				using (var ms = new MemoryStream()) {
					new BinaryFormatter().Serialize(ms, obj);
					return ms.ToArray();
				}
			} catch (System.Exception ex) {
				Debug.LogError(ex);
			}
			return new byte[0];
		}


		public static object BytesToObject (byte[] bytes) {
			if (bytes == null || bytes.Length == 0) { return null; }
			try {
				using (var memStream = new MemoryStream()) {
					memStream.Write(bytes, 0, bytes.Length);
					memStream.Seek(0, SeekOrigin.Begin);
					var obj = new BinaryFormatter().Deserialize(memStream);
					return obj;
				}
			} catch (System.Exception ex) {
				Debug.LogError(ex);
			}
			return null;
		}


		public static bool HasFileIn (string path, params string[] searchPattern) {
			if (PathIsDirectory(path)) {
				for (int i = 0; i < searchPattern.Length; i++) {
					if (new DirectoryInfo(path).GetFiles(searchPattern[i], SearchOption.AllDirectories).Length > 0) {
						return true;
					}
				}
			}
			return false;
		}


		public static FileInfo[] GetFilesIn (string path, bool topOnly, params string[] searchPattern) {
			var allFiles = new List<FileInfo>();
			if (PathIsDirectory(path)) {
				var option = topOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories;
				if (searchPattern.Length == 0) {
					allFiles.AddRange(new DirectoryInfo(path).GetFiles("*", option));
				} else {
					for (int i = 0; i < searchPattern.Length; i++) {
						allFiles.AddRange(new DirectoryInfo(path).GetFiles(searchPattern[i], option));
					}
				}
			}
			return allFiles.ToArray();
		}


		public static DirectoryInfo[] GetDirectsIn (string path, bool topOnly) {
			var allDirs = new List<DirectoryInfo>();
			if (PathIsDirectory(path)) {
				allDirs.AddRange(new DirectoryInfo(path).GetDirectories("*", topOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories));
			}
			return allDirs.ToArray();
		}


		public static void DeleteFile (string path) {
			if (FileExists(path)) {
				File.Delete(path);
			}
		}


		public static void CopyFile (string from, string to) {
			if (FileExists(from)) {
				File.Copy(from, to, true);
			}
		}


		public static bool CopyDirectory (string from, string to, bool copySubDirs, bool ignoreHidden) {

			// Get the subdirectories for the specified directory.
			DirectoryInfo dir = new DirectoryInfo(from);

			if (!dir.Exists) {
				return false;
			}

			DirectoryInfo[] dirs = dir.GetDirectories();
			// If the destination directory doesn't exist, create it.
			if (!Directory.Exists(to)) {
				Directory.CreateDirectory(to);
			}

			// Get the files in the directory and copy them to the new location.
			FileInfo[] files = dir.GetFiles();
			foreach (FileInfo file in files) {
				try {
					string temppath = Path.Combine(to, file.Name);
					if (!ignoreHidden || (file.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden) {
						file.CopyTo(temppath, false);
					}
				} catch { }
			}

			// If copying subdirectories, copy them and their contents to new location.
			if (copySubDirs) {
				foreach (DirectoryInfo subdir in dirs) {
					try {
						string temppath = Path.Combine(to, subdir.Name);
						if (!ignoreHidden || (subdir.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden) {
							CopyDirectory(subdir.FullName, temppath, copySubDirs, ignoreHidden);
						}
					} catch { }
				}
			}
			return true;
		}


		public static void DeleteDirectory (string path) {
			if (DirectoryExists(path)) {
				Directory.Delete(path, true);
			}
		}


		public static void DeleteAllFilesIn (string path) {
			if (DirectoryExists(path)) {
				var files = GetFilesIn(path, false, "*");
				foreach (var file in files) {
					DeleteFile(file.FullName);
				}
			}
		}


		public static float GetFileSizeInMB (string path) {
			float size = -1f;
			if (FileExists(path)) {
				size = (new FileInfo(path).Length / 1024f) / 1024f;
			}
			return size;
		}


		public static int GetFileCount (string path, string search = "", SearchOption option = SearchOption.TopDirectoryOnly) {
			if (DirectoryExists(path)) {
				return Directory.EnumerateFiles(path, search, option).Count();
			}
			return 0;
		}


		public static void MoveFile (string from, string to) {
			if (from != to && FileExists(from)) {
				File.Move(from, to);
			}
		}


		public static bool MoveDirectory (string from, string to) {
			if (from != to && DirectoryExists(from)) {
				try {
					Directory.Move(from, to);
					return true;
				} catch { }
			}
			return false;
		}


		#endregion




		#region --- Path ---


		private const string ROOT_NAME = "Jujube Map Editor";
		private static string ROOT_PATH = "";


		public static string GetRootPath () {
			if (!string.IsNullOrEmpty(ROOT_PATH) && DirectoryExists(ROOT_PATH)) { return ROOT_PATH; }
			var paths = AssetDatabase.GetAllAssetPaths();
			foreach (var path in paths) {
				if (PathIsDirectory(path) && GetNameWithoutExtension(path) == ROOT_NAME) {
					ROOT_PATH = FixedRelativePath(path);
					break;
				}
			}
			return ROOT_PATH;
		}


		public static Texture2D GetImage (string name_ex) {
			Texture2D result = null;
			string path = CombinePaths(GetRootPath(), "Editor", "Image", name_ex);
			if (FileExists(path)) {
				result = AssetDatabase.LoadAssetAtPath<Texture2D>(FixedRelativePath(path));
			}
			return result;
		}


		public static string GetParentPath (string path) => Directory.GetParent(path).FullName;


		public static string GetFullPath (string path) => new FileInfo(path).FullName;


		public static string GetDirectoryFullPath (string path) => new DirectoryInfo(path).FullName;


		public static string CombinePaths (params string[] paths) {
			string path = "";
			for (int i = 0; i < paths.Length; i++) {
				path = Path.Combine(path, paths[i]);
			}
			return path;
		}


		public static string GetExtension (string path) => Path.GetExtension(path);//.txt


		public static string GetNameWithoutExtension (string path) => Path.GetFileNameWithoutExtension(path);


		public static string GetNameWithExtension (string path) => Path.GetFileName(path);


		public static string ChangeExtension (string path, string newEx) => Path.ChangeExtension(path, newEx);


		public static bool DirectoryExists (string path) => Directory.Exists(path);


		public static bool FileExists (string path) => !string.IsNullOrEmpty(path) && File.Exists(path);


		public static bool PathIsDirectory (string path) {
			if (!DirectoryExists(path)) { return false; }
			FileAttributes attr = File.GetAttributes(path);
			if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
				return true;
			else
				return false;
		}


		public static string GetUrl (string path) => string.IsNullOrEmpty(path) ? "" : new System.Uri(path).AbsoluteUri;


		public static string FixPath (string path, bool forUnity = true) {
			char dsChar = forUnity ? '/' : Path.DirectorySeparatorChar;
			char adsChar = forUnity ? '\\' : Path.AltDirectorySeparatorChar;
			path = path.Replace(adsChar, dsChar);
			path = path.Replace(new string(dsChar, 2), dsChar.ToString());
			while (path.Length > 0 && path[0] == dsChar) {
				path = path.Remove(0, 1);
			}
			while (path.Length > 0 && path[path.Length - 1] == dsChar) {
				path = path.Remove(path.Length - 1, 1);
			}
			return path;
		}


		public static string FixedRelativePath (string path) {
			path = FixPath(path);
			if (path.StartsWith("Assets")) {
				return path;
			}
			var fixedDataPath = FixPath(Application.dataPath);
			if (path.StartsWith(fixedDataPath)) {
				return "Assets" + path.Substring(fixedDataPath.Length);
			} else {
				return "";
			}
		}


		public static bool IsChildPath (string pathA, string pathB) {
			if (pathA.Length == pathB.Length) {
				return pathA == pathB;
			} else if (pathA.Length > pathB.Length) {
				return IsChildPathCompar(pathA, pathB);
			} else {
				return IsChildPathCompar(pathB, pathA);
			}
		}


		public static bool IsChildPathCompar (string longPath, string path) {
			if (longPath.Length <= path.Length || !PathIsDirectory(path) || !longPath.StartsWith(path)) {
				return false;
			}
			char c = longPath[path.Length];
			if (c != Path.DirectorySeparatorChar && c != Path.AltDirectorySeparatorChar) {
				return false;
			}
			return true;
		}


		#endregion




		#region --- Message ---


		public static bool Dialog (string title, string msg, string ok, string cancel = "") {
			PauseWatch();
			if (string.IsNullOrEmpty(cancel)) {
				bool sure = EditorUtility.DisplayDialog(title, msg, ok);
				RestartWatch();
				return sure;
			} else {
				bool sure = EditorUtility.DisplayDialog(title, msg, ok, cancel);
				RestartWatch();
				return sure;
			}
		}


		public static int DialogComplex (string title, string msg, string ok, string cancel, string alt) {
			//EditorApplication.Beep();
			PauseWatch();
			int index = EditorUtility.DisplayDialogComplex(title, msg, ok, cancel, alt);
			RestartWatch();
			return index;
		}


		public static void ProgressBar (string title, string msg, float value) {
			value = Mathf.Clamp01(value);
			EditorUtility.ClearProgressBar();
			EditorUtility.DisplayProgressBar(title, msg, value);
		}


		public static void ClearProgressBar () {
			EditorUtility.ClearProgressBar();
		}


		#endregion




		#region --- Watch ---


		private static System.Diagnostics.Stopwatch TheWatch;


		public static void StartWatch () {
			TheWatch = new System.Diagnostics.Stopwatch();
			TheWatch.Start();
		}


		public static void PauseWatch () {
			if (TheWatch != null) {
				TheWatch.Stop();
			}
		}


		public static void RestartWatch () {
			if (TheWatch != null) {
				TheWatch.Start();
			}
		}


		public static double StopWatchAndGetTime () {
			if (TheWatch != null) {
				TheWatch.Stop();
				return TheWatch.Elapsed.TotalSeconds;
			}
			return 0f;
		}


		#endregion




		#region --- Misc ---


		public static string GetTimeString () => System.DateTime.Now.ToString("yyyyMMddHHmmssffff");


		public static long GetLongTime () => System.DateTime.Now.Ticks;


		public static string GetDisplayTimeFromTicks (long ticks) => new System.DateTime(ticks).ToString("yyyy-MM-dd HH:mm");


		public static Vector3 Vector3Lerp3 (Vector3 a, Vector3 b, float x, float y, float z = 0f) => new Vector3(
			Mathf.LerpUnclamped(a.x, b.x, x),
			Mathf.LerpUnclamped(a.y, b.y, y),
			Mathf.LerpUnclamped(a.z, b.z, z)
		);


		public static Vector3 Vector3InverseLerp3 (Vector3 a, Vector3 b, float x, float y, float z = 0f) => new Vector3(
			RemapUnclamped(a.x, b.x, 0f, 1f, x),
			RemapUnclamped(a.y, b.y, 0f, 1f, y),
			RemapUnclamped(a.z, b.z, 0f, 1f, z)
		);


		public static float RemapUnclamped (float l, float r, float newL, float newR, float t) {
			return l == r ? 0 : Mathf.LerpUnclamped(
				newL, newR,
				(t - l) / (r - l)
			);
		}


		public static float Remap (float l, float r, float newL, float newR, float t) {
			return l == r ? l : Mathf.Lerp(
				newL, newR,
				(t - l) / (r - l)
			);
		}


		public static Vector3 Remap (float l, float r, Vector3 newL, Vector3 newR, float t) => new Vector3(
			Remap(l, r, newL.x, newR.x, t),
			Remap(l, r, newL.y, newR.y, t),
			Remap(l, r, newL.z, newR.z, t)
		);


		public static float Snap (float value, float count) => count > 0f ?
			Mathf.Round(value * count) / count : value;


		public static float Snap (float value, float count, float offset) => count > 0f ?
			Mathf.Round((value + offset) * count) / count - offset : value;


		public static bool GetBit (int value, int index) {
			if (index < 0 || index > 31) { return false; }
			var val = 1 << index;
			return (value & val) == val;
		}


		public static int SetBitValue (int value, int index, bool bitValue) {
			if (index < 0 || index > 31) { return value; }
			var val = 1 << index;
			return bitValue ? (value | val) : (value & ~val);
		}


		public static void ShowInExplorer (string path) => System.Diagnostics.Process.Start("Explorer.exe", GetFullPath(path));


		public static T SpawnUI<T> (T prefab, RectTransform root, string name = "") where T : MonoBehaviour {
			root.gameObject.SetActive(true);
			root.parent.gameObject.SetActive(true);
			var obj = UnityEngine.Object.Instantiate(prefab, root);
			var rt = obj.transform as RectTransform;
			rt.name = name;
			rt.SetAsLastSibling();
			rt.localRotation = Quaternion.identity;
			rt.localScale = Vector3.one;
			rt.anchorMin = Vector2.zero;
			rt.anchorMax = Vector2.one;
			rt.offsetMin = rt.offsetMax = Vector2.zero;
			return obj;
		}


		public static bool PointInTriangle (float px, float py, float p0x, float p0y, float p1x, float p1y, float p2x, float p2y) {
			var s = p0y * p2x - p0x * p2y + (p2y - p0y) * px + (p0x - p2x) * py;
			var t = p0x * p1y - p0y * p1x + (p0y - p1y) * px + (p1x - p0x) * py;
			if ((s < 0) != (t < 0)) { return false; }
			var A = -p1y * p2x + p0y * (p2x - p1x) + p0x * (p1y - p2y) + p1x * p2y;
			return A < 0 ? (s <= 0 && s + t >= A) : (s >= 0 && s + t <= A);
		}


		public static bool PointInTriangle (Vector2 p, Vector2 a, Vector2 b, Vector2 c) => PointInTriangle(p.x, p.y, a.x, a.y, b.x, b.y, c.x, c.y);


		public static bool GetExpandComponent<T> () where T : Component {
			bool result = false;
			var g = new GameObject("", typeof(T));
			try {
				g.hideFlags = HideFlags.HideAndDontSave;
				result = UnityEditorInternal.InternalEditorUtility.GetIsInspectorExpanded(
					g.GetComponent(typeof(T))
				);
			} catch { }
			Object.DestroyImmediate(g, false);
			return result;
		}


		public static void SetExpandComponent<T> (bool expand) where T : Component {
			var g = new GameObject("", typeof(T));
			try {
				g.hideFlags = HideFlags.HideAndDontSave;
				UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(
					g.GetComponent(typeof(T)), expand
				);
			} catch { }
			Object.DestroyImmediate(g, false);


		}


		public static Texture2D GetFixedAssetPreview (Object obj) {

			var texture = AssetPreview.GetAssetPreview(obj);
			if (texture == null) { return null; }
			int width = texture.width;
			int height = texture.height;
			if (width == 0 || height == 0) { return null; }
			var pixels = texture.GetPixels32();
			int length = width * height;

			// Remove Background
			Color32 CLEAR = new Color32(0, 0, 0, 0);
			var stack = new Stack<(int x, int y)>();
			RemoveColorAt(0, 0);
			RemoveColorAt(width - 1, 0);
			RemoveColorAt(0, height - 1);
			RemoveColorAt(width - 1, height - 1);

			// Fix Color Brightness
			Color32 pixel;
			for (int i = 0; i < length; i++) {
				pixel = pixels[i];
				if (pixel.a == 0) { continue; }
				pixel.r = (byte)Mathf.Clamp((pixel.r - 128f) * 1.5f + 190f, byte.MinValue, byte.MaxValue);
				pixel.g = (byte)Mathf.Clamp((pixel.g - 128f) * 1.5f + 190f, byte.MinValue, byte.MaxValue);
				pixel.b = (byte)Mathf.Clamp((pixel.b - 128f) * 1.5f + 190f, byte.MinValue, byte.MaxValue);
				pixels[i] = pixel;
			}

			// Final
			var result = new Texture2D(width, height, TextureFormat.RGBA32, false);
			result.alphaIsTransparency = true;
			result.filterMode = FilterMode.Point;
			result.SetPixels32(pixels);
			result.Apply();
			return result;

			// === Func ===
			bool SameColor (Color32 colorA, Color32 colorB) =>
				colorA.r == colorB.r &&
				colorA.g == colorB.g &&
				colorA.b == colorB.b &&
				colorA.a == colorB.a;
			void RemoveColorAt (int _x, int _y) {
				var color32 = pixels[_y * width + _x];
				if (color32.a == 0) { return; }
				stack.Clear();
				stack.Push((_x, _y));
				for (int safeCount = 0; safeCount < length * 8 && stack.Count > 0; safeCount++) {
					(int x, int y) = stack.Pop();
					pixels[y * width + x] = CLEAR;
					AddToStack(x, y - 1, color32);
					AddToStack(x, y + 1, color32);
					AddToStack(x - 1, y, color32);
					AddToStack(x + 1, y, color32);
					AddToStack(x - 1, y - 1, color32);
					AddToStack(x - 1, y + 1, color32);
					AddToStack(x + 1, y - 1, color32);
					AddToStack(x + 1, y + 1, color32);
				}
			}
			void AddToStack (int x, int y, Color32 color32) {
				int i = y * width + x;
				if (
					x >= 0 && y >= 0 && x < width && y < height &&
					SameColor(color32, pixels[i])
				) {
					stack.Push((x, y));
				}
			}
		}


		public static void DestroyAllChirldrenImmediate (Transform target) {
			int childCount = target.childCount;
			for (int i = 0; i < childCount; i++) {
				Object.DestroyImmediate(target.GetChild(0).gameObject, false);
			}
		}


		public static bool Vector3Similar (Vector3 a, Vector3 b) => Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y) && Mathf.Approximately(a.z, b.z);


		public static bool IsChildOf (Transform tf, Transform root) {
			while (tf != null && tf != root) {
				tf = tf.parent;
			}
			return tf == root;
		}


		public static bool InRange (Vector3Int pos, Vector3Int min, Vector3Int max) => pos.x >= min.x && pos.y >= min.y && pos.z >= min.z && pos.x <= max.x && pos.y <= max.y && pos.z <= max.z;


		public static void SetHideFlagForAllChildren (Transform target, HideFlags flag) {
			target.gameObject.hideFlags = flag;
			foreach (Transform t in target) {
				SetHideFlagForAllChildren(t, flag);
			}
		}


		#endregion




	}
}