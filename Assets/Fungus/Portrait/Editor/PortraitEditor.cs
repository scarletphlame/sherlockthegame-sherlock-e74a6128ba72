using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using Rotorz.ReorderableList;


namespace Fungus
{
	
	[CustomEditor (typeof(Portrait))]
	public class PortraitEditor : CommandEditor
	{
		protected SerializedProperty portraitStageProp;
		protected SerializedProperty displayProp;
		protected SerializedProperty characterProp;
		protected SerializedProperty replacedCharacterProp;
		protected SerializedProperty portraitProp;
		protected SerializedProperty offsetProp;
		protected SerializedProperty fromPositionProp;
		protected SerializedProperty toPositionProp;
		protected SerializedProperty facingProp;
		protected SerializedProperty useDefaultSettingsProp;
		protected SerializedProperty fadeDurationProp;
		protected SerializedProperty moveSpeedProp;
		protected SerializedProperty slideOffsetProp;
		protected SerializedProperty waitUntilFinishedProp;
		protected SerializedProperty moveProp;
		protected SerializedProperty startFromOffsetProp;
		
		protected virtual void OnEnable()
		{
			portraitStageProp = serializedObject.FindProperty("portraitStage");
			displayProp = serializedObject.FindProperty("display");
			characterProp = serializedObject.FindProperty("character");
			replacedCharacterProp = serializedObject.FindProperty("replacedCharacter");
			portraitProp = serializedObject.FindProperty("portrait");
			offsetProp = serializedObject.FindProperty("offset");
			fromPositionProp = serializedObject.FindProperty("fromPosition");
			toPositionProp = serializedObject.FindProperty("toPosition");
			facingProp = serializedObject.FindProperty("facing");
			useDefaultSettingsProp = serializedObject.FindProperty("useDefaultSettings");
			fadeDurationProp = serializedObject.FindProperty("fadeDuration");
			moveSpeedProp = serializedObject.FindProperty("moveSpeed");
			slideOffsetProp = serializedObject.FindProperty("slideOffset");
			waitUntilFinishedProp = serializedObject.FindProperty("waitUntilFinished");
			moveProp = serializedObject.FindProperty("move");
			startFromOffsetProp = serializedObject.FindProperty("startFromOffset");
		}
		
