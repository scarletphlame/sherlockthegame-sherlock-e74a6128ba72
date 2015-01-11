﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Fungus
{
	public class Glyph
	{
		public float hideTimer;
		public string character;
		public bool boldActive;
		public bool italicActive;
		public bool colorActive;
		public string colorText;
		public bool hasPunctuationPause;
		public float speed;
	}
	
	public class DialogText
	{
		protected List<Glyph> glyphs = new List<Glyph>();

		public bool boldActive { get; set; }
		public bool italicActive { get; set; }
		public bool colorActive { get; set; }
		public string colorText { get; set; }
		public float defaultSpeed { get; set; }
		public float speed { get; set; }
		public float punctuationPause { get; set; }
		public AudioSource typingAudio { get; set; }
		public bool slowBeeps { get; set; }
		public float slowBeepsAt { get; set; }
		public float fastBeepsAt { get; set; }

		public virtual void Clear()
		{
			glyphs.Clear();
		}

		public virtual void Append(string words)
		{
			if (speed < slowBeepsAt || speed > fastBeepsAt) // beeps match character speed at these speeds
				slowBeeps = true;
			if (typingAudio != null)
			{
				typingAudio.Stop();
				if (!slowBeeps)
					typingAudio.Play();
			}

			bool doPunctuationPause = false;
			for (int i = 0; i < words.Length; ++i)
			{
				char c = words[i];

				// Ignore leading newlines
				if (glyphs.Count == 0 && c == '\n')
				{
					continue;
				}

				Glyph glyph = new Glyph();
				glyph.speed = speed;
				float hideTimer = 0f;
				if (speed > 0f)
				{
					hideTimer = 1f / speed;
				}
				glyph.hideTimer = hideTimer;
				if (doPunctuationPause)
				{
					glyph.hasPunctuationPause = true;
					glyph.hideTimer += punctuationPause;
					doPunctuationPause = false;
				}
				glyph.character = c.ToString();
				glyph.boldActive = boldActive;
				glyph.italicActive = italicActive;
				glyph.colorActive = colorActive;
				glyph.colorText = colorText;
				glyphs.Add(glyph);

				// Special case: pause just before open parentheses
				if (i < words.Length - 2)
				{
					if (words[i + 1] == '(')
					{
						doPunctuationPause = true;
					}
				}
			}
		}

		protected virtual bool IsPunctuation(char character)
		{
			return character == '.' || 
				character == '?' ||  
				character == '!' || 
				character == ',' ||
				character == ':' ||
				character == ';' ||
				character == ')';
		}

		/**
		 * Returns true when all glyphs are visible.
		 */
		public virtual bool UpdateGlyphs(bool instantComplete)
		{
			float elapsedTime = Time.deltaTime;

			foreach (Glyph glyph in glyphs)
			{
				if (instantComplete)
				{
					glyph.hideTimer = 0f;
					continue;
				}

				if (glyph.hideTimer > 0f)
				{
					if (typingAudio != null && glyph.hasPunctuationPause )
					{
						typingAudio.volume = 0f;
					}

					bool finished = false;
					if (elapsedTime > glyph.hideTimer)
					{
						elapsedTime -= glyph.hideTimer;
						glyph.hideTimer = 0f;
						// Some elapsed time left over, so carry on to next glyph
						if (slowBeeps && typingAudio != null)
						{
							if(!typingAudio.isPlaying && 
							   (glyph.character != " " && glyph.character != "\t" && glyph.character != "\n" ) )
							{
								typingAudio.PlayOneShot(typingAudio.clip);
							}
						}
					}
					else
					{
						glyph.hideTimer -= elapsedTime;
						glyph.hideTimer = Mathf.Max(glyph.hideTimer, 0f);
						finished = true;
					}

					// Check if we need to restore audio after a punctuation pause
					if (typingAudio != null &&
					    glyph.hideTimer == 0f &&
					    typingAudio.volume == 0f)
					{
						typingAudio.volume = 1f;
					}

					if (finished)
					{
						return false; // Glyph is still hidden
					}
				}
			}

			if (typingAudio != null)
			{
				typingAudio.Stop();
			}

			return true;
		}

		public virtual string GetDialogText()
		{
			string outputText = "";

			bool hideGlyphs = false;
			foreach (Glyph glyph in glyphs)
			{
				// Wrap each individual character in rich text markup tags (if required)
				string start = "";
				string end = "";
				if (glyph.boldActive)
				{
					start += "<b>"; 
					end += "</b>";
				}
				if (glyph.italicActive)
				{
					start += "<i>"; 
					end = "</i>" + end; // Have to nest tags correctly 
				}
				
				if (!hideGlyphs && 
				    glyph.hideTimer > 0f)
				{
					hideGlyphs = true;
					outputText += "<color=#FFFFFF00>";
				}

				if (!hideGlyphs && 
				    glyph.colorActive)
				{
					start += "<color=" + glyph.colorText + ">"; 
					end += "</color>"; 
				}
				outputText += start + glyph.character + end;
			}

			if (hideGlyphs)
			{
				outputText += "</color>";
			}

			return outputText;
		}
	}

}