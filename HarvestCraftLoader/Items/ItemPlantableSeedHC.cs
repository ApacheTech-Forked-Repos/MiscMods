﻿using System;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace HarvestCraftLoader
{
    class ItemPlantableSeedHC : ItemPlantableSeed
    {
        public override void OnHeldInteractStart(ItemSlot itemslot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling)
        {
            if (blockSel == null) return;

            BlockPos pos = blockSel.Position;

            string lastCodePart = itemslot.Itemstack.Collectible.LastCodePart();

            BlockEntity be = byEntity.World.BlockAccessor.GetBlockEntity(pos);
            if (be is BlockEntityFarmland)
            {
                Block cropBlock = byEntity.World.GetBlock(CodeWithPath("crops-" + lastCodePart + "-0"));
                if (cropBlock == null) return;

                IPlayer byPlayer = null;
                if (byEntity is EntityPlayer) byPlayer = byEntity.World.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);
                bool planted = (be as BlockEntityFarmland).TryPlant(cropBlock);

                if (planted)
                {
                    byEntity.World.PlaySoundAt(new AssetLocation("sounds/block/plant"), pos.X, pos.Y, pos.Z, byPlayer);

                    ((byEntity as EntityPlayer)?.Player as IClientPlayer)?.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);

                    if (byPlayer?.WorldData?.CurrentGameMode != EnumGameMode.Creative)
                    {
                        itemslot.TakeOut(1);
                        itemslot.MarkDirty();
                    }
                }

                if (planted) handHandling = EnumHandHandling.PreventDefault;
            }
        }

        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

            Block cropBlock = world.GetBlock(CodeWithPath("crops-" + inSlot.Itemstack.Collectible.LastCodePart() + "-1"));
            if (cropBlock == null || cropBlock.CropProps == null) return;

            dsc.AppendLine(Lang.Get("soil-nutrition-requirement") + cropBlock.CropProps.RequiredNutrient);
            dsc.AppendLine(Lang.Get("soil-nutrition-consumption") + cropBlock.CropProps.NutrientConsumption);
            dsc.AppendLine(Lang.Get("soil-growth-time") + Math.Round(cropBlock.CropProps.TotalGrowthDays, 1) + " days");
        }
    }
}
