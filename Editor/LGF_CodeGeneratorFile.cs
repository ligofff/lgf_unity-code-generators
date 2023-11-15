using System;

namespace Editor
{
    [Serializable]
    public class LGF_CodeGeneratorFile
    {
        public string fileName;
        public string fileContent;

        public LGF_CodeGeneratorFile(string fileName, string fileContent)
        {
            this.fileName = fileName;
            this.fileContent = fileContent;
        }
    }
}