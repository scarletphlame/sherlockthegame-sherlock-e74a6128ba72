using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace Fungus
{
	[ExecuteInEditMode]
	public class Character : MonoBehaviour 
	{
		public string nameText; // We need a separate name as the object name is used for character variations (e.g. "Smurf Happy", "Smurf Sad")
		public Color nameColor = Color.white;
		public AudioClip soundEffect;
		public Sprite profileSprite;
		public List<Sprite> portraits;
		public facingDirection portraitsFace;	
		public portraitState state;		

		[TextArea(5,10)]
		public string notes;

		static public List<Character> activeCharacters = new List<Character>();
		static public List<Sprite> activePortraits = new List<Sprite>();

		protected virtual void OnEnable()
		{
			if (!activeCharacters.Contains(this))
			{
				activeCharacters.Add(this);
				foreach (Sprite portrait in this.portraits)
				{
					activePortraits.Add(portrait);
				}
			}
		}
		
		protected virtual void OnDisable()
		{
			activeCharacters.Remove(this);
			foreach (Sprite portrait in this.portraits)
			{
				activePortraits.Remove(portrait);
			}
		}
	}

}