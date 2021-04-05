using System;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    [Obsolete("Use IMeshModifier instead", true)]
    // 弃用
    public interface IVertexModifier
    {
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete("use IMeshModifier.ModifyMesh (VertexHelper verts)  instead", true)]
        void ModifyVertices(List<UIVertex> verts);
    }

    // 使用 VertexHelper 修改 Mesh
    public interface IMeshModifier
    {
        [Obsolete("use IMeshModifier.ModifyMesh (VertexHelper verts) instead", false)]
        void ModifyMesh(Mesh mesh);
        void ModifyMesh(VertexHelper verts);
    }
}
