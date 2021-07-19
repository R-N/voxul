﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Voxul.Utilities
{
	[Serializable]
	public struct SerializableGradient
	{
		[Serializable]
		public struct ColorKey
		{
			public Color Color;
			public float Time;

			public ColorKey(GradientColorKey k)
			{
				Color = k.color;
				Time = k.time;
			}
			public GradientColorKey ToColorKey() => new GradientColorKey { color = Color, time = Time };
		}

		[Serializable]
		public struct AlphaKey
		{
			public float Alpha;
			public float Time;

			public AlphaKey(GradientAlphaKey k)
			{
				Alpha = k.alpha;
				Time = k.time;
			}
			public GradientAlphaKey ToAlphaKey() => new GradientAlphaKey { alpha = Alpha, time = Time };
		}

		public ColorKey[] colorKeys;
		public AlphaKey[] alphaKeys;
		public GradientMode mode;

		public SerializableGradient(Gradient defaultGradient)
		{
			colorKeys = defaultGradient.colorKeys.Select(s => new ColorKey(s)).ToArray();
			alphaKeys = defaultGradient.alphaKeys.Select(s => new AlphaKey(s)).ToArray();
			mode = defaultGradient.mode;
		}

		public Gradient ToGradient() => new Gradient
		{
			colorKeys = colorKeys == null ? new GradientColorKey[0] : colorKeys.Select(s => s.ToColorKey()).ToArray(),
			alphaKeys = alphaKeys == null ? new GradientAlphaKey[0] : alphaKeys.Select(s => s.ToAlphaKey()).ToArray(),
			mode = mode,
		};

		public override bool Equals(object obj)
		{
			return obj is SerializableGradient gradient &&
				   EqualityComparer<ColorKey[]>.Default.Equals(colorKeys, gradient.colorKeys) &&
				   EqualityComparer<AlphaKey[]>.Default.Equals(alphaKeys, gradient.alphaKeys) &&
				   mode == gradient.mode;
		}

		public override int GetHashCode()
		{
			int hashCode = 1860550585;
			hashCode = hashCode * -1521134295 + EqualityComparer<ColorKey[]>.Default.GetHashCode(colorKeys);
			hashCode = hashCode * -1521134295 + EqualityComparer<AlphaKey[]>.Default.GetHashCode(alphaKeys);
			hashCode = hashCode * -1521134295 + mode.GetHashCode();
			return hashCode;
		}
	}
}