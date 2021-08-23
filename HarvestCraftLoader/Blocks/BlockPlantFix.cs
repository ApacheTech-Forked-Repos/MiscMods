﻿using Vintagestory.API.Common;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace HarvestCraftLoader
{
    public class BlockPlantFix : Block
    {
        public override void OnJsonTesselation(ref MeshData sourceMesh, ref int[] lightRgbsByCorner, BlockPos pos, Block[] chunkExtBlocks, int extIndex3d)
        {
            int sunLightLevel = lightRgbsByCorner[extIndex3d] & 31;
            bool waveOff = sunLightLevel < 14;

            if (VertexFlags.GrassWindWave)
            {
                setLeaveWaveFlags(sourceMesh, waveOff);
            }
        }

        void setLeaveWaveFlags(MeshData sourceMesh, bool off)
        {
            int grassWave = VertexFlags.FoliageWindWaveBitMask;
            int clearFlags = (~VertexFlags.FoliageWindWaveBitMask) & (~VertexFlags.GroundDistanceBitMask);

            // Iterate over each element face
            for (int vertexNum = 0; vertexNum < sourceMesh.GetVerticesCount(); vertexNum++)
            {
                float y = sourceMesh.xyz[vertexNum * 3 + 1];

                bool notwaving = off || y < 0.5;

                sourceMesh.Flags[vertexNum] &= clearFlags;

                if (!notwaving)
                {
                    sourceMesh.Flags[vertexNum] |= grassWave;
                }
            }
        }


        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            if (CanPlantStay(world.BlockAccessor, blockSel.Position))
            {
                return base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode);
            }

            failureCode = "requirefertileground";

            return false;
        }


        public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
        {
            if (!CanPlantStay(world.BlockAccessor, pos))
            {
                world.BlockAccessor.BreakBlock(pos, null);
                world.BlockAccessor.TriggerNeighbourBlockUpdate(pos);
            }
        }

        public virtual bool CanPlantStay(IBlockAccessor blockAccessor, BlockPos pos)
        {
            Block block = blockAccessor.GetBlock(pos.X, pos.Y - 1, pos.Z);
            return block.Fertility > 0;
        }


        public override bool TryPlaceBlockForWorldGen(IBlockAccessor blockAccessor, BlockPos pos, BlockFacing onBlockFace, LCGRandom worldGenRand)
        {
            if (!CanPlantStay(blockAccessor, pos)) return false;
            return base.TryPlaceBlockForWorldGen(blockAccessor, pos, onBlockFace, worldGenRand);
        }

        public override int GetRandomColor(ICoreClientAPI capi, BlockPos pos, BlockFacing facing)
        {
            int color = base.GetRandomColor(capi, pos, facing);

            if (EntityClass == "Sapling")
            {
                color = capi.World.ApplyColorMapOnRgba(this.ClimateColorMap, this.SeasonColorMap, color, pos.X, pos.Y, pos.Z);
            }

            return color;
        }

    }
}