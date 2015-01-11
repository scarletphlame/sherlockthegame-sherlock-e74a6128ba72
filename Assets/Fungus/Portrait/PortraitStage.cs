using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Fungus
{
	public class PortraitStage : MonoBehaviour 
	{
		public Canvas portraitCanvas;
		public bool dimPortraits;
		public float fadeDuration;
		public float moveSpeed;
		public LeanTweenType fadeEaseType;
		public LeanTweenType moveEaseType;
		public Vector2 slideOffset;
		public Image defaultPosition;
		public List<RectTransform> positions;

		[HideInInspector]
		static public List<PortraitStage> activePortraitStages = new List<PortraitStage>();
		static public List<Character> charactersOnStage = new List<Character>();

		protected virtual void Start()
		{
			foreach (Character c in Character.activeCharacters)
			{
				if (c.portraits.Count > 0 )   // Character has at least one portrait
				{
					Portrait.CreatePortraitObject(c, this);
				}
			}
		}
		[ExecuteInEditMode]
		protected virtual void OnEnable()
		{
			if (!activePortraitStages.Contains(this))
			{
				activePortraitStages.Add(this);
			}
		}
		[ExecuteInEditMode]
		protected virtual void OnDisable()
		{
			activePortraitStages.Remove(this);
		}
	}
}