		public override void DrawCommandGUI() 
		{
			serializedObject.Update();
			
			Portrait t = target as Portrait;

			CommandEditor.ObjectField<PortraitStage>(portraitStageProp, 
			                                         new GUIContent("Portrait Stage", "Stage to display the character portraits on"), 
			                                         new GUIContent("<Default>"),
			                                         PortraitStage.activePortraitStages);
			// Replace NULL with <None>
			string[] displayLabels = Enum.GetNames(typeof(displayType));
			displayLabels[0] = "<None>";
			displayProp.enumValueIndex = EditorGUILayout.Popup("Display", (int)displayProp.enumValueIndex, displayLabels);

			string characterLabel = "Character";
			if (t.display == displayType.Swap)
			{
				CommandEditor.ObjectField<Character>(replacedCharacterProp, 
				                                     new GUIContent("Replace", "Character to swap with"), 
				                                     new GUIContent("<None>"),
				                                     Character.activeCharacters);
				characterLabel = "With";
			}
			
			CommandEditor.ObjectField<Character>(characterProp, 
			                                     new GUIContent(characterLabel, "Character to display"), 
			                                     new GUIContent("<None>"),
			                                     Character.activeCharacters);

			bool showOptionalFields = true;
			PortraitStage ps = t.portraitStage;
			// Only show optional portrait fields once required fields have been filled...
			if (t.character != null)                // Character is selected
			{
				if (t.character.portraits == null ||    // Character has a portraits field
				    t.character.portraits.Count <= 0 )   // Character has at least one portrait
				{
					EditorGUILayout.HelpBox("This character has no portraits. Please add portraits to the character's prefab before using this command.", MessageType.Error);
					showOptionalFields = false; 
				}
				if (t.portraitStage == null)            // If default portrait stage selected
				{
					ps = t.GetFungusScript().portraitStage;;  // Try to get game's default portrait stage
					if (t.portraitStage == null)        // If no default specified, try to get any portrait stage in the scene
					{
						ps = GameObject.FindObjectOfType<PortraitStage>();
					}
				}
				if (ps == null)
				{
					EditorGUILayout.HelpBox("No portrait stage has been set. Please create a new portrait stage using [Game Object > Fungus > Portrait > Portrait Stage].", MessageType.Error);
					showOptionalFields = false; 
				}
			}
			if (t.display != displayType.NULL && t.character != null && showOptionalFields) 
			{
				if (t.display != displayType.Hide && t.display != displayType.MoveToFront) 
				{
					// PORTRAIT
					CommandEditor.ObjectField<Sprite>(portraitProp, 
					                                  new GUIContent("Portrait", "Portrait representing character"), 
					                                  new GUIContent("<Previous>"),
					                                  t.character.portraits);
					if (t.character.portraitsFace != facingDirection.NULL)
					{
						// FACING
						// Display the values of the facing enum as <-- and --> arrows to avoid confusion with position field
						string[] facingArrows = new string[]
						{
							"<Previous>",
							"<--",
							"-->",
						};
						facingProp.enumValueIndex = EditorGUILayout.Popup("Facing", (int)facingProp.enumValueIndex, facingArrows);
					}
					else
					{
						t.facing = facingDirection.NULL;
					}
				}
				else
				{
					t.portrait = null;
					t.facing = facingDirection.NULL;
				}
				string toPositionPrefix = "";
				if (t.move)
				{
					// MOVE
					EditorGUILayout.PropertyField(moveProp);
				}
				if (t.move)
				{
					if (t.display != displayType.Hide) 
					{
						// START FROM OFFSET
						EditorGUILayout.PropertyField(startFromOffsetProp);
					}
				}
				if (t.move)
				{
					if (t.display != displayType.Hide) 
					{
						if (t.startFromOffset)
						{
							t.fromPosition = null;
							// OFFSET
							// Replace NULL with <Previous>
							string[] offsetLabels = Enum.GetNames(typeof(positionOffset));
							offsetLabels[0] = "<None>";
							offsetProp.enumValueIndex = EditorGUILayout.Popup("From Offset", (int)offsetProp.enumValueIndex, offsetLabels);
						}
						else
						{
							t.offset = positionOffset.NULL;
							// FROM POSITION
							CommandEditor.ObjectField<RectTransform>(fromPositionProp, 
							                                         new GUIContent("From Position", "Move the portrait to this position"), 
							                                         new GUIContent("<Previous>"),
							                                         ps.positions);
						}
					}
					toPositionPrefix = "To ";
				}
				else
				{
					t.startFromOffset = false;
					t.fromPosition = null;
					toPositionPrefix = "At ";
				}
				if (t.display == displayType.Show || (t.display == displayType.Hide && t.move) )
				{
					// TO POSITION
					CommandEditor.ObjectField<RectTransform>(toPositionProp, 
					                                         new GUIContent(toPositionPrefix+"Position", "Move the portrait to this position"), 
					                                         new GUIContent("<Previous>"),
					                                         ps.positions);
				}
				else
				{
					t.toPosition = null;
				}
				if (!t.move && t.display != displayType.MoveToFront)
				{
					// MOVE
					EditorGUILayout.PropertyField(moveProp);
				}
				if (t.display != displayType.MoveToFront)
				{
				
					EditorGUILayout.Separator();

					// USE DEFAULT SETTINGS
					EditorGUILayout.PropertyField(useDefaultSettingsProp);
					if (!t.useDefaultSettings) {
						// FADE DURATION
						EditorGUILayout.PropertyField(fadeDurationProp);
						if (t.move)
						{
							// MOVE SPEED
							EditorGUILayout.PropertyField(moveSpeedProp);
						}
						if (t.startFromOffset)
						{
							// SLIDE OFFSET
							EditorGUILayout.PropertyField(slideOffsetProp);
						}
					}
				}
				else
				{
					t.move = false;
					t.useDefaultSettings = true;
					EditorGUILayout.Separator();
				}

				EditorGUILayout.PropertyField(waitUntilFinishedProp);

				if (t.portrait != null && t.display != displayType.Hide)
				{
					Texture2D characterTexture = t.portrait.texture;
					float aspect = (float)characterTexture.width / (float)characterTexture.height;
					Rect previewRect = GUILayoutUtility.GetAspectRect(aspect, GUILayout.Width(100), GUILayout.ExpandWidth(true));
					CharacterEditor characterEditor = Editor.CreateEditor(t.character) as CharacterEditor;
					characterEditor.DrawPreview(previewRect, characterTexture);
					DestroyImmediate(characterEditor);
				}
			}
			serializedObject.ApplyModifiedProperties();
		}
	}
}