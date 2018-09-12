using System.Collections.Generic;

namespace Tests.Nodes
{
    public interface IAllClassesEnumerable
    {
        IEnumerable<ClassNode> AllClasses { get; }
    }
}