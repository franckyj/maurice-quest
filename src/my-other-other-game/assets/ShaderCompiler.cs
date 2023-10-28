using SharpGen.Runtime;
using Vortice.D3DCompiler;
using Vortice.Direct3D;

namespace MyOtherOtherGame.Assets;

internal static class ShaderCompiler
{
    public static Blob CompileShader(string fileName, string entryPoint, string profile)
    {
        ShaderFlags compileFlags = ShaderFlags.EnableStrictness;
        var filePath = Path.GetDirectoryName(fileName);
        using (var includeHandler = new ShaderIncludeHandler(filePath))
        {
            var result = Compiler.CompileFromFile(fileName, null, includeHandler, entryPoint, profile, compileFlags, out Blob tempShaderBlob, out Blob errorBlob);
            if (errorBlob != null)
            {
                throw new Exception(errorBlob.AsString());
            }

            return tempShaderBlob;
        }
    }

    public class ShaderIncludeHandler : CallbackBase, Include
    {
        private readonly List<string> _includeDirectories;

        public ShaderIncludeHandler(params string[] includeDirectories)
        {
            _includeDirectories = new(includeDirectories);
        }

        public Stream Open(IncludeType type, string fileName, Stream? parentStream)
        {
            var includeFile = GetFilePath(fileName);

            if (!File.Exists(includeFile))
                throw new FileNotFoundException($"Include file '{fileName}' not found.");

            var includeStream = new FileStream(includeFile, FileMode.Open, FileAccess.Read);

            return includeStream;
        }

        public void Close(Stream stream)
        {
            stream.Dispose();
        }

        private string? GetFilePath(string fileName)
        {
            if (File.Exists(fileName))
                return fileName;

            for (int i = 0; i < _includeDirectories.Count; i++)
            {
                var filePath = Path.GetFullPath(Path.Combine(_includeDirectories[i], fileName));

                if (File.Exists(filePath))
                {
                    var fileDirectory = Path.GetDirectoryName(filePath);

                    if (!_includeDirectories.Contains(fileDirectory))
                        _includeDirectories.Add(fileDirectory);

                    return filePath;
                }
            }

            return null;
        }
    }
}
