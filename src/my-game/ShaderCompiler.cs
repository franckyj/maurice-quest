using Vortice.D3DCompiler;
using Vortice.Direct3D;

namespace MyGame;

internal static class ShaderCompiler
{
    public static Blob CompileShader(string fileName, string entryPoint, string profile)
    {
        ShaderFlags compileFlags = ShaderFlags.EnableStrictness;

        Blob tempShaderBlob;
        Blob errorBlob;

        var result = Compiler.CompileFromFile(fileName, null, null, entryPoint, profile, compileFlags, out tempShaderBlob, out errorBlob);

        if (errorBlob != null)
        {
            throw new Exception(errorBlob.AsString());
        }

        return tempShaderBlob;
    }
}
