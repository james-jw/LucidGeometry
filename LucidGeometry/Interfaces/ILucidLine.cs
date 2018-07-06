using System.Collections.Generic;

namespace LucidGeometry
{
    public interface ILucidLine : ILucidGeometry {  
        List<List<ILucidVertex>> Paths { get; set; }
    }
}