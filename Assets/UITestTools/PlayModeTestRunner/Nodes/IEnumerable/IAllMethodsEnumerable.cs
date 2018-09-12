using System.Collections.Generic;

namespace Tests.Nodes
{
    public interface IAllMethodsEnumerable
    {
        IEnumerable<MethodNode> AllMethods { get; }
    }
}