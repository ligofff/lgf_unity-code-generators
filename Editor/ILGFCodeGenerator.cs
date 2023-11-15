using System.Collections.Generic;

namespace Editor
{
    public interface ILGFCodeGenerator
    {
        public IEnumerable<LGF_CodeGeneratorFile> Generate(Dictionary<string, object> parameters);
    }
}