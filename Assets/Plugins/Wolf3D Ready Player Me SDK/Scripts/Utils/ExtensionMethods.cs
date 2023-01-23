using System;
using UnityEngine;
using System.Linq;
using System.Collections;

namespace Wolf3D.ReadyPlayerMe.AvatarSDK
{
    public static class ExtensionMethods
    {
        #region Simple Coroutine
        public class CoroutineRunner : MonoBehaviour {
            ~CoroutineRunner() {
                Destroy(gameObject);
            }
        }

        private static CoroutineRunner coroutineRunner;

        public static Coroutine Run(this IEnumerator ienum)
        {
            if (coroutineRunner == null)
            {
                coroutineRunner = new GameObject("[Wolf3D.CoroutineRunner]").AddComponent<CoroutineRunner>();
                coroutineRunner.hideFlags = HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable | HideFlags.DontSaveInBuild;
                coroutineRunner.gameObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable | HideFlags.DontSaveInBuild;
            }

            return coroutineRunner.StartCoroutine(ienum);
        }
        #endregion

        #region Get Mesh
        public enum MeshType 
        { 
            HeadMesh, 
            BeardMesh,
            TeethMesh
        }

        private static readonly string[] HeadMeshNameFilter = { 
            "Wolf3D.Avatar_Renderer_Head", 
            "Wolf3D.Avatar_Renderer_Avatar",
            "Avatar_Head"
        };

        private static readonly string[] BeardMeshNameFilter = {
            "Wolf3D.Avatar_Renderer_Beard",
            "Beard"
        };

        private static readonly string[] TeethMeshNameFilter = {
            "Wolf3D.Avatar_Renderer_Teeth",
            "Teeth"
        };

        public static SkinnedMeshRenderer GetMeshRenderer(this GameObject gameObject, MeshType meshType)
        {
            SkinnedMeshRenderer mesh = null;
            var allChildren = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>().ToList();

            switch (meshType)
            {
                case MeshType.HeadMesh:
                    GetMesh(child => HeadMeshNameFilter.Contains(child.name));
                    break;
                case MeshType.BeardMesh:
                    GetMesh(child => BeardMeshNameFilter.Contains(child.name));
                    break;
                case MeshType.TeethMesh:
                    GetMesh(child => TeethMeshNameFilter.Contains(child.name));
                    break;
            }

            void GetMesh(Func<SkinnedMeshRenderer, bool> func)
            {
                mesh = allChildren.FirstOrDefault(func);

                if (mesh == null)
                {
                    if (meshType == MeshType.HeadMesh)
                    {
                        throw new Exception($"ExtensionMethods.GetMeshRenderer: {meshType} not found on {gameObject.name}. Make sure this method is called on a avatar game object.");
                    }
                    else if(allChildren.Count > 1)
                    {
                        // Debug.Log($"ExtensionMethods.GetMeshRenderer: {meshType} not found on {gameObject.name}.");
                    }
                }
            }

            return mesh;
        }
        #endregion
    }
}
