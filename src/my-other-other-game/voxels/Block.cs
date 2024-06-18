namespace MyOtherOtherGame.Voxels;

enum BlockType
{
    Default = 0,
    Grass,
    Dirt,
    Water,
    Stone,
    Wood,
    Sand
};

internal struct Block
{
    public Block()
    {
        IsActive = true;
        BlockType = BlockType.Default;
    }

    public bool IsActive { get; set; }
    BlockType BlockType { get; set; }
}
