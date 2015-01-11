using UnityEditor;
using UnityEngine;
using System.Collections;

namespace Fungus
{

	[CustomEditor (typeof(Character))]
	public class CharacterEditor : Editor
	{
		protected Material spriteMaterial;

		protected SerializedProperty nameTextProp;
		protected SerializedProperty nameColorProp;
		protected SerializedProperty soundEffectProp;
		protected SerializedProperty portraitsProp;
		protected SerializedProperty portraitsFaceProp;
		protected SerializedProperty notesProp;

		protected virtual void OnEnable()
		{
			nameTextProp = serializedObject.FindProperty ("nameText");
			nameColorProp = serializedObject.FindProperty ("nameColor");
			soundEffectProp = serializedObject.FindProperty ("soundEffect");
			portraitsProp = serializedObject.FindProperty ("portraits");
			portraitsFaceProp = serializedObject.FindProperty ("portraitsFace");
			notesProp = serializedObject.FindProperty ("notes");

			Shader shader = Shader.Find("Sprites/Default");
			if (shader != null)
			{
				spriteMaterial = new Material(shader);
				spriteMaterial.hideFlags = HideFlags.DontSave;
			}
		}

		protected virtual void OnDisable()
		{
			DestroyImmediate(spriteMaterial);
		}

		public override void OnInspectorGUI() 
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(nameTextProp, new GUIContent("Name Text", "Name of the character display in the dialog"));
			EditorGUILayout.PropertyField(nameColorProp, new GUIContent("Name Color", "Color of name text display in the dialog"));
			EditorGUILayout.PropertyField(soundEffectProp, new GUIContent("Sound Effect", "Sound to play when the character is talking. Overrides the setting in the Dialog."));
			EditorGUILayout.PropertyField(portraitsProp, new GUIContent("Portraits", "Character image sprites to display in the dialog"),true);
			string[] facingArrows = new string[]
			{
				"FRONT",
				"<--",
				"-->",
			};
			portraitsFaceProp.enumValueIndex = EditorGUILayout.Popup("Portraits Face", (int)portraitsFaceProp.enumValueIndex, facingArrows);
			EditorGUILayout.PropertyField(notesProp, new GUIContent("Notes", "Notes about this story character (personality, attibutes, etc.)"));

			EditorGUILayout.Separator();

			Character t = target as Character;

			if (t.portraits != null &&
			    t.portraits.Count > 0)
			{
				t.profileSprite = t.portraits[0];
			}
			else
			{
				t.profileSprite = null;
			}

			if (t.profileSprite != null &&
			    spriteMaterial != null)
			{
				float aspect = (float)t.profileSprite.texture.width / (float)t.profileSprite.texture.height;
				Rect imagePreviewRect = GUILayoutUtility.GetAspectRect(aspect, GUILayout.Width(100), GUILayout.ExpandWidth(true));

				DrawPreview(imagePreviewRect, t.profileSprite.texture);
			}

			serializedObject.ApplyModifiedProperties();
		}

		public virtual void DrawPreview(Rect previewRect, Texture2D texture)
		{
			if (texture == null)
			{
				return;
			}
			EditorGUI.DrawPreviewTexture(previewRect, 
			                             texture,
			                             spriteMaterial);
		}
	}

}