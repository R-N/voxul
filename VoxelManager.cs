﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Voxul.Utilities;

namespace Voxul
{
	public class VoxelManager : ScriptableObject
	{
		public const string RESOURCES_FOLDER = "voxul";
		public static VoxelManager Instance => m_instance.Value;
		private static LazyReference<VoxelManager> m_instance = new LazyReference<VoxelManager>(BuildVoxelManager);

		static VoxelManager BuildVoxelManager()
		{
			var path = $"{RESOURCES_FOLDER}/{nameof(VoxelManager)}";
			var vm = Resources.Load<VoxelManager>(path);
			if (!vm)
			{
#if UNITY_EDITOR
				var scriptPath = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets($"{nameof(VoxelManager)} t:script").First());
				var rootPath = Application.dataPath.Replace("/Assets", "/");
				var rscPath = Regex.Replace(scriptPath, $"\\/{nameof(VoxelManager)}.cs$", $"/Resources/{RESOURCES_FOLDER}");
				if (!Directory.Exists(rootPath + rscPath))
				{
					Directory.CreateDirectory(rootPath + rscPath);
				}
				AssetDatabase.CreateAsset(CreateInstance<VoxelManager>(), $"{rscPath}/{nameof(VoxelManager)}.asset");
				vm = Resources.Load<VoxelManager>(path);
#else
				throw new Exception($"Could not find VoxelManager resource at {path}");
#endif
			}
			return vm;
		}

		[Range(8, 1024)]
		public int SpriteResolution = 32;
		public Material DefaultMaterial;
		public Material DefaultMaterialTransparent;
		public Texture2DArray BaseTextureArray;
		public List<Texture2D> Sprites = new List<Texture2D>();
		public Mesh CubeMesh;
		public Material LODMaterial;

		[ContextMenu("Regenerate Spritesheet")]
		public void RegenerateSpritesheet()
		{
#if UNITY_EDITOR
			var texArray = BaseTextureArray;
			var newArray = GenerateArray(Sprites, TextureFormat.ARGB32, SpriteResolution);
			newArray.filterMode = FilterMode.Point;
			newArray.wrapMode = TextureWrapMode.Repeat;
			var currentPath = texArray ? AssetDatabase.GetAssetPath(texArray) : $"{AssetDatabase.GetAssetPath(this)}_spritesheet";
			var tmpPath = AssetCreationHelper.CreateAssetInCurrentDirectory(newArray, "tmp.asset");
			File.WriteAllBytes(currentPath, File.ReadAllBytes(tmpPath));
			AssetDatabase.DeleteAsset(tmpPath);
			AssetDatabase.ImportAsset(currentPath);
			DefaultMaterial.SetTexture("AlbedoSpritesheet", texArray);
			DefaultMaterialTransparent.SetTexture("AlbedoSpritesheet", texArray);
#endif
		}

		static Texture2D ResizeTexture(Texture2D texture2D, int targetX, int targetY)
		{
			RenderTexture rt = new RenderTexture(targetX, targetY, 24);
			RenderTexture.active = rt;
			Graphics.Blit(texture2D, rt);
			Texture2D result = new Texture2D(targetX, targetY);
			result.ReadPixels(new Rect(0, 0, targetX, targetY), 0, 0);
			result.Apply();
			return result;
		}

		static Texture2DArray GenerateArray(IList<Texture2D> textures, TextureFormat format, int size)
		{
			var texture2DArray = new Texture2DArray(size, size, textures.Count, format, false);
			for (int i = 0; i < textures.Count; i++)
			{
				var tex = textures[i];
				if (tex.height != size || tex.width != size)
				{
					tex = ResizeTexture(tex, size, size);
				}
				texture2DArray.SetPixels(tex.GetPixels(), i);
			}

			texture2DArray.Apply();

			return texture2DArray;
		}
	}
}