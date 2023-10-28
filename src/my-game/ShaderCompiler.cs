using Vortice.D3DCompiler;
using Vortice.Direct3D;

namespace MyGame;

internal static class ShaderCompiler
{
    public static Blob CompileShader(string fileName, string entryPoint, string profile)
    {
        Blob tempShaderBlob;
        Blob errorBlob;

        ShaderFlags shaderFlags = ShaderFlags.EnableStrictness;
#if DEBUG
        shaderFlags |= ShaderFlags.Debug;
        shaderFlags |= ShaderFlags.SkipValidation;
#else
        shaderFlags |= ShaderFlags.OptimizationLevel3;
#endif

        var result = Compiler.CompileFromFile(fileName, null, null, entryPoint, profile, shaderFlags, out tempShaderBlob, out errorBlob);

        if (errorBlob != null)
        {
            throw new Exception(errorBlob.AsString());
        }

        return tempShaderBlob;
    }
}
