using System;
using System.Collections.Generic;
using Peppermint.DataBinding;

// Generated class
namespace Peppermint.DataBinding.Example
{
	public partial class AotGeneratedClass
	{
		#region AOT Type Registration
		
		private void RegisterProperties()
		{
			AotUtility.RegisterProperty<object, object>();
			AotUtility.RegisterProperty<object, bool>();
			AotUtility.RegisterProperty<object, double>();
			AotUtility.RegisterProperty<object, float>();
			AotUtility.RegisterProperty<object, int>();
			AotUtility.RegisterProperty<object, long>();
			AotUtility.RegisterProperty<object, Peppermint.DataBinding.Example.BindableClassExample.ShapeType>();
			AotUtility.RegisterProperty<object, Peppermint.DataBinding.Example.ColorTag>();
			AotUtility.RegisterProperty<object, Peppermint.DataBinding.Example.LevelSelectMenu.LevelStarNumber>();
			AotUtility.RegisterProperty<object, Peppermint.DataBinding.Example.SelectorExample.EnumOption.Preset>();
			AotUtility.RegisterProperty<object, Peppermint.DataBinding.Example.SetterExample.Rarity>();
			AotUtility.RegisterProperty<object, Peppermint.DataBinding.Example.ShopItemType>();
			AotUtility.RegisterProperty<object, UnityEngine.SystemLanguage>();
			AotUtility.RegisterProperty<object, System.DateTime>();
			AotUtility.RegisterProperty<object, System.Nullable<int>>();
			AotUtility.RegisterProperty<object, System.Nullable<Peppermint.DataBinding.Example.ComponentCategory>>();
			AotUtility.RegisterProperty<object, System.Nullable<UnityEngine.Color>>();
			AotUtility.RegisterProperty<object, UnityEngine.Color>();
			AotUtility.RegisterProperty<object, UnityEngine.Quaternion>();
			AotUtility.RegisterProperty<object, UnityEngine.Vector2>();
			AotUtility.RegisterProperty<object, UnityEngine.Vector3>();
		}
		
		private void RegisterImplicitOperators()
		{
			AotUtility.RegisterImplicitOperator<UnityEngine.LayerMask, int>();
			AotUtility.RegisterImplicitOperator<int, UnityEngine.LayerMask>();
			AotUtility.RegisterImplicitOperator<UnityEngine.Vector3, UnityEngine.Vector2>();
			AotUtility.RegisterImplicitOperator<UnityEngine.Vector2, UnityEngine.Vector3>();
			AotUtility.RegisterImplicitOperator<UnityEngine.Color, UnityEngine.Vector4>();
			AotUtility.RegisterImplicitOperator<UnityEngine.Vector4, UnityEngine.Color>();
			AotUtility.RegisterImplicitOperator<UnityEngine.Color, UnityEngine.Color32>();
			AotUtility.RegisterImplicitOperator<UnityEngine.Color32, UnityEngine.Color>();
			AotUtility.RegisterImplicitOperator<UnityEngine.Vector3, UnityEngine.Vector4>();
			AotUtility.RegisterImplicitOperator<UnityEngine.Vector4, UnityEngine.Vector3>();
			AotUtility.RegisterImplicitOperator<UnityEngine.Vector2, UnityEngine.Vector4>();
			AotUtility.RegisterImplicitOperator<UnityEngine.Vector4, UnityEngine.Vector2>();
		}
		
		#endregion
	}
}
