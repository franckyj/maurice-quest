# my-game

## references

### directx

- https://github.com/microsoft/DirectXTK/wiki/Getting-Started
- https://fgiesen.wordpress.com/2012/02/12/row-major-vs-column-major-row-vectors-vs-column-vectors/
- https://seanmiddleditch.github.io/matrices-handedness-pre-and-post-multiplication-row-vs-column-major-and-notations/
- left-handness
- shaders are column-major
- https://learn.microsoft.com/en-us/windows/win32/dxmath/pg-xnamath-getting-started
- **
     System.Numeric.Matrix4x4 methods are RIGHT HANDED!
     https://github.com/dotnet/runtime/issues/80332
  **

- works
  no transpose
  no row_major
  mul(M, v)

** ?
   the shader reads the matrix _as if_ it was column_major and therefore
   we need to use a column vector for everything to works
**

### notes

- seperate 'effects' from 'shape'
  - box: effect = solid vs blend colors, shape = cube vs rectangle vs other

- camera near / far planes are along the camera "look_at" vector
  - https://stackoverflow.com/questions/58045444/which-axis-is-used-for-the-near-and-far-plane-when-using-the-perspective-camera

- vortice matrices are sent as column major and read as column major by the shader
  - use post-mul
  - P * V * W * v

# voxel stuff

- cubes
- 3d array, x y z is cube position
- eliminate hidden vertex
- the `chunk` has the vertex + index buffers
- can use instancing since we know where the blocks are with their indexes
  - we can create the "good" mesh for each cube depending on their neighbours

# game objects

game objects have

- position

this is for the mesh generation

- transformation matrix