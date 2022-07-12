using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{

    public enum DecompiledFileTypes : short
    {
        QC,
        ReferenceMesh,
        LodMesh,
        BoneAnimation,
        PhysicsMesh,
        VertexAnimation,
        ProceduralBones,
        TextureBmp,
        Debug,
        DeclareSequenceQci
    }
    public enum Engines : short
    {
        GoldSource,
        Source,
        Source2
    }

    public enum MdlVersions : short
    {
        DoNotOverride,
        MDLv06,
        MDLv10,
        MDLv2531,
        MDLv27,
        MDLv28,
        MDLv29,
        MDLv30,
        MDLv31,
        MDLv32,
        MDLv35,
        MDLv36,
        MDLv37,
        MDLv38,
        MDLv44,
        MDLv45,
        MDLv46,
        MDLv47,
        MDLv48,
        MDLv49,
        MDLv52,
        MDLv53,
        MDLv57
    }
}
