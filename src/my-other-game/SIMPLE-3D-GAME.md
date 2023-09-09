# Simple3DGameDX
## GameMain
  - state machine for the game
  - Simple3DGame
    - game logic
  - GameRenderer
    - graphics rendering

## Simple3DGame
  - GameRenderer
    - graphics renderer
  - Camera
    - view projections
  - Audio
    - sound output
  - m_objects
    - game physics
  - m_renderObjects
    - list of objects to be rendered

## GameRenderer

  - m_constantBufferNeverChanges
    - light sources position
  - m_constantBufferChangeOnResize
    - m_game->GameCamera()->Projection()
  - m_constantBufferChangesEveryFrame
    - view projection (m_game->GameCamera()->View())
  - m_constantBufferChangesEveryPrim
    - created once but passed to every gameObject.Render method to be filled

## GameObjects
  - contains common object properties
  - rendering + physics (Render(), IsTouching())

  - MeshObject m_mesh;
  - Material m_normalMaterial;
    Material m_hitMaterial;

## Mesh

  - vertexBuffer
  - vertexCount
  - indexBuffer
  - indexCount

  - cylinder mesh
  - face mesh
  - sphere mesh
  - world mesh

## Material

  - mesh color
  - diffuse color
  - specular color
  - specular exponent
  - textureResourceView
  - vertexShader
  - pixelShader