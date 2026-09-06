using System.Collections.Generic;
using IncrementalGame.Core;
using UnityEngine;

namespace IncrementalGame.Presentation
{
    public sealed partial class MomentumNeonView
    {
        private readonly Dictionary<int,Transform> _cans=new Dictionary<int,Transform>();
        private Mesh _canMesh,_canRing;
        public int PickupViewCount => _cans.Count;
        private void SyncPickupViews(MomentumSimulation sim)
        {
            foreach(var pickup in sim.Pickups)
            {
                if(!_cans.TryGetValue(pickup.Id,out var root))
                {
                    if(_canMesh==null) { _canMesh=CrystalMeshFactory.Polyhedron(3);_canRing=CrystalMeshFactory.Torus(.82f,.12f,16);_meshes.Add(_canMesh);_meshes.Add(_canRing); }
                    root=Node("B bounty can "+pickup.Id,transform);
                    var body=MeshObject(root,"Green can body",_canMesh,_crystalMaterial,5);
                    body.transform.localScale=new Vector3(.19f,.19f,.37f);body.transform.localRotation=Quaternion.Euler(80,0,0);
                    Tint(body,new Color(.55f,.95f,.12f));
                    foreach(var y in new[]{-.175f,.175f})
                    {
                        var rim=MeshObject(root,"Silver can rim",_canRing,_crystalMaterial,6);
                        rim.transform.localPosition=new Vector3(0,y,0);rim.transform.localScale=Vector3.one*.2f;
                        rim.transform.localRotation=Quaternion.Euler(80,0,0);Tint(rim,new Color(.8f,.95f,1));
                    }
                    Glow(root,"B can halo",Vector3.zero,.4f,new Color(.55f,1,.1f,.2f),3);_cans.Add(pickup.Id,root);
                }
                root.gameObject.SetActive(pickup.Active);root.position=LogicalSpace.ToWorld(pickup.Position);
            }
        }
    }
}
