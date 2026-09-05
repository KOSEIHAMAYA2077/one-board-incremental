using UnityEngine;
namespace IncrementalGame.Presentation
{
    public sealed class CrystalKitLibrary : ScriptableObject
    {
        public Mesh[] Shapes, Wires;
        public Mesh Ring;
        public Material Glass, Metal, Light;
    }
}
