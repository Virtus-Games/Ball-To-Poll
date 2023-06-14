namespace JujubeMapEditor.Core {
	using UnityEngine;
	public abstract class JBlock : MonoBehaviour {
		public abstract void OnBlockLoaded (JujubeRenderer renderer, JujubeBlock block);
		public virtual string GetPaletteLabel () => "J";
		public virtual bool AllowRotate () => true;
	}
}